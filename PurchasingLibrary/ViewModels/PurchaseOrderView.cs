using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasingSystem.ViewModels
{
    public class PurchaseOrderView
    {
        public int PurchaseOrderID { get; set; }
        public DateTime? OrderDate { get; set; }
        //cost for order 
        public decimal SubTotal { get; set; }
        public decimal GST { get; set; }
        public decimal Total { get; set; }

        //vendor info for display on page
        public int VendorID { get; set; }
        public string VendorName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public int PONumber { get; set; } //not in vendor view

        //parts in order
        public List<PurchaseOrderDetailsView> PurchaseOrderDetails { get; set; } = new();
    }
}
