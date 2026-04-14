using SalesReturnsSystem.DAL;
using SalesReturnsSystem.Entities;
using SalesReturnsSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.BLL
{
    public class CouponService
    {
        private readonly eBikeContext _ebikeContext;

        internal CouponService(eBikeContext eBikeContext)
        {
            _ebikeContext = eBikeContext;
        }

        public CouponView GetCouponByID(string couponIDValue)
        {
            if (string.IsNullOrWhiteSpace(couponIDValue))
            {
                throw new ArgumentNullException("Please provide a coupon");
            }

            return _ebikeContext.Coupons
                    .Where(x => x.CouponIDValue == couponIDValue && DateTime.Now > x.StartDate && DateTime.Now < x.EndDate && x.SalesOrService == 1 && !x.RemoveFromViewFlag)
                    .Select(x => new CouponView
                    {
                        CouponID = x.CouponID,
                        CouponIDValue = x.CouponIDValue,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        CouponDiscount = x.CouponDiscount,
                        SalesOrService = x.SalesOrService,
                        RemoveFromViewFlag = x.RemoveFromViewFlag
                    }).FirstOrDefault();
        }
    }
}
