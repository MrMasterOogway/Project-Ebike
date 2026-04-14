using Microsoft.AspNetCore.Components;
using SalesReturnsSystem.BLL;
using SalesReturnsSystem.ViewModels;
using ProjectWebApp.Components;
using MudBlazor;
using System.IO;
using static Azure.Core.HttpHeader;
using Microsoft.AspNetCore.Components.Authorization;

namespace ProjectWebApp.Components.Pages.SalesReturns
{
    public partial class Return
    {
        private int saleInvoiceNum;

        private string reason = string.Empty;

        private string feedbackMessage = string.Empty;

        private string errorMessage = string.Empty;

        private decimal SubTotal;

        private decimal Discount;

        private decimal Tax;

        private decimal Total;

        private string? userId;

        private string? userName;

        private string? email;

        private List<string> roles = new();

        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedbackMessage);

        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage);

        private List<string> errorDetails = new();

        private SaleDetailView SaleDetail;

        private SaleRefundView SaleRefund;

        private List<SaleDetailView> ReturnQuantity;

        [Inject]
        protected IDialogService DialogService { get; set; } = default!;

        [Inject]
        protected SaleService SaleService { get; set; } = default!;

        [Inject]
        protected PartService PartService { get; set; } = default!;

        [Inject]
        protected AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        private SaleView currentSale;

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
        }

        private void Search()
        {
            try
            {
                //  reset the error detail list
                errorDetails.Clear();

                //  reset the error message to an empty string
                errorMessage = string.Empty;

                //  reset feedback message to an empty string
                feedbackMessage = string.Empty;

                //  clear the customer list before we do our search
                currentSale = null;

                if (saleInvoiceNum == 0)
                {
                    throw new ArgumentException("Please provide a sale invoice number");
                }

                currentSale = SaleService.GetSale(saleInvoiceNum);

                if (currentSale == null)
                {
                    feedbackMessage = "No sale invoice were found for your search criteria";
                }
                else
                {
                    feedbackMessage = "Search for sale invoice was successful";
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

        private async Task ProcessReturn()
        {
            try
            {
                bool isNewRefund = false;
                //  reset the error detail list
                errorDetails.Clear();

                //  reset the error message to an empty string
                errorMessage = string.Empty;

                //  reset feedback message to an empty string
                feedbackMessage = string.Empty;

                if (currentSale == null)
                {
                    throw new ArgumentNullException("Sale invoice is required");
                }

                if (currentSale.SaleDetails.Sum(x => x.ReturnQuantity) == 0)
                {
                    errorMessage = "To make a return, at least 1 item is required";
                }

                foreach (SaleDetailView saleDetail in currentSale.SaleDetails)
                {
                    if (saleDetail.ReturnQuantity != 0 && (saleDetail.Reason == "" || saleDetail.Reason == null))
                    {
                        errorMessage = "Reason is required for each item that is being returned";
                    }

                    //isNewRefund = SaleRefund.SaleID == 0;
                    //SaleRefund.EmployeeID = userId;
                    //SaleRefund.TaxAmount = Tax;
                    //SaleRefund.SubTotal = SubTotal;
                    await InvokeAsync(StateHasChanged);
                }

                //if (currentSale.SaleDetails.Any(x => x.ReturnQuantity > 0) && !string.IsNullOrWhiteSpace(reason))
                //{
                //    saleInvoiceNum = 0;
                //    currentSale = null;
                //    feedbackMessage = "";
                //    errorMessage = "";
                //}
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
            SubTotal = currentSale.SaleDetails.Sum(x => x.ReturnQuantity * x.SellingPrice);
            if (currentSale.Discount == null)
            {
                Discount = 0;
            }
            else
            {
                Discount = SubTotal * (decimal)(currentSale.Discount / 100);
            }
            Tax = SubTotal * 0.05M;
            Total = SubTotal + Tax - Discount;

            StateHasChanged();
        }

        private void OnQuantityChanged(SaleDetailView item, int newQuantity)
        {
            item.ReturnQuantity = newQuantity;
            Calculation();
        }

        private async Task Clear()
        {
            bool? results = await DialogService.ShowMessageBox("Confirm Clear", $"Do you wish to clear? All unsaved changes will be lost.", yesText: "Yes", cancelText: "No");

            if (results == true)
            {
                saleInvoiceNum = 0;
                currentSale = null;
                feedbackMessage = "";
                errorMessage = "";
            }
        }
    }
}
