using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Common.Extentions;
using Timez.Controllers.Base;
using Timez.Entities;

namespace Timez.Controllers
{
	/// <summary>
	/// Статусы задач
	/// </summary>
	[Authorize]
	public sealed class TasksStatusesController : BaseController
	{
		[Permission(UserRole.Owner)]
		public PartialViewResult Index(int id)
		{
			ViewData.Model = id;

			return PartialView();
		}

		/// <summary>
		/// Редактирование статусов
		/// </summary>
		/// <param name="boardId"></param>
		/// <param name="id">ид статуса</param>
		/// <returns></returns>
		[Permission("boardId", null, UserRole.Owner)]
		public PartialViewResult Edit(int boardId, int? id)
		{
			ViewData.Model = id.HasValue
				? Utility.Statuses.Get(boardId, id.Value)
				: new TimezStatus();
			return PartialView();
		}

		[HttpPost]
		[Permission("boardId", null, UserRole.Owner)]
		public PartialViewResult Edit(int boardId, TimezStatus status)
		{
			if (ModelState.IsValid)
			{
				if (status.Id > 0)
				{
					Utility.Statuses.Update(
						status.Id,
						status.Name,
						status.PlanningRequired,
						status.MaxTaskCountPerUser,
						status.MaxPlanningTime
					);
				}
				else
				{
					Utility.Statuses.Create(
						status.BoardId,
						status.Name,
						status.PlanningRequired,
						status.MaxTaskCountPerUser,
						status.MaxPlanningTime
					);
				}
			}

			return List(boardId);
		}

		[Permission(UserRole.Owner)]
		public PartialViewResult List(int id)
		{
			ViewData.Model = Utility.Statuses.GetByBoard(id);
			ViewData.Add("BoardId", id);
			return PartialView("List");
		}

		/// <summary>
		/// Редактирование статусов на доске
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[Permission(UserRole.Owner)]
		public void List(int id, FormCollection collection)
		{
			// Сортируем
			if (!collection["StatuesOrder"].IsNullOrEmpty())
			{
				List<int> newOrder = collection["StatuesOrder"].Replace("[]=status_", "")
					.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(o => o.ToInt())
					.ToList(); // Исключаем удаленных
				Utility.Statuses.SetOrder(id, newOrder);
			}
		}

		/// <summary>
		/// Удаление статуса
		/// </summary>
		/// <param name="boardId"></param>
		/// <param name="id">ид статуса</param>
		/// <returns></returns>
		[Permission("boardId", null, UserRole.Owner)]
		public PartialViewResult Delete(int boardId, int id)
		{
			Utility.Statuses.Delete(id);
			return List(boardId);
		}

		/// <summary>
		/// Архивирование задач в статусе
		/// </summary>
		/// <param name="boardId"></param>
		/// <param name="id">ид статуса</param>
		/// <returns></returns>
		[Permission("boardId", null, UserRole.Owner)]
		public string Archive(int boardId, int id)
		{
			try
			{
				Utility.Statuses.Archive(boardId, id);
			}
			catch (TimezException ex)
			{
				return ex.Message;
			}

			return string.Empty;
		}
	}
}
