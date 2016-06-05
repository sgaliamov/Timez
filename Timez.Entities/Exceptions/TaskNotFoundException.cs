using Timez.Entities;

namespace Timez.BLL.Tasks
{
    public class TaskNotFoundException : TimezException
    {
        public TaskNotFoundException()
            : base("Задача не найдена") { }

        public override bool Logging { get { return true; } }
    }
}
