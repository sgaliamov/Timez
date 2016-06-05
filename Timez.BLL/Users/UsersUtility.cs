using System;
using Common;
using System.Linq;
using System.Collections.Generic;
using System.Transactions;
using Common.Extentions;
using Timez.Entities;
using Timez.Entities.Classes;

namespace Timez.BLL.Users
{
	public sealed partial class UsersUtility : BaseUtility<UsersUtility>
	{
		#region События

		public Listener<EventArgs<IUser>> OnCreate = new Listener<EventArgs<IUser>>();

		/// <summary>
		/// Подтвержение имейла
		/// </summary>
		public Listener<EventArgs<IUser>> OnConfirmEmail = new Listener<EventArgs<IUser>>();

		public Listener<EventArgs<IUser>> OnUpdateMailingAdderss = new Listener<EventArgs<IUser>>();

		/// <summary>
		/// Изменение данных о пользователе
		/// </summary>
		public Listener<UpdateEventArgs<IUser>> OnUpdate = new Listener<UpdateEventArgs<IUser>>();

		#endregion

		#region Пароль
		/// <summary>
		/// Шифрование пароля
		/// </summary>
		static string CodePassword(string password, string login)
		{
			return (password.GetMD5Hash() + login.GetMD5Hash()).GetMD5Hash();
		}

		/// <summary>
		/// Проверка парольля пользователя
		/// </summary>
		/// <param name="user"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public static bool CheckPassword(IUser user, string password)
		{
			return user != null && user.Password == CodePassword(password, user.Login);
		}

		/// <summary>
		/// Установка нового пароля
		/// </summary>
		public IUser SetNewPassword(int userId, string newPassword)
		{
			using (TransactionScope scope = new TransactionScope())
			{

				IUser user = Repository.Users.Get(userId);
				MockUser oldUser = new MockUser(user);

				user.Password = CodePassword(newPassword, user.Login);
				Repository.SubmitChanges();

				OnUpdate.Invoke(new UpdateEventArgs<IUser>(oldUser, user));
				scope.Complete();

				return user;
			}
		}

		#endregion

		#region Updates

		public IUser Update(int userId, TimeSpan timeZone, string nick)
		{
			IUser user = Repository.Users.Get(userId);
			int minutes = (int)timeZone.TotalMinutes;

			if (user.Nick != nick || user.TimeZone != minutes)
			{
				MockUser oldUser = new MockUser(user);
				using (TransactionScope scope = new TransactionScope())
				{
					user.TimeZone = minutes;
					user.Nick = nick;
					Repository.SubmitChanges();

					OnUpdate.Invoke(new UpdateEventArgs<IUser>(oldUser, user));

					scope.Complete();
				}
			}

			return user;
		}

		public IUser UpdateRecievOwnEvents(int userId, bool recievOwnEvents)
		{
			IUser user = Repository.Users.Get(userId);

			if (user.RecievOwnEvents != recievOwnEvents)
			{
				MockUser oldUser = new MockUser(user);
				using (TransactionScope scope = new TransactionScope())
				{
					user.RecievOwnEvents = recievOwnEvents;
					Repository.SubmitChanges();

					OnUpdate.Invoke(new UpdateEventArgs<IUser>(oldUser, user));
					scope.Complete();
				}
			}

			return user;
		}

		public void UpdateMailingAdderss(int userId, string email)
		{
			IUser user = Repository.Users.Get(userId);
			if (user.EMail != email && email.IsValidEmail())
			{
				using (TransactionScope scope = new TransactionScope())
				{
					user.EMail = email;
					user.EmailChangeDate = DateTimeOffset.Now;
					user.ConfimKey = Guid.NewGuid().ToString();
					Repository.SubmitChanges();

					OnUpdateMailingAdderss.Invoke(new EventArgs<IUser>(user));

					scope.Complete();
				}
			}
		}

		/// <summary>
		/// Подтверждение статуса реального человека. Подтвержение имейла делается отедльно
		/// Активация пользователя
		/// Создается первая доска
		/// </summary>
		public IUser Activate(string code)
		{
			using (TransactionScope scope = new TransactionScope())
			{
				IUser user = GetByCode(code);
				user.IsConfirmed = true;
				user.EmailChangeDate = 
					user.GetRegistrationType() == RegistrationType.Default
					|| user.GetRegistrationType() == RegistrationType.Google
					|| user.GetRegistrationType() == RegistrationType.Facebook
					? (DateTimeOffset?)null
					: DateTimeOffset.Now; // для вконтакта имейла сразу нет, нужно задать
				Repository.SubmitChanges();

				Utility.Authentication.SignIn(user.Id, false);

				// Создать первую доску
				Utility.Boards.Create("Личная доска", "", user, null, null);

				if (!user.EmailChangeDate.HasValue)
					OnConfirmEmail.Invoke(new EventArgs<IUser>(user));

				scope.Complete();

				return user;
			}
		}

		/// <summary>
		/// Подтверждение имейла пользователя
		/// </summary>
		public void ConfirmEmail(string code)
		{
			using (TransactionScope scope = new TransactionScope())
			{
				IUser user = Repository.Users.GetByCode(code);
				user.EmailChangeDate = null;
				Repository.SubmitChanges();

				OnConfirmEmail.Invoke(new EventArgs<IUser>(user));

				scope.Complete();
			}
		}

		#endregion

		#region Gets

		/// <summary>
		/// Пользователь по иду
		/// Кэшируется
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public IUser Get(int id)
		{
			return Cache.Get(GetCacheKey(id), () => Repository.Users.Get(id));
		}

		/// <summary>
		/// Получение пользователя по одноразовому коду
		/// </summary>
		/// <param name="code">уникальный одноразовый код, после использования сбрасывается</param>
		public IUser GetByCode(string code)
		{
			IUser user = Repository.Users.GetByCode(code);
			if (user == null)
				throw new UserNotFoundException();

			// Присваиваем новый код, чтоб по старому нельзя было получить пользователя
			user.ConfimKey = Guid.NewGuid().ToString();
			Repository.SubmitChanges();

			return user;
		}

		/// <summary>
		/// Получение пользователя по логину
		/// </summary>
		public IUser Get(string login)
		{
			return Repository.Users.GetByLogin(login);
		}

		/// <summary>
		/// Получение пользователя по подтвержденному мылу
		/// </summary>
		public IUser GetByEmail(string email)
		{
			return Repository.Users.GetByEmail(email);
		}

		public List<IUser> GetByProject(int projectId)
		{
			return Repository.Users.GetByProject(projectId).ToList();
		}

		/// <summary>
		/// Текущий пользователь
		/// Определяется через UserId
		/// </summary>
		public IUser CurrentUser
		{
			get
			{
				try
				{
					return Get(Authentication.UserId);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
					Authentication.SignOut();
				}

				return null;
			}
		}

		#endregion

		/// <summary>
		/// Создание пользователя
		/// </summary>        
		/// <exception cref="TariffException"></exception>
		public IUser Add(string nick, string password, string login, TimeSpan timeZone, string inviteCode, RegistrationType type)
		{
			using (TransactionScope scope = new TransactionScope())
			{
				string email = login.IsValidEmail() ? login : "";
				IUser user = Repository.Users.Add(nick, login, CodePassword(password, login), email, timeZone, type);

				if (!inviteCode.IsNullOrEmpty())
				{
					// Пользователь был приглашен кем-то через имейл
					IUsersInvite invite = Repository.Invites.GetInvite(inviteCode);
					if (invite != null && invite.EMail.ToUpper() == login.ToUpper())
					{
						// Инвайт верный, добавляем в организацию
						IOrganization organization = Utility.Organizations.Get(invite.OrganizationId);
						Utility.Organizations.AddUser(organization, user, EmployeeRole.Employee, true);
					}
					else
					{
						// приглашение через ссылку
						IOrganization organization = Utility.Organizations.GetByInviteCode(inviteCode);
						if (organization != null)
						{
							Utility.Organizations.AddUser(organization, user);
						}
					}
				}

				// Первая доска, рабочее время и тп создаются при активации (Activate)
				OnCreate.Invoke(new EventArgs<IUser>(user));

				scope.Complete();

				return user;
			}
		}

		#region Удаления

		/// <summary>
		/// Удаление, нужно для тестирования
		/// </summary>
		/// <param name="login"></param>
		public void Delete(string login)
		{
			Repository.Users.Delete(login);
		}

		/// <summary>
		/// Удаляет неподтвержденных
		/// </summary>
		public IUser[] RemoveUnconfirmed(double days)
		{
			return Repository.Users.RemoveUnconfirmed(days);
		}
		#endregion
	}
}
