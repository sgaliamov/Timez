using System;
using Common.Extentions;
using System.Collections.Generic;
using System.Linq;
using Timez.DAL.DataContext;
using Timez.Entities;

namespace Timez.DAL
{
	public interface IEventHistoryRepository
	{
		/// <summary>
		/// Добавляет событие в журнал
		/// </summary>
		/// <param name="eventText">Текст события</param>
		/// <param name="user">Пользователь инициировашый событие</param>
		/// <param name="task">Задача над которой произошло событие</param>
		/// <param name="eventType"></param>
		/// <returns>Событие</returns>
		void Add(string eventText, IUser user, ITask task, EventType eventType);

		IQueryable<IEventHistory> Get(EventDataFilter filter, out int totalCount);

		void Clear(int id);
	}

	class EventHistoryRepository : BaseRepository<EventHistoryRepository>, IEventHistoryRepository
	{
		/// <summary>
		/// Добавляет событие в журнал
		/// </summary>
		/// <param name="eventText">Текст события</param>
		/// <param name="user">Пользователь инициировашый событие</param>
		/// <param name="task">Задача над которой произошло событие</param>
		/// <param name="eventType"></param>
		/// <returns>Событие</returns>
		public void Add(string eventText, IUser user, ITask task, EventType eventType)
		{
			if (string.IsNullOrWhiteSpace(eventText))
				throw new ArgumentNullException("eventText");

			// Масимально быстро добавляем событие
			_Context.ExecuteCommand(@"
                INSERT INTO EventHistory 
                (EventDateTime, TaskId, TaskName, UserId, UserNick, Event, EventType,ProjectId,ProjectName,BoardId)
                VALUES ({0},{1},{2},{3},{4},{5},{6},{7},{8}, {9})",
													   DateTimeOffset.Now, //0
													   task.Id, // 1
													   task.Name, // 2
													   user.Id, // 3
													   user.Nick, // 4
													   eventText, //5
													   (int)eventType, // 6
														task.ProjectId,// 7
													   task.ProjectName,// 8
													   task.BoardId); // 9
		}

		public IQueryable<IEventHistory> Get(EventDataFilter filter, out int totalCount)
		{
			int boardId = filter.BoardId;
			IEnumerable<int> userIds = filter.UserIds;
			IEnumerable<int> projectIds = filter.ProjectIds;
			var eventType = filter.EventTypes;

			IQueryable<EventHistory> eventData = _Context.EventHistories
				.Where(x => x.BoardId == boardId && (x.EventType & (int)eventType) == x.EventType)
				.OrderByDescending(t => t.EventDateTime);

			if (userIds != null)
			{
				eventData = from t in eventData
							where userIds.Contains(t.UserId)
							select t;
			}

			if (projectIds != null)
			{
				eventData = from t in eventData
							where projectIds.Contains(t.ProjectId)
							select t;
			}

			if (filter.Page.HasValue && filter.ItemsOnPage.HasValue)
			{
				eventData = eventData.GetPaged(filter.Page.Value, filter.ItemsOnPage.Value, out totalCount);
			}
			else
			{
				totalCount = eventData.Count();
			}

			return eventData;

		}

		//static readonly Func<TimezDataContext, int, EventType, IQueryable<EventHistory>> _compiledGetAll =
		//    CompiledQuery.Compile((TimezDataContext ctx, int boardId, EventType eventType) =>
		//        ctx.EventHistories.Where(x => x.BoardId == boardId && (x.EventType & (int)eventType) == x.EventType));

		public void Clear(int id)
		{
			_Context.ExecuteCommand("DELETE [EventHistory] WHERE [BoardId] = {0}", id);
		}
	}
}
