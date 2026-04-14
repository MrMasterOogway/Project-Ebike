using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SalesReturnsSystem.BLL;
using SalesReturnsSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem
{
    public static class SalesReturnsExtension
    {
        public static void SalesReturnBackendDependencies(this IServiceCollection services,
        Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<eBikeContext>(options);

            // Category
            services.AddTransient<CategoryService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBikeContext>();

                return new CategoryService(context);
            });

            // Coupon
            services.AddTransient<CouponService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBikeContext>();

                return new CouponService(context);
            });

            // Customer
            services.AddTransient<CustomerService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBikeContext>();

                return new CustomerService(context);
            });

            // Part
            services.AddTransient<PartService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBikeContext>();

                return new PartService(context);
            });

            // Sale
            services.AddTransient<SaleService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBikeContext>();

                return new SaleService(context);
            });

            // Sale Detail
            services.AddTransient<SaleDetailService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBikeContext>();

                return new SaleDetailService(context);
            });

            // Sale Refund
            services.AddTransient<SaleRefundService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBikeContext>();

                return new SaleRefundService(context);
            });

            // Sale Refund Detail
            services.AddTransient<SaleRefundDetailService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBikeContext>();

                return new SaleRefundDetailService(context);
            });
        }
    }
}
