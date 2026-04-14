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
    public class PartsService
    {
        private readonly EBikeContext _eBike_2025Context;
        internal PartsService(EBikeContext eBikeContext)
        {
            //  Initialize the _eBike_2025Context field with the provided EBikeContext instance.
            _eBike_2025Context = eBikeContext;
        }
        
        //Get parts list
        public List<PartsView> GetPartList(int? vendorID, List<int> existingPartIDs)
        {
            //returns a list of parts from the selected vendor.
            #region business rules

            if (vendorID <= 0 || vendorID == null)
            {
                throw new ArgumentNullException("Vendor needed to populate parts list");
            }

            #endregion

            #region Query

            return _eBike_2025Context.Parts
                .Where(x => !existingPartIDs.Contains(x.PartID) && 
                       x.VendorID == vendorID && !x.RemoveFromViewFlag)
                .Select(x => new PartsView
                {
                    PartsID = x.PartID,
                    Description = x.Description,
                    QOH = x.QuantityOnHand,
                    ROL = x.ReorderLevel,
                    QOO = x.QuantityOnOrder,
                    // Quantity to Order Table display QTO (if positive number) or 0 (if negative number)
                    // QTO = ROL - ( QOH + QOO )
                    // QTO > 0 ? QTO : 0 
                    QTO = (x.ReorderLevel - (x.QuantityOnHand+ x.QuantityOnOrder)) >0 
                    ? (x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder))
                    : 0,
                    PurchasePrice = x.PurchasePrice
                }).OrderByDescending(x => x.QTO).ToList();
                
            #endregion
        }
        public List<PartsView> GetOrderDetailsList(int? vendorId)
        {
            #region business rules

            if (vendorId <= 0 || vendorId == null)
            {
                throw new ArgumentNullException("Please select a vendor");
            }

          
            #endregion

            #region Query

            return _eBike_2025Context.Parts
                .Where(x => x.VendorID == vendorId
                        && !x.RemoveFromViewFlag
                        && (x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder)) > 0
                        )
                .Select(x => new PartsView
                {
                    PartsID = x.PartID,
                    Description = x.Description, 
                    QOH = x.QuantityOnHand,
                    ROL = x.ReorderLevel,
                    QOO = x.QuantityOnOrder,
                    // Quantity to Order Table display QTO (if positive number) or 0 (if negative number)
                    // QTO = ROL - ( QOH + QOO )
                    // QTO > 0 ? QTO : 0 
                    QTO = (x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder)) > 0
                    ? (x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder))
                    : 0,
                    PurchasePrice = x.PurchasePrice
                }).OrderByDescending(x => x.QTO).ToList();

            #endregion
        }

        public List<PartsView> GetPartListByPO(int? vendorID, int purchaseOrderId)
        {
            //returns a list of parts from the selected vendor.
            #region business rules

            if (vendorID <= 0 || vendorID == null)
            {
                throw new ArgumentNullException("Please select a vendor");
            }

            if (purchaseOrderId <= 0 || purchaseOrderId == null)
            {
                throw new ArgumentNullException("Please select a vendor");
            }

            #endregion

            #region Query

            return _eBike_2025Context.PurchaseOrderDetails
                .Where(x => x.PurchaseOrder.PurchaseOrderID == purchaseOrderId 
                        && x.PurchaseOrder.VendorID == vendorID 
                        && !x.RemoveFromViewFlag)
                .Select(x => new PartsView
                {
                    PartsID = x.PartID,
                    Description = x.Part.Description,
                    QOH = x.Part.QuantityOnHand,
                    ROL = x.Part.ReorderLevel,
                    QOO = x.Part.QuantityOnOrder,
                    // Quantity to Order Table display QTO (if positive number) or 0 (if negative number)
                    // QTO = ROL - ( QOH + QOO )
                    // QTO > 0 ? QTO : 0 
                    QTO = (x.Part.ReorderLevel - (x.Part.QuantityOnHand + x.Part.QuantityOnOrder)) > 0
                    ? (x.Part.ReorderLevel - (x.Part.QuantityOnHand + x.Part.QuantityOnOrder))
                    : 0,
                    PurchasePrice = x.PurchasePrice
                }).OrderByDescending(x => x.QTO).ToList();

            #endregion
        }
        //Gets a part
        public PartsView GetPart(int partID)
        {
          
            if (partID <= 0)
            {
                throw new ArgumentNullException("Please provide a part");
            }

            //selected part for the part list
            return _eBike_2025Context.Parts
                    .Where(x => (x.PartID == partID && !x.RemoveFromViewFlag))
                    .Select(x => new PartsView
                    {
                        PartsID = x.PartID,
                        Description = x.Description,
                        QOH = x.QuantityOnHand,
                        ROL = x.ReorderLevel,
                        QOO = x.QuantityOnOrder,
                        QTO = (x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder)) > 0
                        ? (x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder))
                        : 0,
                        PurchasePrice = x.PurchasePrice,
                        VendorID = x.VendorID
                    }).FirstOrDefault();
        }

        public List<PartsView> RemoveParts(int? vendorId, List<PartsView> Inventory, PurchaseOrderView currentPO)
        {
            foreach (PurchaseOrderDetailsView p in currentPO.PurchaseOrderDetails)
            {
                List<PartsView> parts = new();
                int partPOD = p.PartID;
                foreach (var part in Inventory)
                {
                    if (part.PartsID == partPOD)
                    {
                        parts.Add(part);
                    }
                }
            
                foreach (var part in parts)
                {
                    Inventory.Remove(part);
                }
            }    
            return Inventory;
        }

    }
}
