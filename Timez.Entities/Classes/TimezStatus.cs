namespace Timez.Entities
{
    public class TimezStatus : ITasksStatus // !!! ОБНОВИ КОНСТРУКТОР ПОСЛЕ ИЗМЕНЕНИЯ ИНТЕРФЕЙСА !!!
    {
        public const int ArchiveStatusId = -1;

        public TimezStatus(){}

        public TimezStatus(ITasksStatus status)
        {
            Id = status.Id;
            BoardId = status.BoardId;
            IsBacklog = status.IsBacklog;
            MaxTaskCountPerUser = status.MaxTaskCountPerUser;
            Name = status.Name;
            NeedTimeCounting = status.NeedTimeCounting;
            Position = status.Position;
            PlanningRequired = status.PlanningRequired;
            MaxPlanningTime = status.MaxPlanningTime;
        }

        public int Id { get; set; }
        public int BoardId { get; set; }
        public bool IsBacklog { get; set; }
        public int? MaxTaskCountPerUser { get; set; }
        public string Name { get; set; }
        public bool NeedTimeCounting { get; set; }
        public int Position { get; set; }
        public bool PlanningRequired { get; set; }
        public int? MaxPlanningTime { get; set; }
    }
}
