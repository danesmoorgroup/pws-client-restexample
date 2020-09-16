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

			// Who is the current buyer (should be whoever is linked to the login)
			var buyer = newOrder.BuyerContact();

			// Assign first buyer from the customer's contacts' list.
			newOrder.AssignBuyerContact(customer.Contacts().First().PwsObject);

			//// Alternatively we could assign a specific contact name to the order, and let the order system resolve this
			//newOrder.AssignBuyerContact("Douglas", "Adams");

			//// Alternatively we could create a new contact object and assign it to the order
			//try
			//{
			//	newOrder.AssignBuyerContact(CustomerContact.Create(customer, "Mr", "Douglas", "Adams", "Writer", "douglas.adams@pws.co.uk").PwsObject);
			//}
			//catch (RESTException re) { Console.WriteLine(re.Message); }
			//catch (Exception e) { Console.WriteLine(e.Message); }

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

			// Now let us take a different approach to adding dynamic products, where we've preempted the questions and their response.

			// Starting with regular paint-to-order
			var lineBespokeWithOptions1 = newOrder.AddBespokeLine("BX714", new CustomerBespokeLineOption().AddOption("CLR", "DOVE/GREY"), 2);
			// Then middle of the road Alpina Made-to-Measure
			var lineBespokeWithOptions2 = newOrder.AddBespokeLine("LNOBESPOKE", new CustomerBespokeLineOption().AddOption("", "BDRLDSLL").AddOption("SIZE", "Bespoke height", 900).AddOption("SIZE", "Bespoke width", 500), 1);
			// Ending with the complicated Stanhope Made-to-Measure
			var lineBespokeWithOptions3 = newOrder.AddBespokeLine("STABESPOKE",
				new CustomerBespokeLineOption()
					.AddOption("", "BDRDOOR")
					.AddOption("SIZE", "Door height", 600)
					.AddOption("SIZE", "Door width", 500)
					.AddOption("RNGE", "WHITFIELD")
					.AddOption("FNSH", "GLOSS")
					.AddOption("CLR", "CREAM")
					.AddOption("DRIL", "DRILL/LEFT")
				, 1
			);

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

			// Now let's see if we can change this to a week commencing.
			var weekMethod = deliveryMethods.Where(f => f.PwsObject.Description.Contains("week commencing")).FirstOrDefault();
			if (weekMethod != null)
			{
				// Find earliest available week, commencing on Monday at 10am, where there are now blackouts.
				var selectedDate = DateTime.Today.AddHours(10);
				while (selectedDate.DayOfWeek != DayOfWeek.Monday) selectedDate = selectedDate.AddDays(-1);
				do { selectedDate = selectedDate.AddDays(7); } while (weekMethod.Blackouts(selectedDate, selectedDate).Any());
				
				// Set week commencing date
				resumedOrder.AssignDispatchMethod(weekMethod.PwsObject.Type, selectedDate.Date);
			}

			// Now let's see if we can change this to a collect
			var collectMethod = deliveryMethods.Where(f => f.PwsObject.Description.StartsWith("Collect")).FirstOrDefault();
			if (collectMethod != null)
			{
				// Now lets take the first suggestion, which could be an exising booking or next available slot.
				var suggestions = collectMethod.Suggestions(DateTime.Today, DateTime.Today.AddMonths(2));
				if (suggestions.Any())
					resumedOrder.AssignDispatchMethod(collectMethod.PwsObject.Type, suggestions.First().PwsObject.Date);
			}

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
