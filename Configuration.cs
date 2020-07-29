using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pws.Clients.RestLibrary.Transport;

namespace PwsClientRestExample
{
	/*
	 * Copyright (C) PWS Distributors Ltd 2011-2020
	 * All rights reserved.
	 */

	public static class Configuration
	{
		/// <summary>
		/// This is required. It is the type of PWS Web Services deployment to connect to.
		/// Production is the main deployment of services, and is always live. Staging is a
		/// sandbox test deployment, which is safe to make updates upon (e.g. placing test orders).
		/// </summary>
		public static readonly DeploymentType DefaultDeploymentType = DeploymentType.Production;

		/// <summary>
		/// This is required. It is the API Key to use when generating the PWS HTTP header values.
		/// </summary>
		public static readonly Guid DefaultApiKey = new Guid();

		/// <summary>
		/// This is required. It is used as the private key for signing requests, via the PWS HMAC HTTP header values.
		/// </summary>
		public static readonly Guid DefaultPrivateKey = new Guid();

		/// <summary>
		/// This is required. It is the eCommerce username to use when performing authentication.
		/// </summary>
		public static readonly String DefaultUsername = String.Empty;

		/// <summary>
		/// This is required. It is the eCommerce password to use when performing authentication.
		/// </summary>
		public static readonly String DefaultPassword = String.Empty;
	}
}
