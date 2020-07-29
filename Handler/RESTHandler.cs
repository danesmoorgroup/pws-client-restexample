using Pws.Clients.RestLibrary.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwsClientRestExample
{
	public static class RESTHandler<T>
	{
		public static T Invoke(Func<T> request, String method)
		{
			try
			{
				return request();
			}
			catch (PwsServiceConflictException ce)
			{
				throw new RESTException(RESTException.ExceptionType.Conflict, method + " Conflict Exception.", ce);
			}
			catch (PwsServiceAuthenticationException ae)
			{
				throw new RESTException(RESTException.ExceptionType.Authentication, method + " Authentication Exception.", ae);
			}
			catch (PwsServicePermissionException pe)
			{
				throw new RESTException(RESTException.ExceptionType.Permission, method + " Permission Exception.", pe);
			}
			catch (PwsServiceValidationException ve)
			{
				throw new RESTException(RESTException.ExceptionType.Validation, method + " Validation Exception.", ve);
			}
			catch (PwsServiceResponseException we)
			{
				throw new RESTException(RESTException.ExceptionType.Response, method + " Response Exception.", we);
			}
			catch (PwsServiceException pse)
			{
				throw new RESTException(RESTException.ExceptionType.Server, method + " Server Exception.", pse);
			}
		}
	}
}
