namespace Timez.Entities
{
    internal interface IUsersWorkingTime
    {
        int DayOfWeek { get; set; }
        int Id { get; set; }
        int TimeEnd { get; set; }
        int TimeStart { get; set; }
        int UserId { get; set; }
    }
}
