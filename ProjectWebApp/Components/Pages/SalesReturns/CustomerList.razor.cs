using Microsoft.AspNetCore.Components;
using SalesReturnsSystem.BLL;
using SalesReturnsSystem.ViewModels;
using ProjectWebApp.Components;
using MudBlazor;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace ProjectWebApp.Components.Pages.SalesReturns
{
    //Changed class to partial
    public partial class CustomerList
    {
        private string phoneNumber = string.Empty;

        private bool noRecords;

        private string feedbackMessage = string.Empty;

        private string errorMessage = string.Empty;

        private int tempNumber;

        private int partID;

        private int WantedQuantity;

        private decimal SubTotal;

        private decimal Tax;

        private decimal Total;

        private decimal NewTax;

        private decimal Discount;

        private decimal NewTotal;

        private string couponName = string.Empty;

        private bool workingCoupon = false;

        private bool completeSale = false;

        private bool isNewSale;

        private string paymentType { get; set; } = "";

        private string? userId;

        private string? userName;

        private string? email;

        private List<string> roles = new();

        private CategoryView StoredCategory;

        private PartView StoredPart;

        private SaleView sale = new();

        private CouponView Coupon;

        private List<SaleDetailView> tempSaleDetails = new();

        private bool noParts;

        private CustomerSearchView selectedCustomer { get; set; }

        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedbackMessage);

        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage);

        private List<string> errorDetails = new();

        [Inject]
        protected CustomerService CustomerService { get; set; } = default!;

        [Inject]
        protected PartService PartService { get; set; } = default!;

        [Inject]
        protected CategoryService CategoryService { get; set; } = default!;

        [Inject]
        protected CouponService CouponService { get; set; } = default!;

        [Inject]
        protected SaleService SaleService { get; set; } = default!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        protected IDialogService DialogService { get; set; } = default!;

        [Inject]
        protected AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        protected List<CustomerSearchView> Customers { get; set; } = new();

        protected List<CategoryView> Category { get; set; } = new();

        protected List<PartView> Parts { get; set; } = new();

        [Parameter] public int CategoryID { get; set; }


        protected override async Task OnInitializedAsync()
        {
            Category = CategoryService.GetCategories();
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {

                userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                userName = user.Identity.Name;

                email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

                roles = user.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
            }
        }

        private async Task PopulateParts(CategoryView category)
        {
            try
            {
                StoredCategory = category;
                // reset the no records flag
                noRecords = false;

                //  reset the error detail list
                errorDetails.Clear();

                //  reset the error message to an empty string
                errorMessage = string.Empty;

                //  reset feedback message to an empty string
                feedbackMessage = string.Empty;

                if (StoredCategory != null)
                {
                    Parts = PartService.GetParts(StoredCategory.CategoryID);
                }
                WantedQuantity = 0;
                StateHasChanged();
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

        private void Search()
        {
            try
            {
                // reset the no records flag
                noRecords = false;

                //  reset the error detail list
                errorDetails.Clear();

                //  reset the error message to an empty string
                errorMessage = string.Empty;

                //  reset feedback message to an empty string
                feedbackMessage = string.Empty;

                //  clear the customer list before we do our search
                Customers.Clear();

                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    throw new ArgumentException("Please provide a phone number");
                }

                //  search for our customers

                Customers = CustomerService.GetCustomers(phoneNumber);
                if (Customers.Count > 0)
                {
                    feedbackMessage = "Search for customer(s) was successful";
                }
                else
                {
                    feedbackMessage = "No customers were found for your search criteria";
                    noRecords = true;
                }
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

        private void Display(CustomerSearchView customer)
        {
            selectedCustomer = customer;
        }

        private void SearchParts()
        {
            try
            {
                //  reset the error detail list
                errorDetails.Clear();

                //  reset the error message to an empty string
                errorMessage = string.Empty;

                //  reset feedback message to an empty string
                feedbackMessage = String.Empty;

                // reset no parts to false
                noParts = false;

                if (selectedCustomer == null)
                {
                    throw new ArgumentNullException("Please select a customer");
                }
                
                if (StoredCategory == null || StoredCategory.CategoryID == 0)
                {
                    throw new ArgumentException("Please select a category");
                }

                if (StoredPart == null)
                {
                    throw new ArgumentNullException("Please select a part");
                }

                if (WantedQuantity == 0)
                {
                    throw new ArgumentException("Please add at least 1 quantity");
                }
                
                if (WantedQuantity > StoredPart.QuantityOnHand)
                {
                    throw new ArgumentException("Your Quantity is more than there are in stock");
                }

                SaleDetailView newTempSaleDetail = tempSaleDetails.Where(x => x.PartID == StoredPart.PartID).FirstOrDefault();
                if (newTempSaleDetail == null)
                {
                    SaleDetailView newSaleDetail = new();
                    newSaleDetail.SaleDetailID = 0;
                    newSaleDetail.SaleID = 0;
                    newSaleDetail.PartID = StoredPart.PartID;
                    newSaleDetail.Quantity = WantedQuantity;
                    newSaleDetail.SellingPrice = StoredPart.SellingPrice;
                    newSaleDetail.PartDescription = StoredPart.Description;
                    newSaleDetail.RemoveFromViewFlag = false;
                    tempSaleDetails.Add(newSaleDetail);
                    Calculation();
                }
                else
                {
                    newTempSaleDetail.Quantity = WantedQuantity;
                    Calculation();
                }
                
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
                errorMessage = $"{errorMessage}Unable to search for part";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
        }
        private void CheckCoupon()
        {
            try
            {
                //  reset the error detail list
                errorDetails.Clear();

                //  reset the error message to an empty string
                errorMessage = string.Empty;

                //  reset feedback message to an empty string
                feedbackMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(couponName))
                {
                    throw new ArgumentNullException("Please provide a Coupon");
                }

                Coupon = CouponService.GetCouponByID(couponName);

                if (Coupon == null)
                {
                    feedbackMessage = "That Coupon does not exist";
                }

                if (Coupon != null)
                {
                    workingCoupon = true;
                    Calculation();
                    StateHasChanged();
                }
                
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

        private void Calculation()
        {
            SubTotal = tempSaleDetails.Sum(x => x.Quantity * x.SellingPrice);
            Tax = SubTotal * 0.05M;
            Total = SubTotal + Tax;

            if (Coupon == null)
            {
                Discount = 0;
            }
            else
            {
                Discount = SubTotal * ((new decimal(Coupon.CouponDiscount)) / 100);
            }
            NewTax = (SubTotal - Discount) * 0.05M;
            NewTotal = SubTotal + NewTax - Discount;
            StateHasChanged();
        }

        private async Task CompleteSale()
        {
            try
            {
                //  reset the error detail list
                errorDetails.Clear();

                //  reset the error message to an empty string
                errorMessage = string.Empty;

                //  reset feedback message to an empty string
                feedbackMessage = string.Empty;

                if (selectedCustomer == null)
                {
                    throw new ArgumentNullException("Please select a Customer");
                }

                if (tempSaleDetails.Count == 0)
                {
                    throw new ArgumentException("Please add parts to do a sale");
                }

                if (string.IsNullOrWhiteSpace(paymentType))
                {
                    throw new ArgumentNullException("Please select a payment type");
                }

                // SAVE NEEDS TO BE CODED
                isNewSale = sale.SaleID == 0;
                sale.CustomerID = selectedCustomer.CustomerID;
                sale.EmployeeID = userId;
                sale.SaleDate = DateTime.Now;
                sale.TaxAmount = NewTax;
                sale.SubTotal = SubTotal;
                //if (sale.CouponID == null)
                //{
                //    sale.CouponID = 0;
                //}
                //else
                //{
                //    sale.CouponID = Coupon.CouponID;
                //}
                sale.PaymentType = paymentType;
                sale.SaleDetails = tempSaleDetails;
                sale = SaleService.AddSale(sale);
                feedbackMessage = isNewSale
                    ? $"New Sale No {sale.SaleID} was created"
                    : $"Sale No {sale.SaleID} was updated";
                await InvokeAsync(StateHasChanged);
                completeSale = true;
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

        private void DeletePart(int partID)
        {
            SaleDetailView newTempSaleDetail = tempSaleDetails.Where(x => x.PartID == partID).FirstOrDefault();
            if (newTempSaleDetail != null)
            {
                tempSaleDetails.Remove(newTempSaleDetail);
                Calculation();
            }
        }

        private void ClearPartSearch()
        {
            StoredCategory = null;
            StoredPart = null;
            WantedQuantity = 0;
        }

        private void ClearCoupon()
        {
            couponName = "";
            Coupon = null;
            workingCoupon = false;
            Calculation();
        }

        private async Task Clear()
        {
            bool? results = await DialogService.ShowMessageBox("Confirm Clear", $"Do you wish to clear? All unsaved changes will be lost.", yesText: "Yes", cancelText: "No");
            
            if (results == true)
            {
                Customers.Clear();
                phoneNumber = "";
                feedbackMessage = "";
                errorMessage = "";
                selectedCustomer = null;
                tempSaleDetails.Clear();
                paymentType = "";
                StoredCategory = null;
                StoredPart = null;
                WantedQuantity = 0;
                Calculation();
            }
        }
    }
}
