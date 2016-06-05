using System;
using System.Collections.Generic;
using System.Threading;
using Common.Extentions;
using System.Linq;
using System.Web.Mvc;
using Timez.BLL;
using Timez.Entities;
using Timez.Helpers;
using Timez.Services;
using Timez.Utilities;

namespace Timez.Controllers.Base
{
	/// <summary>
	/// Базовый класс для контроллеров
	/// </summary>
	//[HandleError] добавляется в Global.asax
	public abstract class BaseController : Controller
	{
		#region Инициализация

		protected BaseController() 
		{
			Cookies = new CookiesService();
			Utility = new UtilityManager(new CacheService(),  new AuthenticationService(), new SettingsService());
		}		

		protected override void Initialize(System.Web.Routing.RequestContext requestContext)
		{
			base.Initialize(requestContext);

			MailsManager = new MailService(Utility, Url);
		}

		/// <summary>
		/// Доступ к модели
		/// </summary>
		public UtilityManager Utility{get;set;}

		public MailService MailsManager { get; set; }

		/// <summary>
		/// Куки используются для интерфейса, по этому не включены в UtilityManager
		/// </summary>
		public ICookiesService Cookies { get; set; }

		#endregion

		#region Переопределения базовых методов

		// TODO: переделать на фильтры

		/// <summary>
		/// Преред рендерингом
		/// </summary>
		/// <param name="filterContext"></param>
		protected override void OnResultExecuting(ResultExecutingContext filterContext)
		{
			// User's data are needed almost everywhere
			if (Utility.Authentication.IsAuthenticated && Utility.Users.CurrentUser != null)
			{
				ViewData.Add("TimeZone", TimeSpan.FromMinutes(Utility.Users.CurrentUser.TimeZone));
			}

			base.OnResultExecuting(filterContext);
		}

		/// <summary>
		/// Перед выполнением экшена
		/// </summary>
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			ResultType? resultType = НasAccessToOrganization(filterContext) ?? НasAccessToBoard(filterContext);

			#region Нет доступа
			if (resultType.HasValue)
			{
				switch (resultType.Value)
				{
					case ResultType.View:
						filterContext.Result = Message("Не достаточно прав.");
						break;

					case ResultType.JsonError:
						filterContext.Result = JsonError("Нет прав на выполнение операции.", null, "AccessDenied");
						break;

					case ResultType.String:
						filterContext.Result = PartialView("String", "Нет прав на выполнение операции.");
						break;

					case ResultType.Empty:
						filterContext.Result = new EmptyResult();
						break;

					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			#endregion

			base.OnActionExecuting(filterContext);
		}

		/// <summary>
		/// Проверка прав в организации
		/// </summary>
		private ResultType? НasAccessToOrganization(ActionExecutingContext filterContext)
		{
			OrganizationPermissionAttribute attribute = (OrganizationPermissionAttribute)filterContext
						.ActionDescriptor
						.GetCustomAttributes(typeof(OrganizationPermissionAttribute), false)
						.FirstOrDefault();

			if (attribute != null)
			{
				int? organizationId = (int?)filterContext.ActionParameters[attribute.IdParamName];
				if (organizationId.HasValue)
				{
					ViewData.Add("CurrentOrganizationId", organizationId.Value);

					EmployeeSettings employeeSettings = Utility.Organizations
						.GetUserSettings(organizationId.Value, Utility.Authentication.UserId);
					if (employeeSettings == null)
						return attribute.ResultType;

					EmployeeRole userRole = employeeSettings.Settings.GetUserRole();
					ViewData.Add("RoleInOrganization", userRole);

					bool hasAccess = false;
					foreach (EmployeeRole roles in attribute.Roles)
					{
						hasAccess |= userRole.HasTheFlag(roles);
						if (hasAccess)
							break;
					}

					return hasAccess ? (ResultType?)null : attribute.ResultType;
				}
			}

			return null;
		}

		/// <summary>
		/// Проверка прав на доске
		/// </summary>
		private ResultType? НasAccessToBoard(ActionExecutingContext filterContext)
		{
			// TODO: плохой дизайн
			// 1. логика сильно зависит от контекста

			PermissionAttribute attribute = (PermissionAttribute)filterContext
						.ActionDescriptor
						.GetCustomAttributes(typeof(PermissionAttribute), false)
						.FirstOrDefault();

			if (attribute != null)
			{
				int? boardId = (int?)filterContext.ActionParameters[attribute.BoardIdParamName];
				ViewData.Add("CurrentBoardId", boardId);
				if (boardId.HasValue)
				{
					int? taskId = !attribute.TaskIdParamName.IsNullOrEmpty()
									  ? (int?)filterContext.ActionParameters[attribute.TaskIdParamName]
									  : null;
					if (taskId.HasValue)
						ViewData.Add("CurrentTaskId", taskId.Value);

					bool? isArchive = !attribute.IsArchiveParamName.IsNullOrEmpty()
										  ? (bool?)filterContext.ActionParameters[attribute.IsArchiveParamName]
										  : null;
					if (isArchive.HasValue)
						ViewData.Add("CurrentTaskInArchive", isArchive.Value);

					UserSettings settings = Utility.Boards.GetUserSettings(boardId.Value, Utility.Authentication.UserId);
					if (settings == null)
						return attribute.Type;

					ViewData.Add("UserRole", settings.Settings.UserRole);

					bool hasAccess = false;
					foreach (UserRole roles in attribute.Roles)
					{
						hasAccess |= CheckUsersRoles(boardId.Value, taskId, isArchive, settings.Settings,
																		 roles);
						if (hasAccess)
							break;
					}

					return hasAccess
							   ? (ResultType?)null
							   : attribute.Type;
				}
			}

			return null;
		}

		/// <summary>
		/// Проперка разрешений на доске
		/// </summary>
		/// <param name="boardId">ид доски</param>
		/// <param name="taskId">ид задачи</param>
		/// <param name="roles">требуемые привелегии</param>
		/// <param name="isArchive">архивность задачи, имеет значение при taskId != null</param>
		/// <param name="settings">имеющиеся права</param>
		/// <returns>true - требуемые разрешения есть</returns>
		public bool CheckUsersRoles(int boardId, int? taskId, bool? isArchive, IBoardsUser settings, UserRole roles)
		{
			bool? taskAccess = null;
			if (taskId.HasValue)
			{
				ITask task = isArchive.HasValue && isArchive.Value
					? Utility.Tasks.GetFromArchive(taskId.Value)
					: Utility.Tasks.Get(boardId, taskId.Value);

				if (settings.UserRole.HasTheFlag((int)UserRole.Owner))
					taskAccess = true;

				else if (((int)roles & settings.UserRole).HasTheFlag((int)UserRole.Customer))
					taskAccess = task.CreatorUserId == settings.UserId;

				else if (((int)roles & settings.UserRole).HasTheFlag((int)UserRole.Executor))
					taskAccess = task.ExecutorUserId == settings.UserId;

				else
					taskAccess = false;
			}

			return (!taskAccess.HasValue || taskAccess.Value)
				&& settings.IsActive
				&& settings.UserRole.HasTheFlag((int)roles);
		}

		/// <summary>
		/// Перед освобождением ресурсов
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (Utility != null)
			{
				Utility.Dispose();
				Utility = null;
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Хелперы

		/// <summary>
		/// Предпрогрузка в отдельном потоке
		/// </summary>
		protected void PreLoad(int boardId)
		{
			if (!Utility.Tasks.AllCached(boardId))
			{
				//PreloadInfo info = new PreloadInfo
				//                    {
				//                        BoardId = boardId,
				//                        UtilityManager = Utility
				//                    };
				//ThreadPool.QueueUserWorkItem(
				//    stateInfo =>
				//    {
				//        PreloadInfo preloadInfo = (PreloadInfo)stateInfo;
				//        preloadInfo.UtilityManager.Tasks.Preload(preloadInfo.BoardId);
				//    }, info);
				//class PreloadInfo
				//{
				//    public int BoardId;
				//    public UtilityManager UtilityManager;
				//}
				ThreadPool.QueueUserWorkItem(
					stateInfo =>
					{
						int id = (int)stateInfo;
						Utility.Tasks.Preload(id);
					}, boardId);
			}
		}

		protected JsonResult JsonError(string message, object data, string errorType)
		{
			return new JsonResult
			{
				JsonRequestBehavior = JsonRequestBehavior.AllowGet,
				Data = new JsonMessage
				{
					Error = new
					{
						Type = errorType
					},
					Data = data,
					Message = message
				}
			};
		}

		/// <summary>
		/// Возвращает представление сообщения
		/// </summary>
		protected ViewResult Message(string message)
		{
			// (object) нужно указыать явно, так как есть другая сигнатура со стрингами
			return View("Message", (object)message);
		}

#if DEBUG
		public const int CacheDuration = 1;
#else
		public const int CacheDuration = 3600;
#endif


		/// <summary>
		/// Преобразовывает в список интов
		/// В строке находятся иды через запятую
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		internal List<int> GetIds(string data)
		{
			if (data == null)
				return null;

			if (data == string.Empty)
				return new List<int>();

			return data.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
						.Select(x => x.ToInt())
						.ToList();
		}

		/// <summary>
		/// Получение массива значений из чекбоксов на форме
		/// </summary>
		/// <returns></returns>
		internal bool[] GetChecked(FormCollection collection, string name)
		{
			collection[name] = collection[name].Insert(0, ",");
			return collection[name]
				.Split(new[] { "false" }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Trim(',') == "true")
				.ToArray();
		}

		#endregion
	}
}