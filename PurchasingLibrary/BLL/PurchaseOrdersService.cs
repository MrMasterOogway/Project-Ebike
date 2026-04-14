using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.IdentityModel.Tokens;
using PurchasingSystem.DAL;
using PurchasingSystem.Entities;
using PurchasingSystem.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PurchasingSystem.BLL
{
    public class PurchaseOrdersService
    {
        private readonly EBikeContext _eBike_2025Context;
        internal PurchaseOrdersService(EBikeContext eBikeContext)
        {
            //  Initialize the _eBike_2025Context field with the provided EBikeContext instance.
            _eBike_2025Context = eBikeContext;
        }

        public PurchaseOrderView GetPurchaseOrder(int? vendorId)
        {
            #region business rules
           
            int exsistingOrderCount = _eBike_2025Context.PurchaseOrders.Where(x => x.VendorID == vendorId && x.OrderDate == null).Count();
            #endregion
            
            #region Query
            //selected vendor information
            if (exsistingOrderCount > 0)
            {
            return _eBike_2025Context.PurchaseOrders
                .Where(x => x.VendorID == vendorId && x.OrderDate == null)
                .Select(x => new PurchaseOrderView
                {
                    PurchaseOrderID = x.PurchaseOrderID,
                    OrderDate = null,
                    SubTotal = x.SubTotal,
                    GST = x.TaxAmount,
                    Total = x.SubTotal + x.TaxAmount,
                    VendorID = x.VendorID,
                    VendorName = x.Vendor.VendorName,
                    Address = x.Vendor.Address,
                    Phone = x.Vendor.Phone,
                    PONumber = x.PurchaseOrderNumber,
                    PurchaseOrderDetails = _eBike_2025Context.PurchaseOrderDetails
                        .Where(p => p.PurchaseOrderID == x.PurchaseOrderID)
                        .Select(p => new PurchaseOrderDetailsView
                        {
                            PurchasOrderDetailID = p.PurchaseOrderDetailID,
                            PurchaseOrderID = p.PurchaseOrderID,
                            PartID = p.PartID,
                            Quantity = p.Quantity,
                            PurchasePrice = p.PurchasePrice,
                            VendorPartNumber = p.VendorPartNumber,
                            Part = _eBike_2025Context.Parts
                                .Where(pa => pa.PartID == p.PartID)
                                .Select(pa => new PartsView
                                {
                                    PartsID = pa.PartID,
                                    Description = pa.Description,
                                    QOH = pa.QuantityOnHand,
                                    ROL = pa.ReorderLevel,
                                    QOO = pa.QuantityOnOrder,
                                    QTO = (pa.ReorderLevel - (pa.QuantityOnHand+ pa.QuantityOnOrder)) >0 
                    ? (pa.ReorderLevel - (pa.QuantityOnHand + pa.QuantityOnOrder))
                    : 0,
                                    PurchasePrice = pa.PurchasePrice,
                                    VendorID = pa.VendorID
                                }).FirstOrDefault()
                        }).ToList()
                }).FirstOrDefault();
            }
            else
            {
                return new();
            }
            

            

            #endregion
        }

        public PurchaseOrderView CreatePurchaseOrderList(List<PartsView> Inventory, PurchaseOrderView currentPO)
        {
            if (currentPO == null)
            {
                throw new ArgumentNullException("Need to select a vendor to create PO list ");
            }

            foreach (PartsView p in Inventory)
            {
                if (p.QTO > 0)
                {
                    currentPO.PurchaseOrderDetails.Add(new PurchaseOrderDetailsView
                    {
                        
                        PurchaseOrderID = currentPO.PurchaseOrderID,
                        PartID = p.PartsID,
                        Quantity = p.QTO,
                        PurchasePrice = p.PurchasePrice * p.QTO,
                        RemoveFromViewFlag = false,
                        Part = p
                    });
                    
                }
                
            }
            return currentPO;
        }

        public PurchaseOrderView AddEditPurchaseOrder(PurchaseOrderView currentOrderView, string EmployeeID)
        {

            #region business rules
            List<Exception> errorList = new List<Exception>();

            if (currentOrderView == null)
            {
                throw new ArgumentNullException("No Purchase Order was supply");
            }

            if (currentOrderView.VendorID == 0)
            {
                errorList.Add(new Exception("Vendor is required"));
            }

            if (currentOrderView.PurchaseOrderDetails.Count == 0)
            {
                errorList.Add(new Exception("order details are required"));
            }

            foreach (var orderDetail in currentOrderView.PurchaseOrderDetails)
            {
                if (orderDetail.PartID == 0)
                {
                    throw new ArgumentNullException("Missing part ID");
                }
                if (orderDetail.PurchasePrice < 0)
                {
                    string partName = _eBike_2025Context.Parts
                        .Where(x => x.PartID == orderDetail.PartID)
                        .Select(x => x.Description)
                        .FirstOrDefault();
                    errorList.Add(new Exception($"Part {partName} has a price lower than zero."));
                }
            }
            List<string> duplicatedParts = currentOrderView.PurchaseOrderDetails
                                        .GroupBy(x => new { x.PartID })
                                        .Where(gb => gb.Count() > 1)
                                        .OrderBy(gb => gb.Key.PartID)
                                        .Select(gb => _eBike_2025Context.Parts
                                                        .Where(p => p.PartID == gb.Key.PartID)
                                                        .Select(p => p.Description)
                                                        .FirstOrDefault()
                                        ).ToList();
            if (duplicatedParts.Count > 0)
            {
                foreach (var partName in duplicatedParts)
                {
                    errorList.Add(new Exception($"Part {partName} can only be added to the invoice lines once."));
                }
            }

           

            #endregion


            #region Query Select

            PurchaseOrder order = _eBike_2025Context.PurchaseOrders
                .Where(x => x.PurchaseOrderID == currentOrderView.PurchaseOrderID)
                .FirstOrDefault();
            if(currentOrderView.PurchaseOrderID != 0)
            {
                //Create a list of parts from existing data base
                List<PurchaseOrderDetail> OldOrderParts = _eBike_2025Context.PurchaseOrderDetails
                    .Where(x => x.PurchaseOrderID == order.PurchaseOrderID)
                    .Select(x => x)
                    .ToList();
                //Remove them all and replace with the new list after. 
                foreach (var part in OldOrderParts)
                {
                    _eBike_2025Context.PurchaseOrderDetails.Remove(part);
                }
            }
            

            if (order == null)
            {
                order = new PurchaseOrder();

                //Sets PO number if PO is new.
                int maxID = _eBike_2025Context.PurchaseOrders.Where(x => x.VendorID == currentOrderView.VendorID).Any()
                            ? _eBike_2025Context.PurchaseOrders.Max(x => x.PurchaseOrderNumber) + 1 : 1;
                //Sets new PO num
                order.PurchaseOrderNumber = maxID;

            }

            order.OrderDate = currentOrderView.OrderDate;
            order.TaxAmount = currentOrderView.GST;
            order.SubTotal = currentOrderView.SubTotal;
            order.Closed = false;

            //TODO: Is the default null or do i need to add this to the view model?
            //order.Notes = orderView.Notes;
            order.EmployeeID = EmployeeID.ToString();
            order.VendorID = currentOrderView.VendorID;
            order.RemoveFromViewFlag = false;

            foreach (var DetailsViewItem in currentOrderView.PurchaseOrderDetails)
            {
                PurchaseOrderDetail orderDetail = _eBike_2025Context.PurchaseOrderDetails
                                .Where(x => x.PurchaseOrderDetailID == DetailsViewItem.PurchasOrderDetailID)
                                .FirstOrDefault();

                if (orderDetail == null)
                {
                    orderDetail = new PurchaseOrderDetail();
                }
                orderDetail.PartID = DetailsViewItem.PartID;
                orderDetail.Quantity = DetailsViewItem.Quantity;
                orderDetail.PurchasePrice = DetailsViewItem.PurchasePrice;
                orderDetail.VendorPartNumber = DetailsViewItem.VendorPartNumber;

                if (orderDetail.PurchaseOrderDetailID == 0)
                {
                    order.PurchaseOrderDetails.Add(orderDetail);
                }
                else
                {
                    _eBike_2025Context.PurchaseOrderDetails.Update(orderDetail);
                }

            }

            if (order.PurchaseOrderID == 0)
            {
                _eBike_2025Context.PurchaseOrders.Add(order);
            }
            else
            {
                _eBike_2025Context.PurchaseOrders.Update(order);
            }

            if (errorList.Count > 0)

            {
                // Clear changes to maintain data integrity.
                _eBike_2025Context.ChangeTracker.Clear();
                string errorMsg = "Unable to add or edit Invoice or Invoice Lines.";
                errorMsg += " Please check error message(s)";
                throw new AggregateException(errorMsg, errorList);
            }
            else
            {
                try
                {
                    _eBike_2025Context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception($"An error occurred while saving: {ex.Message}", ex);
                }
            }


            return currentOrderView = GetPurchaseOrder(order.PurchaseOrderID);

            #endregion
        }
        public void DeletePurchaseOrder(PurchaseOrderView order)
        {
            #region business rules
            //Find the order to be deleted.
            var exsistingOrder = _eBike_2025Context.PurchaseOrders
                .Where(x => x.PurchaseOrderID == order.PurchaseOrderID)
                .Include(x => x.PurchaseOrderDetails)
                .FirstOrDefault();
            //Cant Delete nothing
            if (exsistingOrder == null)
            {
                throw new Exception("Purchase Order not found.");
            }
            #endregion

            //Remove Chilren 
            if (exsistingOrder.PurchaseOrderDetails != null 
                && exsistingOrder.PurchaseOrderDetails.Any())
            {
                _eBike_2025Context.PurchaseOrderDetails
                    .RemoveRange(exsistingOrder.PurchaseOrderDetails);
            }
            //Removes Order
            _eBike_2025Context.PurchaseOrders.Remove(exsistingOrder);
            //Saves Changes
            _eBike_2025Context.SaveChanges(); // Save the delete
        }

    }//End Of PurchaseOrderService Class
}//End of NameSpace
