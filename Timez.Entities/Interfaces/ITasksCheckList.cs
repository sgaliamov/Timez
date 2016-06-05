namespace Timez.Entities
{
    public interface ITasksCheckList
    {
        int Id { get; set; }
        bool IsDone { get; set; }
        string Name { get; set; }
        int Position { get; set; }
        int TaskId { get; set; }
    }
}
