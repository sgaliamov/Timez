using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Common.Extentions;
using Timez.Controllers.Base;
using Timez.Entities;

namespace Timez.Controllers
{
	public class ParticipantController : BaseController
	{
		// TODO: check why HttpGet. if it is nessesary post the comment
		//[HttpGet]
		[Permission(UserRole.Owner)]
		public PartialViewResult List(int id)
		{
			// Настройки пользователей на доске
			List<UserSettings> userSettingses = Utility.Boards.GetParticipants(id);
			ViewData.Model = userSettingses;

			// Остальные сотрудники организации
			IBoard board = Utility.Boards.Get(id);
			if (board.OrganizationId.HasValue)
			{
				List<EmployeeSettings> employees = Utility.Organizations
					.GetEmployees(board.OrganizationId.Value)
					.Where(x => !userSettingses.Any(y => y.User.Id == x.User.Id))
					.ToList();
				ViewData.Add("employees", employees);

				IOrganization organization = Utility.Organizations.Get(board.OrganizationId.Value);
				ViewData.Add("organization", organization);
			}

			return PartialView("List");
		}

		/// <summary>
		/// Форма редактированиея
		/// </summary>
		/// <param name="boardId"></param>
		/// <param name="id">ид пользователя</param>
		/// <returns></returns>
		[HttpGet]
		[Permission("boardId", null, ResultType.JsonError, UserRole.Owner)]
		public PartialViewResult Edit(int boardId, int id)
		{
			UserSettings settings = Utility.Boards.GetUserSettings(boardId, id);
			ViewData.Model = settings;
			return PartialView();
		}

		/// <summary>
		/// Редактирование учасников
		/// id - пользователь
		/// </summary>
		[HttpPost]
		[Permission("boardId", null, ResultType.JsonError, UserRole.Owner)]
		public PartialViewResult Edit(int boardId, int id, FormCollection collection)
		{
			if (collection.AllKeys.Contains("save"))
			{
				bool isActive = collection["IsActive"] != "false";
				Utility.Boards.UpdateUserOnBoard(boardId, id, isActive);

				UserRole role = UserRole.Belong;
				bool isOwner = collection["IsOwner"] != "false";
				if (isOwner)
				{
					role = UserRole.All;
				}
				else
				{
					bool isCustomer = collection["IsCustomer"] != "false";
					bool isObserver = collection["IsObserver"] != "false";
					bool isExecutor = collection["IsExecutor"] != "false";

					if (isCustomer) role |= UserRole.Customer;
					if (isObserver) role |= UserRole.Observer;
					if (isExecutor) role |= UserRole.Executor;
				}

				try
				{
					Utility.Boards.UpdateRole(boardId, id, role);
				}
				catch (NeedAdminException ex)
				{
					ModelState.AddModelError("NeedAdminException", ex);
				}
			}
			else if (collection.AllKeys.Contains("delete"))
			{
				try
				{
					IBoard board = Utility.Boards.Get(boardId);
					Utility.Boards.RemoveUserFromBoard(board, id);
				}
				catch (TasksExistsException ex)
				{
					string message = "Пользователя нельзя исключить с доски." + Environment.NewLine
									 + ex.Message + Environment.NewLine
									 + "Переназначте задачи на другого пользователя.";
					ModelState.AddModelError("TasksExistsException", message);
				}
			}

			return List(boardId);
		}

		[Permission("boardId", UserRole.Owner)]
		public PartialViewResult Add(int boardId, int id)
		{
			Utility.Boards.AddExecutorToBoard(boardId, id);
			return List(boardId);
		}

		/// <summary>
		/// Вью переназначения задач
		/// <param name="id">ид пользователя, чьи задачи</param>
		/// </summary>        
		[HttpGet]
		[Permission("boardId", null, ResultType.JsonError, UserRole.Owner)]
		public PartialViewResult Tasks(int boardId, int id)
		{
			TaskFilter filter = Utility.Tasks.CreateFilter(boardId);
			filter.ExecutorIds = new[] { id };
			List<ITask> tasks = Utility.Tasks.Get(filter);

			List<IUser> users = Utility.Boards
				.GetAllExecutorsOnBoard(boardId)
				.Where(x => x.Id != id)
				.ToList();

			ViewData.Model = users;
			ViewData.Add("TaskCount", tasks.Count);

			return PartialView("Tasks");
		}

		/// <summary>
		/// Переназначение задач между учасниками
		/// <param name="id">ид пользователя, чьи задачи</param>
		/// </summary>        
		[HttpPost]
		[Permission("boardId", null, ResultType.JsonError, UserRole.Owner)]
		public PartialViewResult Tasks(int boardId, int id, FormCollection collection)
		{
			int toUserId = collection["participant-id"].ToInt();
			Utility.Tasks.Reassign(boardId, id, toUserId);
			return List(boardId);
		}
	}
}
