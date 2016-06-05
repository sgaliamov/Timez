using System.Web.Mvc;
using System.Web.SessionState;
using Timez.BLL.Texts;
using Timez.Controllers.Base;
using Timez.Entities;
using Timez.Helpers;
using Timez.Models;

namespace Timez.Controllers
{
	[SessionState(SessionStateBehavior.Disabled)]
	public class NewsController : BaseController
	{
		[OutputCache(Duration = CacheDuration)]
		public PartialViewResult List(int page = 1, int count = Pager.DefaultItemsOnPage)
		{
			TextsUtility.TextsCollection texts = Utility.Texts.Get(TextType.News, page, count, true);
			ViewData.Model = texts;
			return PartialView();
		}

		[OutputCache(Duration = CacheDuration)]
		public ViewResult Details(int id)
		{
			IText text = Utility.Texts.Get(id);
			ViewData.Model = new News(text);
			return View();
		}

	}
}
