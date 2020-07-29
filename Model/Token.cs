using Pws.Clients.RestLibrary.Service;
using Pws.Clients.RestLibrary.ECommerce.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwsClientRestExample
{
    public static class Token
    {
        public static IPwsObjectWrapper<Token_V1> GetNewToken()
        {
            return RESTHandler<IPwsObjectWrapper<Token_V1>>.Invoke(() => Application.GetApplication().GetDefaultToken(), "Token");
        }
    }
}
