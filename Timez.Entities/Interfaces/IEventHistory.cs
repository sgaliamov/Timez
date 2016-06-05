namespace Timez.Entities
{
    using System;
    public interface IEventHistory : IId
    {
        string Event { get; set; }
        DateTimeOffset EventDateTime { get; set; }
        int TaskId { get; set; }
        string TaskName { get; set; }
        int UserId { get; set; }
        string UserNick { get; set; }
        EventType EventType { get; }
        int ProjectId { get; set; }
        string ProjectName { get; set; }
        int BoardId { get; set; }
    }
}
