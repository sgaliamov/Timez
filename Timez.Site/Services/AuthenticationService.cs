using System;
using System.Web;
using System.Web.Security;
using Common.Exceptions;
using Timez.BLL;
using Timez.Entities;

namespace Timez.Services
{
	/// <summary>
	/// Обертка над стандартной авторизацией, что бы можно было переопределеить
	/// </summary>
	public class AuthenticationService : IAuthenticationService
	{
		private int? _UserId;
		public void SignIn(int userId, bool createPersistentCookie)
		{
			FormsAuthentication.SetAuthCookie(userId.ToString(), createPersistentCookie);
			_UserId = userId;
		}

		public void SignOut()
		{
			FormsAuthentication.SignOut();
			_UserId = null;
		}

		public bool IsAuthenticated
		{
			get
			{
				return _UserId.HasValue
						|| (HttpContext.Current != null
							&& HttpContext.Current.User != null
							&& HttpContext.Current.User.Identity.IsAuthenticated);
			}
		}

		public int UserId
		{
			get
			{
				if (!IsAuthenticated)
					throw new UserAuthenticationException();

				return _UserId ?? int.Parse(HttpContext.Current.User.Identity.Name);
			}
		}

		/// <summary>
		/// ИСПОЛЬЗОВАТЬ С ОСТОРОЖНОСТЬЮ, ТАК КАК ПОЗВОЛЯЕТ СОЗДАВАТЬ ПОЛЬЗОВАТЕЛЕЙ БЕЗ ПОДТВЕРЖДЕНИЯ.
		/// Авторизует/Создает пользователя с указанным имейлом.
		/// Добавляет в организацию пользователя по инвайт коду
		/// </summary>
		public static void OAuth(UtilityManager utility, string nick, string emailOrLogin, string invite, double timezone, RegistrationType registrationType)
		{
			// RegistrationType.Default недопустим в данном случае
			if (registrationType == RegistrationType.Default)
				throw new InvalidOperationException();

			IUser user = utility.Users.Get(emailOrLogin) ?? utility.Users.GetByEmail(emailOrLogin);

			if (user == null)
			{
				// Сразу подтверждаем регистрацию
				utility.Users.OnCreate += (s, e) => utility.Users.Activate(e.Data.ConfimKey);

				user = utility.Users.Add(nick
					, Membership.GeneratePassword(10, 4), emailOrLogin
					, TimeSpan.FromHours(timezone), invite, registrationType);
			}
			else
			{
				IOrganization organization = utility.Organizations.GetByInviteCode(invite);
				if (organization != null)
				{
					utility.Organizations.AddUser(organization, user);
				}
			}

			utility.Authentication.SignIn(user.Id, true);
		}
	}
}