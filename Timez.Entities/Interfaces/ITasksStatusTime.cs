using System;
namespace Timez.Entities
{
    public interface ITasksStatusTime
    {
        int BoardId { get; set; }
        int ElapsedTime { get; set; }        
        int TaskId { get; set; }
        int TaskStatusId { get; set; }
        DateTimeOffset UdateDateTime { get; set; }
    }
}
