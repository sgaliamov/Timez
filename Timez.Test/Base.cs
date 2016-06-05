using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Timez.BLL;
using Timez.Controllers.Base;
using Timez.Helpers;
using Common.Extentions;
using Timez.Services;
using Timez.Utilities;
using CacheKeyValue = System.Collections.Generic.KeyValuePair<Timez.BLL.CacheKey, string>;
using CacheKeyCollection = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<Timez.BLL.CacheKey, string>>;

namespace Timez.Test
{
	class Base
	{
		public static T GetController<T>()
			where T : BaseController, new()
		{
			// TODO: нормальную архитектуру с DI/IoC
			
			T controller = new T();
			controller.Utility.Dispose();
			RequestContext requestContext = new RequestContext(new MockHttpContext(), new RouteData());
			controller.Url = new UrlHelper(requestContext);
			controller.ControllerContext = new ControllerContext
			{
				Controller = controller,
				RequestContext = requestContext
			};

			UtilityManager utility = new UtilityManager(new CacheService(), _AuthenticationService, new SettingsService());
			controller.Utility = utility;
			controller.Cookies = _MockCookies;
			controller.MailsManager = new MailService(controller.Utility, controller.Url);
			
			return controller;
		}

		#region Заглушка для авторизации
		static readonly IAuthenticationService _AuthenticationService = new MockFormsAuthenticationService();

		class MockFormsAuthenticationService : IAuthenticationService
		{
			int? _UserId;
			public void SignIn(int userId, bool createPersistentCookie)
			{
				_UserId = userId;
			}

			public void SignOut()
			{
				_UserId = null;
			}


			public bool IsAuthenticated
			{
				get { return _UserId.HasValue; }
			}

			public int UserId
			{
				get { return _UserId.Value; }
			}
		}
		#endregion

		#region Заглушка для кук
		class MockCookieUtility : ICookiesService
		{
			readonly Dictionary<string, string> _Data = new Dictionary<string, string>();
			public void AddToCookie(string name, string value)
			{
				AddToCookie(name, value, false);
			}

			public string GetFromCookies(string name)
			{
				return _Data.ContainsKey(name) ? _Data[name] : null;
			}


			public void AddToCookie(string name, string value, bool httpOnly)
			{
				if (!_Data.ContainsKey(name))
					_Data.Add(name, value);
			}
		}

		static readonly ICookiesService _MockCookies = new MockCookieUtility();
		#endregion

		private class MockHttpContext : HttpContextBase
		{
			private readonly IPrincipal _User = new GenericPrincipal(new GenericIdentity("someUser"), null /* roles */);
			private readonly HttpRequestBase _Request = new MockHttpRequest();

			public override IPrincipal User
			{
				get
				{
					return _User;
				}
				set
				{
					base.User = value;
				}
			}

			public override HttpRequestBase Request
			{
				get
				{
					return _Request;
				}
			}
		}

		private class MockHttpRequest : HttpRequestBase
		{
			private readonly Uri _Url = new Uri("http://localhost:777/");

			public override Uri Url
			{
				get
				{
					return _Url;
				}
			}
		}
	}
}
