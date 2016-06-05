using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Common.Exceptions;
using Common.Extentions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Timez.Controllers;
using Timez.Controllers.Base;
using Timez.DAL.DataContext;
using Timez.Entities;

namespace Timez.Test
{
    /// <summary>
    /// Summary description for Main
    /// </summary>
    [TestClass]
    public class Main
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        const string Email0 = "test0@test.ru";
        const string Email1 = "test1@test.ru";
        const string Email2 = "test2@test.ru";
        const string Email3 = "test3@test.ru";
        const string Email4 = "test4@test.ru";

        static UserController _UserController;

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            ClassCleanup();

            _UserController = Base.GetController<UserController>();
        }

        //
        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup]
        public static void ClassCleanup()
        {
            Repositories repositories = new Repositories(ConfigurationManager.ConnectionStrings["TimezConnectionString"].ConnectionString);
            try { repositories.Users.Delete(Email3); }
            catch { }
            try { repositories.Users.Delete(Email4); }
            catch { }
            try { repositories.Users.Delete(Email1); }
            catch { }
            try { repositories.Users.Delete(Email2); }
            catch { }
            try { repositories.Users.Delete(Email0); }
            catch { }

            repositories.Boards.DeleteEmpty();
            if (_UserController != null)
                _UserController.Dispose();
        }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void MainTest()
        {
            ViewResult result;
            RedirectToRouteResult redirectToRouteResult;
            FormCollection collection = new FormCollection();

            #region Регистрация/Атовризация

            IUser user0 = Registation(Email0);
            IUser user2 = Registation(Email2, out result, out redirectToRouteResult, null);
            IUser user1 = Registation(Email1, out result, out redirectToRouteResult, null);

            #endregion

            #region Создание доски
            BoardsController boardsController = Base.GetController<BoardsController>();
            collection.Clear();
            collection.Add("name", "test");
            collection.Add("description", "description");
            redirectToRouteResult = (RedirectToRouteResult)boardsController.Create(collection);
            int boardId = redirectToRouteResult.RouteValues["Id"].To<int>();
            var board = boardsController.Utility.Boards.Get(boardId);
            Assert.IsNotNull(board);
            #endregion

            #region Проверяем статусы

            TasksStatusesController tasksStatusesController = Base.GetController<TasksStatusesController>();
			PartialViewResult resultBase = tasksStatusesController.List(boardId);
            IEnumerable<ITasksStatus> statuses = (IEnumerable<ITasksStatus>)resultBase.Model;
            Assert.AreEqual(statuses.First().Name, "Беклог");

            tasksStatusesController = Base.GetController<TasksStatusesController>();
			resultBase = tasksStatusesController.Edit(boardId,
			                                          new TimezStatus
			                                          	{
			                                          		Name = "test",
			                                          		BoardId = boardId
			                                          	});

            statuses = (IEnumerable<ITasksStatus>)resultBase.Model;
            Assert.AreEqual(statuses.ElementAt(statuses.Count() - 1).Name, "test");

            #endregion

            #region проверяем задачи

            var user = boardsController.Utility.Users.Get(Email1);
            var boardStatusList = boardsController.Utility.Statuses.GetByBoard(board.Id);

            // получаем доску
            var boardData = (ViewResultBase)boardsController.Edit(boardId);
            Assert.IsInstanceOfType(boardData.Model, typeof(IBoard));
            ProjectsController projectController = Base.GetController<ProjectsController>();
            IEnumerable<IProject> projs = (IEnumerable<IProject>)projectController.List(boardId).Model;
            Assert.IsNotNull(projs);

            // получаем цвета доски
            var boardColor = (ViewResultBase)boardsController.EditColorList(boardId);
            Assert.IsInstanceOfType(boardColor.ViewData.Model, typeof(IEnumerable<IBoardsColor>));
            IEnumerable<IBoardsColor> boardColors = boardColor.ViewData.Model as IEnumerable<IBoardsColor>;

            // Создание задачи
            collection.Clear();
            collection["task-id"] = null;
            collection["task-statusid"] = boardStatusList[0].Id.ToString();
            collection["task-userid"] = user.Id.ToString();
            collection["task-name"] = "Тестовая задача";
            collection["task-description"] = "Описание тестовой задачи";

            collection["task-projectsid"] = projs.First().Id.ToString();
            collection["task-colorid"] = boardColors.First().Id.ToString(CultureInfo.InvariantCulture);

            // проверяем создание
            KanbanController kanbanController = Base.GetController<KanbanController>();
			ViewResultBase taskData = (ViewResultBase)kanbanController.TaskPopup(boardId, null, collection);
            ITask task = CheckTask(taskData);
            Assert.IsTrue(task.CreatorUserId == kanbanController.Utility.Authentication.UserId);

            // архивирование
            collection["task-name"] = "Тестовая задача2";
            taskData = (ViewResultBase)kanbanController.TaskPopup(boardId, null, collection);
            ITask task2 = CheckTask(taskData);
            kanbanController.ToArchive(boardId, task2.Id);
            TasksController tasksController = Base.GetController<TasksController>();
            taskData = tasksController.Details(boardId, task2.Id, true);
            Assert.IsTrue((taskData.Model as ITask).Id == task2.Id);
            Assert.IsTrue((bool)taskData.ViewData["isArchive"]);

            // обновляем задачу
            collection.Clear();
            collection["task-id"] = task.Id.ToString();
            ITasksStatus planningStatus = boardStatusList.First(x => x.PlanningRequired);
            collection["task-statusid"] = planningStatus.Id.ToString();
            collection["task-userid"] = user.Id.ToString();
            collection["task-name"] = "Тестовая задачаю. Обновление";
            collection["task-description"] = "Описание тестовой задачи. Обновление";
            collection["task-projectsid"] = projs.First().Id.ToString();
            collection["task-colorid"] = boardColors.ToList()[1].Id.ToString();
            kanbanController.Dispose();
            kanbanController = Base.GetController<KanbanController>();
            var json = (JsonResult)kanbanController.TaskPopup(boardId, task.Id, collection);
            Assert.IsTrue((json.Data as JsonMessage).Message == new PlanningTimeRequered(planningStatus).Message);


            // проверяем изменение цвета 
            kanbanController = Base.GetController<KanbanController>();
            var color = kanbanController.SetColor(task.Id, boardColors.ToList()[0].Id, boardId);
            Assert.AreNotEqual(color, "");

            // проверям установку времени
            kanbanController = Base.GetController<KanbanController>();
            collection.Clear();
            collection["task-forsed-count"] = "true";
            collection["task-forsed-time"] = "false";
#pragma warning disable 168
            JsonResult planningTime = (JsonResult)kanbanController.SetPlanningTime(boardId, task.Id, 180, task.TaskStatusId, collection);
#pragma warning restore 168

            // проверяем обновление статуса
            kanbanController = Base.GetController<KanbanController>();
            collection.Clear();
            collection["task-forsed-count"] = "true";
            collection["task-forsed-time"] = "false";
            kanbanController.UpdateStatus(boardId, task.Id, boardStatusList[0].Id, collection);

            // проверяем исполнителя
            kanbanController = Base.GetController<KanbanController>();
			user = kanbanController.Utility.Users.Get(Email2);
            kanbanController.SetExecutor(task.BoardId, task.Id, user.Id);

            // проверяем проект
            // сначала создадим проект
            projectController.Dispose();
            projectController = Base.GetController<ProjectsController>();
            collection.Clear();
            string projectName = "тестовый проект " + DateTime.Now.Ticks.ToString();
            collection["Name"] = projectName;
            var projectData = (ViewResultBase)projectController.Edit(boardId, null, collection);
            // проверяем создание проекта
            Assert.IsInstanceOfType(projectData.ViewData.Model, typeof(IEnumerable<IProject>));
            var projects = projectData.ViewData.Model as IEnumerable<IProject>;
            var project = projects.SingleOrDefault(p => p.Name == projectName);
            Assert.IsNotNull(project);

            // изменяем проект
            kanbanController = Base.GetController<KanbanController>();
            kanbanController.SetProject(task.BoardId, task.Id, project.Id);

            // переназначение
            ParticipantController participantController = Base.GetController<ParticipantController>();
            collection.Clear();
            collection["participant-id"] = user1.Id.ToString();
            participantController.Tasks(boardId, user0.Id, collection);
            collection["participant-id"] = user2.Id.ToString();
            participantController.Tasks(boardId, user0.Id, collection);
            // удаление
            kanbanController = Base.GetController<KanbanController>();
            kanbanController.DeleteTask(task2.Id, task2.BoardId);
            kanbanController.DeleteTask(task.Id, task.BoardId);

            // проверяем удаление
            kanbanController = Base.GetController<KanbanController>();
            kanbanController.Index(boardId);
            kanbanController = Base.GetController<KanbanController>();
			collection.Clear();
			PartialViewResult kanban = kanbanController.Kanban(boardId, collection);
            Assert.IsInstanceOfType(kanban.ViewData["Tasks"], typeof(IEnumerable<ITask>));
            IEnumerable<ITask> boardTasks = kanban.ViewData["Tasks"] as IEnumerable<ITask>;
            ITask deletedTask = boardTasks.FirstOrDefault(t => t.Id == task.Id);
            Assert.IsNull(deletedTask);

            // Проверка попадания в журнал ошибок и предупреждений
            var logController = Base.GetController<LogController>();
			var logData = (ViewResultBase)logController.Items(boardId, null);

            Assert.IsInstanceOfType(logData.ViewData.Model, typeof(IEnumerable<IEventHistory>));
            var logs = logData.ViewData.Model as IEnumerable<IEventHistory>;
            var boardEvents = logs.Where(p => p.BoardId == boardId);
            Assert.AreNotEqual(boardEvents.Count(), 0);

            Assert.AreNotEqual(boardEvents.Count(e => (e.EventType & EventType.CreateTask) == EventType.CreateTask), 0);
            Assert.AreNotEqual(boardEvents.Count(e => (e.EventType & EventType.Delete) == EventType.Delete), 0);
            Assert.AreNotEqual(boardEvents.Count(e => (e.EventType & EventType.Update) == EventType.Update), 0);
            Assert.AreNotEqual(boardEvents.Count(e => (e.EventType & EventType.TaskColorChanged) == EventType.TaskColorChanged), 0);
            Assert.AreNotEqual(boardEvents.Count(e => (e.EventType & EventType.TaskAssigned) == EventType.TaskAssigned), 0);
            Assert.AreNotEqual(boardEvents.Count(e => (e.EventType & EventType.ProjectChanged) == EventType.ProjectChanged), 0);
            Assert.AreNotEqual(boardEvents.Count(e => (e.EventType & EventType.PlaningTimeChanged) == EventType.PlaningTimeChanged), 0);


            #endregion

            #region Удаление досок
            boardsController.Dispose();
            boardsController = Base.GetController<BoardsController>();
            PartialViewResult partialResult = boardsController.List(null);
            Assert.IsInstanceOfType(partialResult.Model, typeof(List<IBoard>));
            foreach (IBoard item in partialResult.Model as List<IBoard>)
            {
				boardsController = Base.GetController<BoardsController>();
                boardsController.Delete(item.Id);
                try
                {
					board = boardsController.Utility.Boards.Get(item.Id);
                    Assert.IsNull(board);
                }
                catch (Exception ex)
                {
                    Assert.IsInstanceOfType(ex, typeof(AccessDeniedException));
                }
            }
            #endregion

			boardsController.Utility.Users.Delete(Email1);
			Assert.IsNull(boardsController.Utility.Users.Get(Email1));
        }

        public static IUser Registation(string email)
        {
            RedirectToRouteResult redirectToRouteResult;
            ViewResult result;
            return Registation(email, out result, out redirectToRouteResult, null);
        }

        public static IUser Registation(string email, out ViewResult result, out RedirectToRouteResult redirectToRouteResult, string invite)
        {
            FormCollection collection = new FormCollection
            {
                {"email", email},
                {"password", email},
                {"confirmPassword", email},
                {"timezone", "0"}
            };
            var userController = Base.GetController<UserController>();
            result = (ViewResult)userController.Register(invite, null, collection);
            Repositories repositories = new Repositories(ConfigurationManager.ConnectionStrings["TimezConnectionString"].ConnectionString);
            IUser user = repositories.Users.GetByEmail(email);
            userController.Activate(user.ConfimKey);
            repositories.Dispose();
			userController = Base.GetController<UserController>();
            redirectToRouteResult = (RedirectToRouteResult)userController.Login(null, email, email, false, null);

            return user;
        }

        private static ITask CheckTask(ViewResultBase taskData)
        {
            Assert.IsInstanceOfType(taskData.ViewData.Model, typeof(ITask));
            ITask task = taskData.ViewData.Model as ITask;
            Assert.IsNotNull(task);
            Assert.AreNotEqual(task.Id, 0);
            return task;
        }
    }
}

