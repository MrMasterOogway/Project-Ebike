using SalesReturnsSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.BLL
{
    public class SaleRefundDetailService
    {
        private readonly eBikeContext _ebikeContext;

        internal SaleRefundDetailService(eBikeContext eBikeContext)
        {
            _ebikeContext = eBikeContext;
        }
    }
}
