using Pws.Clients.RestLibrary;
using Pws.Clients.RestLibrary.Customers;
using Pws.Clients.RestLibrary.Customers.Contacts;
using Pws.Clients.RestLibrary.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwsClientRestExample.Model
{
	public static class CustomerContact
	{
		public static IPwsObjectWrapper<Contact_V1> Create(IPwsObjectWrapper<Customer_V1> customer, String title, String firstname, String surname, String jobTitle, String emailAddress)
		{
			return RESTHandler<IPwsObjectWrapper<Contact_V1>>.Invoke(() => customer.Post(f => f.Contacts, new Contact_V1() { Title = title, FirstName = firstname, Surname = surname, JobTitle = jobTitle, Email = new Uri("mailto://" + emailAddress) }), "Create Customer Contact");
		}
	}
}
