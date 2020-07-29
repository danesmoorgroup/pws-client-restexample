using Pws.Clients.RestLibrary.Customers;
using Pws.Clients.RestLibrary.Service;
using Pws.Clients.RestLibrary.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwsClientRestExample
{
    /*
	 * Copyright (C) PWS Distributors Ltd 2020
	 * All rights reserved.
	 * 
	 */

    class Program
    {
        static void Main(string[] args)
        {
            // Insert API Key and Private Key here.
            Application.SetKeys(new Guid("<insert API key>"), new Guid("<insert Private key>"), DeploymentType.Staging);

            // Insert UserId and Password here.
            Application.SetLogin("<insert UserId>", "<insert Password>");

            // Get primary customer for this login
            try
            {
                var customer = Customer.GetPrimary(Token.GetNewToken());

                // Alternatively we could select another customer by passing in the account code (if the user is permitted)
                // var customer = Customer.GetCustomerAccount(Token.GetNewToken(), "<accountCode>");

                // Demonstrate Product Navigation, pricing and categories
                DemonstrateProductNaviagtion(customer);

                // Demonstrate Order Retrieval and Creation
                DemonstrateOrderRetrievalAndCreation(customer);
            }
            catch (RESTException re) { Console.WriteLine(re.Message); }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }

        static void DemonstrateProductNaviagtion(IPwsObjectWrapper<Customer_V1> customer)
		{
            Console.WriteLine("Demonstrate Product Naviagtion.");

            // Producst are filtered using Navigation.
            var root = Product.GetChoices(customer);

            // We can retrieve a list of filter choices
            var availableFilters = root.PwsObject.NextChoices;

            // We can apply filters in any order, but here start with 'Kitchen' room.
            var kitchen = root.Navigate("Room", "Kitchen");

            // Can check what choices we've made so far
            var appliedFilters = kitchen.PwsObject.PreviousChoices;

            // Now let us filter a little further to the 'Broadoak Natural' Kitchen range
            var broadoak = kitchen.Navigate("Section", "Doors").Navigate("Range", "Broadoak Natural");

            // From Broadoak we can retrieve a list of all contained within that Navigation, containing specific prices for this customer.
            var products = broadoak.ProductsWithPrices();

            // Once a few filters are applied, there is a roll-up of navigations into useful product sets
            var rollup = broadoak.RollupWithProductsAndPrices();
			
            // Now that we have a list of products let us fetch some information about the first product in the list.
            // Staring with a list of individual details (i.e. dimensions, colour, finish, style, etc.)
            var categories = products.First().Categories();

            // How about some pricing and availability details
            var product = products.First().PwsObject.CustomerProduct_V1;
            var stock = product.Price.AvailableQuantity;
            var available = product.Price.AvailableDate;
            var gross = product.Price.GrossPrice;
            var discount = product.Price.DiscountPercentage;
            var net = product.Price.NetPrice;

            Console.WriteLine("Completed.");
        }

        static void DemonstrateOrderRetrievalAndCreation(IPwsObjectWrapper<Customer_V1> customer)
		{
            Console.WriteLine("Demonstrate Order Retrieval and Creation.");

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
            var deliveryMethods = resumedOrder.DeliveryMethods();

            // Set week commencing date
            resumedOrder.AssignDeliveryMethod("WEEKBEGN", new DateTime(2020, 9, 21));

            // Release resumed order
            resumedOrder.Release();

            // Now retrieve the released order based on the unqiue PWS OrderId
            var checkOrder = CustomerOrder.Retrieve(customer, resumedOrder.PwsObject.OrderId);

            Console.WriteLine("Completed.");
        }
    }
}
