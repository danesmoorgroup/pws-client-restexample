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
	public static class ProductNavigation
	{
		public static void Run(IPwsObjectWrapper<Customer_V1> customer)
		{
			Console.WriteLine("Demonstrate Product Navigation.");

			// Products are filtered using Navigation.
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

			// Now lets make an enquiry against a Porter dynamic kit and retrieve a price
			var porterDynamic = kitchen.Navigate("Section", "Doors").Navigate("Range Family", "Porter");
			var firstPorterBespokeItem = porterDynamic.ProductsWithPrices().Where(f => f.PwsObject.IsBespoke).FirstOrDefault();
			if (firstPorterBespokeItem != null)
			{
				var porterBespokeItem = firstPorterBespokeItem.BespokeKit();
				while (porterBespokeItem.PwsObject.NextBespokeOption != null)
				{
					porterBespokeItem = firstPorterBespokeItem.BespokePricing(porterBespokeItem.PwsObject, porterBespokeItem.PwsObject.NextBespokeOption.Options.Last(), porterBespokeItem.PwsObject.NextBespokeOption.Selection?.MinQuantity);
				}

				var price = porterBespokeItem.PwsObject.Price;
			}

			// Alternatively you can retrieve a product
			var product3203 = Product.GetProductWithPrice(customer, "3203");
			var pricing = product3203.PwsObject.Price;
			var multiple = pricing.OrderMultiple;

			Console.WriteLine("Completed.");
		}
	}
}
