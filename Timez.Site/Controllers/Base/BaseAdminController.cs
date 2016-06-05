using System.Web.Mvc;
using Common.Exceptions;

namespace Timez.Controllers.Base
{
	public abstract class BaseAdminController : BaseController
    {
		// ReSharper disable RedundantOverridenMember
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
            if (!Utility.Users.CurrentUser.IsAdmin)
                throw new AccessDeniedException(); 

			base.OnActionExecuting(filterContext);
		}
		// ReSharper restore RedundantOverridenMember    
    }
}
