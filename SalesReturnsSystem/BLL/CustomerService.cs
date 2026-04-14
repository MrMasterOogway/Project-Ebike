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
    public class CustomerService
    {
        private readonly eBikeContext _ebikeContext;

        internal CustomerService(eBikeContext eBikeContext)
        {
            _ebikeContext = eBikeContext;
        }

        public List<CustomerSearchView> GetCustomers(string phone)
        {
            //if (string.IsNullOrWhiteSpace(phone))
            //{
            //    throw new ArgumentNullException("Please provide a phone number");
            //}

            //if (string.IsNullOrWhiteSpace(phone))
            //{
            //    phone = Guid.NewGuid().ToString();
            //}

            return _ebikeContext.Customers
                    .Where(x => x.ContactPhone.Contains(phone) && !x.RemoveFromViewFlag)
                    .Select(x => new CustomerSearchView
                    {
                        CustomerID = x.CustomerID,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        Address = x.Address,
                        City = x.City,
                        Province = x.Province,
                        PostalCode = x.PostalCode,
                        ContactPhone = x.ContactPhone,
                        RemoveFromViewFlag = x.RemoveFromViewFlag
                    })
                    .ToList();
        }

        //public CustomerEditView GetCustomer(int customerID)
        //{
        //    if (customerID == 0)
        //    {
        //        throw new ArgumentNullException("Please provide a customer");
        //    }

        //    return _ebikeContext.Customers
        //            .Where(x => x.CustomerID == customerID && !x.RemoveFromViewFlag)
        //            .Select(x => new CustomerEditView
        //            {
        //                CustomerID = x.CustomerID,
        //                FirstName = x.FirstName,
        //                LastName = x.LastName,
        //                Address = x.Address,
        //                City = x.City,
        //                Province = x.Province,
        //                PostalCode = x.PostalCode,
        //                ContactPhone = x.ContactPhone,
        //                EmailAddress = x.EmailAddress,
        //                RemoveFromViewFlag = x.RemoveFromViewFlag
        //            }).FirstOrDefault();
        //}
    }
}
