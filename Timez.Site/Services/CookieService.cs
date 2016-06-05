using System;
using System.Collections.Generic;
using System.Web;

namespace Timez.Helpers
{
	public class CookiesService : ICookiesService
	{
		public string GetFromCookies(string name)
		{
			var request = HttpContext.Current.Request;
			return request.Cookies.Get(name) != null
				? request.Cookies.Get(name).Value
				: null;
		}

		public void AddToCookie(string name, string value)
		{
			AddToCookie(name, value, true);
		}


		public void AddToCookie(string name, string value, bool httpOnly)
		{
			HttpContext context = HttpContext.Current;
			HttpCookie cookie = new HttpCookie(name, value)
			{
				HttpOnly = httpOnly,
				Expires = DateTime.MaxValue
			};
			context.Response.Cookies.Add(cookie);
		}


		public static void ClearAll()
		{
			if (HttpContext.Current == null)
				return;

			// просрачиваем куки
			HttpCookieCollection cookies = HttpContext.Current.Request.Cookies;
			List<HttpCookie> list = new List<HttpCookie>(cookies.Count);
			foreach (string name in cookies)
			{
				cookies[name].Expires = DateTime.Now.AddDays(-1);
				list.Add(cookies[name]);
			}
			cookies.Clear();

			// отдаем клиенту просроченные куки
			cookies = HttpContext.Current.Response.Cookies;
			cookies.Clear();
			foreach (HttpCookie cookie in list)
			{
				cookies.Add(cookie);
			}
		}
	}

	public interface ICookiesService
	{
		void AddToCookie(string name, string value);
		void AddToCookie(string name, string value, bool httpOnly);
		string GetFromCookies(string name);
	}
}