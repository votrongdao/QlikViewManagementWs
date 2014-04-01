using System;
using System.Web.Http;

namespace QlikViewManagement.Controllers
{
    public class HealthController : ApiController
    {
        public static string TextMessagePrefix = "QlikView Management Self Host Service is working ";

        public string GetHealth()
        {
            return TextMessagePrefix + (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
    }
}
