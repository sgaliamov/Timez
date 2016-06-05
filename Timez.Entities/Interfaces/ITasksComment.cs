using System;

namespace Timez.Entities
{   
    public interface ITasksComment
    {
        int Id { get; set; }
        int TaskId { get; set; }
        string Comment { get; set; }
        int AuthorUserId { get; set; }
        string AuthorUser { get; set; }
        int? ParentId { get; set; }
        string ParentComment { get; set; }
        DateTimeOffset CreationDate { get; set; }
        bool IsDeleted { get; set; }
        int BoardId { get; set; }
    }
}