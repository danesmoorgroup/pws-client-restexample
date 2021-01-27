using Pws.Clients.RestLibrary.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwsClientRestExample
{
	/*
	 * Example of PWS Client Restful service
	 * Copyright (C) PWS Distributors Ltd 2021
	 * All rights reserved.
	 * 
	 * Requires download of NuGet package Pws.Clients.RestLibrary.Service from
	 * https://nuget.pkg.github.com/danesmoorgroup/index.json
	 * Email: systems.development@pws.co.uk
	 */

	class Program
    {
        static void Main(string[] args)
        {
			// Insert API Key and Private Key here.
			Model.Application.SetKeys(new Guid("<insert API key>"), new Guid("<insert Private key>"), DeploymentType.Staging);

			// Insert UserId and Password here.
			Model.Application.SetLogin("<insert UserId>", "<insert Password>");

			// Get primary customer for this login
			try
            {
                var customer = Model.Customer.GetPrimary(Model.Token.GetNewToken());

				//// Alternatively we could select another customer by passing in the account code (if the user is permitted)
				//var customer = Model.Customer.GetCustomerAccount(Model.Token.GetNewToken(), "<accountCode>");

				// Demonstrate Product Navigation, pricing and categories
				Demo.ProductNavigation.Run(customer);

                // Demonstrate Creation
                //Demo.OrderCreation.Run(customer);

				// Demonstrate Order Retrieval/ Progression
				//Demo.OrderRetrievalAndProgression.Run(customer);
			}
            catch (RESTException re) { Console.WriteLine(re.Message); }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }
    }
}
