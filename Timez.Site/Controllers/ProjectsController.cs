using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Common.Extentions;
using Timez.Controllers.Base;
using Timez.Entities;

namespace Timez.Controllers
{
	/// <summary>
	/// Управление проектами
	/// </summary>
	[Authorize]
	public sealed class ProjectsController : BaseController
	{
		#region Board projects

		[Permission(UserRole.Owner)]
		public PartialViewResult List(int id)
		{
			List<IProject> projects = Utility.Projects.GetByBoard(id);
			ViewData.Model = projects;

			int? count = Utility.Tariffs.GetAvailableProjectsCount(id);
			ViewData.Add("AvailableProjectsCount", count);

			return PartialView("List");
		}

		/// <summary>
		/// Редактирование настроек проектов на доске
		/// </summary>
		/// <param name="boardId">доска</param>
		/// <param name="id">ид проекта</param>
		/// <param name="collection"></param>
		[HttpPost]
		[Permission("boardId", UserRole.Owner)]
		public PartialViewResult Edit(int boardId, int? id, FormCollection collection)
		{
			string name = collection["Name"];
			if (id.HasValue)
			{
				Utility.Projects.Update(id.Value, name);
			}
			else
			{
				Utility.Projects.Create(boardId, name, Utility.Users.CurrentUser);
			}

			return List(boardId);
		}

		/// <summary>
		/// Форма редактирования проекта
		/// </summary>
		/// <param name="boardId">доска</param>
		/// <param name="id">ид проекта</param>
		/// <returns>PartialView</returns>
		[Permission("boardId", UserRole.Owner)]
		public PartialViewResult Edit(int boardId, int? id)
		{
			if (id.HasValue)
			{
				IProject project = Utility.Projects.Get(boardId, id.Value);
				ViewData.Model = project;
			}
			return PartialView("Edit");
		}

		[Permission("boardId", UserRole.Owner)]
		public PartialViewResult Delete(int boardId, int id)
		{
			try
			{
				Utility.Projects.Delete(boardId, id);
			}
			catch (NeedProjectException exception)
			{
				ModelState.AddModelError("NeedProjectException", exception);
			}

			return List(boardId);
		}

		#endregion

		#region Users projects
		/// <summary>
		/// Проекты пользователя на всех досках
		/// </summary>
		[HttpGet]
		public PartialViewResult UsersProjects()
		{
			// Все проекты на всех досках
			var boards = Utility.Boards.GetByUser(Utility.Authentication.UserId);
			IEnumerable<IProject> projects = boards
				.SelectMany(x => Utility.Projects.GetByBoard(x.Id))
				.OrderBy(x => x.Name);
			ViewData.Model = projects;

			return PartialView();
		}

		/// <summary>
		/// Сохранение настроек для проекта
		/// Либо переключение текущего проекта
		/// </summary>
		[HttpPost]
		[Permission("boardId", UserRole.Belong)]
		public PartialViewResult Settings(int boardId, FormCollection collection)
		{
			int projId = collection["project-id"].ToInt();

			// Если нажали на Сохранить
			if (collection.AllKeys.Contains("submit"))
			{
				// проверка прав делается вручную, так как в сигнатуру экшена не удобно добавлять boardId
				//CheckAccess(boardId);

				if (ModelState.IsValid)
				{
					// Обновление настроек рассылки по проектам
					bool taskAssigned = collection["taskAssigned"].StartsWith("true");
					bool taskStatusChanged = collection["taskStatusChanged"].StartsWith("true");
					bool taskCreated = collection["taskCreated"].StartsWith("true");

					ReciveType type
						= (taskAssigned ? ReciveType.TaskAssigned : ReciveType.NotDefined)
						  | (taskStatusChanged ? ReciveType.TaskStatusChanged : ReciveType.NotDefined)
						  | (taskCreated ? ReciveType.TaskCreated : ReciveType.NotDefined);

					Utility.Projects.Update(projId, Utility.Authentication.UserId, type);
				}
			}

			return Settings(boardId, projId);
		}

		/// <summary>
		/// Настройки выбранного проекта
		/// </summary>
		[HttpGet]
		[Permission("boardId", UserRole.Belong)]
		public PartialViewResult Settings(int boardId, int id)
		{
			// настройки проекта, могут не сущесвовать, если не разу не задавались
			IProjectsUser settings = Utility.Projects
				.GetSettings(Utility.Authentication.UserId)
				.FirstOrDefault(x => x.ProjectId == id);

			IBoard board = Utility.Boards.Get(boardId);

			//// проверка прав делается вручную, так как в сигнатуру экшена не удобно добавлять boardId
			//CheckAccess(boardId);

			ViewData.Add("Board", board);
			ViewData.Model = settings;

			return PartialView("Settings");
		}

		#endregion
	}
}
