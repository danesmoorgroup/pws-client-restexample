using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwsClientRestExample
{
	public class RESTQueryBuilder
	{
		private readonly IList<Tuple<String, Operator, object>> values = new List<Tuple<String, Operator, object>>();

		public enum Operator { Lt, Le, Eq, Ge, Gt, Ne }

		public String QueryString
		{
			get
			{
				var query = values.Select(f =>
					{
						var key = f.Item1 + " " + f.Item2.ToString();
						var value = ((DateTime)f.Item3).ToString("s");
						return key + " " + value;
					}).Aggregate((lh, rh) => lh + " AND " + rh);

				if (String.IsNullOrWhiteSpace(query))
					return String.Empty;

				return "filter=" + query;
			}
		}

		public RESTQueryBuilder AddDate(String property, Operator op, DateTime date)
		{
			values.Add(new Tuple<string, Operator, object>(property, op, date));
			return this;
		}
	}
}
