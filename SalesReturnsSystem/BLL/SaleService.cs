using SalesReturnsSystem.DAL;
using SalesReturnsSystem.ViewModels;
using SalesReturnsSystem.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;

namespace SalesReturnsSystem.BLL
{
    public class SaleService
    {
        private readonly eBikeContext _ebikeContext;

        internal SaleService(eBikeContext eBikeContext)
        {
            _ebikeContext = eBikeContext;
        }

        public string GetCustomerFullName(int customerID)
        {
            return _ebikeContext.Customers
                    .Where(x => x.CustomerID == customerID)
                    .Select(x => $"{x.FirstName} {x.LastName}").FirstOrDefault() ?? string.Empty;
        }

        public SaleView GetSale(int saleID)
        {
            return _ebikeContext.Sales
                    .Where(x => x.SaleID == saleID && !x.RemoveFromViewFlag)
                    .Select(x => new SaleView
                    {
                        SaleID = saleID,
                        SaleDate = x.SaleDate,
                        CustomerID = x.CustomerID,
                        CustomerName = x.Customer.FirstName + " " + x.Customer.LastName,
                        EmployeeID = x.EmployeeID,
                        TaxAmount = x.TaxAmount,
                        CouponID = x.CouponID,
                        Discount = x.Coupon.CouponDiscount,
                        PaymentType = x.PaymentType,
                        SaleDetails = _ebikeContext.SaleDetails
                                            .Where(sd => sd.SaleID == saleID)
                                            .Select(sd => new SaleDetailView
                                            {
                                                SaleDetailID = sd.SaleDetailID,
                                                SaleID = sd.SaleID,
                                                PartID = sd.PartID,
                                                Quantity = sd.Quantity,
                                                PartDescription = sd.Part.Description,
                                                Discount = sd.Sale.Coupon.CouponDiscount,
                                                Refundable = sd.Part.Refundable,
                                                ReturnedQuantity = _ebikeContext.SaleRefundDetails
                                                                    .Where(srd => srd.PartID == sd.PartID && srd.SaleRefund.SaleID == sd.SaleID && !srd.RemoveFromViewFlag)
                                                                    .Sum(srd => srd.Quantity),
                                                SellingPrice = sd.SellingPrice,
                                                RemoveFromViewFlag = sd.RemoveFromViewFlag
                                            }).ToList(),
                        SaleRefunds = _ebikeContext.SaleRefunds
                                        .Where(sf => sf.SaleID == saleID)
                                        .Select(sf => new SaleRefundView
                                        {
                                            SaleRefundID = sf.SaleRefundID,
                                            SaleRefundDate = sf.SaleRefundDate,
                                            SaleID = sf.SaleID,
                                            EmployeeID = sf.EmployeeID,
                                            TaxAmount = sf.TaxAmount,
                                            SubTotal = sf.SubTotal,
                                            SaleRefundDetails = _ebikeContext.SaleRefundDetails
                                                                    .Where(sfd => sfd.SaleRefundID == sf.SaleRefundID)
                                                                    .Select(sfd => new SaleRefundDetailView
                                                                    {
                                                                        SaleRefundDetailID = sfd.SaleRefundDetailID,
                                                                        SaleRefundID = sfd.SaleRefundID,
                                                                        PartID = sfd.PartID,
                                                                        Quantity = sfd.Quantity,
                                                                        SellingPrice = sfd.SellingPrice,
                                                                        Reason = sfd.Reason,
                                                                        RemoveFromViewFlag = sfd.RemoveFromViewFlag
                                                                    }).ToList(),
                                            RemoveFromViewFlag = sf.RemoveFromViewFlag

                                        }).ToList()
                    }).FirstOrDefault();
        }

        public SaleView AddSale(SaleView saleView)
        {
            List<Exception> errorList = new List<Exception>();
            if (saleView == null)
            {
                throw new ArgumentNullException("No sale was supply");
            }
            if (saleView.CustomerID == 0)
            {
                throw new ArgumentNullException("No customer was provided");
            }
            if (string.IsNullOrEmpty(saleView.EmployeeID))
            {
                throw new ArgumentNullException("No employee was provided");
            }

            foreach (var saleDetail in saleView.SaleDetails)
            {
                if (saleDetail.PartID == 0)
                {
                    throw new ArgumentNullException("Missing part ID");
                }

                if (saleDetail.Quantity < 1)
                {
                    errorList.Add(new Exception($"Sale detail {saleDetail.PartDescription} has a value less than 1"));
                }
            }

            if (errorList.Any())
            {
                _ebikeContext.ChangeTracker.Clear();
                throw new AggregateException("Unable to save sale. Check Concerns", errorList);
            }

            List<SaleDetailView> referenceSaleDetailViews = _ebikeContext.SaleDetails
                .Where(x => x.SaleID == saleView.SaleID)
                .Select(x => new SaleDetailView
                {
                    SaleDetailID = x.SaleDetailID,
                    SaleID = x.SaleID,
                    PartID = x.PartID,
                    PartDescription = x.Part.Description,
                    Quantity = x.Quantity,
                    Refundable = x.Part.Refundable,
                    SellingPrice = x.SellingPrice,
                    RemoveFromViewFlag = x.RemoveFromViewFlag
                }).ToList();

            List<Part> parts = _ebikeContext.Parts
                .Select(x => x).ToList();

            Sale? sale = _ebikeContext.Sales
                .Where(x => x.SaleID == saleView.SaleID)
                .Select(x => x).FirstOrDefault();

            List<SaleDetail> saleDetails = _ebikeContext.SaleDetails
                .Where(x => x.SaleID == saleView.SaleID)
                .Select(x => x).ToList();

            if (sale == null)
            {
                sale = new Sale();
            }
            sale.SaleDate = saleView.SaleDate;
            sale.CustomerID = saleView.CustomerID;
            sale.EmployeeID = saleView.EmployeeID;
            sale.TaxAmount = saleView.TaxAmount;
            sale.SubTotal = saleView.SubTotal;
            sale.CouponID = saleView.CouponID;
            sale.PaymentType = saleView.PaymentType;

            foreach (var saleDetailView in saleView.SaleDetails)
            {
                // existing sale detail
                if (saleDetailView.SaleDetailID > 0) // This might not be needed because we're adding a new Sale detail not editing it
                {
                    SaleDetail? saleDetail = sale.SaleDetails
                        .Where(x => x.SaleDetailID == saleDetailView.SaleDetailID)
                        .Select(x => x).FirstOrDefault();
                    if (saleDetail == null) // If we're following the top, put the SaleDetail saleDetail = new(); in this if statement and remove everything else
                    {
                        string missingSaleDetail = $"Sale Detail for {saleDetailView.PartDescription} ";
                        missingSaleDetail = missingSaleDetail + "cannot be found in the existing sale detail";
                        errorList.Add(new Exception(missingSaleDetail));
                    }
                    else
                    {
                        saleDetail.Quantity = saleDetailView.Quantity;
                        saleDetail.RemoveFromViewFlag = saleDetailView.RemoveFromViewFlag;
                    }
                }
                else
                {
                    SaleDetail saleDetail = new();
                    saleDetail.SaleID = saleDetailView.SaleID;
                    saleDetail.PartID = saleDetailView.PartID;
                    saleDetail.Quantity = saleDetailView.Quantity;
                    saleDetail.SellingPrice = saleDetailView.SellingPrice;
                    saleDetail.RemoveFromViewFlag = saleDetailView.RemoveFromViewFlag;

                    // update part QOH
                    Part? part = parts.Where(x => x.PartID == saleDetailView.PartID)
                        .Select(x => x).FirstOrDefault();
                    if (part != null)
                    {
                        part.QuantityOnHand = part.QuantityOnHand - saleDetailView.Quantity;
                        // updated the parts.
                        _ebikeContext.Parts.Update(part);
                    }

                    sale.SaleDetails.Add(saleDetail);
                }
            }

            #region  Update parts quantity on hand (QOH)
            foreach (var saleDetailView in saleView.SaleDetails)
            {
                // get the part that we might be updating
                Part? part = parts.Where(x => x.PartID == saleDetailView.PartID)
                    .Select(x => x).FirstOrDefault();

                // get the sale detail that is stored in the database
                SaleDetailView? referenceSaleDetailView = referenceSaleDetailViews
                    .Where(x => x.SaleDetailID == saleDetailView.SaleDetailID)
                    .Select(x => x).FirstOrDefault();

                if (referenceSaleDetailView != null && part != null)
                {
                    if (referenceSaleDetailView.Quantity != saleDetailView.Quantity)
                    {
                        part.QuantityOnHand = part.QuantityOnHand - (referenceSaleDetailView.Quantity - saleDetailView.Quantity);
                        _ebikeContext.Parts.Update(part);
                    }
                }
            }
            #endregion

            #region Remove any details that have been deleted
            foreach (var referenceSaleDetail in referenceSaleDetailViews)
            {
                if (!saleView.SaleDetails.Any(x => x.SaleDetailID == referenceSaleDetail.SaleDetailID))
                {
                    Part? part = parts.Where(x => x.PartID == referenceSaleDetail.PartID)
                        .Select(x => x).FirstOrDefault();
                    if (part != null)
                    {
                        part.QuantityOnHand = part.QuantityOnHand + referenceSaleDetail.Quantity;
                        _ebikeContext.Parts.Update(part);
                    }

                    SaleDetail? deletedSaleDetail = _ebikeContext.SaleDetails
                        .Where(x => x.SaleDetailID == referenceSaleDetail.SaleDetailID)
                        .Select(x => x).FirstOrDefault();
                    if (deletedSaleDetail != null)
                    {
                        _ebikeContext.SaleDetails.Remove(deletedSaleDetail);
                    }
                }
            }
            #endregion // This might be for returning

            // new employee
            if (sale.SaleID == 0)
                _ebikeContext.Sales.Add(sale);
            else
                _ebikeContext.Sales.Update(sale);

            #region Final Error Check and Save Operation
            if (errorList.Count > 0)
            {
                _ebikeContext.ChangeTracker.Clear();
                throw new AggregateException("Unable to add sale. Please check error message(s)", errorList);
            }
            else
            {
                _ebikeContext.SaveChanges();
            }
            #endregion

            return GetSale(sale.SaleID);
        }

        public SaleView EditSale(SaleView saleView)
        {
            List<Exception> errorList = new List<Exception>();
            if (saleView == null)
            {
                throw new ArgumentNullException("No sale was supply");
            }
            //if (saleView.CustomerID == 0)
            //{
            //    throw new ArgumentNullException("No customer was provided");
            //}
            //if (string.IsNullOrWhiteSpace(saleView.EmployeeID))
            //{
            //    throw new ArgumentNullException("No employee was provided");
            //}

            foreach (var saleRefund in saleView.SaleRefunds)
            {
                if (saleRefund.SaleID == 0)
                {
                    throw new ArgumentNullException("Missing sale ID");
                }

                if (string.IsNullOrWhiteSpace(saleRefund.EmployeeID))
                {
                    throw new ArgumentNullException("No employee was provided");
                }

                foreach (var saleRefundDetail in saleRefund.SaleRefundDetails)
                {
                    if (saleRefundDetail.PartID == 0)
                    {
                        throw new ArgumentNullException("Missing part ID");
                    }

                    if (saleRefundDetail.Quantity < 1)
                    {
                        errorList.Add(new Exception($"Sale refund {saleRefundDetail.PartID} has a value less than 1"));
                    }

                    if (string.IsNullOrWhiteSpace(saleRefundDetail.Reason))
                    {
                        errorList.Add(new Exception("Sale refund reason cannot be empty"));
                    }
                }
            }
            if (errorList.Any())
            {
                _ebikeContext.ChangeTracker.Clear();
                throw new AggregateException("Unable to return item. Check Concerns", errorList);
            }

            #region Fetching Data and Setting Up References
            List<SaleRefundDetailView> referenceSaleRefundDetailViews = _ebikeContext.SaleRefundDetails
                .Where(x => x.SaleRefund.SaleID == saleView.SaleID)
                .Select(x => new SaleRefundDetailView
                {
                    SaleRefundDetailID = x.SaleRefundDetailID,
                    SaleRefundID = x.SaleRefundID,
                    PartID = x.PartID,
                    PartDescription = x.Part.Description,
                    Quantity = x.Quantity,
                    SellingPrice = x.SellingPrice,
                    Reason = x.Reason,
                    RemoveFromViewFlag = x.RemoveFromViewFlag
                }).ToList();

            List<Part> parts = _ebikeContext.Parts
                .Select(x => x).ToList();

            Sale? sale = _ebikeContext.Sales
                .Where(x => x.SaleID == saleView.SaleID)
                .Select(x => x).FirstOrDefault();

            List<SaleRefundDetail> saleRefundDetails = _ebikeContext.SaleRefundDetails
                .Where(x => x.SaleRefund.SaleID == saleView.SaleID)
                .Select(x => x).ToList();
            #endregion

            #region Processing Sale Refund
            foreach (var saleRefundView in saleView.SaleRefunds)
            {
                if (saleRefundView.SaleRefundID > 0)
                {
                    SaleRefund? saleRefund = sale.SaleRefunds
                        .Where(x => x.SaleRefundID == saleRefundView.SaleRefundID)
                        .Select(x => x).FirstOrDefault();
                    if (saleRefund == null)
                    {
                        string missingSaleRefund = $"Sale refund for {saleRefundView.SaleRefundID} ";
                        missingSaleRefund = missingSaleRefund + "cannot be found in the existing Sale Refunds";
                        errorList.Add(new Exception(missingSaleRefund));
                    }
                    else
                    {
                        saleRefund.EmployeeID = saleRefundView.EmployeeID;
                        saleRefund.TaxAmount = saleRefundView.TaxAmount;
                        saleRefund.SubTotal = saleRefund.SubTotal;
                        saleRefund.RemoveFromViewFlag = saleRefund.RemoveFromViewFlag;
                        foreach (var saleRefundDetailView in saleRefundView.SaleRefundDetails)
                        {
                            SaleRefundDetail saleRefundDetail = new();
                            saleRefundDetail.PartID = saleRefundDetailView.PartID;
                            saleRefundDetail.Quantity = saleRefundDetailView.Quantity;
                            saleRefundDetail.SellingPrice = saleRefundDetailView.SellingPrice;
                            saleRefundDetail.Reason = saleRefundDetailView.Reason;
                            saleRefundDetail.RemoveFromViewFlag = saleRefundDetailView.RemoveFromViewFlag;

                            Part? part = parts.Where(x => x.PartID == saleRefundDetailView.PartID)
                                .Select(x => x).FirstOrDefault();
                            if (part != null)
                            {
                                part.QuantityOnHand = part.QuantityOnHand + saleRefundDetailView.Quantity;
                                _ebikeContext.Parts.Update(part);
                            }

                            if (saleRefundDetail.SaleRefundDetailID == 0)
                            {
                                _ebikeContext.SaleRefundDetails.Add(saleRefundDetail);
                            }
                        }
                    }
                }
            }
            #endregion

            #region Update parts quantity on hand (QOH)
            foreach (var saleRefundView in saleView.SaleRefunds)
            {
                foreach (var saleRefundDetailView in saleRefundView.SaleRefundDetails)
                {
                    Part? part = parts.Where(x => x.PartID == saleRefundDetailView.PartID)
                        .Select(x => x).FirstOrDefault();
                    if (part != null)
                    {
                        part.QuantityOnHand = part.QuantityOnHand + saleRefundDetailView.Quantity;
                        _ebikeContext.Parts.Update(part);
                    }
                    else
                    {
                        throw new ArgumentNullException("Part was not found");
                    }
                }
            }


            #endregion

            #region Final Error Check and Save Operation
            if (errorList.Count > 0)
            {
                _ebikeContext.ChangeTracker.Clear();
                throw new AggregateException("Unable to refund item. Please check erro message(s)", errorList);
            }
            else
            {
                _ebikeContext.SaveChanges();
            }
            #endregion
            return GetSale(sale.SaleID);
        }

        //public SaleView GetSale(int saleID)
        //{
        //        sale = _ebikeContext.Sales
        //                .Where(x => x.SaleID == saleID && !x.RemoveFromViewFlag)
        //                .Select(x => new SaleView
        //                {
        //                    SaleID = saleID,
        //                    SaleDate = x.SaleDate,
        //                    CustomerID = x.CustomerID,
        //                    EmployeeID = x.EmployeeID,
        //                    TaxAmount = x.TaxAmount,
        //                    SaleDetails = _ebikeContext.SaleDetails
        //                                    .Where(saleDetail => saleDetail.SaleID == saleID)
        //                                    .Select(saleDetail => new SaleDetailView
        //                                    {
        //                                        SaleDetailID = saleDetail.SaleDetailID,
        //                                        SaleID = saleDetail.SaleID,
        //                                        PartID = saleDetail.PartID,
        //                                        Quantity = saleDetail.Quantity,
        //                                        SellingPrice = saleDetail.SellingPrice,
        //                                        RemoveFromViewFlag = saleDetail.RemoveFromViewFlag
        //                                    }).ToList()
        //                }).FirstOrDefault() ?? new SaleView();
        //        customerID = sale.CustomerID;
        //    }
        //    sale.CustomerName = GetCustomerFullName(customerID);
        //    return sale;
        //}
        //public SaleView Save(SaleView saleView)
        //{
        //    List<Exception> errorList = new List<Exception>();

        //}
    }
}
