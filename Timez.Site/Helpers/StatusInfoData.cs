using System.Collections.Generic;
using Timez.Entities;

namespace Timez.Helpers
{
    /// <summary>
    /// Данные о статусе
    /// </summary>
    public class StatusInfoData
    {
        public ITasksStatus Status { get; set; }
        public Dictionary<string, int> PlanningTimes { get; set; }
        public int TotalTasks { get; set; }
    }
}