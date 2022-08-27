using Pws.Clients.RestLibrary;
using Pws.Clients.RestLibrary.Customers;
using Pws.Clients.RestLibrary.Customers.Orders;
using Pws.Clients.RestLibrary.Customers.Products;
using Pws.Clients.RestLibrary.Products;
using Pws.Clients.RestLibrary.Products.Navigation;
using Pws.Clients.RestLibrary.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwsClientRestExample.Model
{
	public static class Product
	{
		public static IPwsObjectWrapper<NavigationChoice_V1> GetChoices(IPwsObjectWrapper<Customer_V1> customer)
		{
			return RESTHandler<IPwsObjectWrapper<NavigationChoice_V1>>.Invoke(() => customer.Follow<NavigationChoice_V1>(f => f.ProductNavigation), "Product Navigation"
			);
		}

		public static IPwsObjectWrapper<ProductInformation_V1> GetProductInfo(IPwsObjectWrapper<Customer_V1> customer, String productCode)
		{
			return RESTHandler<IPwsObjectWrapper<ProductInformation_V1>>.Invoke(
				() => GetChoices(customer).FollowList<ProductInformation_V1>(f => f.ProductList, "filter=ProductId Eq '" + productCode + "'").FirstOrDefault(),
				"ProductInfo"
			);
		}

		public static IPwsObjectWrapper<Product_V1> GetProductWithPrice(IPwsObjectWrapper<Customer_V1> customer, String productCode)
		{
			return RESTHandler<IPwsObjectWrapper<Product_V1>>.Invoke(() => customer.FollowList<Product_V1>(f => f.Products, "filter=ProductId Eq '" + productCode + "'").FirstOrDefault(), "Product");
		}
	}

	public static class ProductExtensions
	{
		public static IPwsObjectWrapper<NavigationChoice_V1> Navigate(this IPwsObjectWrapper<NavigationChoice_V1> choice, String categoryKey, String categoryValue)
		{
			return RESTHandler<IPwsObjectWrapper<NavigationChoice_V1>>.Invoke(
				() => choice.Follow<NavigationChoice_V1>(f => f.NextChoices.Where(fl => fl.ClassificationName == categoryKey).First().Categories.Where(fl => fl.CategoryName == categoryValue).First().NavigationChoice),
				"Product Navigation"
			);
		}

		#region Navigation Rollup

		public static IPwsObjectWrapper<NavigationChoice_ProductInformation_V1V1>[] Rollup(this IPwsObjectWrapper<NavigationChoice_V1> choice)
		{
			if (choice.PwsObject.NavigationList == null)
			{
				throw new Exception("Product Navigation Rollup not available.");
			}

			return RESTHandler<IPwsObjectWrapper<NavigationChoice_ProductInformation_V1V1>[]>.Invoke(
				() => choice.FollowList<NavigationChoice_ProductInformation_V1V1>(f => f.NavigationList).ToArray(),
				"Product Navigation Rollup"
			);
		}

		public static IPwsObjectWrapper<NavigationChoice_ProductInformation_CustomerProduct_V1V1V1>[] RollupWithProductsAndPrices(this IPwsObjectWrapper<NavigationChoice_V1> choice)
		{
			if (choice.PwsObject.NavigationList == null)
			{
				throw new Exception("Product Navigation Rollup not available.");
			}

			return RESTHandler<IPwsObjectWrapper<NavigationChoice_ProductInformation_CustomerProduct_V1V1V1>[]>.Invoke(
				() => choice.FollowList<NavigationChoice_ProductInformation_CustomerProduct_V1V1V1>(f => f.NavigationList).ToArray(),
				"Product Navigation Rollup"
			);
		}

		#endregion

		#region Products and Categories

		public static IPwsObjectWrapper<ProductInformation_V1>[] Products(this IPwsObjectWrapper<NavigationChoice_V1> choice)
		{
			return RESTHandler<IPwsObjectWrapper<ProductInformation_V1>[]>.Invoke(
				() => choice.FollowList<ProductInformation_V1>(f => f.ProductList).ToArray(),
				"Product List"
			);
		}

		public static IPwsObjectWrapper<Category_V1>[] Categories(this IPwsObjectWrapper<ProductInformation_V1> info)
		{
			return RESTHandler<IPwsObjectWrapper<Category_V1>[]>.Invoke(
				() => info.FollowList<Category_V1>(f => f.Categories).ToArray(),
				"Category List"
			);
		}

		public static IPwsObjectWrapper<Product_V2> BespokeKit(this IPwsObjectWrapper<ProductInformation_V1> info)
		{
			var link = info.PwsObject.CustomerBespokeKits.Where(f => f.Href.AbsolutePath.ToLower().EndsWith("bespokekit")).FirstOrDefault();
			if (link == null)
				return null;

			return RESTHandler<IPwsObjectWrapper<Product_V2>>.Invoke(() => info.Post(f => link, new Product_V2() { }), "Bespoke Kit");
		}

		public static IPwsObjectWrapper<Product_V2> BespokeDrillingKit(this IPwsObjectWrapper<ProductInformation_V1> info)
		{
			var link = info.PwsObject.CustomerBespokeKits.Where(f => f.Href.AbsolutePath.ToLower().Contains("drilling")).FirstOrDefault();
			if (link == null)
				return null;

			return RESTHandler<IPwsObjectWrapper<Product_V2>>.Invoke(() => info.Post(f => link, new Product_V2() { }), "Bespoke Drilling");
		}

		public static IPwsObjectWrapper<Product_V2> BespokePricing(this IPwsObjectWrapper<ProductInformation_V1> info, Product_V2 product, BespokeKit_V1.BespokeOption.BespokeSelection selection, decimal? selectionQuantity = null)
		{
			if (product.NextBespokeOption == null)
			{
				throw new Exception("All bespoke options assigned.");
			}

			product.NextBespokeOption.Selection = selection;
			if (selectionQuantity != null)
			{
				product.NextBespokeOption.Selection.Quantity = selectionQuantity;
			}

			return RESTHandler<IPwsObjectWrapper<Product_V2>>.Invoke(() => info.Post(f => product.FindSelfLink(), product), "Bespoke Pricing");
		}

		public static IPwsObjectWrapper<ProductInformation_CustomerProduct_V1V1>[] ProductsWithPrices(this IPwsObjectWrapper<NavigationChoice_V1> choice)
		{
			return RESTHandler<IPwsObjectWrapper<ProductInformation_CustomerProduct_V1V1>[]>.Invoke(
				() => choice.FollowList<ProductInformation_CustomerProduct_V1V1>(f => f.ProductList).ToArray(),
				"Product List"
			);
		}

		public static IPwsObjectWrapper<Category_V1>[] Categories(this IPwsObjectWrapper<ProductInformation_CustomerProduct_V1V1> info)
		{
			return RESTHandler<IPwsObjectWrapper<Category_V1>[]>.Invoke(
				() => info.FollowList<Category_V1>(f => f.Categories).ToArray(),
				"Category List"
			);
		}

		public static IPwsObjectWrapper<Product_V2> BespokeKit(this IPwsObjectWrapper<ProductInformation_CustomerProduct_V1V1> info)
		{
			var link = info.PwsObject.CustomerBespokeKits.Where(f => f.Href.AbsolutePath.ToLower().EndsWith("bespokekit")).FirstOrDefault();
			if (link == null)
				return null;

			return RESTHandler<IPwsObjectWrapper<Product_V2>>.Invoke(() => info.Post(f => link, new Product_V2() { }), "Bespoke Kit");
		}

		public static IPwsObjectWrapper<Product_V2> BespokeDrillingKit(this IPwsObjectWrapper<ProductInformation_CustomerProduct_V1V1> info)
		{
			var link = info.PwsObject.CustomerBespokeKits.Where(f => f.Href.AbsolutePath.ToLower().Contains("drilling")).FirstOrDefault();
			if (link == null)
				return null;

			return RESTHandler<IPwsObjectWrapper<Product_V2>>.Invoke(() => info.Post(f => link, new Product_V2() { }), "Bespoke Drilling");
		}

		public static IPwsObjectWrapper<Product_V2> BespokePricing(this IPwsObjectWrapper<ProductInformation_CustomerProduct_V1V1> info, Product_V2 product, BespokeKit_V1.BespokeOption.BespokeSelection selection, decimal? selectionQuantity = null)
		{
			if (product.NextBespokeOption == null)
			{
				throw new Exception("All bespoke options assigned.");
			}

			product.NextBespokeOption.Selection = selection;
			if (selectionQuantity != null)
			{
				product.NextBespokeOption.Selection.Quantity = selectionQuantity;
			}

			return RESTHandler<IPwsObjectWrapper<Product_V2>>.Invoke(() => info.Post(f => product.FindSelfLink(), product), "Bespoke Pricing");
		}

		#endregion
	}
}
