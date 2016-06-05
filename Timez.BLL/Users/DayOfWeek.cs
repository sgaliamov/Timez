using Common.Alias;

namespace Timez.BLL.Users
{
    public enum TimezDayOfWeek
    {
        [Alias("Понедельник")]
        Monday = 1,

        [Alias("Вторник")]
        Tuesday = 2,

        [Alias("Среда")]
        Wednesday = 3,

        [Alias("Четверг")]
        Thursday = 4,

        [Alias("Пятница")]
        Friday = 5,

        [Alias("Суббота")]
        Saturday = 6,

        [Alias("Воскресенье")]
        Sunday = 7,

        [Alias("Будни")]
        WorkingDays = -5,

        [Alias("Все дни недели")]
        AllDays = -7
    }
}
