using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReceivingSystem.DAL;
using ReceivingSystem.ViewModels;

namespace ReceivingSystem.BLL
{
    public class PurchaseOrdersService
    {
        private readonly eBikeContext _eBike_2025Context;
        internal PurchaseOrdersService(eBikeContext eBikeContext)
        {
            //  Initialize the _eBike_2025Context field with the provided EBikeContext instance.
            _eBike_2025Context = eBikeContext;
        }
        public List<PurchaseOrderView> GetOrders(int purchaseOrderNumber)
        {
            var query = _eBike_2025Context.PurchaseOrders
                .Where(po => !po.RemoveFromViewFlag && !po.Closed && po.OrderDate != null);

            if (purchaseOrderNumber > 0)
            {
                query = query.Where(po => po.PurchaseOrderNumber == purchaseOrderNumber);
            }

            return query
                .Select(po => new PurchaseOrderView
                {
                    PurchaseOrderID = po.PurchaseOrderID,
                    PurchaseOrderNumber = po.PurchaseOrderNumber,
                    OrderDate = po.OrderDate,
                    VendorName = po.Vendor.VendorName,
                    VendorPhone = po.Vendor.Phone,
                    RemoveFromViewFlag = po.RemoveFromViewFlag
                })
                .OrderBy(po => po.OrderDate)
                .ToList();
        }

        public PurchaseOrderView GetOrderDetails(int purchaseOrderNumber)
        {
            return _eBike_2025Context.PurchaseOrders
                .Where(po => po.PurchaseOrderNumber == purchaseOrderNumber)
                .Select(po => new PurchaseOrderView
                {
                    PurchaseOrderID = po.PurchaseOrderID,
                    PurchaseOrderNumber = po.PurchaseOrderNumber,
                    OrderDate = po.OrderDate,
                    VendorName = po.Vendor.VendorName,
                    VendorPhone = po.Vendor.Phone,
                    RemoveFromViewFlag = po.RemoveFromViewFlag,
                    PurchaseOrderDetails = po.PurchaseOrderDetails
                        .Where(pod => !pod.RemoveFromViewFlag)
                        .Select(pod => new PurchaseOrderDetailView
                        {
                            PurchaseOrderDetailID = pod.PurchaseOrderDetailID,
                            PartID = pod.PartID,
                            Description = pod.Part.Description,
                            Outstanding = pod.Quantity - pod.ReceiveOrderDetails.Sum(rod => rod.QuantityReceived),
                            QOO = pod.Part.QuantityOnOrder,
                            RemoveFromViewFlag = pod.RemoveFromViewFlag
                        })
                        .ToList()
                })
                .OrderBy(po => po.OrderDate)
                .FirstOrDefault();
        }
        public void ForceCloseOrder(int purchaseOrderId, string reason)
        {
            var po = _eBike_2025Context.PurchaseOrders.FirstOrDefault(p => p.PurchaseOrderID == purchaseOrderId);
            if (po == null)
                throw new Exception("Purchase order not found.");

            po.Closed = true;

            var details = _eBike_2025Context.PurchaseOrderDetails
                .Where(d => d.PurchaseOrderID == purchaseOrderId)
                .ToList();

            foreach (var detail in details)
            {
                var receivedList = _eBike_2025Context.ReceiveOrderDetails
                    .Where(r => r.PurchaseOrderDetailID == detail.PurchaseOrderDetailID)
                    .ToList();

                int totalReceived = 0;
                foreach (var r in receivedList)
                {
                    totalReceived += r.QuantityReceived;
                }

                int remaining = detail.Quantity - totalReceived;

                if (remaining > 0)
                {
                    var part = _eBike_2025Context.Parts.FirstOrDefault(p => p.PartID == detail.PartID);
                    if (part != null)
                    {
                        part.QuantityOnOrder -= remaining;
                        if (part.QuantityOnOrder < 0)
                            part.QuantityOnOrder = 0;
                    }
                }
            }
        }
    }
}
