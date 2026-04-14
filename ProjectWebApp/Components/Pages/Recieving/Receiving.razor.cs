using Microsoft.AspNetCore.Components;
using MudBlazor;
using ReceivingSystem.BLL;
using ReceivingSystem.ViewModels;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
namespace ProjectWebApp.Components.Pages.Recieving
{
    public partial class Receiving
    {
        #region Fields
        private int? purchaseOrderNumber;
        private bool noRecords;
        private string feedbackMessage = string.Empty;
        private string errorMessage = string.Empty;
        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedbackMessage);
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage);
        private string forceCloseReason = string.Empty;
        private List<string> errorDetails = new();
        private UnorderedPurchaseItemCartView newUnorderedItem = new();

        #region Authentication
        private string? userId;
        private string? userName;
        private string? email;
        private List<string> roles = new();
        #endregion
        #endregion

        #region Properties
        [Inject] protected PurchaseOrdersService PurchaseOrderService { get; set; } = default!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        [Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
        protected List<PurchaseOrderView> PurchaseOrder { get; set; } = new();
        protected List<PurchaseOrderDetailView> OrderDetails { get; set; } = new();
        protected List<UnorderedPurchaseItemCartView> UnorderedItems { get; set; } = new();
        private PurchaseOrderView? selectedOrder;
        #endregion

        #region Methods
        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                userName = user.Identity.Name;
                email = user.FindFirst(ClaimTypes.Email)?.Value;
                roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            }

            await base.OnInitializedAsync();
            try
            {
                PurchaseOrder = PurchaseOrderService.GetOrders(0);
            }
            catch (Exception ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
            await InvokeAsync(StateHasChanged);
        }

        private void Search()
        {
            try
            {
                noRecords = false;
                errorDetails.Clear();
                errorMessage = string.Empty;
                feedbackMessage = string.Empty;
                PurchaseOrder.Clear();

                int searchNumber = purchaseOrderNumber ?? 0;
                PurchaseOrder = PurchaseOrderService.GetOrders(searchNumber);

                if (PurchaseOrder.Count > 0)
                {
                    feedbackMessage = $"Search found {PurchaseOrder.Count} record(s).";
                }
                else
                {
                    feedbackMessage = "No records found for your search criteria.";
                    noRecords = true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
        }

        private void ViewOrder(PurchaseOrderView order)
        {
            selectedOrder = order;
            feedbackMessage = string.Empty;
            errorMessage = string.Empty;
            errorDetails.Clear();
            UnorderedItems.Clear();

            try
            {
                var result = PurchaseOrderService.GetOrderDetails(order.PurchaseOrderNumber);
                selectedOrder = result;
                OrderDetails = result.PurchaseOrderDetails;
            }
            catch (Exception ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
        }

        private async Task Reset()
        {
            bool? confirmed = await DialogService.ShowMessageBox(
                "Confirm Reset",
                "Do you want to reset all changes?",
                yesText: "Yes", cancelText: "No");

            if (confirmed == true)
            {
                ViewOrder(selectedOrder);
            }
        }

        private async Task ForceClose()
        {
            if (string.IsNullOrWhiteSpace(forceCloseReason))
            {
                errorMessage = "Force Close requires a reason.";
                return;
            }

            bool? confirmed = await DialogService.ShowMessageBox(
                "Force Close",
                "Are you sure you want to force close this order?",
                yesText: "Yes", cancelText: "No");

            if (confirmed != true)
            {
                return;
            }

            string reason = "Force closed manually";

            try
            {
                PurchaseOrderService.ForceCloseOrder(selectedOrder!.PurchaseOrderID, reason);

                feedbackMessage = "Order force-closed.";
                selectedOrder = null;
                OrderDetails.Clear();
                UnorderedItems.Clear();
                PurchaseOrder = PurchaseOrderService.GetOrders(0);
                forceCloseReason = string.Empty;
            }
            catch (Exception ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
        }

        private void InsertUnorderedItem()
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(newUnorderedItem.Description) ||
                string.IsNullOrWhiteSpace(newUnorderedItem.VendorPartNumber) ||
                newUnorderedItem.Quantity <= 0)
            {
                errorMessage = "All unordered item fields are required.";
                return;
            }

            UnorderedItems.Add(new UnorderedPurchaseItemCartView
            {
                Description = newUnorderedItem.Description,
                VendorPartNumber = newUnorderedItem.VendorPartNumber,
                Quantity = newUnorderedItem.Quantity
            });

            newUnorderedItem = new();
            feedbackMessage = "Unordered item added.";
        }

        private void Receive()
        {
            errorDetails.Clear();
            bool valid = true;

            foreach (var item in OrderDetails)
            {
                if (item.ReceiveQuantity > item.Outstanding)
                {
                    errorDetails.Add($"Received quantity for Part ID {item.PartID} exceeds outstanding amount.");
                    valid = false;
                }

                if (item.ReturnQuantity > item.Outstanding)
                {
                    errorDetails.Add($"Returned quantity for Part ID {item.PartID} exceeds outstanding amount.");
                    valid = false;
                }

                if (item.ReturnQuantity > 0 && string.IsNullOrWhiteSpace(item.Reason))
                {
                    errorDetails.Add($"Return reason is required for Part ID {item.PartID}.");
                    valid = false;
                }

                if ((item.ReturnQuantity + item.ReceiveQuantity) > item.Outstanding || (item.ReturnQuantity + item.ReceiveQuantity) < item.Outstanding)
                {
                    errorDetails.Add($"Returned quantity and Received quantity for Part ID {item.PartID} does not equal Outstanding amount.");
                    valid = false;
                }
            }

            foreach (var item in UnorderedItems)
            {
                if (string.IsNullOrWhiteSpace(item.Description) ||
                    string.IsNullOrWhiteSpace(item.VendorPartNumber) ||
                    item.Quantity <= 0)
                {
                    errorDetails.Add("Unordered items require description, vendor part number, and quantity.");
                    valid = false;
                }
            }

            if (!valid)
            {
                errorMessage = "Validation errors occurred.";
                return;
            }

            try
            {
                feedbackMessage = "Receiving processed successfully.";
                selectedOrder = null;
                OrderDetails.Clear();
                UnorderedItems.Clear();
                PurchaseOrder = PurchaseOrderService.GetOrders(0);
                valid = true;
            }
            catch (Exception ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
        }
        private void RemoveUnorderedItem(UnorderedPurchaseItemCartView item)
        {
            UnorderedItems.Remove(item);
        }
        #endregion
    }
}
