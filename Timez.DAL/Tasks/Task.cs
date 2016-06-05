using Timez.Entities;

namespace Timez.DAL.DataContext
{
    internal partial class Task : ITask
    {
        #region Переопределяем базовые методы в целях оптимизации

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Task);
        }

        /// <summary>
        /// Задачи считаются одинаковыми, если у них одинаковые ид
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Equals(Task obj)
        {
            return obj != null && Id == obj.Id;
        }

        #endregion
    }

    //internal partial class TasksStatusTime : ITasksStatusTime
    //{
    //    public override int GetHashCode()
    //    {
    //        return TaskId.GetHashCode() + TaskStatusId.GetHashCode();
    //    }

    //    public override bool Equals(object obj)
    //    {
    //        return Equals(obj as TasksStatusTime);
    //    }

    //    public bool Equals(TasksStatusTime obj)
    //    {
    //        return obj != null
    //                && obj.TaskId.Equals(TaskId)
    //                && obj.TaskStatusId.Equals(TaskStatusId)
    //                && obj.UdateDateTime.Equals(UdateDateTime);
    //    }
    //}

    internal partial class TasksComment : ITasksComment { }
}
