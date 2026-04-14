using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.ViewModels
{
    public class SaleRefundDetailView
    {
        public int SaleRefundDetailID { get; set; }
        public int SaleRefundID { get; set; }
        public int PartID { get; set; }
        public string PartDescription { get; set; }
        public int Quantity { get; set; }
        public decimal SellingPrice { get; set; }
        public string Reason { get; set; }
        public bool RemoveFromViewFlag { get; set; }
    }
}
