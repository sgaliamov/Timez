using System;
using Common.Extentions;
using System.Collections.Generic;
using System.Linq;
using Timez.Entities;

namespace Timez.Helpers
{
	[Obsolete]
	public class PagedEventHistoryData
	{
		protected PagedEventHistoryData() { }

		/// <summary>
		/// Получение страницы page задач из большого списка tasks
		/// </summary>
		public PagedEventHistoryData(int page, IEnumerable<IEventHistory> eventData)
		{
			Page = page;
			int total;
			EventData = eventData.AsQueryable().GetPaged(page, Pager.DefaultItemsOnPage, out total);
			TotalCount = total;
		}

		public IEnumerable<IEventHistory> EventData { get; set; }
		public int TotalCount { get; private set; }
		public int Page { get; set; }
	}
}
