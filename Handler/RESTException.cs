using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PwsClientRestExample
{
	public class RESTException : Exception
	{
		public enum ExceptionType { Conflict, Authentication, Permission, Validation, Response, Server }

		public ExceptionType Type { get; private set; }

		internal RESTException(ExceptionType type, string message, Exception innerException) : base(message, innerException)
		{
			Type = type;
		}
	}
}
