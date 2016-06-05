using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Alias;
using Common.Extentions;
using Timez.Entities;

namespace Timez.BLL.EventHistory
{
	public sealed class EventHistoryUtility : BaseUtility<EventHistoryUtility>
	{
		#region Base
		/// <summary>
		/// Регистрирует новое событие.
		/// </summary>
		/// <param name="eventText">Текст события</param>
		/// <param name="user">Инициатор события</param>
		/// <param name="task">Задача над которой совершили действие</param>
		/// <param name="eventType">Тип события</param>
		public void Add(IUser user, ITask task, string eventText, EventType eventType)
		{
			Repository.EventHistory.Add(eventText, user, task, eventType);
		}

		public List<IEventHistory> Get(EventDataFilter filter, out int totalCount)
		{
			//totalCount = Repository.EventHistory.Get(filter).Count();
			return Repository.EventHistory.Get(filter, out totalCount).ToList();
		}

		public void Clear(int id)
		{
			Repository.EventHistory.Clear(id);
		}
		#endregion

		internal override void Init()
		{
			Utility.Tasks.OnCreate += (s, e) => TasksUtility_OnCreate(e.Data);
			Utility.Tasks.OnDelete.Add(TasksUtility_OnDelete);

			Utility.Tasks.OnUpdate.Add(TasksUtility_OnUpdate);
			Utility.Tasks.OnUpdateStatus.Add(TasksUtility_OnUpdate);

			Utility.Tasks.OnTaskAssigned += (s, e) => TasksUtility_OnAssigned(e.OldData, e.NewData);
			Utility.Tasks.OnUpdateColor += (s, e) => TasksUtility_OnUpdateColor(e.OldData, e.NewData);
			Utility.Tasks.OnUpdatePlaningTime += (s, e) => TasksUtility_OnUpdatePlaningTime(e.OldData, e.NewData);
			Utility.Tasks.OnUpdateProject += (s, e) => TasksUtility_OnUpdateProject(e.OldData, e.NewData);
			Utility.Tasks.OnTaskToArchive.Add(TasksUtility_OnTaskToArchive);
			Utility.Tasks.OnRestore += TasksUtility_OnRestore;

			Utility.Tasks.OnTaskCountLimitIsReached += Tasks_OnTaskCountLimitIsReached;
			Utility.Tasks.OnTaskPlanningTimeIsExceeded += Tasks_OnTaskPlanningTimeIsExceeded;
		}

		internal override void Free()
		{
			Utility.Tasks.OnTaskCountLimitIsReached -= Tasks_OnTaskCountLimitIsReached;
			Utility.Tasks.OnTaskPlanningTimeIsExceeded -= Tasks_OnTaskPlanningTimeIsExceeded;
		}

		void Tasks_OnTaskCountLimitIsReached(ITask task, int count)
		{
			Add(Utility.Users.CurrentUser, task,
				EventType.CountLimitIsReached.GetAlias(), EventType.CountLimitIsReached | EventType.Warning);

		}

		/// <summary>
		/// Случилось превышение допустимого времени
		/// </summary>
		/// <param name="task">изменяемая задача</param>
		/// <param name="status">желаемый статус, в котором произошло превышение лимитов</param>
		/// <param name="minutes">сколько уже времени запланировано</param>
		void Tasks_OnTaskPlanningTimeIsExceeded(ITask task, ITasksStatus status, int minutes)
		{
			EventType type = EventType.PlanningTimeIsExceeded;

			// Время гарантированно должно быть у стауса
			// ReSharper disable PossibleInvalidOperationException
			int time = status.MaxPlanningTime.Value;
			// ReSharper restore PossibleInvalidOperationException
			if (minutes > time + 12 * 60)
			{
				time += 12 * 60;
				type |= EventType.Error;
			}
			else if (minutes > time)
				type |= EventType.Warning;

			Add(Utility.Users.CurrentUser, task,
				EventType.PlanningTimeIsExceeded.GetAlias().Params(time), type);

		}

		void TasksUtility_OnRestore(object sender, EventArgs<ITask> arg)
		{
			var task = arg.Data;
			Add(
				Utility.Users.CurrentUser,
				task, EventType.TaskRestore.GetAlias(),
				EventType.TaskRestore);
		}

		void TasksUtility_OnTaskToArchive(object sender, EventArgs<ITask> arg)
		{
			var task = arg.Data;
			Add(
				Utility.Users.CurrentUser,
				task, EventType.TaskToArchive.GetAlias(),
				EventType.TaskToArchive);
		}

		void TasksUtility_OnAssigned(ITask oldTask, ITask newTask)
		{
			Add(Utility.Users.CurrentUser, newTask,
				EventType.TaskAssigned.GetAlias() + " '" + newTask.ExecutorNick + "'",
				EventType.TaskAssigned);
		}

		void TasksUtility_OnUpdateColor(ITask oldTask, ITask newTask)
		{
			Add(Utility.Users.CurrentUser, newTask,
				EventType.TaskColorChanged.GetAlias() + ": '" + oldTask.ColorName + "' → '" + newTask.ColorName + "'",
				EventType.TaskColorChanged);
		}

		void TasksUtility_OnUpdatePlaningTime(ITask oldTask, ITask newTask)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(EventType.PlaningTimeChanged.GetAlias());
			builder.Append(oldTask.PlanningTime.HasValue ? " c " + oldTask.PlanningTime.ToString() : "");
			builder.Append(newTask.PlanningTime.HasValue ? " на " + newTask.PlanningTime.ToString() : "");
			Add(Utility.Users.CurrentUser, newTask, builder.ToString(), EventType.PlaningTimeChanged);
		}

		void TasksUtility_OnUpdateProject(ITask oldTask, ITask newTask)
		{
			Add(Utility.Users.CurrentUser, newTask,
				EventType.ProjectChanged.GetAlias() + ": '" + oldTask.ProjectName + "' → '" + newTask.ProjectName + "'",
				EventType.ProjectChanged);
		}

		void TasksUtility_OnUpdate(object sender, UpdateEventArgs<ITask> args)
		{
			string eventText = GetEventText(args.OldData, args.NewData, EventType.Update);
			if (!string.IsNullOrWhiteSpace(eventText))
				Add(Utility.Users.CurrentUser, args.NewData, eventText, EventType.Update);
		}

		void TasksUtility_OnDelete(object sender, EventArgs<ITask> arg)
		{
			ITask entity = arg.Data;
			string eventText = GetEventText(entity, entity, EventType.Delete);
			Add(Utility.Users.CurrentUser, entity, eventText, EventType.Delete);
		}

		void TasksUtility_OnCreate(ITask entity)
		{
			string eventText = GetEventText(null, entity, EventType.CreateTask);
			Add(Utility.Users.CurrentUser, entity, eventText, EventType.CreateTask);
		}

		/// <summary>
		/// Возвращает текст события
		/// </summary>
		/// <param name="oldTask"></param>
		/// <param name="changingTask"></param>
		/// <param name="eventType"></param>
		/// <returns></returns>
		string GetEventText(ITask oldTask, ITask changingTask, EventType eventType)
		{
			StringBuilder eventText = new StringBuilder();

			#region Update
			if ((eventType & EventType.Update) == EventType.Update)
			{
				if (oldTask.TaskStatusId != changingTask.TaskStatusId)
				{
					var oldTaskStatus = Utility.Statuses.Get(oldTask.BoardId, oldTask.TaskStatusId);
					var newTaskStatus = Utility.Statuses.Get(changingTask.BoardId, changingTask.TaskStatusId);
					eventText.AppendLine("Изменен статус задачи: '" + oldTaskStatus.Name + "' → '" + newTaskStatus.Name);
				}

				if (oldTask.Name != changingTask.Name)
				{
					eventText.AppendLine("Изменено название: '" + oldTask.Name + "' → '" + changingTask.Name);
				}

				if (oldTask.Description != changingTask.Description)
				{
					eventText.Append("Изменено описание задачи: '" + oldTask.Description + "' → '" + changingTask.Description + "'");
				}

				return eventText.ToString();
			}
			#endregion

			// если задача добавлена
			if ((eventType & EventType.CreateTask) == EventType.CreateTask)
				return "Создана задача №" + changingTask.Id + " и назначена на пользователя '" + changingTask.ExecutorNick + "'";

			// если задача удалена
			if ((eventType & EventType.Delete) == EventType.Delete)
				return "Задача '" + changingTask.Name + "' удалена";

			throw new NotImplementedException();
		}
	}
}
