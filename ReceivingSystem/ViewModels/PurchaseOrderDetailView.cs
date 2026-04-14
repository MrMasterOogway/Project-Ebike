using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingSystem.ViewModels
{
    public class PurchaseOrderDetailView
    {
        public int PurchaseOrderDetailID { get; set; }
        public int PartID { get; set; }
        public string Description { get; set; }
        public int Outstanding { get; set; }
        public int ReceiveQuantity { get; set; }
        public int ReturnQuantity { get; set; }
        public string Reason { get; set; }
        public int QOO { get; set; }
        public bool? RemoveFromViewFlag { get; set; }
    }
}
