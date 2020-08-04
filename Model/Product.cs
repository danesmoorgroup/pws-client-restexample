using Pws.Clients.RestLibrary.Customers;
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
			return RESTHandler<IPwsObjectWrapper<NavigationChoice_V1>>.Invoke(
				() => customer.Follow<NavigationChoice_V1>(f => f.ProductNavigation),
				"Product Navigation"
			);
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

		public static IPwsObjectWrapper<Category_V1>[] Categories(this IPwsObjectWrapper<ProductInformation_V1> product)
		{
			return RESTHandler<IPwsObjectWrapper<Category_V1>[]>.Invoke(
				() => product.FollowList<Category_V1>(f => f.Categories).ToArray(),
				"Category List"
			);
		}

		public static IPwsObjectWrapper<ProductInformation_CustomerProduct_V1V1>[] ProductsWithPrices(this IPwsObjectWrapper<NavigationChoice_V1> choice)
		{
			return RESTHandler<IPwsObjectWrapper<ProductInformation_CustomerProduct_V1V1>[]>.Invoke(
				() => choice.FollowList<ProductInformation_CustomerProduct_V1V1>(f => f.ProductList).ToArray(),
				"Product List"
			);
		}

		public static IPwsObjectWrapper<Category_V1>[] Categories(this IPwsObjectWrapper<ProductInformation_CustomerProduct_V1V1> product)
		{
			return RESTHandler<IPwsObjectWrapper<Category_V1>[]>.Invoke(
				() => product.FollowList<Category_V1>(f => f.Categories).ToArray(),
				"Category List"
			);
		}

		#endregion
	}
}
