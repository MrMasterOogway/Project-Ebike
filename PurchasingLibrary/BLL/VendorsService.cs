using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PurchasingSystem.DAL;
using PurchasingSystem.Entities;
using PurchasingSystem.ViewModels;

namespace PurchasingSystem.BLL
{
    public class VendorsService
    {
        private readonly EBikeContext _eBike_2025Context;
        internal VendorsService(EBikeContext eBikeContext)
        {
            //  Initialize the _eBike_2025Context field with the provided EBikeContext instance.
            _eBike_2025Context = eBikeContext;
        }
        public List<VendorsView> GetVendorList()
        {
            #region business rules



            #endregion

            #region Query
            //Drop down menu list
            return _eBike_2025Context.Vendors
                .Select(x => new VendorsView
                {
                    VendorID = x.VendorID,
                    VendorName = x.VendorName,
                    Address = x.Address,
                    Phone = x.Phone
                })
                .ToList();
            #endregion
        }
    } 
}
