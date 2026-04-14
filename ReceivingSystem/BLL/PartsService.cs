using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReceivingSystem.DAL;

namespace ReceivingSystem.BLL
{
    public class PartsService
    {
        private readonly eBikeContext _eBike_2025Context;
        internal PartsService(eBikeContext eBikeContext)
        {
            //  Initialize the _eBike_2025Context field with the provided EBikeContext instance.
            _eBike_2025Context = eBikeContext;
        }
    }
}
