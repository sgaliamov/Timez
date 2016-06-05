using System;
using System.Globalization;

namespace Timez.Entities
{
    public class TaskCountLimitIsReached : TimezException
    {
        public TaskCountLimitIsReached(int maxCount)
            : base("Достигнут лимит количества задач в статусе." 
				+ Environment.NewLine
				+ "Максимальное количество: " + maxCount.ToString(CultureInfo.InvariantCulture)
				+ Environment.NewLine 
				+ "Вы подтверждаете добавление задачи в статус?") { }

        public override bool Logging { get { return false; } }
        public override bool ReThrow { get { return false; } }
    }
}
