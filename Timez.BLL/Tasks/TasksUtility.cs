using System;
using System.Collections.Generic;
using System.Linq;
using Common.Extentions;
using System.Transactions;
using Timez.Entities;

namespace Timez.BLL.Tasks
{
	// TODO: попробовать сделать эти операции асинхронными, так как они наиболее частые, провести сравнение
	public sealed partial class TasksUtility : BaseUtility<TasksUtility>
	{
		public Listener<EventArgs<ITask>> OnCreate = new Listener<EventArgs<ITask>>();
		public Listener<UpdateEventArgs<ITask>> OnTaskAssigned = new Listener<UpdateEventArgs<ITask>>();
		public Listener<UpdateEventArgs<ITask>> OnUpdateColor = new Listener<UpdateEventArgs<ITask>>();
		public Listener<UpdateEventArgs<ITask>> OnUpdateProject = new Listener<UpdateEventArgs<ITask>>();
		public Listener<UpdateEventArgs<ITask>> OnUpdatePlaningTime = new Listener<UpdateEventArgs<ITask>>();

		public Listener<UpdateEventArgs<ITask>> OnUpdateStatus = new Listener<UpdateEventArgs<ITask>>();
		public Listener<UpdateEventArgs<ITask>> OnUpdate = new Listener<UpdateEventArgs<ITask>>();
		public Listener<EventArgs<ITask>> OnDelete = new Listener<EventArgs<ITask>>();
		public Listener<EventArgs<ITask>> OnTaskToArchive = new Listener<EventArgs<ITask>>();
		public Listener<EventArgs<ITask>> OnRestore = new Listener<EventArgs<ITask>>();
		public Listener<EventArgs<Tuple<int, int, int>>> OnReassign = new Listener<EventArgs<Tuple<int, int, int>>>();

		public event TaskErrorEventHandler OnTaskCountLimitIsReached = delegate { };
		public event Action<ITask, ITasksStatus, int> OnTaskPlanningTimeIsExceeded = delegate { };

		#region Работа с архивом

		/// <summary>
		/// Перенос задачи в архив
		/// </summary>
		/// <param name="task"></param>
		public void ToArchive(ITask task)
		{
			// для архива переносим задачу в специальную архивную таблицу
			Repository.Tasks.FastDelete(task.Id);
			Repository.Tasks.AddToArchive(task);

			OnTaskToArchive.Invoke(this, new EventArgs<ITask>(task));
		}

		/// <summary>
		/// Получени задачи из архива
		/// </summary>
		public ITask GetFromArchive(int id)
		{
			var archive = Repository.Tasks.GetFromArchiveById(id) as ITask;
			TimezTask task = new TimezTask(archive);
			return task;
		}

		public List<ITask> GetFromArchive(TaskFilter filter)
		{
			var tasksArchives = Repository.Tasks.GetFromArchiveByBoard(filter);
			return tasksArchives.ToList();
		}

		public void DeleteFromArchive(int taskId)
		{
			Repository.Tasks.DeleteFromArchive(taskId);
		}

		/// <summary>
		/// Восстановление задачи из архива
		/// </summary>
		/// <param name="taskId"></param>
		/// <returns></returns>
		public ITask Restore(int taskId)
		{
			using (TransactionScope scope = new TransactionScope())
			{
				ITask task = Repository.Tasks.Get(taskId);

				if (task != null)
				{
					// Задачи при отправке в архив не удаляются, а лишь помечаются удаленными
					task.IsDeleted = false;
					Repository.SubmitChanges();
				}
				else
				{
					DeleteFromArchive(taskId);

					// архивная задача обязательно должа быть
					throw new TaskNotFoundException();
				}

				DeleteFromArchive(taskId);

				var args = new EventArgs<ITask> { Data = task };
				OnRestore.Invoke(args);

				scope.Complete();
				return task;
			}
		}

		public void ClearArchive(int boardId)
		{
			Repository.Tasks.ClearArchive(boardId);
		}

		#endregion

		#region Статусы

		/// <summary>
		/// Основная функция обновления статуса
		/// Она проверяет все что нужно
		/// </summary>
		public ITask UpdateStatus(int taskId, int statusId, Limits limits)
		{
			ITask task = _Get(taskId);
			TimezTask oldTask = new TimezTask(task);

			if (task.TaskStatusId != statusId)
			{
				ITasksStatus newStatus = Utility.Statuses.Get(task.BoardId, statusId);

				CheckStatusLimits(newStatus, task.PlanningTime);
				CheckTaskLimits(newStatus, task.ExecutorUserId, limits, task);

				// Для обычных статусов только обновляем поля у задачи
				Repository.Tasks.UpdateStatus(task.Id, newStatus);

				// Обновляем у этой сущности, что б не запрашивать из бд
				task.TaskStatusId = newStatus.Id;
				task.TaskStatusPosition = newStatus.Position;
				task.StatusChangeDateTime = DateTime.Now;
				task.TaskStatusName = newStatus.Name;

				OnUpdateStatus.Invoke(this, new UpdateEventArgs<ITask>(oldTask, task));
			}

			return task;
		}

		/// <summary>
		/// Проверка лимитов по задачам
		/// </summary>
		static void CheckStatusLimits(ITasksStatus status, int? planingTime)
		{
			if (status.PlanningRequired && !planingTime.HasValue)
				throw new PlanningTimeRequered(status);
		}

		/// <summary>
		/// Достигнут ли предел количества задач в статусе для пользователя
		/// task = null - если задача новая
		/// </summary>
		void CheckTaskLimits(ITasksStatus status, int executorId, Limits toCheck, ITask task)
		{
			var filter = CreateFilter(status.BoardId);
			filter.ExecutorIds = new[] { executorId };
			filter.Statuses = new[] { status.Id };
			var tasks = Get(filter);

			// Проверяем на лимит количества задач
			if ((toCheck & Limits.TaskCountLimitIsReached) == Limits.TaskCountLimitIsReached)
			{
				if (status.MaxTaskCountPerUser.HasValue && status.MaxTaskCountPerUser.Value > 0)
				{
					// если количество задач больше лимита
					if (status.MaxTaskCountPerUser.Value < tasks.Count)
					{
						OnTaskCountLimitIsReached(task, tasks.Count);
						throw new TaskCountLimitIsReached(status.MaxTaskCountPerUser.Value);
					}

					// Проверять на равенство лимиту не нужно в попапе редактирования
					if ((toCheck & Limits.PopupUpdating) != Limits.PopupUpdating && status.MaxTaskCountPerUser.Value == tasks.Count)
					{
						OnTaskCountLimitIsReached(task, tasks.Count);
						throw new TaskCountLimitIsReached(status.MaxTaskCountPerUser.Value);
					}
				}
			}

			// Проверяем суммарную планируемую длительность
			if ((toCheck & Limits.PlanningTimeIsExceeded) == Limits.PlanningTimeIsExceeded)
			{
				int timeSum = tasks.Sum(x => x.PlanningTime ?? 0) + task.PlanningTime ?? 0;
				if (status.MaxPlanningTime.HasValue && timeSum > status.MaxPlanningTime.Value)
				{
					OnTaskPlanningTimeIsExceeded(task, status, timeSum);
					throw new PlanningTimeIsExceeded(status.MaxPlanningTime.Value);
				}
			}
		}

		#endregion

		#region Задачи

		/// <summary>
		/// Переназначить задачи
		/// </summary>
		public void Reassign(int boardId, int fromUserId, int toUserId)
		{
			Repository.Tasks.Reassign(boardId, fromUserId, toUserId);

			Tuple<int, int, int> data = new Tuple<int, int, int>(boardId, fromUserId, toUserId);
			OnReassign.Invoke(new EventArgs<Tuple<int, int, int>>(data));
		}

		/// <summary>
		/// Фильтр инициализированный идами полных коллекций
		/// </summary>
		public TaskFilter CreateFilter(int boardId)
		{
			int[] projects = Utility.Projects.GetByBoard(boardId).Select(x => x.Id).ToArray();
			int[] users = Utility.Boards.GetAllExecutorsOnBoard(boardId).Select(x => x.Id).ToArray();
			int[] colorsList = Utility.Boards.GetColors(boardId).Select(x => x.Id).ToArray();
			int[] statusesList = Utility.Statuses.GetByBoard(boardId).Select(x => x.Id).ToArray();

			IBoardsUser settings = Utility.Boards.GetUserSettings(boardId, Utility.Authentication.UserId).Settings;

			return new TaskFilter(boardId, settings, statusesList, users, projects, colorsList);
		}

		/// <summary>
		/// Создание задачи
		/// </summary>
		/// <exception cref="PlanningTimeRequered"></exception>
		/// <exception cref="PlanningTimeIsExceeded"></exception>
		public ITask Create(string name, string desc, int executorId, int projectId, int colorId, int statusId, int boardId, int? planingTime = null, Limits checkLimits = Limits.NoLimits)
		{
			if (name.IsNullOrEmpty())
				throw new ArgumentException("Name");

			IBoardsColor color = Utility.Boards.GetColor(boardId, colorId);
			IProject project = Utility.Projects.Get(boardId, projectId);
			IUser executor = Utility.Users.Get(executorId);
			ITasksStatus status = Utility.Statuses.Get(boardId, statusId);
			int creatorUserId = Utility.Authentication.UserId;

			// Передаем данные о создаваемой задаче, что бы проверить по ней ограничения
			CheckTaskLimits(status, executorId, checkLimits, new TimezTask
			{
				Name = name,
				Description = desc,
				ExecutorUserId = executor.Id,
				ExecutorNick = executor.Nick,
				ExecutorEmail = executor.EMail,
				BoardId = boardId,
				ColorHEX = color.Color,
				ColorId = color.Id,
				ColorName = color.Name,
				ColorPosition = color.Position,
				CreationDateTime = DateTimeOffset.Now,
				CreatorUserId = creatorUserId,
				Id = 0,
				PlanningTime = planingTime,
				ProjectId = project.Id,
				ProjectName = project.Name,
				StatusChangeDateTime = DateTimeOffset.Now,
				TaskStatusId = status.Id,
				TaskStatusPosition = status.Position
			});

			CheckStatusLimits(status, planingTime);

			//using (TransactionScope scope = new TransactionScope())
			{
				ITask task = Repository.Tasks.Create(name, desc, executor, project, color, status, planingTime, creatorUserId);

				OnCreate.Invoke(new EventArgs<ITask>(task));
				OnTaskAssigned.Invoke(new UpdateEventArgs<ITask>(null, task));

				//scope.Complete();

				return task;
			}
		}

		/// <summary>
		/// Получение неудаленной задачи из БД
		/// </summary>
		/// <param name="taskId">ид задачи</param>
		ITask _Get(int taskId)
		{
			ITask task = Repository.Tasks.Get(taskId);
			if (task == null || task.IsDeleted)
				throw new TaskNotFoundException();

			return task;
		}

		/// <summary>
		/// Обновлеяе основные параметры задачи
		/// Кроме статуса! Так как там сложная логика
		/// Обновление задачи, если задача была в кеше, то обновление происходит асинхронно
		/// Если в кеше задачи не было, то обновление синхронное и после задача добавляется в кеш
		/// </summary>
		public ITask Update(int boardId, int taskId, string name, string description, int userId, int projectId, int colorId, int? planingTime, Limits limits)
		{
			if (name.IsNullOrEmpty())
				throw new ArgumentException("Name");

			// TODO: Не вызывать апдейт, если ничиго не поменялось
			// TODO: Добавить колонку с временем обновления для поддержки версионности

			ITask task = _Get(taskId);
			TimezTask oldTask = new TimezTask(task);

			IProject project = Utility.Projects.Get(boardId, projectId);
			IBoardsColor color = Utility.Boards.GetColor(project.BoardId, colorId);
			IUser executor = Utility.Users.Get(userId);
			ITasksStatus status = Utility.Statuses.Get(boardId, task.TaskStatusId);

			CheckTaskLimits(status, userId, limits, task);

			bool taskAssigned = task.ExecutorUserId != executor.Id;
			bool colorChanged = task.ColorId != color.Id;
			bool projectChanged = task.ProjectId != project.Id;
			bool planingTimeChanged = task.PlanningTime != planingTime;

			task.Name = name;
			task.Description = description;
			task.ExecutorUserId = executor.Id;
			task.ExecutorEmail = executor.EMail;
			task.ExecutorNick = executor.Nick;
			task.ProjectId = project.Id;
			task.ProjectName = project.Name;
			task.ColorId = color.Id;
			task.ColorHEX = color.Color;
			task.ColorName = color.Name;
			task.ColorPosition = color.Position;
			task.PlanningTime = planingTime;

			using (TransactionScope scope = new TransactionScope())
			{
				Repository.SubmitChanges();

				OnUpdate.Invoke(new UpdateEventArgs<ITask>(oldTask, task));

				if (taskAssigned)
					OnTaskAssigned.Invoke(new UpdateEventArgs<ITask>(oldTask, task));

				if (colorChanged)
					OnUpdateColor.Invoke(new UpdateEventArgs<ITask>(oldTask, task));

				if (projectChanged)
					OnUpdateProject.Invoke(new UpdateEventArgs<ITask>(oldTask, task));

				if (planingTimeChanged)
					OnUpdatePlaningTime.Invoke(new UpdateEventArgs<ITask>(oldTask, task));

				scope.Complete();
			}

			return task;
		}

		public IBoardsColor SetColor(int taskId, int colorId, int boardId)
		{
			IBoardsColor color = Utility.Boards.GetColor(boardId, colorId);
			ITask task = _Get(taskId);

			if (task.ColorId != color.Id)
			{
				ITask oldTask = new TimezTask(task);

				task.ColorId = color.Id;
				task.ColorHEX = color.Color;
				task.ColorName = color.Name;
				task.ColorPosition = color.Position;
				Repository.SubmitChanges();

				OnUpdateColor.Invoke(new UpdateEventArgs<ITask>(oldTask, task));
			}

			return color;
		}

		public IProject SetProject(int taskId, int projectId, int boardId)
		{
			IProject project = Utility.Projects.Get(boardId, projectId);
			ITask task = _Get(taskId);

			if (task.ProjectId != project.Id)
			{
				ITask oldTask = new TimezTask(task);

				task.ProjectId = project.Id;
				task.ProjectName = project.Name;
				Repository.SubmitChanges();
				OnUpdateProject.Invoke(new UpdateEventArgs<ITask>(oldTask, task));
			}

			return project;
		}

		public IUser SetExecutor(int taskId, int userId)
		{
			IUser executor = Utility.Users.Get(userId);
			ITask task = _Get(taskId);

			if (task.ExecutorUserId != executor.Id)
			{
				TimezTask oldTask = new TimezTask(task);

				task.ExecutorUserId = executor.Id;
				task.ExecutorEmail = executor.EMail;
				task.ExecutorNick = executor.Nick;
				Repository.SubmitChanges();

				OnTaskAssigned.Invoke(new UpdateEventArgs<ITask>(oldTask, task));
			}

			return executor;
		}

		public ITask SetPlanningTime(int boardId, int newStatusId, int taskId, int time, Limits limits)
		{
			ITask task = _Get(taskId);
			TimezTask oldTask = new TimezTask(task);

			var status = Utility.Statuses.Get(boardId, newStatusId); // проверяем в новом статусе лимиты
			CheckTaskLimits(status, task.ExecutorUserId, limits, task);

			task.PlanningTime = time;
			Repository.SubmitChanges();

			OnUpdatePlaningTime.Invoke(new UpdateEventArgs<ITask>(oldTask, task));

			return task;
		}

		/// <summary>
		/// Удаление задачи через флаг IsDeleted
		/// </summary>
		public bool Delete(ITask task)
		{
			bool isDeleted = Repository.Tasks.FastDelete(task.Id);

			if (isDeleted)
				OnDelete.Invoke(this, new EventArgs<ITask> { Data = task });

			return isDeleted;
		}

		#endregion
	}
}
