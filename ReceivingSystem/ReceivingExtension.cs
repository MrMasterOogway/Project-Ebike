using ReceivingSystem.BLL;
using ReceivingSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReceivingSystem.BLL;
using ReceivingSystem.DAL;

namespace ReceivingSystem
{
    public static class ReceivingExtension
    {
            //Creating an extension method for the web app to access purchasings private data
        public static void ReceivingBackendDependencies(this IServiceCollection services,
        Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<eBikeContext>(options);

            //Parts
            services.AddTransient<PartsService>((ServiceProvider) =>
            {
                //  Retrieve an instance of EBikeContext from the service provider.
                var context = ServiceProvider.GetService<eBikeContext>();

                // Create a new instance of CategoryService,
                //   passing the EBikeContext instance as a parameter.
                return new PartsService(context);
            });

            //Purchase Order
            services.AddTransient<PurchaseOrdersService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBikeContext>();

                return new PurchaseOrdersService(context);
            });

            //Purchase Order Details
            services.AddTransient<PurchaseOrderDetailsService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBikeContext>();

                return new PurchaseOrderDetailsService(context);
            });

            //Receive Order
            services.AddTransient<ReceiveOrdersService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBikeContext>();

                return new ReceiveOrdersService(context);
            });

            //Receive Order Details
            services.AddTransient<ReceiveOrderDetailsService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBikeContext>();

                return new ReceiveOrderDetailsService(context);
            });

            //Returned Order Details
            services.AddTransient<ReturnedOrderDetailsService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBikeContext>();

                return new ReturnedOrderDetailsService(context);
            });

            //Unordered Purchase Item Cart
            services.AddTransient<UnorderedPurchaseItemCartsService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBikeContext>();

                return new UnorderedPurchaseItemCartsService(context);
            });

            //Vendor
            services.AddTransient<VendorsService>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBikeContext>();

                return new VendorsService(context);
            });
        }  
    }
}
