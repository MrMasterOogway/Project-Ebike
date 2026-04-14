using SalesReturnsSystem.DAL;
using SalesReturnsSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.BLL
{
    public class PartService
    {
        private readonly eBikeContext _ebikeContext;

        internal PartService(eBikeContext eBikeContext)
        {
            _ebikeContext = eBikeContext;
        }

        public List<PartView> GetParts(int categoryID)
        {
            if (categoryID == 0)
            {
                throw new ArgumentNullException("Category ID is required");
            }
            return _ebikeContext.Parts
                    .Where(x => x.CategoryID == categoryID && !x.RemoveFromViewFlag)
                    .Select(x => new PartView
                    {
                        PartID = x.PartID,
                        Description = x.Description,
                        SellingPrice = x.SellingPrice,
                        QuantityOnHand = x.QuantityOnHand,
                        CategoryID = x.CategoryID
                    }).ToList();
        }

        public PartView GetPart(int partID)
        {
            if (partID == 0)
            {
                throw new ArgumentNullException("Please provide a part");
            }

            return _ebikeContext.Parts
                    .Where(x => x.PartID == partID && !x.RemoveFromViewFlag)
                    .Select(x => new PartView
                    {
                        PartID = x.PartID,
                        Description = x.Description,
                        SellingPrice = x.SellingPrice,
                        QuantityOnHand = x.QuantityOnHand,
                        CategoryID = x.CategoryID,
                        RemoveFromViewFlag = x.RemoveFromViewFlag
                    }).FirstOrDefault();
        }
    }
}
