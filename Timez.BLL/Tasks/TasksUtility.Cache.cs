using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Timez.Entities;
using CacheKeyValue = System.Collections.Generic.KeyValuePair<Timez.BLL.CacheKey, string>;

namespace Timez.BLL.Tasks
{
	public sealed partial class TasksUtility
	{
		#region Инициализация

		internal override void Init()
		{
			#region

			// Следим за другими утилитами, что б поддерживать кеш задач в валидном состоянии
			Utility.Projects.OnDelete += ProjectsUtility_OnDelete;

			// Удаляем из кеша все задачи удаленного пользователя
			Utility.Boards.OnRemoveUserFromBoard += BoardsUtility_OnRemoveUserFromBoard;

			// Обновляем цвета у задач
			Utility.Boards.OnUpdateColor +=
				(s, e) =>
				{
					IBoardsColor color = e.Data;

					HashSet<ITask> cache = GetTasksCache(color.BoardId);
					if (cache != null)
					{
						using (new UpdateLock(color.BoardId))
						{
							ITask[] tasks = cache
									.Where(x => x.ColorId == color.Id)
									.ToArray();

							foreach (var item in tasks)
							{
								item.ColorHEX = color.Color;
								item.ColorPosition = color.Position;
								item.ColorName = color.Name;
							}
						}
					}
				};

			Utility.Boards.OnDeleteColor +=
				(s, e) =>
				{
					IBoardsColor color = e.Data;

					var key = GetAllTasksKey(color.BoardId);
					Cache.Clear(key);
				};
			#endregion

			#region Обновляем имена проектов в задачах
			Utility.Projects.OnUpdate.Add(
					(sender, e) =>
					{
						IProject project = e.Data;
						TaskFilter filter = CreateFilter(project.BoardId);
						filter.ProjectIds = new[] { project.Id };
						List<ITask> tasks = Get(filter);
						tasks.ForEach(x => x.ProjectName = project.Name);
					});
			#endregion

			#region Обновляем имена статусов в задачах
			Utility.Statuses.OnUpdate.Add(
					(sender, e) =>
					{
						ITasksStatus status = e.Data;
						TaskFilter filter = CreateFilter(status.BoardId);
						filter.Statuses = new[] { status.Id };
						List<ITask> tasks = Get(filter);
						tasks.ForEach(
							x =>
							{
								x.TaskStatusName = status.Name;
								x.TaskStatusPosition = status.Position;
							});
					});
			#endregion

			#region При удалении статуса чистим кеш доски
			Utility.Statuses.OnDelete.Add(
				(sender, e) =>
				{
					ITasksStatus status = e.Data;
					var key = Cache.GetKeys(CacheKey.Status, status.Id);
					Cache.Clear(key);

					key = GetAllTasksKey(status.BoardId);
					Cache.Clear(key);
				});
			#endregion

			#region Обновление исполнителя

			Utility.Users.OnUpdate.Add(
				(sender, e) =>
				{
					IUser user = e.NewData;
					Repository.Tasks.UpdateExecutor(user);

					List<IBoard> boards = Utility.Boards.GetByUser(user.Id);
					foreach (var board in boards)
					{
						var cache = GetTasksCache(board.Id);
						if (cache != null)
						{
							using (new UpdateLock(board.Id))
							{
								foreach (ITask task in cache.Where(x => x.ExecutorUserId == user.Id))
								{
									task.ExecutorEmail = user.EMail;
									task.ExecutorNick = user.Nick;
								}
							}
						}
					}
				});

			#endregion

			// TODO: протестить лики из-за подписок
		}

		/// <summary>
		/// При удалении пользователя с доски удаляются все связанные задачи в кеше
		/// </summary>
		/// <param name="userOnBoard"></param>
		void BoardsUtility_OnRemoveUserFromBoard(IBoardsUser userOnBoard)
		{
			using (new UpdateLock(userOnBoard.BoardId))
			{
				HashSet<ITask> tasks = GetTasksCache(userOnBoard.BoardId);
				if (tasks != null)
					tasks.RemoveWhere(t => t.ExecutorUserId == userOnBoard.UserId);
			}
		}

		void ProjectsUtility_OnDelete(object s, EventArgs<IProject> data)
		{
			IProject project = data.Data;

			using (new UpdateLock(project.BoardId))
			{
				// Удаляем из кеша все задачи удаленного проекта
				HashSet<ITask> tasks = GetTasksCache(project.BoardId);
				if (tasks != null)
					tasks.RemoveWhere(t => t.ProjectId == project.Id);
			}
		}

		#endregion

		public TasksUtility()
		{
			#region Удаление
			OnDelete += (s, e) =>
				{
					ITask task = e.Data;

					using (new UpdateLock(task.BoardId))
					{
						HashSet<ITask> cache = GetTasksCache(task.BoardId);
						if (cache != null)
							cache.RemoveWhere(x => x.Id == task.Id);
					}
				};
			#endregion

			#region Создание

			OnCreate +=
			(s, e) =>
			{
				ITask task = e.Data;

				using (new UpdateLock(task.BoardId))
				{
					HashSet<ITask> cache = GetTasksCache(task.BoardId);
					if (cache != null)
						cache.Add(task);
				}
			};
			#endregion

			#region Архивирование
			OnTaskToArchive += (s, e) =>
				{
					ITask task = e.Data;

					using (new UpdateLock(task.BoardId))
					{
						HashSet<ITask> cache = GetTasksCache(task.BoardId);
						if (cache != null)
							cache.RemoveWhere(x => x.Id == task.Id);
					}
				};
			#endregion

			#region Восстановление из архива
			OnRestore += (s, e) =>
				{
					ITask task = e.Data;

					using (new UpdateLock(task.BoardId))
					{
						HashSet<ITask> cache = GetTasksCache(task.BoardId);
						if (cache != null)
							cache.Add(task);
					}
				};
			#endregion

			#region Переназначение задач
			OnReassign.Add(
					(sender, e) =>
					{
						int boardId = e.Data.Item1;
						int fromUserId = e.Data.Item2;
						int toUserId = e.Data.Item3;

						IUser toUser = Utility.Users.Get(toUserId);

						TaskFilter filter = CreateFilter(boardId);
						filter.ExecutorIds = new[] { fromUserId };
						List<ITask> tasks = Get(filter);
						tasks.ForEach(
							x =>
							{
								x.ExecutorUserId = toUser.Id;
								x.ExecutorEmail = toUser.EMail;
								x.ExecutorNick = toUser.Nick;
							});

						filter.ExecutorIds = null;
						filter.CreatorIds = new[] { fromUserId };
						tasks = Get(filter);
						tasks.ForEach(x => x.CreatorUserId = toUserId);
					});
			#endregion

			#region Обновление проекта

			OnUpdateProject += (s, e) =>
								{
									ITask task = e.NewData;
									ITask cached = Get(task.BoardId, task.Id);
									cached.ProjectId = task.ProjectId;
									cached.ProjectName = task.ProjectName;
								};

			#endregion

			#region Обновление цвета

			OnUpdateColor += (s, e) =>
			{
				ITask task = e.NewData;
				ITask cached = Get(task.BoardId, task.Id);
				cached.ColorId = task.ColorId;
				cached.ColorHEX = task.ColorHEX;
				cached.ColorName = task.ColorName;
				cached.ColorPosition = task.ColorPosition;
			};

			#endregion

			#region Обновление исполнителся

			OnTaskAssigned += (s, e) =>
			{
				ITask task = e.NewData;
				ITask cached = Get(task.BoardId, task.Id);
				cached.ExecutorUserId = task.ExecutorUserId;
				cached.ExecutorNick = task.ExecutorNick;
				cached.ExecutorEmail = task.ExecutorEmail;
			};

			#endregion

			#region Обновление планового времени

			OnUpdatePlaningTime += (s, e) =>
			{
				ITask task = e.NewData;
				ITask cached = Get(task.BoardId, task.Id);
				cached.PlanningTime = task.PlanningTime;
			};

			#endregion

			#region Обновление статуса

			OnUpdateStatus += (s, e) =>
			{
				ITask task = e.NewData;
				ITask cached = Get(task.BoardId, task.Id);
				cached.StatusChangeDateTime = task.StatusChangeDateTime;
				cached.TaskStatusId = task.TaskStatusId;
				cached.TaskStatusName = task.TaskStatusName;
				cached.TaskStatusPosition = task.TaskStatusPosition;
			};

			#endregion

			#region Update

			OnUpdate += (s, e) =>
			{
				ITask task = e.NewData;

				using (new UpdateLock(task.BoardId))
				{
					HashSet<ITask> cache = GetTasksCache(task.BoardId);
					if (cache != null)
					{
						ITask cached = cache.FirstOrDefault(x => x.Id == task.Id);
						if (cached != null)
						{
							// удаляем из кеша, что б он обновлися обновленной задачей
							cache.Remove(cached);
						}
						cache.Add(task);
					}
				}
			};

			#endregion
		}

		#region Кеш

		/// <summary>
		/// Таймаут на чтение данных
		/// </summary>
		public static readonly int ReadTimeout = 60;

		/// <summary>
		/// Получение объекта синхронизации для доски
		/// </summary>
		static ReaderWriterLockSlim GetLocker(int boardId)
		{
			// lock, так как коллекция доступна нескольким потокам
			lock (_Lockers)
			{
				if (_Lockers.ContainsKey(boardId))
					return _Lockers[boardId];

				ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
				_Lockers.Add(boardId, lockSlim);
				return lockSlim;
			}
		}
		/// <summary>
		/// Коллекция объектов синхронизации.
		/// Для каждой доски свой объект
		/// </summary>
		static readonly Dictionary<int, ReaderWriterLockSlim> _Lockers = new Dictionary<int, ReaderWriterLockSlim>();
		class WriteLock : IDisposable
		{
			private readonly ReaderWriterLockSlim _LockSlim;

			public WriteLock(int boardId) : this(GetLocker(boardId)) { }

			private WriteLock(ReaderWriterLockSlim lockSlim)
			{
				_LockSlim = lockSlim;
				_LockSlim.EnterWriteLock();
			}

			public void Dispose()
			{
				_LockSlim.ExitWriteLock();
			}
		}
		class ReadLock : IDisposable
		{
			private readonly ReaderWriterLockSlim _LockSlim;

			public ReadLock(int boardId) : this(GetLocker(boardId)) { }

			private ReadLock(ReaderWriterLockSlim lockSlim)
			{
				_LockSlim = lockSlim;
				_LockSlim.TryEnterReadLock(TimeSpan.FromSeconds(ReadTimeout));
			}

			public void Dispose()
			{
				if (_LockSlim.IsReadLockHeld)
					_LockSlim.ExitReadLock();
			}
		}
		class UpdateLock : IDisposable
		{
			private readonly ReaderWriterLockSlim _LockSlim;

			public UpdateLock(int boardId) : this(GetLocker(boardId)) { }

			private UpdateLock(ReaderWriterLockSlim lockSlim)
			{
				_LockSlim = lockSlim;
				_LockSlim.EnterUpgradeableReadLock();
			}

			public void Dispose()
			{
				_LockSlim.ExitUpgradeableReadLock();
			}
		}

		/// <summary>
		/// Возвращает ссылку на большой кеш доски
		/// </summary>
		HashSet<ITask> GetTasksCache(int boardId)
		{
			IEnumerable<CacheKeyValue> key = GetAllTasksKey(boardId);
			HashSet<ITask> allTasks = Cache.Get(key) as HashSet<ITask>;
			return allTasks;
		}
		IEnumerable<CacheKeyValue> GetAllTasksKey(int boardId)
		{
			return Cache.GetKeys(CacheKey.Board, boardId, CacheKey.Task, CacheKey.All);
		}

		public bool AllCached(int boardId)
		{
			return GetTasksCache(boardId) != null;
		}

		#endregion

		#region Задачи

		/// <summary>
		/// Получение задач
		/// ПЕЙДЖИНГА ТУТ БЫТЬ НЕ ДОЛЖНО
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		public List<ITask> Get(TaskFilter filter)
		{
			using (new ReadLock(filter.BoardId))
			{
				HashSet<ITask> allTasks = GetTasksCache(filter.BoardId);

				if (allTasks == null)
				{
					List<ITask> tasks = Repository.Tasks.Get(filter).ToList();
					return tasks;
				}
				else
				{
					// данные по такому фильтру получали, по этому берем из кеша
					IQueryable<ITask> tasks = allTasks.AsQueryable();
					IOrderedQueryable<ITask> ordered = filter.Order(filter.Where(tasks));
					return ordered.ToList();
				}
			}
		}

		/// <summary>
		/// Получение всех задач на доске
		/// </summary>
		/// <param name="boardId"></param>
		/// <returns></returns>
		public void Preload(int boardId)
		{
			IEnumerable<CacheKeyValue> key = GetAllTasksKey(boardId);
			using (new WriteLock(boardId))
			{
				HashSet<ITask> cache = Cache.Get(key) as HashSet<ITask>;
				if (cache == null)
				{
					ITask[] tasks = Repository.Tasks.GetAll(boardId).ToArray();
					cache = new HashSet<ITask>(tasks);
					Cache.Set(key, cache);
				}
			}
		}

		public ITask Get(int boardId, int taskId, bool? isArchive)
		{
			ITask task = isArchive.HasValue && isArchive.Value
				? GetFromArchive(taskId)
				: Get(boardId, taskId);
			return task;
		}
		public ITask Get(int boardId, int taskId)
		{
			// Получаем задачу из кеша, так как мы на нее попадаем из списков, а для заполнения списка используется кеш
			HashSet<ITask> cache = GetTasksCache(boardId);
			if (cache != null)
			{
				using (new ReadLock(boardId))
				{
					ITask task = cache.FirstOrDefault(x => x.Id == taskId);
					if (task == null)
					{
						// На случай если кеш очистился
						task = _Get(taskId);
						cache.Add(task);
					}

					return task;
				}
			}

			return _Get(taskId);
		}

		#endregion		
	}
}
