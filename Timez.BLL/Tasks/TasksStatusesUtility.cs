using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Timez.Entities;

namespace Timez.BLL.Tasks
{
	public sealed partial class TasksStatusesUtility : BaseUtility<TasksStatusesUtility>
	{
		/// <summary>
		/// Подпичики:
		/// - TasksModel, обновление кеша задач
		/// </summary>
		public readonly Listener<EventArgs<ITasksStatus>> OnUpdate = new Listener<EventArgs<ITasksStatus>>();

		public readonly Listener<EventArgs<ITasksStatus>> OnCreate = new Listener<EventArgs<ITasksStatus>>();

		/// <summary>
		/// Подпичики:
		/// - TasksModel, очистка кеша задач
		/// </summary>
		public readonly Listener<EventArgs<ITasksStatus>> OnDelete = new Listener<EventArgs<ITasksStatus>>();

		/// <summary>
		/// Создает статус и помещает ее перед беклогом
		/// </summary>
		public ITasksStatus Create(int boardId, string name, bool planningRequired, int? maxTaskCountPerUser, int? maxPlanningTime)
		{
			using (TransactionScope scope = new TransactionScope())
			{
				ITasksStatus status = Repository.TasksStatuses.Create(
					boardId, name,
					false, planningRequired,
					maxTaskCountPerUser, maxPlanningTime);

				OnCreate.Invoke(new EventArgs<ITasksStatus>(status));

				scope.Complete();
				return status;
			}
		}

		/// <summary>
		/// Статусы + беклог
		/// Архив не статус
		/// </summary>
		public List<ITasksStatus> GetByBoard(int boardId)
		{
			var key = Cache.GetKeys(
					CacheKey.Board, boardId,
					CacheKey.Status, CacheKey.All);

			return Cache.Get(key, () => Repository.TasksStatuses.GetByBoard(boardId).ToList());
		}

		public ITasksStatus Get(int boardId, int statusId)
		{
			var cache = GetByBoard(boardId);
			var status = cache.FirstOrDefault(x => x.Id == statusId);
			if (status == null)
			{
				status = Repository.TasksStatuses.Get(statusId);
				cache.Add(status);
			}

			return status;
		}

		public ITasksStatus GetBacklog(int boardId)
		{
			return GetByBoard(boardId).First(x => x.IsBacklog);
		}

		public ITasksStatus Delete(int statusId)
		{
			ITasksStatus status = Repository.TasksStatuses.Get(statusId);
			if (!status.IsBacklog)
			{
				using (TransactionScope scope = new TransactionScope())
				{
					Repository.TasksStatuses.Delete(status);

					OnDelete.Invoke(new EventArgs<ITasksStatus>(status));

					scope.Complete();
				}
			}
			else
			{
				throw new InvalidOperationException("Запрещено удалять Беклог");
			}

			return status;
		}

		/// <summary>
		/// Изменение статуса на доске
		/// </summary>
		public ITasksStatus Update(int statusId, string name, bool planningRequired, int? maxTaskCountPerUser, int? maxPlanningTime)
		{
			using (TransactionScope scope = new TransactionScope())
			{
				var status = Repository.TasksStatuses.Get(statusId);
				bool nameChanged = status.Name != name;

				status.Name = name;
				status.PlanningRequired = planningRequired;
				status.MaxTaskCountPerUser = maxTaskCountPerUser;
				status.MaxPlanningTime = maxPlanningTime;
				Repository.SubmitChanges();

				_OnUpdate(status, nameChanged);

				scope.Complete();

				return status;
			}
		}

		private void _OnUpdate(ITasksStatus status, bool additionalUpdate)
		{
			if (additionalUpdate)
				Repository.Tasks.UpdateStatus(status);

			OnUpdate.Invoke(new EventArgs<ITasksStatus>(status));
		}

		/// <summary>
		/// Задает порядок статуса на доске
		/// </summary>
		/// <param name="boardId">доска</param>
		/// <param name="newOrder">иды статусов в нужной последовательности</param>
		public void SetOrder(int boardId, IEnumerable<int> newOrder)
		{
			ITasksStatus[] statuses = Repository.TasksStatuses.GetByBoard(boardId).ToArray();
			Repository.SetOrder(newOrder, statuses, x => _OnUpdate(x, true));
		}

		public void Archive(int boardId, int id)
		{
			TaskFilter filter = Utility.Tasks.CreateFilter(boardId);
			filter.Statuses = new[] {id};
			List<ITask> tasks = Utility.Tasks.Get(filter);
			foreach (ITask task in tasks)
			{
				Utility.Tasks.ToArchive(task);
			}
		}
	}
}
