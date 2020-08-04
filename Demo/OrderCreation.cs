﻿using Pws.Clients.RestLibrary.Customers;
using Pws.Clients.RestLibrary.Service;
using PwsClientRestExample.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwsClientRestExample.Demo
{
	public static class OrderCreation
	{
		public static void Run(IPwsObjectWrapper<Customer_V1> customer)
		{
			Console.WriteLine("Demonstrate Order Creation.");

			// Create a new basket order with generated test customer reference
			var newOrder = CustomerOrder.Create(customer, "TEST" + DateTime.Now.ToString("yyyyMMddHHmmss"));

			//// We could change the reference if we wanted to
			// newOrder.PwsObject.OrderReference = "<unique reference>";
			// newOrder.Update();

			// Add regular door and retrieve its pricing
			var lineDoor = newOrder.AddLine("BLG716", 1);
			var linePrice = lineDoor.PwsObject.Price;
			var lineValue = lineDoor.PwsObject.LineTotal;

			// If we add an invalid code we should get an exception and the change will not be persisted.
			try
			{
				var invalidItem1 = newOrder.AddLine("BLG716AB", 1);
			}
			catch (RESTException re) { Console.WriteLine(re.Message); }
			catch (Exception e) { Console.WriteLine(e.Message); }

			// Add some door buffers
			var lineBuffers = newOrder.AddBespokeLine("3203", 96);

			// If we try to alter the order multiple so it is invalid we shoudl get an exception and the change will not be persisted.
			try
			{
				lineBuffers.PwsObject.OrderQuantity = 100;
				lineBuffers.Update();
			}
			catch (RESTException re) { Console.WriteLine(re.Message); }
			catch (Exception e) { Console.WriteLine(e.Message); }

			//// Add a painted door, which is a dynamic product.  This will post back a question with a set of options.  We need to post back
			// an answer, which may then return another question.  Once all questions are answered then the line will be finally created.
			// For brevity here we will select the very last option from the list of options for each question.
			var lineBespoke = newOrder.AddBespokeLine("BX716", 2);
			while (lineBespoke.PwsObject.NextBespokeOption != null)
			{
				lineBespoke = newOrder.AddBespokeLine(lineBespoke.PwsObject, lineBespoke.PwsObject.NextBespokeOption.Options.Last());
			}

			// Now take the regular door line we added before.  We would like to add some drilling to it.
			// This follows a similar process to the dynamic product above.

			// Firstly, we can if this line supports it: it will return null if it doesn't.
			// Again, for brevity, we will select the very last option from the list of options for each question.
			var lineToDrill = lineDoor.CastToBespokeLine();
			var drilling = lineToDrill.AddBespokeDrilling();
			if (drilling != null)
			{
				while (drilling.PwsObject.NextBespokeOption != null)
				{
					drilling = lineToDrill.AddBespokeDrilling(drilling.PwsObject, drilling.PwsObject.NextBespokeOption.Options.Last());
				}
			}

			// How do you cancel a line
			var lineToCancel = newOrder.AddBespokeLine("0038", 80);
			lineToCancel.Cancel();

			// What is the total value of our order.  We need to refresh first.
			newOrder.Refresh();
			var totalExcVat = new { Value = newOrder.PwsObject.OrderTotal.AmountExcludingVat, Currency = newOrder.PwsObject.OrderTotal.CurrencyCode };

			// Assign first address from the customer's standard address list.
			newOrder.AssignAddress(customer.Addresses().First().PwsObject);

			//// Alternatively we could assign a specific address to the order, although this may incur a charge
			// newOrder.AssignAddress("Somebody", "1 Streetname", "Town Name", "County Name", String.Empty, String.Empty, "DL1 4ZZ");

			// Retrieve a list of outstanding orders that are in the basket.
			var savedOrders = CustomerOrder.GetOutstanding(customer).Where(f => f.PwsObject.IsUpdateableByCustomer == true).ToArray();

			// Let us resume our order.
			var resumedOrder = savedOrders.Where(f => f.PwsObject.OrderReference == newOrder.PwsObject.OrderReference).FirstOrDefault();

			// Retrieve what delivery methods are available for this customer.
			var deliveryMethods = resumedOrder.DispatchMethods();

			// Set week commencing date
			resumedOrder.AssignDispatchMethod("WEEKBEGN", new DateTime(2020, 9, 21));

			// Release resumed order
			resumedOrder.Release();

			// Now retrieve the released order based on the unqiue PWS OrderId
			var checkOrder = CustomerOrder.Retrieve(customer, resumedOrder.PwsObject.OrderId);

			// Retrieve progress review of order.  This is split by fullfillment, each containing a list of tasks to complete.
			// Some of these tasks will include links to documents that can be downloaded.
			var progressOfOrder = checkOrder.Progress();

			Console.WriteLine("Completed.");
		}
	}
}
