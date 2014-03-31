using System;
using System.Web.Http;

namespace QlikViewManagement.Controllers
{
    public class HomeController : ApiController
    {
        public static string TextMessagePrefix = "QlikView Management Self Host Service is working ";
        public string Index()
        {
            return TextMessagePrefix + DateTime.Now.Millisecond;
        }
    }
}
