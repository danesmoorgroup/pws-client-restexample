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
	public static class OrderRetrievalAndProgression
	{
		public static void Run(IPwsObjectWrapper<Customer_V1> customer)
		{
			Console.WriteLine("Demonstrate Order Retrieval/ Progress.");

			// Retrieve a list of outstanding orders.
			var orders = CustomerOrder.GetOutstanding(customer).Where(f => f.PwsObject.IsUpdateableByCustomer == false).ToArray();

			// Navigate through each order
			foreach (var order in orders)
			{
				// Get order identity and placement date
				var orderId = order.PwsObject.OrderId;
				var orderDate = order.PwsObject.OrderDate;
				var reference = order.PwsObject.OrderReference;

				// Get order and payment status
				var status = order.PwsObject.Status;
				var payment = order.PwsObject.IsPaymentRequired;

				// Get delivery details
				var deliveryAddress = order.Address();
				var deliveryMethod = order.DispatchMethod();
				var expectedDate = deliveryMethod.PwsObject.ExpectedDate;

				// Get order totals
				var goodsTotal = order.PwsObject.GoodsTotal;
				var discountTotal = order.PwsObject.DiscountTotal;
				var carriageTotal = order.PwsObject.CarriageTotal;
				var orderTotal = order.PwsObject.OrderTotal;
				
				// Orders are broken up into fulfillments, which are then handled by separate
				// parts of the business.  Sometimes a separate fulfillment will be raised to
				// fulfill a back order, or if more is added to an order after it is released.
				foreach(var fullfilment in order.Progress().PwsObject.Fulfilments)
				{
					// Each fulfillment is represented here by a set of events (also made up of tasks).
					foreach(var task in fullfilment.Events)
					{
						var dated = task.DateTime;
						var description = task.Description;
						var isComplete = task.IsCompleted;

						// If there a link to a binary file then download it
						if (task.Link?.MediaType.ToLower() == "application/pdf")
						{
							var title = task.Link.Title;
							var file = Model.Token.GetNewToken().BinaryFile(task.Link);
							System.Diagnostics.Process.Start(file.FullName);
						}
					}
				}
			}

			Console.WriteLine("Completed.");
		}
	}
}
