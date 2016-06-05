using System;
using Common.Extentions;
using System.Web;
using System.Web.Mvc;
using Timez.Controllers.Base;
using Timez.Entities;

namespace Timez.Helpers
{
	/// <summary>
	/// TODO: плохой дизайн
	/// 1. Данные заполняются в другом месте
	/// 2. Сложно будет использовать в другом контексте
	/// 3. Возможны проблемы с тестами
	/// </summary>
	public static class ViewContextExtention
	{
		public static IUser GetCurrentUser(this ViewContext context)
		{
			return ((BaseController)context.Controller).Utility.Users.CurrentUser;
		}

		/// <summary>
		/// Права пользователя на текущей доске
		/// </summary>
		public static UserRole? GetUserRole(this ViewContext context)
		{
			return context.ViewData.ContainsKey("UserRole") ? (UserRole)context.ViewData["UserRole"] : (UserRole?)null;
		}

		/// <summary>
		/// ИД текущей доски
		/// </summary>
		public static int? GetCurrentBoardId(this ViewContext context)
		{
			return (int?)context.ViewData["CurrentBoardId"];
		}

		/// <summary>
		/// Текущий ид
		/// </summary>
		public static int GetCurrentTaskId(this ViewContext context)
		{
			return (int)context.ViewData["CurrentTaskId"];
		}

		public static bool CurrentTaskInArchive(this ViewContext context)
		{
			return (bool)context.ViewData["CurrentTaskInArchive"];
		}

		public static TimeSpan GetTimeZone(this ViewContext context)
		{
			if (context.ViewData["TimeZone"] != null)
				return (TimeSpan)context.ViewData["TimeZone"];

			HttpCookie cookie = context.HttpContext.Request.Cookies.Get("clientTimezoneOffset");
			if (cookie != null && !cookie.Value.IsNullOrEmpty())
				return TimeSpan.FromMinutes(-cookie.Value.ToInt());

			return TimeSpan.FromHours(4);
		}

		public static DateTimeOffset ToUserTime(this ViewContext context, DateTimeOffset dateTime)
		{
			return dateTime.ToOffset(context.GetTimeZone());
		}
	}
}
