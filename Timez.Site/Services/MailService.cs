using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Web.Mvc;
using Common;
using Common.Extentions;
using Timez.BLL;
using Timez.Entities;

namespace Timez.Utilities
{
	/// <summary>
	/// Отвечает за рассылку мыла по событиям
	/// </summary>
	public sealed class MailService
	{
		#region Поток отпраки сообщений

		/// <summary>
		/// TODO: Почему почтовый клиент статический?
		/// Почему нельзя создавать его ва каждом запросе
		/// </summary>
		static readonly SmtpClient _SmtpClient;

		/// <summary>
		/// Стек сообщений
		/// </summary>
		static readonly Stack<MailMessage> _Messages = new Stack<MailMessage>();

		/// <summary>
		/// Поток отправки сообщений
		/// синглтон
		/// </summary>
		static readonly Thread _SendingThread;
		static readonly object _SendingThreadLocker = new object();

		static MailService()
		{
			_SmtpClient = new SmtpClient();

			_SendingThread = new Thread(Sending)
			{
				Priority = ThreadPriority.Lowest,
				IsBackground = true
			};
			_SendingThread.Start();
		}

		/// <summary>
		/// Для паузы потока, если нет сообщений для отправки
		/// </summary>
		static readonly EventWaitHandle _EventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

		/// <summary>
		/// Производит отправку письма
		/// Добавляет мыло в очередь для отправки
		/// </summary>
		static void _SendMail(string to, string subj, string message)
		{
			if (string.IsNullOrEmpty(to))// || !to.IsValidEmail() у ФБ странный имейл
				return;

			// Формируем сообщение
			var mail = new MailMessage();
			mail.To.Add(to);
			mail.Subject = subj;
			mail.Body = message;
			mail.IsBodyHtml = true;

			// Добавлем в очередь
			lock (_SendingThreadLocker)
			{
				_Messages.Push(mail);

				// появилось сообщения запускаем поток отправки
				_EventWaitHandle.Set();
			}
		}

		/// <summary>
		/// Обрамляет текст сообщения
		/// </summary>
		internal void SendMail(IUser to, string subj, string rawMessage)
		{
			// не отправляем сообщения на неподтвержденный ящик, если его долго не подтверждают
			if (to.EmailChangeDate.HasValue && to.EmailChangeDate.Value < DateTimeOffset.Now.AddDays(-1))
				return;

			string message = @"<h3>Здравствуйте, "
				+ to.Nick + "!</h3>"
				+ rawMessage
				+ "<br/>"
				+ "<p>С наилучшими пожеланиями,<br/>Ваш <a href='mailto:info@timez.org'>Timez.Org</a>.</p>";

			_SendMail(to.EMail, subj, message);
		}

		internal void SendMail(string toEmail, string subj, string rawMessage)
		{
			string message = @"<h1>Здравствуйте!</h1><br/>"
				+ rawMessage
				+ @"<br/><br/>
<p>С наилучшими пожеланиями,<br/>Ваш <a href='mailto:info@timez.org'>Timez.Org</a>.</p>";

			_SendMail(toEmail, subj, message);
		}

		/// <summary>
		/// Поток отправки сообщений
		/// </summary>
		static void Sending()
		{
			try
			{
				while (true)
				{
					MailMessage toSend = null;
					lock (_SendingThreadLocker)
					{
						if (_Messages.Count != 0)
							toSend = _Messages.Pop();
					}

					if (toSend == null)
					{
						// Сообщений в стеке нет, ждем пока не появится новое
						_EventWaitHandle.Reset();
						_EventWaitHandle.WaitOne();
						continue;
					}

					try
					{
						// Блокируем клиент, так как он не может отправлять несколько писем одновременно                    
						lock (_SmtpClient)
						{
							_SmtpClient.Send(toSend);
						}
					}
					catch (Exception ex)
					{
						// TODO: Уведомлять администрацию, что ящик не доступен
						// TODO: Может создать таблицу в БД для хранения неотправленных сообщений
						//lock (_Messages)
						//{
						//    _Messages.Push(toSend);
						//}
						Log.Exception(ex);
					}

					Thread.Sleep(1000);
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		#endregion

		#region Отправка по событиям

		/// <summary>
		/// Для построения урлов
		/// </summary>
		private readonly UrlHelper Url;
		private readonly UtilityManager Utility;

		public MailService(UtilityManager utility, UrlHelper url)
		{
			Utility = utility;
			Url = url;

			Utility.Tasks.OnTaskAssigned += (s, e) => TasksUtility_OnTaskAssigned(e.OldData, e.NewData);
			Utility.Tasks.OnUpdateStatus.Add(TasksUtility_OnUpdateStatus);
			Utility.Tasks.OnCreate += (s, e) => TasksUtility_OnCreate(e.Data);

			Utility.Users.OnConfirmEmail +=
				(s, e) =>
				{
					IUser user = e.Data;
					SendMail(user, "TimeZ.org", @"<p>Ваш email подтвержден.</p>");
				};

			Utility.Users.OnUpdateMailingAdderss += (s, e) =>
			{
				IUser user = e.Data;

				// изменение ящика
				if (user.EmailChangeDate.HasValue)
				{
					string href = Url.Action("ConfirmEmail", "User", new { id = user.ConfimKey }, "http");

					SendMail(user, "Подтвердите ваш новый имейл",
						"<p>Вы изменили ваш почтовый ящик.</p>" +
						"<p>Пройдите по ссылке <a href='{0}'>{0}</a>, чтобы подтвердить изменение.</p>"
						.Params(href));
				}

			};

			Utility.Users.OnCreate.Add(
				(s, e) =>
				{
					IUser user = e.Data;
					switch (user.GetRegistrationType())
					{
						case RegistrationType.Default:
							SendConfirmEmail(user);
							break;

						case RegistrationType.Facebook:
						case RegistrationType.Google:
							SendMail(user, "Регистрация на TimeZ.org", @"<p>Спасибо за регистрацию.</p>");
							break;

						case RegistrationType.Vkontakte:
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}
				});
		}

		/// <summary>
		/// Выслать имейл за подтверждением
		/// </summary>
		/// <param name="user"></param>
		internal void SendConfirmEmail(IUser user)
		{
			string href = Url.Action("Activate", "User", new { id = user.ConfimKey }, "http");

			SendMail(user, "Регистрация на TimeZ.org", @"
				<p>Спасибо за регистрацию.</p>
				<p>Пройдите, пожалуйста, по ссылке <a href='{0}'>{0}</a>, чтобы активировать аккаунт.</p>".Params(href));
		}

		/// <summary>
		/// Создание сообщения по задаче
		/// </summary>
		/// <param name="reciveType"></param>
		/// <param name="template">
		/// {0} - проект
		/// {1} - урл на задачу
		/// {2} - название задачи</param>
		/// <param name="task"></param>
		void TaskMessage(ITask task, ReciveType reciveType, string template)
		{
			// Все пользователи в проекте
			List<IUser> users = Utility.Users.GetByProject(task.ProjectId);
			IProject proj = Utility.Projects.Get(task.BoardId, task.ProjectId);

			if (proj != null && users != null && users.Count > 0)
			{
				string url = Url.Action("Details", "Tasks", new { boardId = task.BoardId, id = task.Id }, "http");

				foreach (var user in users)
				{
					if (CheckSettings(reciveType, task, user))
					{
						SendMail(user,
							"timez.org", template.Params
							(
								proj.Name, // 0
								url, // 1
								task.Name) // 2
							);
					}
				}
			}
		}

		/// <summary>
		/// При создании задачи
		/// </summary>
		/// <param name="entity"></param>
		void TasksUtility_OnCreate(ITask entity)
		{
			TaskMessage(entity, ReciveType.TaskCreated, "В проекте \"{0}\" создана новая задача <a href='{1}'>\"{2}\"</a>.");
		}

		/// <summary>
		/// Обновили статус задачи
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		void TasksUtility_OnUpdateStatus(object sender, UpdateEventArgs<ITask> args)
		{
			ITask newTask = args.NewData;
			ITasksStatus status = Utility.Statuses.Get(newTask.BoardId, newTask.TaskStatusId);
			if (status != null)
			{
				TaskMessage(newTask, ReciveType.TaskStatusChanged, "В проекте \"{0}\" задаче <a href='{1}'>\"{2}\"</a> установлен статус \"" + status.Name + "\".");
			}
		}

		/// <summary>
		/// Задача назначена на пользователя, отсылаем ему ведомление
		/// </summary>
		void TasksUtility_OnTaskAssigned(ITask oldTask, ITask newTask)
		{
			TaskMessage(newTask, ReciveType.TaskAssigned, "На вас назначена задача <a href='{1}'>\"{2}\"</a> в проекте \"{0}\".");
		}

		/// <summary>
		/// Проверка настроек уведомлений для пользователя в проекте
		/// </summary>
		/// <param name="reciveType">какой тип уведомлений требуется</param>
		/// <param name="task">проверяемая задача</param>
		/// <param name="user">проверяемый пользователь</param>
		/// <returns></returns>
		bool CheckSettings(ReciveType reciveType, ITask task, IUser user)
		{
			// Текущий пользователь является инициатором сообщения
			if (!user.RecievOwnEvents && Utility.Authentication.UserId == user.Id)
			{
				// текущий пользователь не хочет получать свои сообщения
				return false;
			}

			IProjectsUser setting = Utility.Projects.GetSettings(user.Id)
				.FirstOrDefault(x => x.ProjectId == task.ProjectId);

			bool isSet = setting != null && setting.ReciveEMail.HasTheFlag((int)reciveType);

			if (isSet && reciveType == ReciveType.TaskAssigned)
			{
				return task.ExecutorUserId == user.Id;
			}

			return isSet;
		}

		#endregion
	}
}