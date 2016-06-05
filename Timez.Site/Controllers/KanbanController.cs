using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Common.Alias;
using Common.Extentions;
using Timez.BLL.Tasks;
using Timez.Controllers.Base;
using Timez.Entities;
using Timez.Helpers;
using Timez.Utilities;

namespace Timez.Controllers
{
	[Authorize]
	public class KanbanController : BaseController
	{
		readonly KanbanFilterUtility _FilterUtility;

		public KanbanController()
		{
			_FilterUtility = new KanbanFilterUtility(this);
		}

		/// <summary>
		/// Фильтрация
		/// </summary>
		[HttpPost]
		[Permission(UserRole.Executor, UserRole.Customer, UserRole.Observer)]		
		public PartialViewResult Index(int id, FormCollection collection)
		{
			return Kanban(id, collection);
		}

		/// <summary>
		/// Для возврата колонки статуса аяксом
		/// </summary>
		[HttpPost]
		[Permission("boardId", UserRole.Executor, UserRole.Customer, UserRole.Observer)]
		public PartialViewResult StatusTasks(int boardId, int statusId, int? page)
		{
			var filter = _FilterUtility.GetCurrentFilter(boardId, false);
			filter.Statuses = new[] { statusId };

			IEnumerable<ITask> tasks = Utility.Tasks.Get(filter);

			#region Задачи для отображения и пейджинга
			// Пейджинг
			if (!page.HasValue)
				page = 1;

			var pagedTasks = new PagedTasks(page.Value, tasks);
			#endregion

			ViewData.Model = pagedTasks;

			return PartialView("StatusTasks");
		}

		/// <summary>
		/// Для диалога информации о статусе
		/// </summary>
		/// <param name="boardId"></param>
		/// <param name="statusId"></param>
		/// <returns></returns>
		[Permission("boardId", UserRole.Executor, UserRole.Customer, UserRole.Observer)]
		public PartialViewResult StatusInfo(int boardId, int statusId)
		{
			TaskFilter filter = _FilterUtility.GetCurrentFilter(boardId, false);
			filter.Statuses = new[] { statusId };

			var tasks = Utility.Tasks.Get(filter);

			// Планируемое время
			var groups = tasks.GroupBy(x => x.ExecutorNick).Select(x => new
			{
				User = x.Key, // Имя пользователя
				TotalPlanningTime = x.Sum(y => y.PlanningTime ?? 0) // всего запланированно
			});

			var planningTimes = groups.ToDictionary(item => item.User, item => item.TotalPlanningTime);

			ViewData.Model = new StatusInfoData
			{
				TotalTasks = tasks.Count,
				Status = Utility.Statuses.Get(boardId, statusId),
				PlanningTimes = planningTimes
			};

			return PartialView("StatusInfo");
		}

		/// <summary>
		/// Представление доски
		/// Разные части подгружаются сами через рендер экшн
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[Permission(UserRole.Executor, UserRole.Customer, UserRole.Observer)]
		public ViewResult Index(int id)
		{
			ViewData.Model = Utility.Boards.Get(id);

			PreLoad(id);

			return View();
		}

		/// <summary>
		/// Отрисовка самой доски
		/// </summary>
		[Permission(UserRole.Executor, UserRole.Customer, UserRole.Observer)]
		[ChildActionOnly]
		public PartialViewResult Kanban(int id, FormCollection collection)
		{
			#region Получаем данные либо из запроса либо из кук, в зависимости от типа действия

			TaskFilter filter = _FilterUtility.GetCurrentFilter(id, false, collection);
			IEnumerable<int> userIds = filter.ExecutorIds.ToArray();

			if (collection.Count > 0)
			{
				// Сюда попадаем при фильтрации
				// Запоминаем фильтры пользователя
				collection["Statuses"] = string.Empty;
				_FilterUtility.SaveFilterToCookies(id, collection);
			}
			string rawCollapsedStatuses = Cookies.GetFromCookies("CollapsedStatuses");

			// Если просматриваем конкретного пользователя, то и при создании задач используем его. 
			// Этот пользователь текущий
			if (userIds.Count() == 1)
				Cookies.AddToCookie("SelectedUser", userIds.First().ToString(CultureInfo.InvariantCulture), false);

			#endregion

			#region Парсим сырые данные

			// Исключенные статусы
			// исключаем архив, так как в канбане его не показываем
			List<int> excludedStatuses = new List<int>();
			if (!rawCollapsedStatuses.IsNullOrEmpty())
			{
				excludedStatuses.AddRange(rawCollapsedStatuses
					.UrlDecode()
					.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries)
					.Select(x => x.ToInt())
					.ToList());
			}

			// Данные для отображения
			var statuses = Utility.Statuses.GetByBoard(id);

			IEnumerable<int> needTaskStatuses = statuses
				.Select(x => x.Id)
				.Except(excludedStatuses)
				.ToArray();
			filter.Statuses = needTaskStatuses;
			var tasks = Utility.Tasks.Get(filter);

			#endregion

			#region Подготавливаем данные

			foreach (int status in needTaskStatuses)
			{
				string key = "status-page-" + status.ToString(CultureInfo.InvariantCulture);
				int page = Cookies.GetFromCookies(key).TryToInt() ?? 1;
				ViewData.Add(key, page);
			}

			ViewData.Model = statuses;
			ViewData.Add("Tasks", tasks);
			ViewData.Add("CollapsedStatuses", excludedStatuses);

			// Определяем ширину статуса
			// если алгоритм поменяется, поменять в Kanban.js
			float sw = Cookies.GetFromCookies("document.width").To(1280 - 17);
			float hidenCount = excludedStatuses.Count();
			float count = statuses.Count() - hidenCount; // количество развернутых статусов
			sw -= hidenCount * 31;
			float width = sw / count - 1;
			ViewData.Add("StatusWidth", width);

			#endregion

			return PartialView("Kanban");
		}

		/// <summary>
		/// Отрисовка фильтра
		/// </summary>
		/// <param name="id"></param>
		/// <param name="showStatusFilter"></param>
		/// <returns></returns>
		[Permission(UserRole.Executor, UserRole.Customer, UserRole.Observer)]
		[ChildActionOnly]
		public PartialViewResult KanbanFilter(int id, bool showStatusFilter)
		{
			#region Считываем данные из кук если они там есть

			List<int> userIds;
			List<int> projectIds;
			List<int> colorIds;
			List<int> statusIds;
			TasksSortType sortType;
			_FilterUtility.GetCurrentFilter(id, out userIds, out projectIds, out colorIds, out sortType, out statusIds, null);

			#endregion

			#region Подготавливаем данные для представления

			var userData = Utility.Boards.GetAllExecutorsOnBoard(id);
			var usersView = new DropdownCheckList
				{
					SelectList = userData.Select(x => new SelectListItem
					{
						Selected = userIds == null || userIds.Contains(x.Id),
						Text = x.Nick,
						Value = x.Id.ToString()
					}).ToList(),
					Label = "Все исполнители",
					Title = "Исполнители",
					Name = "Users"
				};

			List<IProject> projectsData = Utility.Projects.GetByBoard(id);
			DropdownCheckList projectsView = new DropdownCheckList
				{
					SelectList = projectsData.Select(x => new SelectListItem
					{
						Selected = projectIds == null || projectIds.Contains(x.Id),
						Text = x.Name,
						Value = x.Id.ToString()
					}).ToList(),
					Label = "Все проекты",
					Title = "Проекты",
					Name = "Projects"
				};

			var statusesData = Utility.Statuses.GetByBoard(id);
			var statusView = new DropdownCheckList
			{
				SelectList = statusesData.Select(x => new SelectListItem
				{
					Selected = statusIds == null || statusIds.Contains(x.Id),
					Text = x.Name,
					Value = x.Id.ToString()
				}).ToList(),
				Label = "Все статусы",
				Title = "Статусы",
				Name = "Statuses"
			};

			var colorsData = Utility.Boards.GetColors(id);
			var colorsView = new DropdownCheckList
				 {
					 SelectList = colorsData.Select(x => new SelectListItem
					 {
						 Selected = colorIds == null || colorIds.Contains(x.Id),
						 Text = x.Name,
						 Value = x.Id.ToString()
					 }).ToList(),
					 Label = "Все приоритеты",
					 Title = "Приоритеты",
					 Name = "Colors"
				 };

			var sortTypeView = new List<SelectListItem>();
			foreach (TasksSortType item in Enum.GetValues(typeof(TasksSortType)))
			{
				if (!showStatusFilter && item == TasksSortType.ByStatus)
					continue;

				sortTypeView.Add(new SelectListItem
				{
					Text = item.GetAlias(),
					Value = ((int)item).ToString(),
					Selected = item == sortType
				});
			}

			#endregion

			ViewData.Add("Users", usersView);
			ViewData.Add("Projects", projectsView);
			ViewData.Add("Colors", colorsView);
			ViewData.Add("SortTypeView", sortTypeView);
			ViewData.Add("StatusesView", statusView);
			return PartialView("KanbanFilter");
		}

		/// <summary>
		/// Все операции над задачами должны проходить через эту функцию, 
		/// так как нужно обрабатывать разные эксепшены
		/// </summary>
		/// <param name="taskId"></param>
		/// <param name="statusId"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		ActionResult TaskAction(int? taskId, int? statusId, Func<ActionResult> action)
		{
			try
			{
				return action();
			}
			catch (TimezException ex)
			{
				return JsonError(ex.Message, new { TaskId = taskId, NewStatusId = statusId }, ex.GetType().Name);
			}
		}

		#region Контекстное меню

		// TODO: операции должны быть болько постами, никаких изменений данных через геты!!!

		/// <summary>
		/// Обновляет статус задачи
		/// </summary>
		[Permission("boardId", "taskId", ResultType.JsonError, UserRole.Executor, UserRole.Customer)]
		public ActionResult UpdateStatus(int boardId, int taskId, int statusId, FormCollection collection)
		{
			return TaskAction(taskId, statusId, delegate
			{
				// Проверяем ограничения на количество задач
				Limits limits = LimitsToCheck(collection);
				ITask task = Utility.Tasks.UpdateStatus(taskId, statusId, limits);
				ViewData.Model = task;

				return PartialView("Task");
			});
		}

		/// <summary>
		/// Установка планируемого времени
		/// </summary>
		/// <param name="time"></param>
		/// <param name="statusId">Нужен, что бы проверять а не преувеличивает ли допустимое время в новом статусе</param>
		/// <param name="boardId"></param>
		/// <param name="taskId"></param>
		/// <param name="collection"></param>
		/// <returns></returns>
		[Permission("boardId", "taskId", ResultType.JsonError, UserRole.Customer)]
		public ActionResult SetPlanningTime(int boardId, int taskId, int time, int statusId, FormCollection collection)
		{
			return TaskAction(taskId, statusId, delegate
			{
				Limits limits = LimitsToCheck(collection);
				Utility.Tasks.SetPlanningTime(boardId, statusId, taskId, time, limits);
				return new JsonResult();
			});
		}

		[Permission("boardId", "taskId", ResultType.JsonError, UserRole.Executor)]
		public ActionResult ToArchive(int boardId, int taskId)
		{
			return TaskAction(taskId, null, delegate
			{
				ITask task = Utility.Tasks.Get(boardId, taskId);
				Utility.Tasks.ToArchive(task);
				return null;
			});
		}

		/// <summary>
		/// Исполнитель
		/// </summary>
		[Permission("boardId", "taskId", ResultType.JsonError, UserRole.Executor, UserRole.Customer)]
		public ActionResult SetExecutor(int boardId, int taskId, int userId)
		{
			return TaskAction(taskId, null, delegate
			{
				IUser user = Utility.Tasks.SetExecutor(taskId, userId);
				return Content(user != null
					? "<a href='mailto:" + user.EMail + "' target='_blank'>" + user.Nick + "</a>"
					: string.Empty);
			});
		}

		[Permission("boardId", "taskId", ResultType.JsonError, UserRole.Executor, UserRole.Customer)]
		public ActionResult SetColor(int taskId, int colorId, int boardId)
		{
			return TaskAction(taskId, null, delegate
			{
				IBoardsColor color = Utility.Tasks.SetColor(taskId, colorId, boardId);
				return Content(color != null ? color.Color : string.Empty);
			});
		}

		[Permission("boardId", "taskId", ResultType.JsonError, UserRole.Executor, UserRole.Customer)]
		public ActionResult SetProject(int boardId, int taskId, int projectId)
		{
			return TaskAction(taskId, null, delegate
			{
				IProject prj = Utility.Tasks.SetProject(taskId, projectId, boardId);
				return Content(prj != null ? prj.Name : string.Empty);
			});
		}

		[Permission("boardId", "taskId", ResultType.JsonError, UserRole.Owner, UserRole.Customer)]
		public void DeleteTask(int taskId, int boardId)
		{
			TaskAction(taskId, null, delegate
			{
				ITask task = Utility.Tasks.Get(boardId, taskId);
				Utility.Tasks.Delete(task);
				return null;
			});
		}

		/// <summary>
		/// Подменю задач
		/// </summary>
		/// <param name="id">ид доски</param>
		/// <returns></returns>
		[Permission(ResultType.Empty, UserRole.Executor, UserRole.Customer)]
		[ChildActionOnly]
		public PartialViewResult TaskSubMenu(int id)
		{
			var statuses = Utility.Statuses.GetByBoard(id);

			ViewData.Add("Statuses", statuses);
			ViewData.Add("Colors", Utility.Boards.GetColors(id));
			ViewData.Add("Projects", Utility.Projects.GetByBoard(id));
			ViewData.Add("Users", Utility.Boards.GetExecutorsToAssing(id));

			return PartialView("TaskSubMenu");
		}

		#endregion

		#region Попап редактирования задачи

		/// <summary>
		/// Добавляет в ViewData значение key из кук
		/// Нужно для попапа задач, что бы не выбирались не существующие на доске иды
		/// </summary>
		/// <param name="key"></param>
		/// <param name="collection"></param>
		/// <param name="defaultId"></param>
		void AddSelectedFromCookie(string key, IEnumerable<IId> collection, Func<int> defaultId)
		{
			var cookieId = Cookies.GetFromCookies(key).ToNullable<int>();
			if (cookieId.HasValue && collection.Any(x => x.Id == cookieId))
			{
				ViewData.Add(key, cookieId);
			}
			else
			{
				ViewData.Add(key, defaultId());
			}
		}

		/// <summary>
		/// Представление для попапа задачи
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[Permission(ResultType.Empty, UserRole.Executor, UserRole.Customer)]
		[ChildActionOnly]
		public PartialViewResult TaskPopup(int id)
		{
			List<IUser> users = Utility.Boards.GetExecutorsToAssing(id);
			List<IBoardsColor> colors = Utility.Boards.GetColors(id);
			List<IProject> projs = Utility.Projects.GetByBoard(id);
			List<ITasksStatus> statuses = Utility.Statuses.GetByBoard(id); // Статус не достаем из кук, так как он определяется нажатием на + в интерфейсе

			AddSelectedFromCookie("SelectedColor", colors, () => colors.First(x => x.IsDefault).Id);
			AddSelectedFromCookie("SelectedUser", users, () => Utility.Authentication.UserId);
			AddSelectedFromCookie("SelectedProject", projs, () => projs.First().Id);

			ViewData.Add("BacklogId", statuses.First(x => x.IsBacklog).Id);
			ViewData.Add("UsersCollection", users);
			ViewData.Add("ProjectsCollection", projs);
			ViewData.Add("ColorsCollection", colors);
			ViewData.Add("StatusCollection", statuses);

			return PartialView("TaskPopup");
		}

		/// <summary>
		/// Создание обновление задачи через попап
		/// </summary>
		/// <param name="id">Ид доски</param>
		/// <param name="taskId"></param>
		/// <param name="collection"></param>
		/// <returns></returns>
		[HttpPost, ValidateInput(false)]
		[Permission("id", "taskId", ResultType.JsonError, UserRole.Executor, UserRole.Customer)]
		public ActionResult TaskPopup(int id, int? taskId, FormCollection collection)
		{
			taskId = collection["task-id"].ToNullable<int>();
			int statusId = collection["task-statusid"].ToInt();

			return TaskAction(taskId, statusId, delegate
			{
				int userId = collection["task-userid"].ToInt();
				string name = collection["task-name"];
				string desc = collection["task-description"];

				int projectId = collection["task-projectsid"].ToInt();
				int colorId = collection["task-colorid"].ToInt();
				int planingHours = collection["task-planning-hours"].TryToInt(0) * 60;
				int planingMinutes = collection["task-planning-minutes"].TryToInt(0);

				int? planingTime = planingHours + planingMinutes;
				if (planingTime <= 0)
					planingTime = null;

				// Статус не запоминаем куках, так как он определяется нажатием на + в интерфейсе
				Cookies.AddToCookie("SelectedProject", projectId.ToString());
				Cookies.AddToCookie("SelectedUser", userId.ToString());
				Cookies.AddToCookie("SelectedColor", colorId.ToString());

				// Какие лимиты нужно проверять
				Limits limits = LimitsToCheck(collection);

				#region Создание/Редактирование

				ITask task;
				if (taskId.HasValue)
				{
					limits |= Limits.PopupUpdating;

					// Обновляем задачу
					task = Utility.Tasks.Update(
						id,
						taskId.Value,
						name,
						desc,
						userId,
						projectId,
						colorId,
						planingTime,
						limits);

					Utility.Tasks.UpdateStatus(task.Id, statusId, limits);
				}
				else
				{
					// Создаем задачу
					task = Utility.Tasks.Create(name, desc, userId, projectId, colorId, statusId, id, planingTime, limits);
				}

				ViewData.Model = task;
				return PartialView("Task");

				#endregion
			});
		}

		/// <summary>
		/// Какие лимиты нужно проверять
		/// </summary>
		/// <param name="collection"></param>
		/// <returns></returns>
		private static Limits LimitsToCheck(FormCollection collection)
		{
			// Проеряем флаги принудительного редактирования
			bool checkCountLimits = collection["task-forsed-count"] != "true";
			bool checkTimeLimits = collection["task-forsed-time"] != "true";

			Limits limits = Limits.NoLimits;
			if (checkCountLimits)
				limits |= Limits.TaskCountLimitIsReached;
			if (checkTimeLimits)
				limits |= Limits.PlanningTimeIsExceeded;

			return limits;
		}

		/// <summary>
		/// Инфа о задачи в JSON
		/// </summary>
		/// <param name="id"></param>
		/// <param name="boardId"></param>
		/// <returns></returns>
		[Permission("boardId", "id", ResultType.JsonError, UserRole.Executor, UserRole.Customer)]
		public JsonResult Task(int boardId, int id)
		{
			ITask task = Utility.Tasks.Get(boardId, id);
			//Передаем только необходимые данные
			var data = new
			{
				task.Id,
				task.Name,
				task.ColorId,
				task.Description,
				task.ExecutorUserId,
				task.PlanningTime,
				task.ProjectId,
				task.TaskStatusId
			};
			return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		#endregion
	}
}
