namespace Timez.Entities
{
    public class TasksExistsException : TimezException
    {
        public TasksExistsException(IUser user, IBoard board)
            : base("На пользователя " + user.Nick + " назначены задачи на доске \"" + board.Name + "\".") { }

        public override bool Logging { get { return false; } }
    }
}
