using System;

namespace Timez.Entities
{
    public interface ITasksArchive : ITask
    {
        int ArchiveId { get; }
    }
}
