using System;
using System.Globalization;

namespace Timez.Entities
{
	public class PlanningTimeIsExceeded : TimezException
	{
		public PlanningTimeIsExceeded(int maxHours)
			: base(
				"Суммарное планируемое время задач в статусе превшает заданное в настройках ограничаение."
				+ Environment.NewLine
				+ "Максимальное количество минут: " + maxHours.ToString(CultureInfo.InvariantCulture)
				+ Environment.NewLine
				+ "Вы подтверждаете добавление задачи в статус?") { }

		public override bool Logging { get { return false; } }
		public override bool ReThrow { get { return false; } }
	}
}
