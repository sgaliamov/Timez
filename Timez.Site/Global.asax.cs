using System;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Common;
using Timez.Entities;
using Timez.Helpers;
using Timez.Utilities;

namespace Timez
{
	public class MvcApplication : HttpApplication
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute("{*allashx}", new { allashx = @".*\.ashx(/.*)?" });
			routes.IgnoreRoute("sitemap.xml");

			// Если указано 2 параметра, то первый- ид доски
			routes.MapRoute(null,
				"{controller}/{action}/{boardId}/{id}/{organizationId}",
				new { controller = "Kanban", organizationId = UrlParameter.Optional },
				new[] { "Timez.Controllers" }
			).DataTokens["UseNamespaceFallback"] = false;

			routes.MapRoute(null,
				"{controller}/{action}/{id}", 
				new { controller = "Home", action = "Index", id = UrlParameter.Optional },
				new[] { "Timez.Controllers" }
			).DataTokens["UseNamespaceFallback"] = false;
		}

		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}

		protected void Application_AcquireRequestState(object sender, EventArgs e)
		{
			CultureInfo ci = new CultureInfo("ru");

			System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
			System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
		}

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);
		}

		protected void Application_Error(object sender, EventArgs e)
		{
			Exception exception = Server.GetLastError();
			TimezException te = exception as TimezException;

			if (te != null && !te.Logging) return;

			// Если это системная ошибка 
			// или её нужно логировать
			Log.Exception(exception);
		}
	}
}