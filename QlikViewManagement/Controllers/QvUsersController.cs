using System.Web.Http;

namespace QlikViewManagement.Controllers
{
    public class QvUsersController : ApiController
    {
        public string QvUserManager(string[] args)
        {
            return qv_user_manager.Program.Run(args);
        }
    }
}
