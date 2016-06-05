using System.Web.Mvc;
using System.Web.SessionState;
using Timez.Controllers.Base;
using Timez.Entities;

namespace Timez.Controllers
{
	[SessionState(SessionStateBehavior.Disabled)]
	public class TextsController : BaseController
	{
		[OutputCache(Duration = CacheDuration)]
		[ChildActionOnly]
		public PartialViewResult Text(int id)
		{
			IText text = Utility.Texts.Get(id);
			ViewData.Model = text;
			return PartialView();
		}

		[OutputCache(Duration = CacheDuration)]
		public ViewResult Index(int id)
		{
			IText text = Utility.Texts.Get(id);
			ViewData.Model = text;
			return View();
		}
	}
}
