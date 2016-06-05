using Timez.Entities;

namespace Timez.DAL.DataContext
{
    internal partial class TasksStatus : ITasksStatus
    {
        public override string ToString()
        {
            return Name + "|" + Id.ToString();
        }
    }
}
