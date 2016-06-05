using System;
using Timez.Entities;

namespace Timez.DAL.DataContext
{
    internal partial class TasksArchive : ITasksArchive
    {       
        public int TaskStatusId
        {
            get { return TimezStatus.ArchiveStatusId; }
            set { throw new NotImplementedException(); }
        }

        public int TaskStatusPosition
        {
            get { return int.MaxValue; }
            set { throw new NotImplementedException(); }
        }

        public string TaskStatusName
        {
            get { return "Archive"; }
            set { throw new NotImplementedException(); }
        }

        public bool IsDeleted
        {
            get { return false; }
            set { throw new NotImplementedException(); }
        }
    }
}
