using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PurchasingSystem.BLL;
using PurchasingSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasingSystem
{
    public static class PurchasingExtension
    {
        //Creating an extension method for the web app to access purchasings private data
        public static void PurchasingBackendDependencies(this IServiceCollection services,
            Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<EBikeContext>(options);

            //Category
            services.AddTransient<CategoryService>((ServiceProvider) =>
            {
                //  Retrieve an instance of EBikeContext from the service provider.
                var context = ServiceProvider.GetService<EBikeContext>();

                // Create a new instance of CategoryService,
                //   passing the EBikeContext instance as a parameter.
                return new CategoryService(context);
            });

            //Parts
            services.AddTransient<PartsService>((ServiceProvider) =>
            {              
                var context = ServiceProvider.GetService<EBikeContext>();
               
                return new PartsService(context);
            });

            //Purchase OrderDetails
            services.AddTransient<PurchaseOrderDetailsService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<EBikeContext>();

                return new PurchaseOrderDetailsService(context);
            });

            //Purshase Order
            services.AddTransient<PurchaseOrdersService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<EBikeContext>();

                return new PurchaseOrdersService(context);
            });

            //Vendor Service
            services.AddTransient<VendorsService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<EBikeContext>();

                return new VendorsService(context);
            });
        }
    }
}
