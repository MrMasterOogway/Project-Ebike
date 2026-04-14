using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasingSystem.ViewModels
{
    public class PartsView
    {
        public int PartsID { get; set; }
        public string Description { get; set; }
        public int QOH { get; set; } //On Hand
        public int ROL { get; set; } //ReOrder Level
        public int QOO { get; set; } //On Order set by QTO plus QOO
        public int QTO { get; set; } //To Order = ROL - QOO + QOH
        public decimal PurchasePrice { get; set; }
        public int VendorID { get; set; }
    }
}
