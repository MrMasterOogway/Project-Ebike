using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingSystem.ViewModels
{
    public class PurchaseOrderView
    {
        public int PurchaseOrderID { get; set; }
        public int PurchaseOrderNumber { get; set; }
        public DateTime? OrderDate { get; set; }
        public string VendorName { get; set; }
        public string VendorPhone { get; set; }
        public bool? RemoveFromViewFlag { get; set; }
        public List<PurchaseOrderDetailView> PurchaseOrderDetails { get; set; } = new();
    }
}
