using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.ViewModels
{
    public class PartView
    {
        public int PartID { get; set; }
        public string Description { get; set; }
        public decimal SellingPrice { get; set; }
        public int QuantityOnHand { get; set; }
        public int CategoryID { get; set; }
        public bool RemoveFromViewFlag { get; set; }
    }
}
