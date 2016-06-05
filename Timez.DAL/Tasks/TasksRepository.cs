using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Timez.Entities;
using Timez.DAL.DataContext;
using System.Data.Linq;

namespace Timez.DAL.Tasks
{
	public interface ITasksRepository
	{
		ITask Create(string name, string desc, IUser user, IProject project, IBoardsColor color, ITasksStatus status, int? planingTime, int creatorUserId);

		/// <summary>
		/// Получение задач всеми возможными способами
		/// </summary>
		IOrderedQueryable<ITask> Get(TaskFilter filter);

		/// <summary>
		/// Получение задачи по иду
		/// включая удаленных
		/// </summary>
		ITask Get(int id);

		/// <summary>
		/// Получение всех задач для доски
		/// </summary>
		IQueryable<ITask> GetAll(int boardId);

		/// <summary>
		/// Перманентное удаление задачи
		/// </summary>
		/// <returns></returns>
		ITask Delete(int id);

		/// <summary>
		/// Быстрое удаление задачи через статус IsDeleted
		/// </summary>
		bool FastDelete(int id);

		/// <summary>
		/// Быстрое изменение статуса задачи
		/// Например, при перетаскивании в канбане
		/// </summary>
		void UpdateStatus(int taskId, ITasksStatus newStatus);

		/// <summary>
		/// Есть ли у пользователя задачи на доске,
		/// где он исполнитель или создатель
		/// </summary>
		bool HasTasks(int boardId, int userId);

		ITasksArchive AddToArchive(ITask task);
		ITasksArchive GetFromArchiveById(int taskId);
		IOrderedQueryable<ITask> GetFromArchiveByBoard(TaskFilter filter);
		void DeleteFromArchive(int taskId);
		void ClearArchive(int boardId);

		/// <summary>
		/// Преназанчение и исполнителей и заказчиков
		/// </summary>        
		void Reassign(int boardId, int fromUserId, int toUserId);

		/// <summary>
		/// Обновляет у всех задач проекта измененное имя проекта
		/// </summary>
		void UpdateProjectName(IProject project);

		/// <summary>
		/// Обновляет денормализованную инфу о статусе в задачах
		/// </summary>
		void UpdateStatus(ITasksStatus status);

		/// <summary>
		/// Обновляет денормализованную инфу о цвете в задачах
		/// </summary>
		void UpdateColor(IBoardsColor color);

		/// <summary>
		/// Обновляет денормализованную инфу о цвете в задачах
		/// </summary>
		void UpdateColorPosition(IBoardsColor color);

		/// <summary>
		/// Обновляет денормализованную инфу о исполнителе
		/// </summary>
		void UpdateExecutor(IUser user);
	}

	class TasksRepository : BaseRepository<TasksRepository>, ITasksRepository
	{
		#region Рабочие задачи
		public ITask Create(string name, string desc, IUser user, IProject project, IBoardsColor color, ITasksStatus status, int? planingTime, int creatorUserId)
		{
			Task task = new Task
			{
				BoardId = project.BoardId,
				ColorId = color.Id,
				ColorHEX = color.Color,
				ColorName = color.Name,
				ColorPosition = color.Position,
				CreationDateTime = DateTimeOffset.Now,
				Description = desc,
				ExecutorUserId = user.Id,
				ExecutorEmail = user.EMail,
				ExecutorNick = user.Nick,
				Name = name,
				ProjectId = project.Id,
				ProjectName = project.Name,
				StatusChangeDateTime = DateTimeOffset.Now,
				TaskStatusId = status.Id,
				TaskStatusPosition = status.Position,
				TaskStatusName = status.Name,
				PlanningTime = planingTime,
				CreatorUserId = creatorUserId
			};

			const string sql = @"
						INSERT INTO [dbo].[Tasks]
								   ([Name]
								   ,[Description]
								   ,[BoardId]
								   ,[CreatorUserId]
								   ,[CreationDateTime]
								   ,[StatusChangeDateTime]
								   ,[PlanningTime]
								   ,[ColorId]
								   ,[ColorHEX]
								   ,[ColorName]
								   ,[ColorPosition]
								   ,[ProjectId]
								   ,[ProjectName]
								   ,[ExecutorUserId]
								   ,[ExecutorNick]
								   ,[ExecutorEmail]
								   ,[TaskStatusId]
								   ,[TaskStatusPosition]
								   ,[TaskStatusName]
								   ,[IsDeleted])
							 VALUES
								   (@Name
								   ,@Description
								   ,@BoardId
								   ,@CreatorUserId
								   ,@CreationDateTime
								   ,@StatusChangeDateTime
								   ,@PlanningTime
								   ,@ColorId
								   ,@ColorHEX
								   ,@ColorName
								   ,@ColorPosition
								   ,@ProjectId
								   ,@ProjectName
								   ,@ExecutorUserId
								   ,@ExecutorNick
								   ,@ExecutorEmail
								   ,@TaskStatusId
								   ,@TaskStatusPosition
								   ,@TaskStatusName
								   ,@IsDeleted);
						SELECT CAST(@@IDENTITY AS INT)";

			using (DbCommand command = _Context.Connection.CreateCommand())
			{
				command.CommandText = sql;
				command.Parameters.Add(new SqlParameter("@Name", task.Name));
				command.Parameters.Add(new SqlParameter("@Description", task.Description));
				command.Parameters.Add(new SqlParameter("@BoardId", task.BoardId));
				command.Parameters.Add(new SqlParameter("@CreatorUserId", task.CreatorUserId));
				command.Parameters.Add(new SqlParameter("@CreationDateTime", task.CreationDateTime));
				command.Parameters.Add(new SqlParameter("@StatusChangeDateTime", task.StatusChangeDateTime));
				command.Parameters.Add(new SqlParameter("@PlanningTime", task.PlanningTime ?? (object)DBNull.Value));
				command.Parameters.Add(new SqlParameter("@ColorId", task.ColorId));
				command.Parameters.Add(new SqlParameter("@ColorHEX", task.ColorHEX));
				command.Parameters.Add(new SqlParameter("@ColorName", task.ColorName));
				command.Parameters.Add(new SqlParameter("@ColorPosition", task.ColorPosition));
				command.Parameters.Add(new SqlParameter("@ProjectId", task.ProjectId));
				command.Parameters.Add(new SqlParameter("@ProjectName", task.ProjectName));
				command.Parameters.Add(new SqlParameter("@ExecutorUserId", task.ExecutorUserId));
				command.Parameters.Add(new SqlParameter("@ExecutorNick", task.ExecutorNick));
				command.Parameters.Add(new SqlParameter("@ExecutorEmail", task.ExecutorEmail));
				command.Parameters.Add(new SqlParameter("@TaskStatusId", task.TaskStatusId));
				command.Parameters.Add(new SqlParameter("@TaskStatusPosition", task.TaskStatusPosition));
				command.Parameters.Add(new SqlParameter("@TaskStatusName", task.TaskStatusName));
				command.Parameters.Add(new SqlParameter("@IsDeleted", task.IsDeleted));
				_Context.Connection.Open();
				task.Id = (int)command.ExecuteScalar();
				_Context.Connection.Close();
			}

			//task.Id = _Context.ExecuteQuery<int>(sql
			//, task.Name
			//, task.Description
			//, task.BoardId
			//, task.CreatorUserId
			//, task.CreationDateTime
			//, task.StatusChangeDateTime
			//, task.PlanningTime ?? (object)DBNull.Value
			//, task.ColorId
			//, task.ColorHEX
			//, task.ColorName
			//, task.ColorPosition
			//, task.ProjectId
			//, task.ProjectName
			//, task.ExecutorUserId
			//, task.ExecutorNick
			//, task.ExecutorEmail
			//, task.TaskStatusId
			//, task.TaskStatusPosition
			//, task.TaskStatusName
			//, task.IsDeleted)
			//.First();

			return task;
		}

		/// <summary>
		/// Получение задач всеми возможными способами
		/// </summary>
		public IOrderedQueryable<ITask> Get(TaskFilter filter)
		{
			// TODO: переписать все на native SQL?
			IQueryable<Task> tasks = _Context.Tasks.Where(x => !x.IsDeleted);
			IOrderedQueryable<ITask> ordered = filter.Order(filter.Where(tasks));
			return ordered;
		}

		/// <summary>
		/// Получение задачи по иду
		/// включая удаленных
		/// </summary>
		public ITask Get(int id)
		{
			return _Get(_Context, id);
		}
		static readonly Func<TimezDataContext, int, Task> _Get =
			CompiledQuery.Compile((TimezDataContext ctx, int taskId) => ctx.Tasks.FirstOrDefault(x => x.Id == taskId));

		/// <summary>
		/// Получение всех задач для доски
		/// </summary>
		public IQueryable<ITask> GetAll(int boardId)
		{
			return _GetAll(_Context, boardId);
		}
		static readonly Func<TimezDataContext, int, IQueryable<Task>> _GetAll =
			CompiledQuery.Compile((TimezDataContext ctx, int boardId) =>
				ctx.Tasks.Where(x => x.BoardId == boardId && !x.IsDeleted));

		/// <summary>
		/// Перманентное удаление задачи
		/// </summary>
		/// <returns></returns>
		public ITask Delete(int id)
		{
			Task task = _Get(_Context, id);
			TimezTask dummy = null;

			if (task != null)
			{
				dummy = new TimezTask(task);
				_Context.Tasks.DeleteOnSubmit(task);
				_Context.SubmitChanges();
			}
			return dummy;
		}

		/// <summary>
		/// Быстрое удаление задачи через статус IsDeleted
		/// </summary>
		public bool FastDelete(int id)
		{
			return _Context.ExecuteCommand("UPDATE [Tasks] SET [IsDeleted] = 1 WHERE [Id] = {0}", id) == 1;
		}

		/// <summary>
		/// Быстрое изменение статуса задачи
		/// Например, при перетаскивании в канбане
		/// </summary>
		public void UpdateStatus(int taskId, ITasksStatus newStatus)
		{
			_Context.ExecuteCommand(@"UPDATE Tasks SET StatusChangeDateTime = {0}, TaskStatusId = {1},  TaskStatusPosition = {2} WHERE Id = {3}",
				DateTimeOffset.Now, // 0
				newStatus.Id, // 1
				newStatus.Position, // 2
				taskId); // 3
		}

		/// <summary>
		/// Есть ли у пользователя задачи на доске,
		/// где он исполнитель или создатель
		/// Удаленные побыстрому неучитываются
		/// </summary>
		public bool HasTasks(int boardId, int userId)
		{
			return _Context.Tasks
				.Any(x => x.BoardId == boardId && !x.IsDeleted
					&& (x.ExecutorUserId == userId || x.CreatorUserId == userId));
		}

		#endregion

		#region Работа с архивом

		public ITasksArchive AddToArchive(ITask task)
		{
			TasksArchive archive = new TasksArchive
			{
				Id = task.Id,
				BoardId = task.BoardId,
				ColorId = task.ColorId,
				ColorHEX = task.ColorHEX,
				ColorName = task.ColorName,
				ColorPosition = task.ColorPosition,
				CreationDateTime = task.CreationDateTime,
				Description = task.Description,
				ExecutorUserId = task.ExecutorUserId,
				ExecutorEmail = task.ExecutorEmail,
				ExecutorNick = task.ExecutorNick,
				Name = task.Name,
				ProjectId = task.ProjectId,
				ProjectName = task.ProjectName,
				StatusChangeDateTime = task.StatusChangeDateTime,
				PlanningTime = task.PlanningTime,
				CreatorUserId = task.CreatorUserId
			};

			_Context.TasksArchives.InsertOnSubmit(archive);
			_Context.SubmitChanges();

			return archive;
		}

		public ITasksArchive GetFromArchiveById(int taskId)
		{
			return _GetFromArchiveById(_Context, taskId);
		}
		static readonly Func<TimezDataContext, int, TasksArchive> _GetFromArchiveById =
			CompiledQuery.Compile((TimezDataContext ctx, int taskId)
				=> ctx.TasksArchives.FirstOrDefault(x => x.Id == taskId));

		public IOrderedQueryable<ITask> GetFromArchiveByBoard(TaskFilter filter)
		{
			var tasks = _Context.TasksArchives;
			IOrderedQueryable<ITask> ordered = filter.Order(filter.Where(tasks));
			return ordered;
		}

		public void DeleteFromArchive(int taskId)
		{
			var task = _GetFromArchiveById(_Context, taskId);
			_Context.TasksArchives.DeleteOnSubmit(task);
			_Context.SubmitChanges();
		}

		public void ClearArchive(int boardId)
		{
			_Context.ExecuteCommand(@"
				DELETE FROM [Tasks] 
				WHERE [BoardId] = {0} AND EXISTS(SELECT * FROM [TasksArchive] a WHERE a.[ID] = [Tasks].[ID] AND [Tasks].[IsDeleted] = 1)", boardId);

			_Context.ExecuteCommand(@"
				DELETE FROM [TasksArchive] 
				WHERE [BoardId] = {0}", boardId);
		}

		#endregion

		/// <summary>
		/// Преназанчение и исполнителей и заказчиков
		/// </summary>        
		public void Reassign(int boardId, int fromUserId, int toUserId)
		{
			_Context.ReassignTasks(boardId, fromUserId, toUserId);
		}

		#region Поддержка денормализации

		/// <summary>
		/// Обновляет у всех задач проекта измененное имя проекта
		/// </summary>
		public void UpdateProjectName(IProject project)
		{
			// Масимально быстрое обновление, так как задач может быть много
			_Context.ExecuteCommand(@"UPDATE Tasks SET ProjectName = {0} WHERE ProjectId = {1}",
				project.Name,
				project.Id);
		}

		/// <summary>
		/// Обновляет денормализованную инфу о статусе в задачах
		/// </summary>
		public void UpdateStatus(ITasksStatus status)
		{
			_Context.ExecuteCommand(@"UPDATE Tasks SET TaskStatusName = {0}, TaskStatusPosition = {1} WHERE TaskStatusId = {2}",
				status.Name, // 0
				status.Position, // 1
				status.Id); // 2
		}

		/// <summary>
		/// Обновляет денормализованную инфу о цвете в задачах
		/// </summary>
		public void UpdateColor(IBoardsColor color)
		{
			// Масимально быстрое обновление, так как задач может быть много
			_Context.ExecuteCommand(@"UPDATE Tasks SET ColorHEX = {0}, ColorName = {1} WHERE ColorId = {2}",
				color.Color,
				color.Name,
				color.Id);
		}

		/// <summary>
		/// Обновляет денормализованную инфу о цвете в задачах
		/// </summary>
		public void UpdateColorPosition(IBoardsColor color)
		{
			// Масимально быстрое обновление, так как задач может быть много
			_Context.ExecuteCommand(@"UPDATE Tasks SET ColorPosition = {0} WHERE ColorId = {1}",
				color.Position,
				color.Id);
		}

		/// <summary>
		/// Обновляет денормализованную инфу о исполнителе
		/// </summary>
		public void UpdateExecutor(IUser user)
		{
			// Масимально быстрое обновление, так как задач может быть много
			_Context.ExecuteCommand(@"UPDATE Tasks SET ExecutorNick={0}, ExecutorEmail={1} WHERE ExecutorUserId={2}",
				user.Nick,
				user.EMail,
				user.Id);
		}

		#endregion
	}
}
