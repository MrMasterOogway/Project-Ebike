using SalesReturnsSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.BLL
{
    public class SaleRefundService
    {
        private readonly eBikeContext _ebikeContext;

        internal SaleRefundService(eBikeContext eBikeContext)
        {
            _ebikeContext = eBikeContext;
        }
    }
}
