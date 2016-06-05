using System.Collections.Generic;
using System.Linq;
using Common.Extentions;
using Timez.Entities;

namespace Timez.Helpers
{
	public class PagedTasks
	{
		protected PagedTasks() { }

		/// <summary>
		/// Получение страницы page задач из большого списка tasks
		/// <param name="page">Требуемая страница</param>
		/// <param name="tasks">Все задачи коллекции</param>
		/// </summary>
		public PagedTasks(int page, IEnumerable<ITask> tasks)
		{
			Page = page;
			int total;
			Tasks = tasks.AsQueryable().GetPaged(page, Pager.DefaultItemsOnPage, out total);
			TotalCount = total;

			TotalMinutes = tasks.Sum(x => x.PlanningTime ?? 0);
		}

		public IEnumerable<ITask> Tasks { get; set; }
		public int TotalCount { get; set; }
		public int TotalMinutes { get; set; }
		public int Page { get; set; }
	}
}