using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Timez.Entities;

namespace Timez.BLL.Tasks
{
	public sealed partial class TasksStatusesUtility
	{
		public TasksStatusesUtility()
		{
			OnCreate += (s, e) =>
							{
								ITasksStatus status = e.Data;

								CacheClear(status.BoardId);
							};

			OnDelete += (s, e) =>
			{
				ITasksStatus status = e.Data;

				CacheClear(status.BoardId);
			};

			OnUpdate += (s, e) =>
			{
				ITasksStatus status = e.Data;

				CacheClear(status.BoardId);
			};
		}

		private void CacheClear(int boardId, int? statusId = null)
		{
			var key = Cache.GetKeys(
				CacheKey.Board, boardId,
				CacheKey.Status, CacheKey.All);
			Cache.Clear(key);

			if (statusId.HasValue)
			{
				key = Cache.GetKeys(CacheKey.Status, statusId.Value);
				Cache.Clear(key);
			}
		}
	}
}
