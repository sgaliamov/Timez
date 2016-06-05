using System;
using System.Collections.Generic;
using System.Linq;
using Common.Exceptions;
using Common.Extentions;

namespace Timez.Entities
{
	// При добавлении параметра в функцию нужно:
	// 1. Поправить ключ фильтра
	// 2. Дописать в Where
	// 3. Поправить NeedAllTasks
	public sealed class TaskFilter
	{
		private readonly IEnumerable<int> _AllUserIds;
		private readonly IEnumerable<int> _AllProjectIds;
		private readonly IEnumerable<int> _AllColorIds;
		private readonly IBoardsUser _Settings;
		private readonly IEnumerable<int> _AllStatuses;

		public TaskFilter(int boardId, IBoardsUser settings
			, IEnumerable<int> allStatuses, IEnumerable<int> allUserIds
			, IEnumerable<int> allProjectIds, IEnumerable<int> allColorIds)
		{
			BoardId = boardId;
			_Settings = settings;
			_AllStatuses = allStatuses;
			_AllColorIds = allColorIds;
			_AllProjectIds = allProjectIds;
			_AllUserIds = allUserIds;
		}

		public TasksSortType SortType { get; set; }

		/// <summary>
		/// Исполнитель
		/// </summary>
		public IEnumerable<int> ExecutorIds { get; set; }
		public int BoardId { get; private set; }
		public IEnumerable<int> ProjectIds { get; set; }
		public IEnumerable<int> ColorIds { get; set; }
		public IEnumerable<int> Statuses { get; set; }
		public string Search { get; set; }
		public IEnumerable<int> CreatorIds { set; get; }

		public string Key
		{
			get
			{
				return
					"Board."
					+ BoardId.ToString()
					+ ".Tasks."
				   + ExecutorIds.ToString('_')
				   + CreatorIds.ToString('_')
				   + ProjectIds.ToString('_')
				   + ColorIds.ToString('_')
					// + sortType.ToString() не влияет наличие задач в кеше, по этому не нужно кешировать по этому значению
				   + Statuses.ToString('_')
					//+ (ItemsOnPage.HasValue ? ItemsOnPage.Value.ToString() : string.Empty)
					//+ (Page.HasValue ? Page.Value.ToString() : string.Empty)
				   + (Search ?? string.Empty);
			}
		}

		/// <summary>
		/// Соответствует фильтр всем задачам
		/// </summary>
		public bool NeedAllTasks
		{
			get
			{
				return IsAllIn(ExecutorIds, _AllUserIds)
					&& IsAllIn(ProjectIds, _AllProjectIds)
					&& IsAllIn(ColorIds, _AllColorIds)
					&& IsAllIn(Statuses, _AllStatuses)
					&& IsAllIn(CreatorIds, _AllUserIds)
					&& string.IsNullOrWhiteSpace(Search);
			}
		}

		/// <summary>
		/// Все ли опции выбраны
		/// </summary>
		static bool IsAllIn(IEnumerable<int> filterList, IEnumerable<int> allIds)
		{
			if (filterList == null)
				return true;

			if (allIds == null)
				return false;

			// только те, которые есть в полном наботе ids
			List<int> validList = filterList.Where(allIds.Contains).ToList();
			return allIds.All(validList.Contains);
		}

		public IQueryable<ITask> Where(IQueryable<ITask> tasks)
		{
			#region Фильтры

			tasks = tasks.Where(x => x.BoardId == BoardId);

			if (!IsAllIn(ExecutorIds, _AllUserIds))
			{
				tasks = from t in tasks
						where ExecutorIds.Contains(t.ExecutorUserId)
						select t;
			}

			if (!IsAllIn(CreatorIds, _AllUserIds))
			{
				tasks = from t in tasks
						where CreatorIds.Contains(t.CreatorUserId)
						select t;
			}

			if (!IsAllIn(ProjectIds, _AllProjectIds))
			{
				tasks = from t in tasks
						where ProjectIds.Contains(t.ProjectId)
						select t;
			}

			if (!IsAllIn(ColorIds, _AllColorIds))
			{
				tasks = from t in tasks
						where ColorIds.Contains(t.ColorId)
						select t;
			}

			if (!IsAllIn(Statuses, _AllStatuses))
			{
				if (Statuses.Any(x => x == TimezStatus.ArchiveStatusId || x <= 0))
					throw new ArgumentOutOfRangeException();

				tasks = from t in tasks
						where Statuses.Contains(t.TaskStatusId)
						select t;
			}

			if (!string.IsNullOrWhiteSpace(Search))
			{
				tasks = from t in tasks
						where t.Name.ToUpper().Contains(Search.Trim().ToUpper())
						select t;
			}

			#endregion

			#region Ограничение по роли пользователя в доске

			UserRole role = _Settings.GetUserRole();
			if (!role.HasTheFlag(UserRole.Owner) && !role.HasTheFlag(UserRole.Observer))
			{
				// владелец и наблюдатель получают все задачи
				// исполнитель и заказчик только свои
				if (role.HasAnyFlag(UserRole.Executor | UserRole.Customer))
				{
					// есть обе роли, значит нужно оба типа задач
					tasks = from t in tasks
							where t.ExecutorUserId == _Settings.UserId // назначенные на пользователя
							|| t.CreatorUserId == _Settings.UserId // назначенные пользоватем
							select t;
				}
				else if (role.HasTheFlag(UserRole.Executor))
				{
					tasks = from t in tasks
							where t.ExecutorUserId == _Settings.UserId // только назначенные на пользователя
							select t;
				}
				else if (role.HasTheFlag(UserRole.Customer))
				{
					tasks = from t in tasks
							where t.CreatorUserId == _Settings.UserId // только назначенные пользоватем
							select t;
				}
				else
				{
					throw new AccessDeniedException();
				}
			}

			#endregion

			return tasks;
		}

		public IOrderedQueryable<ITask> Order(IQueryable<ITask> tasks)
		{
			IOrderedQueryable<ITask> ordered;

			switch (SortType)
			{
				case TasksSortType.ByName:
					ordered = from t in tasks orderby t.Name select t;
					break;

				case TasksSortType.ByColor:
					ordered = from t in tasks
							  orderby t.ColorPosition, t.Name
							  select t;
					break;

				case TasksSortType.ByProject:
					ordered = from t in tasks
							  orderby t.ProjectName, t.Name
							  select t;
					break;

				case TasksSortType.ByExecutor:
					ordered = from t in tasks
							  orderby t.ExecutorNick, t.Name
							  select t;
					break;

				case TasksSortType.ByStatus:
					ordered = from t in tasks
							  orderby t.TaskStatusPosition, t.Name
							  select t;
					break;

				case TasksSortType.ByCreationDate:
					ordered = tasks.OrderBy(t => t.CreationDateTime);
					break;

				case TasksSortType.ByStatusChangeDateTime:
					ordered = tasks.OrderByDescending(t => t.StatusChangeDateTime);
					break;

				case TasksSortType.ByPlanningTime:
					ordered = tasks.OrderByDescending(t => t.PlanningTime ?? 0);
					break;

				default:
					throw new ArgumentException("TasksSortType");
			}

			return ordered;
		}
	}
}
