using System;

namespace Timez.BLL.Tasks
{
	/// <summary>
	/// Виды лимитов
	/// </summary>
	[Flags]
	public enum Limits
	{
		NoLimits = 0,
		TaskCountLimitIsReached = 1,
		PlanningTimeIsExceeded = 2,

		/// <summary>
		/// Происходит обновление через попап
		/// Что бы не ругался при равенстве максимальному количеству
		/// </summary>
		PopupUpdating = 4
	}
}