using SalesReturnsSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.BLL
{
    public class SaleDetailService
    {
        private readonly eBikeContext _ebikeContext;

        internal SaleDetailService(eBikeContext eBikeContext)
        {
            _ebikeContext = eBikeContext;
        }
    }
}
