using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.ViewModels
{
    public class SaleRefundView
    {
        public int SaleRefundID { get; set; }
        public DateTime SaleRefundDate { get; set; }
        public int SaleID { get; set; }
        public string EmployeeID { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal SubTotal { get; set; }
        public bool RemoveFromViewFlag { get; set; }
        public List<SaleRefundDetailView> SaleRefundDetails { get; set; } = new List<SaleRefundDetailView>();
    }
}
