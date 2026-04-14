using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PurchasingSystem.DAL;
using PurchasingSystem.Entities;
using PurchasingSystem.ViewModels;


namespace PurchasingSystem.BLL
{

    public class PurchaseOrderDetailsService
    {
        private readonly EBikeContext _eBike_2025Context;
        internal PurchaseOrderDetailsService(EBikeContext eBikeContext)
        {
            //  Initialize the _eBike_2025Context field with the provided EBikeContext instance.
            _eBike_2025Context = eBikeContext;
        }



        public List<PurchaseOrderDetailsView> GetExistingOrderDetailsList(int vendorID, int purchaseOrderId)
        {
           
            return _eBike_2025Context.PurchaseOrderDetails
                .Where(p => p.PurchaseOrder.VendorID == vendorID && p.PurchaseOrderID == purchaseOrderId)
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
                            QTO = (pa.ReorderLevel - (pa.QuantityOnHand + pa.QuantityOnOrder)) > 0
                ? (pa.ReorderLevel - (pa.QuantityOnHand + pa.QuantityOnOrder))
                : 0,
                            PurchasePrice = pa.PurchasePrice,
                            VendorID = pa.VendorID
                        }).FirstOrDefault()
                }).ToList();
        }
    }
}
