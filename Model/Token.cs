using Pws.Clients.RestLibrary.Service;
using Pws.Clients.RestLibrary.ECommerce.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pws.Clients.RestLibrary;
using System.IO;

namespace PwsClientRestExample.Model
{
    public static class Token
    {
        public static IPwsObjectWrapper<Token_V1> GetNewToken()
        {
            return RESTHandler<IPwsObjectWrapper<Token_V1>>.Invoke(() => Application.GetApplication().GetDefaultToken(), "Token");
        }
    }

	public static class TokenExtensions
	{
		public static FileInfo BinaryFile(this IPwsObjectWrapper<Token_V1> token, Link_V1 link)
		{
			return RESTHandler<FileInfo>.Invoke(() => Application.GetService().GetBinaryFile(link.Href, link.MediaType, token.PwsObject), "Download Binary File");
		}
	}
}
