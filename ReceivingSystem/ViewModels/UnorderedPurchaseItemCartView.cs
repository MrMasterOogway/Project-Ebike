using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingSystem.ViewModels
{
    public class UnorderedPurchaseItemCartView
    {
        public int ItemCartID { get; set; }
        public string Description { get; set; }
        public string VendorPartNumber { get; set; }
        public int Quantity { get; set; }
    }
}
