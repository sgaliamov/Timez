using System.Collections.Generic;
using Timez.Entities;
using CacheKeyValue = System.Collections.Generic.KeyValuePair<Timez.BLL.CacheKey, string>;

namespace Timez.BLL.Boards
{
	public sealed partial class BoardsUtility
	{
		public BoardsUtility()
		{
			// добавление исполнителя
			OnAddUserToBoard += (userId, boardId) =>
			{
				Cache.Clear(GetBoardCacheKey(boardId));

				var key = Cache.GetKeys(CacheKey.User, userId);
				Cache.Clear(key);

				key = Cache.GetKeys(
				   CacheKey.Board, boardId,
				   CacheKey.Participant, CacheKey.All);
				Cache.Clear(key);
			};

			// Исключение с доски
			OnRemoveUserFromBoard += entity =>
			{
				Cache.Clear(GetBoardCacheKey(entity.BoardId));

				var key = Cache.GetKeys(CacheKey.User, entity.UserId);
				Cache.Clear(key);

				Cache.Clear(Cache.GetKeys(CacheKey.Board, entity.BoardId));
			};

			OnUpdateUserOnBoard += (sender, e) =>
			{
				IBoardsUser user = e.Data;

				// список участников на доске
				var key = Cache.GetKeys(
					CacheKey.Board, user.BoardId,
					CacheKey.Participant, CacheKey.All);
				Cache.Clear(key);

				// список досок пользователя
				key = GetAllBoardsOfUserCacheKey(user.UserId);
				Cache.Clear(key);
			};

			OnDelete += (sender, e) =>
			{
				int boardId = e.Data;
				foreach (var item in GetParticipants(boardId))
					Cache.Clear(Cache.GetKeys(CacheKey.User, item.User.Id));

				IEnumerable<CacheKeyValue> key = GetBoardCacheKey(boardId);
				Cache.Clear(key);
			};

			OnUpdate.Add((sender, e) =>
			{
				int id = e.Data.Id;
				Cache.Clear(GetBoardCacheKey(id));

				// Очищаем кеш доски у учасников
				List<UserSettings> participants = GetParticipants(id);
				foreach (var user in participants)
				{
					var key = GetAllBoardsOfUserCacheKey(user.User.Id);
					Cache.Clear(key);
				}
			});

			OnCreate += (s, e) =>
			{
				IBoardsUser board = e.Data;
				// очищается все что связано с пользователем:
				// - список досок
				// - список проектов, так как на новой доске создается новый проект
				var key = Cache.GetKeys(CacheKey.User, board.UserId);
				Cache.Clear(key);
			};

			OnUpdateColor += (s, e) =>
			{
				IBoardsColor color = e.Data;

				var key = ColorsCacheKey(color.BoardId);
				Cache.Clear(key);
			};

			OnDeleteColor += (s, e) =>
			{
				IBoardsColor color = e.Data;

				var key = ColorsCacheKey(color.BoardId);
				Cache.Clear(key);
			};

			OnCreateColor += (s, e) =>
			{
				IBoardsColor color = e.Data;

				var key = ColorsCacheKey(color.BoardId);
				Cache.Clear(key);
			};
		}

		IEnumerable<CacheKeyValue> GetBoardCacheKey(int boardId)
		{
			var key = Cache.GetKeys(CacheKey.Board, boardId);
			return key;
		}

		/// <summary>
		/// Все доски пользователя
		/// </summary>
		IEnumerable<CacheKeyValue> GetAllBoardsOfUserCacheKey(int userId)
		{
			var key = Cache.GetKeys(
						CacheKey.User, userId,
						CacheKey.Board, CacheKey.All);
			return key;
		}
		IEnumerable<CacheKeyValue> ColorsCacheKey(int boardId)
		{
			var key = Cache.GetKeys(
				CacheKey.Board, boardId,
				CacheKey.Color, CacheKey.All);

			return key;
		}
	}
}
