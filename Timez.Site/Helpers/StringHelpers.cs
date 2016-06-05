using System.Web.Mvc;

namespace Timez.Helpers
{
	public static class StringHelpers
	{
		/// <summary>
		/// Минуты преводит в чч:мм
		/// </summary>
		public static MvcHtmlString GetTimeString(this HtmlHelper helper, int? minutes)
		{
			return minutes.HasValue
					? new MvcHtmlString((minutes.Value / 60).ToString("D2") + ":" + (minutes.Value % 60).ToString("D2"))
					: MvcHtmlString.Empty;
		}

		public static MvcHtmlString Script(this UrlHelper helper, string url)
		{
			string html = "<script type='text/javascript' src='" + helper.Content(url) + "'></script>";
			return new MvcHtmlString(html);
		}

		public static MvcHtmlString Css(this UrlHelper helper, string url)
		{
			string html = "<link rel='stylesheet' type='text/css' href='" + helper.Content(url) + "' />";
			return new MvcHtmlString(html);
		}
	}
}