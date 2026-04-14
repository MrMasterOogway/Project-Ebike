using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.ViewModels
{
    public class SaleView
    {
        public int SaleID { get; set; }
        public DateTime SaleDate { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string EmployeeID { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal SubTotal { get; set; }
        public int? CouponID { get; set; }
        public int? Discount { get; set; }
        public string PaymentType { get; set; }
        public bool RemoveFromViewFlag { get; set; }
        public List<SaleDetailView> SaleDetails { get; set; } = new();
        public List<SaleRefundView> SaleRefunds { get; set; } = new();
    }
}
