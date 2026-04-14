using PurchasingSystem.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasingSystem.ViewModels
{
    public class VendorsView
    {
        public int VendorID { get; set; }
        public string VendorName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string ProvinceID { get; set; }
        public string PostalCode { get; set; }
        public List<PartsView> Parts { get; set; } = new();
        public List<PurchaseOrderView> PurchaseOrders { get; set; } = new();
    }
}
