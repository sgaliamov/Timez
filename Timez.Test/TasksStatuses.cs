using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Timez.Controllers;
using Timez.DAL.DataContext;
using Timez.Entities;

namespace Timez.Test
{
	/// <summary>
	/// Summary description for Main
	/// </summary>
	[TestClass]
	public class TasksStatuses
	{
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		const string Email0 = "test0@test.ru";

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize]
		public static void ClassInitialize(TestContext testContext)
		{
			ClassCleanup();
		}

		//
		// Use ClassCleanup to run code after all tests in a class have run
		[ClassCleanup]
		public static void ClassCleanup()
		{
			Repositories repositories = new Repositories(ConfigurationManager.ConnectionStrings["TimezConnectionString"].ConnectionString);
			IUser user = repositories.Users.GetByEmail(Email0);
			if (user != null)
			{
				var boards = repositories.Boards.GetByUserId(user.Id).ToArray();
				foreach (IBoard board in boards)
				{
					repositories.Tasks.ClearArchive(board.Id);
					var tasks = repositories.Tasks.GetAll(board.Id).ToArray();
					foreach (ITask task in tasks)
					{
						repositories.Tasks.Delete(task.Id);
					}
				}

				repositories.Users.Delete(Email0);
			}
		}

		#endregion

		[TestMethod]
		public void Test()
		{
			FormCollection collection = new FormCollection();

			IUser user = Main.Registation(Email0);

			// доски
			BoardsController boardsController = Base.GetController<BoardsController>();
			PartialViewResult result = boardsController.List(null);
			IBoard board = (result.Model as List<IBoard>).First();

			// статусы
			TasksStatusesController statusesController = Base.GetController<TasksStatusesController>();
			result = statusesController.List(board.Id);
			List<ITasksStatus> statuses = result.Model as List<ITasksStatus>;

			// проекты
			ProjectsController projectsController = Base.GetController<ProjectsController>();
			result = projectsController.List(board.Id);
			List<IProject> projects = result.Model as List<IProject>;

			result = boardsController.EditColorList(board.Id);
			List<IBoardsColor> colors = result.Model as List<IBoardsColor>;

			KanbanController kanbanController = Base.GetController<KanbanController>();

			collection["task-id"] = null;
			collection["task-statusid"] = statuses.First().Id.ToString();
			collection["task-userid"] = user.Id.ToString();
			collection["task-name"] = "Тестовая задача";
			collection["task-description"] = "Описание тестовой задачи";

			collection["task-projectsid"] = projects.First().Id.ToString();
			collection["task-colorid"] = colors.First().Id.ToString();

			for (int i = 0; i < 100; i++)
			{
				kanbanController.TaskPopup(board.Id, null, collection);
			}

			Stopwatch sw = new Stopwatch();
			sw.Start();
			statusesController.Archive(board.Id, statuses.First().Id);
			sw.Stop();
			Trace.WriteLine("Time: " + sw.ElapsedMilliseconds.ToString());
		}
	}
}

