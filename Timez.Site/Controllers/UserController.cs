using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Common.Extentions;
using Timez.BLL;
using Timez.BLL.Users;
using Timez.Controllers.Base;
using Timez.Entities;

namespace Timez.Controllers
{
	public sealed class UserController : BaseController
	{
		#region Details
		[Authorize]
		public ViewResult Index()
		{
			return View("Details", Utility.Users.CurrentUser);
		}

		/// <summary>
		/// Профиль пользователя
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[Authorize]
		public ViewResult Details(int id)
		{
			ViewData.Model = Utility.Users.Get(id);

			return View();
		}

		static List<SelectListItem> GetTimeZones(TimeSpan? selected = null)
		{
			var zones = TimeZoneInfo.GetSystemTimeZones()
				.GroupBy(x => x.BaseUtcOffset)
				.Select(t => new SelectListItem
				{
					Text = string.Format("{2}{0:D2}:{1:D2}", t.Key.Hours, Math.Abs(t.Key.Minutes), t.Key.TotalMinutes > 0 ? "+" : ""),
					Value = t.Key.TotalMinutes.ToString(CultureInfo.InvariantCulture),
					Selected = selected.HasValue && t.Key == selected.Value
				})
				.Distinct()
				.ToList();

			return zones;
		}

		#endregion

		#region Registration
		/// <summary>
		/// Страница регистрации
		/// </summary>
		/// <returns></returns>
		public ViewResult Register(string id)
		{
			ViewData.Model = GetTimeZones();

			if (!id.IsNullOrEmpty())
			{
				IUsersInvite invite = Utility.Invites.GetInvite(id);
				if (invite != null)
				{
					ViewData["EMail"] = invite.EMail.Trim();
				}
			}

			return View();
		}

		/// <summary>
		/// Регистрируем пользователя
		/// </summary>
		/// <param name="id">Инвайт</param>
		/// <param name="returnUrl"></param>
		/// <param name="collection"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult Register(string id, string returnUrl, FormCollection collection)
		{
			string password = collection["password"];
			string confirmPassword = collection["confirmPassword"];
			string email = collection["email"].Trim(); // во время обычной регистрации имейл это логин
			TimeSpan offset = TimeSpan.FromMinutes(collection["timezone"].ToInt());

			try
			{
				#region Валидация
				if (password.IsNullOrEmpty())
				{
					ModelState.AddModelError("password", "Введите пароль.");
				}
				if (email.IsNullOrEmpty())
				{
					ModelState.AddModelError("email", "Введите email.");
				}

				if (!email.IsValidEmail())
				{
					ModelState.AddModelError("email", "Введите правильный email.");
				}

				if (password != confirmPassword)
				{
					ModelState.AddModelError("confirmPassword", "Повторите пароль правильно.");
				}

				// Пользователь регестрируется через имейл, по этому он является логином
				IUser oldUser = Utility.Users.Get(email);
				if (oldUser != null)
				{
					ViewData.Add("email", email);
					ModelState.AddModelError("email", "Пользователь уже зарегестрирован.");

					// на всякий случай высылаем повторно конфирм, если пользователь решил ввести данные повторно
					if (!oldUser.IsConfirmed && UsersUtility.CheckPassword(oldUser, password))
						MailsManager.SendConfirmEmail(oldUser);
				}
				#endregion

				if (ModelState.IsValid)
				{
					string nick = email.Split('@')[0];
					try
					{
						Utility.Users.Add(
							nick,
							password,
							email,
							offset,
							id, // Код инвайта
							RegistrationType.Default);
					}
					catch (TariffException exception)
					{
						return Message(exception.Message);
					}

					if (returnUrl.IsNullOrEmpty())
					{
						return View("Registration");
					}

					return Redirect(returnUrl);
				}

				ViewData.Model = GetTimeZones();

				return View();
			}
			catch
			{
				Utility.Users.Delete(email);
				throw;
			}
		}

		/// <summary>
		/// Активация пользователя
		/// </summary>
		/// <param name="id">код подтверждения</param>
		/// <returns></returns>
		public RedirectToRouteResult Activate(string id)
		{
			Utility.Users.Activate(id);

			return RedirectToAction("Congratulations");
		}

		/// <summary>
		/// Подтверждение имейла после изменения
		/// </summary>
		/// <param name="id">код подтверждения</param>
		/// <returns></returns>
		public ViewResult ConfirmEmail(string id)
		{
			Utility.Users.ConfirmEmail(id);

			return Message("Почтовый ящик подтвержден.");
		}

		public ViewResult Congratulations()
		{
			ViewData.Model = Utility.Users.CurrentUser;
			return View();
		}
		#endregion

		/// <summary>
		/// Быстрое приглашение на доску через ссылку
		/// </summary>
		/// <param name="id">InviteCode доски</param>
		/// <returns></returns>
		public ActionResult Invite(string id)
		{
			if (Utility.Authentication.IsAuthenticated)
			{
				IOrganization organization = Utility.Organizations.GetByInviteCode(id);
				try
				{
					Utility.Organizations.AddUser(organization, Utility.Users.CurrentUser);
				}
				catch (TariffException ex)
				{
					return Message(ex.Message);
				}
				return RedirectToAction("Index", "Boards");
			}

			return RedirectToAction("Register", "User", new { id });
		}

		#region Authorization

		/// <summary>
		/// Выход
		/// </summary>
		/// <returns></returns>
		[Authorize]
		public RedirectToRouteResult SignOut()
		{
			Utility.Authentication.SignOut();
			return RedirectToAction("Index", "Home");
		}

		public ViewResult Login(string id)
		{
			return View();
		}

		/// <summary>
		/// Авторизация
		/// </summary>
		[HttpPost]
		public ActionResult Login(string id, string login, string password, bool rememberme, string returnUrl)
		{
			string inviteCode = id;

			if (login.IsNullOrEmpty())
			{
				ModelState.AddModelError("login", "Введите логин.");
			}
			if (password.IsNullOrEmpty())
			{
				ModelState.AddModelError("password", "Введите пароль.");
			}

			if (ModelState.IsValid)
			{
				IUser user = Utility.Users.Get(login);

				if (user != null)
				{
					if (UsersUtility.CheckPassword(user, password))
					{
						if (user.IsConfirmed)
						{
							Utility.Authentication.SignIn(user.Id, rememberme);

							if (!inviteCode.IsNullOrEmpty())
							{
								IOrganization organization = Utility.Organizations.GetByInviteCode(inviteCode);
								try
								{
									Utility.Organizations.AddUser(organization, user);
								}
								catch (TariffException exception)
								{
									return Message(exception.Message);
								}
							}

							if (!returnUrl.IsNullOrEmpty())
								return Redirect(returnUrl);

							return RedirectToAction("index", "boards");
						}

						ModelState.AddModelError("login", "Регистрация не подтверждена. Проверьте почту для получения инструкций.");
					}
					else
					{
						ModelState.AddModelError("password", "Пароль неверный.");
					}
				}
				else
				{
					ModelState.AddModelError("login", "Пользователя не существует.");
				}
			}

			return View();
		}

		#endregion

		#region Edits

		/// <summary>
		/// Редактировании инфы о себе
		/// </summary>
		[Authorize]
		public ViewResult Edit()
		{
			ViewData.Model = Utility.Users.CurrentUser;

			//if (Utility.Users.CurrentUser.EmailChangeDate.HasValue)
			//{
			//    ModelState.AddModelError("EmailChangeDate", "Имейл не подтвержден");
			//}

			ViewData.Add("timezones", GetTimeZones(TimeSpan.FromMinutes(Utility.Users.CurrentUser.TimeZone)));

			return View();
		}

		[HttpPost]
		[Authorize]
		public PartialViewResult EditCommonSettings(FormCollection collection)
		{
			TimeSpan offset = TimeSpan.FromMinutes(collection["timezones"].ToInt());
			string email = collection["email"].Trim();
			string nick = collection["Nick"].Trim();

			if (!email.IsValidEmail())
				ModelState.AddModelError("email", "Email не правильный");
			if (nick.IsNullOrEmpty())
				ModelState.AddModelError("Nick", "Введите имя");

			if (ModelState.IsValid)
			{
				// Обновление настройки пользователя
				bool recievOwnEvents = collection["RecievOwnEvents"].StartsWith("true");
				Utility.Users.UpdateRecievOwnEvents(Utility.Authentication.UserId, recievOwnEvents);
				Utility.Users.UpdateMailingAdderss(Utility.Authentication.UserId, email);
				// Смена временной зоны
				Utility.Users.Update(Utility.Authentication.UserId, offset, nick);
			}

			ViewData.Model = Utility.Users.CurrentUser;
			ViewData.Add("timezones", GetTimeZones(TimeSpan.FromMinutes(Utility.Users.CurrentUser.TimeZone)));

			return PartialView("EditCommonSettings");
		}

		#endregion

		#region Изменение пароля

		[HttpPost]
		[Authorize]
		public ViewResult EditPassword(FormCollection collection)
		{
			string oldPassword = collection["oldPassword"];
			if (oldPassword.IsNullOrEmpty())
			{
				ModelState.AddModelError("oldPassword", "Введите старый пароль.");
			}
			else if (!UsersUtility.CheckPassword(Utility.Users.CurrentUser, oldPassword))
			{
				ModelState.AddModelError("oldPassword", "Пароль неверный.");
			}

			if (TrySetNewPassword(Utility.Authentication.UserId, collection))
			{
				ViewData.Add("message", "Пароль успешно изменен.");
			}

			ViewData.Model = Utility.Users.CurrentUser;

			// Отображаем ошибки
			return View("EditPassword");
		}

		private bool TrySetNewPassword(int userId, FormCollection collection)
		{
			string newPassword = collection["password"];
			string confirmPassword = collection["confirmPassword"];
			if (newPassword != confirmPassword)
			{
				ModelState.AddModelError("confirmPassword", "Повторите новый пароль правильно.");
			}

			if (ModelState.IsValid)
			{
				Utility.Users.SetNewPassword(userId, newPassword);
				return true;
			}

			return false;
		}

		public ViewResult RestorePassword()
		{
			return View();
		}

		/// <summary>
		/// Восстановление пароля
		/// </summary>
		[HttpPost]
		public ViewResult RestorePassword(string loginOrEmail)
		{
			#region Валидация

			if (loginOrEmail.IsNullOrEmpty())
			{
				ModelState.AddModelError("loginOrEmail", "Вы ничего не ввели.");
			}

			IUser user = Utility.Users.Get(loginOrEmail) ?? Utility.Users.GetByEmail(loginOrEmail);
			if (user == null)
			{
				ModelState.AddModelError("loginOrEmail", "Такой пользователь не зарегистрирован.");
			}

			#endregion

			if (ModelState.IsValid)
			{
				string url = Url.Action("NewPassword", "User", new { id = user.ConfimKey }, "http");

				// TODO: более развернутое сообщение, с учетом того, что форма доступна всем и можно ввести чужой имейл.
				string message = "<p>Вы воспользовались функцией восстановления пароля.</p>" +
								 "<p>Пройдите по ссылке <a href='{0}'>{0}</a> и задайте новый пароль.</p>".Params(url);
				MailsManager.SendMail(user, "Установка нового пароля", message);

				ViewData.Model = "Письмо с инструкциями выслано на Ваш ящик.";
			}

			return View();
		}

		/// <summary>
		/// Страница установки нового пароля
		/// </summary>
		/// <param name="id">Код пользователя</param>
		[HttpGet]
		public ViewResult NewPassword(string id)
		{
			return View();
		}

		[HttpPost]
		public RedirectToRouteResult NewPassword(string id, FormCollection collection)
		{
			IUser user = Utility.Users.GetByCode(id);

			Utility.Authentication.SignIn(user.Id, true);

			TrySetNewPassword(user.Id, collection);

			return RedirectToAction("Index", "Boards");
		}

		#endregion

		[Authorize]
		public RedirectToRouteResult ReconfirmEmail()
		{
			Utility.Users.OnUpdateMailingAdderss.Invoke(new EventArgs<IUser>(Utility.Users.CurrentUser));
			return RedirectToAction("Edit");
		}
	}
}
