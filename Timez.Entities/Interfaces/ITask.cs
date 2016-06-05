using System;
namespace Timez.Entities
{
    public interface ITask : IId
    {
        string Name { get; set; }
        string Description { get; set; }
        int BoardId { get; set; }
        int CreatorUserId { get; set; }
        DateTimeOffset CreationDateTime { get; set; }
        DateTimeOffset StatusChangeDateTime { get; set; }
        int? PlanningTime { get; set; }
        int ColorId { get; set; }
        string ColorHEX { get; set; }
        string ColorName { get; set; }
        int ColorPosition { get; set; }
        int ProjectId { get; set; }
        string ProjectName { get; set; }        
        int ExecutorUserId { get; set; }
        string ExecutorNick { get; set; }
        string ExecutorEmail { get; set; }
        int TaskStatusId { get; set; }
        int TaskStatusPosition { get; set; }
        string TaskStatusName { get; set; }
        bool IsDeleted { get; set; }
    }
}
