using Pws.Clients.RestLibrary.Service;
using Pws.Clients.RestLibrary.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PwsClientRestExample
{
     public static class Application
    {
        private static DeploymentType _deployment = Configuration.DefaultDeploymentType;
        private static Guid _apiKey = Configuration.DefaultApiKey;
        private static Guid _privateKey = Configuration.DefaultPrivateKey;
        private static String _userName = Configuration.DefaultUsername;
        private static String _password = Configuration.DefaultPassword;

        public static void SetKeys(Guid apiKey, Guid privateKey, DeploymentType deployment)
        {
            _apiKey = apiKey;
            _privateKey = privateKey;
            _deployment = deployment;
        }

        public static void SetLogin(String userName, String password)
        {
            _userName = userName;
            _password = password;
        }

        public static IPwsService GetService()
        {
            return new PwsService(_apiKey, _privateKey, _deployment);
        }

        public static IPwsApplication GetApplication()
        {
            return new PwsApplication(GetService(), _userName, _password);
        }
    }
}
