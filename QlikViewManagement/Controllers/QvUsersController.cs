using System;
using System.Web.Http;

namespace QlikViewManagement.Controllers
{
    public class QvUsersController : ApiController
    {
        public string GetQvUsers(string sargs)
        {
            string[] args = null;
            if(!String.IsNullOrEmpty(sargs))
                args = sargs.Split(',');
            return qv_user_manager.Program.Run(args);
        }
    }
}
