using System;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Web.Mvc;
using System.Web.SessionState;
using Common.Extentions;
using Timez.BLL.Tasks;
using Timez.Controllers.Base;
using Timez.Entities;
using Timez.Utilities;

namespace Timez.Controllers
{
	[SessionState(SessionStateBehavior.Disabled)]
	public sealed class HomeController : BaseController
	{
		//[OutputCache(Duration = CacheDuration)]
		public ViewResult Index()
		{
			ViewBag.LastNewsOnPage = Utility.Settings.LastNewsOnPage;
			return View();
		}

		/// <summary>
		/// Ежедневная профилактика
		/// </summary>
		public string DailyJob()
		{
			// Удаляем неподтвержденных
			IEnumerable<IUser> users = Utility.Users.RemoveUnconfirmed(7);
			string siteUrl = Url.Action("Index", "Home", null, "http");
			foreach (IUser user in users)
			{
				const string message =
					"<p>К сожалению, Ваш аккаунт был удален в связи с не подтвержденной регистрацией. :(</p>" +
					"<p>Но мы будем рады, если Вы повторно зарегистрируетесь на нашем <a href='{0}'>сайте</a>.</p>";
				MailsManager.SendMail(user, "Удаление аккаунта", message.Params(siteUrl));
			}

			return "OK";
		}

		public string MonthlyJob()
		{
			Utility.Invites.RemoveOldInvites(7);

			// TODO: Чистить удаленные задачи, но не те которые в архиве.

			// 1. создать задачу и удалить ее через контекстное меню
			// 2. поместить задачу в архив
			// 3. исключить пользователя из доски
			// 4. останутся задачи привязанные к пользователю, к которым нет доступа ниоткуда. чистить их тоже
			// 5. стирать неподтвержденные ящики

			// очистка пустых организация

			return "OK";
		}

		/// <summary>
		/// Поддержка сайта в актуальном состоянии
		/// </summary>
		public void KeepAlive()
		{
			// TODO: Подкачивать наиболее часто используемуе доски
			//Utility.Users.get
		}

		//// TODO: разобраться для чего функция
		//public ViewResult OAuth2Callback(string id)
		//{
		//    return View(id);
		//}

		static readonly Random _Random = new Random();

		public int FillTestDB(int count, int userId, int? boardId = null)
		{
			IUser user = Utility.Users.Get(userId);
			Utility.Authentication.SignIn(user.Id, false);
			IBoard board = boardId.HasValue
				? Utility.Boards.Get(boardId.Value)
				: Utility.Boards.Create("Test_" + DateTime.Now.ToString(), "", user);
			IProject project = Utility.Projects.GetByBoard(board.Id).First();
			IBoardsColor color = Utility.Boards.GetColors(board.Id).First();
			List<ITasksStatus> statuses = Utility.Statuses.GetByBoard(board.Id);

			for (int i = 0; i < count; i++)
			{
				Utility.Tasks.Create("Test " + i, "", userId, project.Id, color.Id, statuses[_Random.Next(statuses.Count)].Id, board.Id, 60);
			}

			return board.Id;
		}

		public string Test(int boardId, int userId)
		{
			StringBuilder sb = new StringBuilder();
			Stopwatch sw = new Stopwatch();
			sw.Start();

			Utility.Authentication.SignIn(userId, false);
			var participants = Utility.Boards.GetExecutorsToAssing(boardId);
			userId = participants[_Random.Next(participants.Count)].Id;
			Utility.Authentication.SignIn(userId, false);

			if (_Random.Next(1000) < 50)
			{
				int count;
				Utility.Events.Get(new EventDataFilter { BoardId = boardId }, out count);
				sb.AppendLine("Events");
			}

			if (_Random.Next(1000) < 10)
			{
				new CacheService().ClearAll();
				sb.AppendLine("Cache clear");
			}

			List<ITask> tasks = Utility.Tasks.Get(Utility.Tasks.CreateFilter(boardId));
			ITask task = tasks[_Random.Next(tasks.Count)];
			List<ITasksStatus> statuses = Utility.Statuses.GetByBoard(boardId);
			Utility.Tasks.UpdateStatus(task.Id, statuses[_Random.Next(statuses.Count)].Id, Limits.NoLimits);

			if (_Random.Next(1000) < 100)
			{
				IProject project = Utility.Projects.GetByBoard(boardId).First();
				IBoardsColor color = Utility.Boards.GetColors(boardId).First();
				Utility.Tasks.Delete(task);
				Utility.Tasks.Create("Test " + DateTime.Now.ToString(), "", userId, project.Id, color.Id
					, statuses[_Random.Next(statuses.Count)].Id, boardId, 60);
				sb.AppendLine("Create delete");
			}

			sw.Stop();

			sb.AppendLine("Time: " + sw.ElapsedMilliseconds.ToString());

			return sb.ToString().Replace(Environment.NewLine, "<br>");
		}
	}
}
