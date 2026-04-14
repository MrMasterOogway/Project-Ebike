using SalesReturnsSystem.DAL;
using SalesReturnsSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.BLL
{
    public class CategoryService
    {
        private readonly eBikeContext _ebikeContext;

        internal CategoryService(eBikeContext eBikeContext)
        {
            _ebikeContext = eBikeContext;
        }

        public List<CategoryView> GetCategories()
        {
            return _ebikeContext.Categories
                    .Where(x => !x.RemoveFromViewFlag)
                    .Select(x => new CategoryView
                    {
                        CategoryID = x.CategoryID,
                        Description = x.Description,
                        Parts = _ebikeContext.Parts
                            .Where(p => p.CategoryID == x.CategoryID)
                            .Select(p => new PartView
                            {
                                PartID = p.PartID,
                                Description = p.Description,
                                SellingPrice = p.SellingPrice,
                                QuantityOnHand = p.QuantityOnHand,
                                CategoryID = p.CategoryID,
                                RemoveFromViewFlag = p.RemoveFromViewFlag
                            })
                            .ToList(),
                        RemoveFromViewFlag = x.RemoveFromViewFlag
                    }).ToList();
        }
    }
}
