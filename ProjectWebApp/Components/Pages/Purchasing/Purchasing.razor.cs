using System.IO;
using System.Runtime.CompilerServices;
using System.Globalization;
using Microsoft.AspNetCore.Components;
//MudBlazor
using MudBlazor;
using static MudBlazor.Icons;
//In APP Systems
using PurchasingSystem.ViewModels;
using PurchasingSystem.BLL;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace ProjectWebApp.Components.Pages.Purchasing
{
    public partial class Purchasing
    {
        #region feilds
        //variables for the list of vendors 
        private List<VendorsView> VendorListView { get; set; } = new();
        private Dictionary<int, VendorsView> vendors = [];
        //for the bind value
        private int? selectedVendor;
        //need to update the selected vendor to a seperate variable to access it using the search button
        private VendorsView? SelectedVendorView;
        #region Authentication

        private string? userId;
        private string? userName;
        private string? email;
        private List<string> roles = new();

        #endregion
        #region Validation
        //front end error handling
        private string feedbackMessage = string.Empty;
        private string errorMessage = string.Empty;
        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedbackMessage);
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage);
        private List<string> errorDetails = new();
        bool wasSearchButtonPressed = false;
        // flag to if the form is valid.
        private bool isFormValid;
        // flag if data has change
        private bool hasDataChanged = false;
        // disables save and place if QTO = 0
        private bool disableSave = true;
        private bool disablePlace = true; 
        // set text for cancel/close button
        private string closeButtonText => hasDataChanged ? "Delete" : "Delete";
        #endregion
        #region Part Tables
        public PurchaseOrderView currentPurchaseOrder { get; set; } = new();
        public List<PartsView> Inventory { get; set; } = new();
        public PurchaseOrderDetailsView tempPOD { get; set; } = new();

        #endregion
        #endregion

        #region Properties
        // Services
        [Inject]
        protected PurchaseOrdersService PurchaseOrdersService { get; set; } = default!;
        [Inject]
        protected PurchaseOrderDetailsService PurchaseOrderDetailsService { get; set; } = default!;
        [Inject]
        protected PartsService PartService { get; set; } = default!;
        [Inject]
        protected VendorsService VendorsService { get; set; } = default!;
        // Authentication
        [Inject]
        protected AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        // Navigation
        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject]
        protected IDialogService DialogService { get; set; } = default!;
        #endregion

        #region Methods
        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {

                userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                userName = user.Identity.Name;

                email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

                roles = user.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
            }

            //  reset the error detail list
            errorDetails.Clear();

            //  reset the error message to an empty string
            errorMessage = string.Empty;

            //  reset feedback message to an empty string
            feedbackMessage = String.Empty;
            //  check to see if we are navigating using a valid customer CustomerID.

            //populates the dropdown
            await base.OnInitializedAsync();
            try
            {
                VendorListView = VendorsService.GetVendorList();
                PopulateCollections();
                await InvokeAsync(StateHasChanged);
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
            catch (ArgumentException ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
            catch (AggregateException ex)
            {
                //  have a collection of errors
                //  each error should be place into a separate line
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"{errorMessage}{Environment.NewLine}";
                }

                errorMessage = $"{errorMessage}Unable to search for customer";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
        }
        //Populate the vendor drop down
        private void PopulateCollections()
        {
            try
            {
                foreach (var vendor in VendorListView)
                {
                    vendors.Add(vendor.VendorID, vendor);
                }
            }
            catch (Exception ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;

            }
        }
        //The Search Vendor Button
        private async Task SearchVendorGetInfo()
        {
            hasDataChanged = false;
            try
            {
                //clears the error messages
                errorDetails.Clear();
                errorMessage = string.Empty;
                feedbackMessage = string.Empty;



                if (selectedVendor.HasValue)
                {
                    //creates a useable view of the selected vendor
                    SelectedVendorView = vendors[selectedVendor.Value];

                    //get the purchase order and return null or existing
                    currentPurchaseOrder = PurchaseOrdersService.GetPurchaseOrder(selectedVendor);

                    //Was getting no vendor on save when creating new PO
                    if (currentPurchaseOrder.VendorID == 0)
                    {
                        currentPurchaseOrder.VendorID = selectedVendor.Value;

                    }

                    //Gets the Purchase Order || Creates new Purchase order.
                    Inventory = PartService.GetPartList(selectedVendor, new List<int>());



                    if (currentPurchaseOrder.PurchaseOrderID == 0)
                    {
                        //creates a new list of items to order
                        PurchaseOrdersService.CreatePurchaseOrderList(Inventory, currentPurchaseOrder);
                    }


                    //Populate the Inventory of the Vendor
                    Inventory = PartService.RemoveParts(selectedVendor, Inventory, currentPurchaseOrder);

                    //UpdateOrder(selectedVendor, currentPurchaseOrder);
                    //Creates a list of OrderDetails parts from existing P.O. 
                    wasSearchButtonPressed = true;
                    await InvokeAsync(StateHasChanged);
                }
            }
            catch (Exception ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
        }
        //Front End variables
        private string GetSaveUpdateText(PurchaseOrderView currentPurchaseOrder)
        {
            if (currentPurchaseOrder == null || currentPurchaseOrder.PurchaseOrderID == 0)
            {
                return ("Save");
            }
            return ("Update");
        }
        private string GetnewOrExistingPO(PurchaseOrderView currentPurchaseOrder)
        {
            return (currentPurchaseOrder.PurchaseOrderID == 0 ? "New Order" : currentPurchaseOrder.PurchaseOrderID.ToString());
        }
        //Buttons
        private async Task Save()
        {
            List<Exception> errorList = new List<Exception>();
            hasDataChanged = false;
            disablePlace = true;
            try
            {

                errorDetails.Clear();
                errorMessage = string.Empty;
                feedbackMessage = string.Empty;

                #region Error Handling
                if (currentPurchaseOrder.PurchaseOrderDetails.Any(x => x.Quantity == 0))
                {
                    throw new ArgumentException("Order Parts cannot be zero");
                }

                if (currentPurchaseOrder == null)
                {
                    throw new ArgumentNullException("No Purchase Order was supply");
                }

                if (currentPurchaseOrder.VendorID == 0)
                {
                    throw new ArgumentException("Vendor is required");
                }

                foreach (var orderDetail in currentPurchaseOrder.PurchaseOrderDetails)
                {
                    if (orderDetail.PartID == 0)
                    {
                        throw new ArgumentNullException("Missing part ID");
                    }
                }

                if (currentPurchaseOrder.PurchaseOrderDetails.Count == 0)
                {
                    throw new ArgumentException("Orders Must have parts in order to save");
                }
                #endregion

                PurchaseOrdersService.AddEditPurchaseOrder(currentPurchaseOrder, userId);

            }
            #region Catches
            catch (ArgumentNullException ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }

            catch (ArgumentException ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
            catch (AggregateException ex)
            {
                //  have a collection of errors
                //  each error should be place into a separate line
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"{errorMessage}{Environment.NewLine}";
                }
                errorMessage = $"{errorMessage}Unable to Save";
                foreach (var error in ex.InnerExceptions)
                {

                    errorDetails.Add(error.Message);
                }
            }
            #endregion
            if (errorDetails.IsNullOrEmpty() && errorMessage.IsNullOrEmpty())
            {
                
                if (currentPurchaseOrder.PurchaseOrderID == 0)
                { feedbackMessage = "Order Saved"; }
                else
                { feedbackMessage = "Order Updated"; }
                await SearchVendorGetInfo();
            }

        }
        private async Task PlaceOrder()
        {
            bool? results = await DialogService.ShowMessageBox("Confirm Cancel",
                            $"Order is being placed, is everything correct and you wish to place the Order?",
                            yesText: "Yes", cancelText: "No");
            if (results == null)
            {
                return;
            }
            try
            {
                errorDetails.Clear();
                errorMessage = string.Empty;
                feedbackMessage = string.Empty;
                if (currentPurchaseOrder.PurchaseOrderID == 0)
                {
                    throw new ArgumentNullException("No Order to Place, Please Save ORDER!");
                }

                if (currentPurchaseOrder.OrderDate != null)
                {
                    throw new ArgumentNullException("Order was already placed; Error in System - Place Order. ");
                }

                if (currentPurchaseOrder.PurchaseOrderDetails.Any(x => x.Quantity == 0))
                {
                    throw new ArgumentException("Order Parts cannot be zero");
                }

                if (currentPurchaseOrder == null)
                {
                    throw new ArgumentNullException("No Purchase Order was supply");
                }

                if (currentPurchaseOrder.VendorID == 0)
                {
                    throw new ArgumentException("Vendor is required");
                }

                foreach (var orderDetail in currentPurchaseOrder.PurchaseOrderDetails)
                {
                    if (orderDetail.PartID == 0)
                    {
                        throw new ArgumentNullException("Missing part ID");
                    }
                }

                if (currentPurchaseOrder.PurchaseOrderDetails.Count == 0)
                {
                    throw new ArgumentException("Orders Must have parts in order to save");
                }

                currentPurchaseOrder.OrderDate = DateTime.Now;
                PurchaseOrdersService.AddEditPurchaseOrder(currentPurchaseOrder, userId);
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
            catch (ArgumentException ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
            catch (AggregateException ex)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"{errorMessage}{Environment.NewLine}";
                }
                errorMessage = $"{errorMessage}Unable to Save";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }

            if (errorDetails.IsNullOrEmpty() && errorMessage.IsNullOrEmpty())
            {
                await SearchVendorGetInfo();
                feedbackMessage = "Order Successfully Placed";
            }

        }
        private async Task DeleteOrder()
        {


            bool? results = await DialogService.ShowMessageBox("Confirm Cancel",
                            $"Do you wish to DELETE your Purchase Order? All unsaved changes will be lost.",
                            yesText: "Yes", cancelText: "No");
            if (results == null)
            {
                return;
            }
            else
            {
                PurchaseOrdersService.DeletePurchaseOrder(currentPurchaseOrder);
                hasDataChanged = false;
                await SearchVendorGetInfo();
                feedbackMessage = "Deleted - (Note: Deliverable 2 the PO Will Change to zero once the database is updated. then the New PO will be generated with QTO Suggestions)";
            }
        }
        private async Task ClearOrder()
        {
            if (hasDataChanged)
            {
                bool? results = await DialogService.ShowMessageBox("Confirm Cancel",
                                $"You Have UNSAVED changes, Do you wish to Clear your Order and start over?",
                                yesText: "Yes", cancelText: "No");
                if (results == null)
                {

                    return;

                }
                else
                {
                    //Fake delete
                    hasDataChanged = false;
                    await SearchVendorGetInfo();
                    feedbackMessage = "Order Cleared, New Order Ready to be Saved";
                }
            }

        }
        //Add - Remove Buttons
        private async Task AddPartToCart(int partsId)
        {
            hasDataChanged = true;
            var part = Inventory.FirstOrDefault(x => x.PartsID == partsId);

            if (part != null)
            {
                currentPurchaseOrder.PurchaseOrderDetails.Add(new PurchaseOrderDetailsView
                {
                    PurchasOrderDetailID = tempPOD.PurchasOrderDetailID,
                    PurchaseOrderID = tempPOD.PurchaseOrderID,
                    PartID = part.PartsID,
                    Quantity = part.QTO,
                    PurchasePrice = part.PurchasePrice,
                    VendorPartNumber = tempPOD.VendorPartNumber,
                    RemoveFromViewFlag = tempPOD.RemoveFromViewFlag,
                    Part = new PartsView
                    {
                        PartsID = part.PartsID,
                        Description = part.Description,
                        QOH = part.QOH,
                        ROL = part.ROL,
                        QOO = part.QOO,
                        QTO = part.QTO,
                        PurchasePrice = part.PurchasePrice
                    }

                });

                Inventory.Remove(part);
                await InvokeAsync(StateHasChanged);
            }
        }
        private async Task RemovePartFromCart(int partsId)
        {
            hasDataChanged = true;
            var part = PartService.GetPart(partsId);
            if (part != null)
            {
                Inventory.Add(part);
                Inventory = Inventory.OrderBy(x => x.Description).ToList();

                var invoiceLine = currentPurchaseOrder.PurchaseOrderDetails.FirstOrDefault(x => x.Part.PartsID == partsId);
                if (invoiceLine != null)
                {
                    currentPurchaseOrder.PurchaseOrderDetails.Remove(invoiceLine);
                }
                await InvokeAsync(StateHasChanged);
            }
        }
        //Custom @bind for overloading functionality on Quantity change
        private void OnQuantityChanged(int newValue, PurchaseOrderDetailsView item)
        {

            if (item.Quantity != newValue)
            {
                item.Quantity = newValue;
                hasDataChanged = true;
            }

            if(currentPurchaseOrder.PurchaseOrderID == 0)
            {
                feedbackMessage = "Save Order before placing.";
                disablePlace = false;
            }
            
        }   
        #endregion
    }
}
