using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Timez.BLL.Texts;
using Timez.Controllers.Base;
using Timez.Entities;
using Timez.Models;
using Timez.Utilities;
using Timez.Helpers;

namespace Timez.Controllers
{
	/// <summary>
	/// Управление сайтом
	/// </summary>
	public sealed class AdminController : BaseAdminController
	{
		public ViewResult Index()
		{
			return View();
		}

		public string ClearCache()
		{
			int count = new CacheService().ClearAll();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			return count.ToString();
		}

		public string Info()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("EffectivePercentagePhysicalMemoryLimit: {0}", HttpRuntime.Cache.EffectivePercentagePhysicalMemoryLimit).AppendLine();
			sb.AppendFormat("EffectivePrivateBytesLimit: {0}", HttpRuntime.Cache.EffectivePrivateBytesLimit).AppendLine();
			Process proc = Process.GetCurrentProcess();
			sb.AppendFormat("PrivateMemorySize64: {0}", proc.PrivateMemorySize64.ToString()).AppendLine();
			sb.AppendFormat("Count: {0}", HttpRuntime.Cache.Count).AppendLine();

			sb.AppendLine();

			List<string> keys = new List<string>();

			foreach (DictionaryEntry item in HttpRuntime.Cache)
			{
				string key = item.Key.ToString();
				if (key.StartsWith(CacheService.Prefix))
				{
					keys.Add(key);
				}
			}

			keys.Sort();

			sb.Append(keys.Aggregate((x, y) => x + "<br/>" + y));

			return sb.ToString().Replace(Environment.NewLine, "<br/>");
		}

		public RedirectToRouteResult Backdoor(int id)
		{
			var user = Utility.Users.Get(id);
			Utility.Authentication.SignIn(user.Id, false);
			return RedirectToAction("Index", "Boards");
		}

		public ViewResult Test()
		{
			return View();
		}

		#region Редактирование текстов
		public ViewResult Edit(int? id)
		{
			News news;
			if (id.HasValue)
			{
				IText text = Utility.Texts.Get(id.Value);
				news = new News(text);
			}
			else
			{
				news = new News();
				news.IsVisible = true;
			}
			ViewData.Model = news;

			return View();
		}

		// TODO: редактирование новостей только для админов
		[HttpPost]
		[ValidateInput(false)]
		public RedirectToRouteResult Edit(News news)
		{
			IText text = Utility.Texts.Save(news.Id, news.Title, news.Content, TextType.News, news.IsVisible);
			return RedirectToAction("Edit", new { text.Id });
		}

		public RedirectToRouteResult Delete(int id)
		{
			Utility.Texts.Delete(id);

			return RedirectToAction("Index");
		}

		public PartialViewResult List(int page = 1)
		{
			TextsUtility.TextsCollection textsCollection = Utility.Texts.Get(TextType.News, page, Pager.DefaultItemsOnPage);
			ViewData.Model = textsCollection;
			return PartialView();
		}

		#endregion
	}
}
