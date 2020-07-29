using Pws.Clients.RestLibrary;
using Pws.Clients.RestLibrary.Customers;
using Pws.Clients.RestLibrary.Customers.Orders;
using Pws.Clients.RestLibrary.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwsClientRestExample
{
	public static class CustomerOrder
	{
		public static IPwsObjectWrapper<Order_V1>[] GetOutstanding(IPwsObjectWrapper<Customer_V1> customer)
		{
			return RESTHandler<IPwsObjectWrapper<Order_V1>[]>.Invoke(() => customer.FollowList<Order_V1>(f => f.OutstandingOrders).ToArray(), "Outstanding Orders");
		}

		public static IPwsObjectWrapper<Order_V1> Retrieve(IPwsObjectWrapper<Customer_V1> customer, String orderId)
		{
			return RESTHandler<IPwsObjectWrapper<Order_V1>>.Invoke(() => customer.FollowList<Order_V1>(f => f.AllOrders, "filter=OrderId Eq '" + orderId + "'").FirstOrDefault(), "Retrieve Order");
		}

		public static IPwsObjectWrapper<Order_V1> Create(IPwsObjectWrapper<Customer_V1> customer, String uniqueReference)
		{
			return RESTHandler<IPwsObjectWrapper<Order_V1>>.Invoke(() => customer.Post(f => f.OutstandingOrders, new Order_V1() { OrderReference = uniqueReference }), "Create Order");
		}
	}

	public static class CustomerOrderExtensions
	{
		public static IPwsObjectWrapper<Order_V1> Refresh(this IPwsObjectWrapper<Order_V1> order)
		{
			return RESTHandler<IPwsObjectWrapper<Order_V1>>.Invoke(() => order.Refresh(), "Refresh Order");
		}

		public static void Cancel(this IPwsObjectWrapper<Order_V1> order)
		{
			RESTHandler<object>.Invoke(() => { order.DeleteSelf(); return null; }, "Cancel Order");
		}

		public static IPwsObjectWrapper<Order_V1> Update(this IPwsObjectWrapper<Order_V1> order)
		{
			return RESTHandler<IPwsObjectWrapper<Order_V1>>.Invoke(() => order.PutSelf(), "Update Order");
		}

		#region Standard Line

		public static IPwsObjectWrapper<OrderLine_V1> AddLine(this IPwsObjectWrapper<Order_V1> order, String productId, decimal quantity)
		{
			return RESTHandler<IPwsObjectWrapper<OrderLine_V1>>.Invoke(() => order.Post(f => f.Lines, new OrderLine_V1() { ProductId = productId, OrderQuantity = quantity }), "Add Order Line");
		}

		public static void Cancel(this IPwsObjectWrapper<OrderLine_V1> line)
		{
			RESTHandler<object>.Invoke(() => { line.DeleteSelf(); return null; }, "Cancel Order Line");
		}

		public static IPwsObjectWrapper<OrderLine_V1> Update(this IPwsObjectWrapper<OrderLine_V1> line)
		{
			return RESTHandler<IPwsObjectWrapper<OrderLine_V1>>.Invoke(() => line.PutSelf(), "Update Order Line");
		}

		#endregion

		#region Complex Line

		public static IPwsObjectWrapper<OrderLine_V2> CastToBespokeLine(this IPwsObjectWrapper<OrderLine_V1> line)
		{
			return RESTHandler<IPwsObjectWrapper<OrderLine_V2>>.Invoke(() => line.Follow<OrderLine_V2>(f => f.FindSelfLink()), "Cast Bespoke Order Line");
		}

		public static IPwsObjectWrapper<OrderLine_V2> AddBespokeLine(this IPwsObjectWrapper<Order_V1> order, String productId, decimal quantity)
		{
			return RESTHandler<IPwsObjectWrapper<OrderLine_V2>>.Invoke(() => order.Post(f => f.Lines, new OrderLine_V2() { ProductId = productId, OrderQuantity = quantity }), "Add Bespoke Order Line");
		}

		public static IPwsObjectWrapper<OrderLine_V2> AddBespokeLine(this IPwsObjectWrapper<Order_V1> order, OrderLine_V2 line, BespokeKit_V1.BespokeOption.BespokeSelection selection)
		{
			if (line.NextBespokeOption == null)
			{
				throw new Exception("All bespoke options assigned.");
			}

			line.NextBespokeOption.Selection = selection;
			return RESTHandler<IPwsObjectWrapper<OrderLine_V2>>.Invoke(() => order.Post(f => f.Lines, line), "Add Bespoke Order Line");
		}

		private static Link_V1 GetDrillingKit(IPwsObjectWrapper<OrderLine_V2> line)
		{
			return line.PwsObject.BespokeKits.Where(fl => fl.Title.ToLower().Contains("drilling")).FirstOrDefault();
		}

		public static IPwsObjectWrapper<BespokeKit_V1> AddBespokeDrilling(this IPwsObjectWrapper<OrderLine_V2> line)
		{
			if (GetDrillingKit(line) == null)
				return null;

			return line.Post(f => GetDrillingKit(line), new BespokeKit_V1());
		}

		public static IPwsObjectWrapper<BespokeKit_V1> AddBespokeDrilling(this IPwsObjectWrapper<OrderLine_V2> line, BespokeKit_V1 bespoke, BespokeKit_V1.BespokeOption.BespokeSelection selection)
		{
			bespoke.NextBespokeOption.Selection = selection;
			return RESTHandler<IPwsObjectWrapper<BespokeKit_V1>>.Invoke(() => line.Post(f => GetDrillingKit(line), bespoke), "Add Bespoke Drilling to Order Line");
		}

		public static void Cancel(this IPwsObjectWrapper<OrderLine_V2> line)
		{
			RESTHandler<object>.Invoke(() => { line.DeleteSelf(); return null; }, "Cancel Bespoke Order Line");
		}

		public static IPwsObjectWrapper<OrderLine_V2> Update(this IPwsObjectWrapper<OrderLine_V2> line)
		{
			return RESTHandler<IPwsObjectWrapper<OrderLine_V2>>.Invoke(() => line.PutSelf(), "Update Bespoke Order Line");
		}

		#endregion

		#region Address

		public static IPwsObjectWrapper<Address_V1> AssignAddress(this IPwsObjectWrapper<Order_V1> order, Address_V1 address)
		{
			return RESTHandler<IPwsObjectWrapper<Address_V1>>.Invoke(() => order.Put(f => f.DeliveryAddress, address), "Assign Address to Order");
		}

		public static IPwsObjectWrapper<Address_V1> AssignAddress(this IPwsObjectWrapper<Order_V1> order, String name, String line1, String line2, String line3, String line4, String line5, String postcode)
		{
			return RESTHandler<IPwsObjectWrapper<Address_V1>>.Invoke(() => order.Put(f => f.DeliveryAddress,
				new Address_V1() { AccountCode = String.Empty, AddressNumber = String.Empty, Name = name, Line1 = line1, Line2 = line2, Line3 = line3, Line4 = line4, Postcode = postcode }
			), "Assign Manual Address to Order");
		}

		#endregion

		#region Dispatch Method

		public static IPwsObjectWrapper<DispatchMethod_V1>[] DeliveryMethods(this IPwsObjectWrapper<Order_V1> order)
		{
			return order.FollowList<DispatchMethod_V1>(f => f.AvailableDispatchMethods).ToArray();
		}

		public static IPwsObjectWrapper<DispatchMethod_V1> AssignDeliveryMethod(this IPwsObjectWrapper<Order_V1> order, String type, DateTime? required)
		{
			var selectedDispatchMethod = order.DeliveryMethods().Where(f => f.PwsObject.Type == type).FirstOrDefault()?.PwsObject;
			if (selectedDispatchMethod == null)
			{
				throw new Exception("No matching dispatch method available.");
			}
			if (selectedDispatchMethod.IsDateRequired && required == null)
			{
				throw new Exception("Selected dispatch method requires date.");
			}

			if (required != null)
			{
				selectedDispatchMethod.SelectedDate = required;
			}

			return RESTHandler<IPwsObjectWrapper<DispatchMethod_V1>>.Invoke(() => order.Put<DispatchMethod_V1>(f => f.SelectedDispatchMethod, selectedDispatchMethod), "Assign Dispatch Method to Order");
		}

		#endregion

		#region Release

		public static IPwsObjectWrapper<Order_V1> Release(this IPwsObjectWrapper<Order_V1> order)
		{
			var releaseMethods = order.FollowList<Release_V1>(f => f.AvailableReleaseMethods);
			var chosenReleaseMethod = releaseMethods.Where(f => f.PwsObject.IsExternalPaymentMethod == false).First();
			RESTHandler<IPwsObjectWrapper<Release_V1>>.Invoke(() => order.Post(f => f.ReleaseMethod, chosenReleaseMethod.PwsObject), "Release Order");
			return RESTHandler<IPwsObjectWrapper<Order_V1>>.Invoke(() => order.Refresh(), "Release Order Refresh");
		}

		#endregion
	}
}
