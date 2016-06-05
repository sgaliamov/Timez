using System;
using System.Data.Linq;
using System.Linq;
using Timez.DAL.DataContext;
using Timez.Entities;

namespace Timez.DAL.Tasks
{
    public interface ITasksStatusesRepository
    {
        /// <summary>
        /// Создает статус
        /// помещается перед беклогом триггером
        /// </summary>
        ITasksStatus Create(
            int boardId, string name,
            bool isBacklog,
            bool planningRequired, int? maxTaskCountPerUser, int? maxPlanningTime);

        IOrderedQueryable<ITasksStatus> GetByBoard(int boardId);
        ITasksStatus Get(int statusId);
        void Delete(ITasksStatus status);
    }

    /// <summary>
    /// Статусы задач
    /// </summary>
    class TasksStatusesRepository : BaseRepository<TasksStatusesRepository>, ITasksStatusesRepository
    {
        /// <summary>
        /// Создает статус
        /// помещается перед беклогом триггером
        /// </summary>
        public ITasksStatus Create(
            int boardId, string name,
            bool isBacklog,
            bool planningRequired, int? maxTaskCountPerUser, int? maxPlanningTime)
        {
            // Создаем
            TasksStatus status = new TasksStatus
            {
                BoardId = boardId,
                IsBacklog = isBacklog,
                Name = name,
                NeedTimeCounting = false,
                PlanningRequired = planningRequired,
                MaxTaskCountPerUser = maxTaskCountPerUser,
                MaxPlanningTime = maxPlanningTime,
                Position = 0
            };

            _Context.TasksStatus.InsertOnSubmit(status);
            _Context.SubmitChanges();

            // так как срабатывает задающий позицию триггер, обновляем позицию вручную
            // создаем обертку так как status.Position задавать нельзя, так как это назначение будет пытаться обновить БД при последующих SubmitChanges
            return new TimezStatus(status) { Position = status.Id };
        }

        public IOrderedQueryable<ITasksStatus> GetByBoard(int boardId)
        {
            return _GetByBoard(_Context, boardId);
        }
        static readonly Func<TimezDataContext, int, IOrderedQueryable<TasksStatus>> _GetByBoard =
            CompiledQuery.Compile((TimezDataContext ctx, int boardId) =>
                ctx.TasksStatus
                .Where(s => s.BoardId == boardId)
                .OrderBy(s => s.Position));

        public ITasksStatus Get(int statusId)
        {
            return _Get(_Context, statusId);
        }
        static readonly Func<TimezDataContext, int, TasksStatus> _Get =
            CompiledQuery.Compile((TimezDataContext ctx, int statusId) =>
                ctx.TasksStatus.FirstOrDefault(s => s.Id == statusId));

        public void Delete(ITasksStatus status)
        {
            _Context.TasksStatus.DeleteOnSubmit(status as TasksStatus);
            _Context.SubmitChanges();
        }
    }
}
