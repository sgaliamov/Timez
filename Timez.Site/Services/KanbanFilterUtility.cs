using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Common.Extentions;
using Timez.BLL;
using Timez.Controllers.Base;
using Timez.Entities;

namespace Timez.Utilities
{
	/// <summary>
	/// методы работы с фильтром задач
	/// </summary>
	sealed class KanbanFilterUtility
	{
		private readonly BaseController _Controller;
		private UtilityManager Utility { get { return _Controller.Utility; } }

		internal KanbanFilterUtility(BaseController controller)
		{
			_Controller = controller;
		}

		#region Получение из коллекции или кук

		/// <summary>
		/// Получение значений фильтра из кук или коллекции формы
		/// Все для текущей доски, кроме тех кто в куках сохранен как снятый
		/// </summary>
		internal void GetCurrentFilter(int boardId, out List<int> userIds, out List<int> projectIds, out List<int> colorIds, out TasksSortType sortType, out List<int> statusIds, FormCollection collection)
		{
			userIds = GetChecked(collection, "Users", () => Utility.Boards.GetParticipants(boardId).Select(x => x.User.Id));
			projectIds = GetChecked(collection, "Projects", () => Utility.Projects.GetByBoard(boardId).Select(x => x.Id));
			colorIds = GetChecked(collection, "Colors", () => Utility.Boards.GetColors(boardId).Select(x => x.Id));
			statusIds = GetChecked(collection, "Statuses", () => Utility.Statuses.GetByBoard(boardId).Select(x => x.Id));

			// Для типа сортировки логика получения сохраненного значения простая
			string rawSortType = collection != null && collection["SortType"] != null
				? collection["SortType"]
				: _Controller.Cookies.GetFromCookies("SortType");

			sortType = rawSortType.IsNullOrEmpty() ? TasksSortType.ByColor : (TasksSortType)rawSortType.TryToInt((int)TasksSortType.ByColor);
		}

		/// <summary>
		/// Получение значений фильтра из кук или коллекции формы
		/// Все для текущей доски, кроме тех кто в куках сохранен как снятый
		/// </summary>
		internal TaskFilter GetCurrentFilter(int boardId, bool fillStatuses = true, FormCollection collection = null)
		{
			List<int> userIds;
			List<int> projectIds;
			List<int> colorIds;
			List<int> statusIds;
			TasksSortType sortType;
			string search = collection != null ? collection["Search"] : null;
			GetCurrentFilter(boardId, out userIds, out projectIds, out colorIds, out sortType, out statusIds, collection);

			TaskFilter filter = Utility.Tasks.CreateFilter(boardId);
			filter.ExecutorIds = userIds;
			filter.ProjectIds = projectIds;
			filter.ColorIds = colorIds;
			filter.SortType = sortType;
			if (fillStatuses)
				filter.Statuses = statusIds;
			filter.Search = search;

			return filter;
		}

		/// <summary>
		/// Выбранные в коллекции значения
		/// </summary>
		/// <param name="collection"></param>
		/// <param name="name"></param>
		/// <param name="getAllIds">Что бы получить выбранные на основе исключенных</param>
		/// <returns></returns>
		private List<int> GetChecked(FormCollection collection, string name, Func<IEnumerable<int>> getAllIds)
		{
			if (collection != null && collection.Count != 0 && collection["X-Requested-With"] != null)
			{
				// Происходит аякс запрос
				if (collection[name] == null)
				{
					// Возвращаем пустой список, так как чеклист ничего не передает, когда ничего не выбрано
					return new List<int>();
				}

				// Происходит фильтрация
				return _Controller.GetIds(collection[name]);
			}

			// В случаи первой загрузки из кук берем исключенные иды коллекции и на основе getAllIds
			// восстанавливаем выбранные
			var allIds = getAllIds().ToList();

			// Тут неотмеченные, нужно получить остальных на доске
			string rawStr = _Controller.Cookies.GetFromCookies("Except" + name);
			if (!rawStr.IsNullOrEmpty())
			{
				var nonChecked = _Controller.GetIds(rawStr);

				// исключяем неотмеченных
				return nonChecked == null
						   ? allIds
						   : allIds.Except(nonChecked).ToList();
			}

			return allIds;
		}

		#endregion

		#region Сохранение в куках

		/// <summary>
		/// Сохраняет фильтр в куках на основе коллекции формы        
		/// </summary>
		internal void SaveFilterToCookies(int boardId, FormCollection collection)
		{
			if (collection.Count > 0)
			{
				if (collection["SortType"] != null)
					_Controller.Cookies.AddToCookie("SortType", collection["SortType"]);

				// на самом деле сохраняет тех кто не отмечен
				AddNonCheckedToCookies(collection, "Colors", () => Utility.Boards.GetColors(boardId).Select(x => x.Id));
				AddNonCheckedToCookies(collection, "Projects", () => Utility.Projects.GetByBoard(boardId).Select(x => x.Id));
				AddNonCheckedToCookies(collection, "Users", () => Utility.Boards.GetParticipants(boardId).Select(x => x.User.Id));
				AddNonCheckedToCookies(collection, "Statuses", () => Utility.Statuses.GetByBoard(boardId).Select(x => x.Id));

			}
		}

		/// <summary>
		/// Добавление в куки неотмеченных name из collection
		/// </summary>
		/// <param name="collection">коллекция отмеченных</param>
		/// <param name="name">название коллекции</param>
		/// <param name="getAllIds">метод получение всех идов</param>
		private void AddNonCheckedToCookies(FormCollection collection, string name, Func<IEnumerable<int>> getAllIds)
		{
			var allIds = getAllIds().ToList();
			if (collection.AllKeys.Contains(name))
			{
				if (collection[name] != string.Empty)
				{
					var checkedIds = _Controller.GetIds(collection[name]);
					// сохраняем тех кто не отмечен
					_Controller.Cookies.AddToCookie("Except" + name, allIds.Except(checkedIds).ToString(','));
				}
			}
			else
			{
				_Controller.Cookies.AddToCookie("Except" + name, allIds.ToString(','));
			}
		}

		#endregion
	}
}