using System;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Web.Http;
using System.Web.Http.SelfHost;       

namespace QlikViewManagement
{
    class Program
    {
        /// <summary>
        /// Must run with Administrator Privileges!!!
        /// Version1: http://www.asp.net/web-api/overview/hosting-aspnet-web-api/self-host-a-web-api
        /// Version2: http://www.codeproject.com/Tips/622260/WCF-Self-Hosting-with-Example
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var config = new HttpSelfHostConfiguration(ConfigurationManager.AppSettings["ServiceUrl"]);

            config.Routes.MapHttpRoute(
                "API Default", "api/{controller}/{id}",
                new { id = RouteParameter.Optional });

            using (var server = new HttpSelfHostServer(config))
            {
                server.OpenAsync().Wait();
                Console.WriteLine("Press Enter to quit.");
                Console.ReadLine();
            }
        }
    }
}
