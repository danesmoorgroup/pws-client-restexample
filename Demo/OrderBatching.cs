using Pws.Clients.RestLibrary.Customers;
using Pws.Clients.RestLibrary.Service;
using PwsClientRestExample.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwsClientRestExample.Demo
{
	public static class OrderBatching
	{
		public static void Run(IPwsObjectWrapper<Customer_V1> customer)
		{
			Console.WriteLine("Demonstrate Order Batching.");

			// Create basics of a master order
			var newOrderMaster = CustomerOrder.Create(customer, "TESTA" + DateTime.Now.ToString("yyyyMMddHHmmss"));
			newOrderMaster.AddLine("3203", 96);

			// Create basics of a related order
			var newOrderToConsolidate = CustomerOrder.Create(customer, "TESTB" + DateTime.Now.ToString("yyyyMMddHHmmss"));
			newOrderToConsolidate.AddLine("3203", 96);

			// Create new batch by adding second order to first order
			var newBatch = newOrderMaster.Batch().AddToBatch(newOrderToConsolidate.PwsObject.OrderId);

			// Retrieve all outstanding jobs (aka master orders)
			var batchedJobs = CustomerOrder.GetOutstandingJobs(customer);

			// Now try removing the second order from batch, which should close it
			var UpdatedBatch = newOrderMaster.Batch().RemoveFromBatch(newOrderToConsolidate.PwsObject.OrderId);

			// Retrieve all outstanding jobs (aka master orders)
			var batchedJobsAfterRemoval = CustomerOrder.GetOutstandingJobs(customer);

			Console.WriteLine("Completed.");
		}
	}
}
