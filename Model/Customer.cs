using Pws.Clients.RestLibrary.Customers;
using Pws.Clients.RestLibrary.ECommerce.Users;
using Pws.Clients.RestLibrary.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwsClientRestExample
{
	public static class Customer
	{
		public static IPwsObjectWrapper<Customer_V1> GetPrimary(IPwsObjectWrapper<Token_V1> token)
		{
			return RESTHandler<IPwsObjectWrapper<Customer_V1>>.Invoke(() => token.Follow<Customer_V1>(f => f.PrimaryCustomer), "Customer");
		}

		public static IPwsObjectWrapper<Customer_V1> GetCustomerAccount(IPwsObjectWrapper<Token_V1> token, String accountCode)
		{
			return RESTHandler<IPwsObjectWrapper<Customer_V1>>.Invoke(() => token.FollowList<Customer_V1>(f => f.SecondaryCustomers).Where(f => f.PwsObject.AccountCode == accountCode).FirstOrDefault(), "Customer");
		}
	}

	public static class CustomerExtensions
	{
		public static IPwsObjectWrapper<Address_V1>[] Addresses(this IPwsObjectWrapper<Customer_V1> customer)
		{
			return RESTHandler<IPwsObjectWrapper<Address_V1>[]>.Invoke(() => customer.FollowList<Address_V1>(f => f.Addresses).ToArray(), "Addresses");
		}
	}
}
