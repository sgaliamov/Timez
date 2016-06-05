namespace Timez.Entities
{
    public class PlanningTimeRequered : TimezException
    {
        public PlanningTimeRequered(ITasksStatus status)
            : base("В статусе \"" + status.Name + "\" обязательно требуется указывать планируемое время.") { }

        public override bool Logging { get { return false; } }
        public override bool ReThrow { get { return false; } }
    }
}
