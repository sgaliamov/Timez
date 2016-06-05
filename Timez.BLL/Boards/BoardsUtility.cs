using System;
using System.Collections.Generic;
using System.Linq;
using Common.Extentions;
using System.Transactions;
using Timez.Entities;
using CacheKeyValue = System.Collections.Generic.KeyValuePair<Timez.BLL.CacheKey, string>;

namespace Timez.BLL.Boards
{
	/// <summary>
	/// Логика досок
	/// </summary>
	public sealed partial class BoardsUtility : BaseUtility<BoardsUtility>
	{
		#region Event handlers

		public Listener<EventArgs<IBoardsUser>> OnCreate = new Listener<EventArgs<IBoardsUser>>();
		public Listener<EventArgs<IBoard>> OnUpdate = new Listener<EventArgs<IBoard>>();
		public Listener<EventArgs<int>> OnDeleting = new Listener<EventArgs<int>>();
		public Listener<EventArgs<int>> OnDelete = new Listener<EventArgs<int>>();


		public event EntityEventHandler<IBoardsUser> OnRemoveUserFromBoard = delegate { };
		public Listener<EventArgs<IBoardsUser>> OnUpdateUserOnBoard = new Listener<EventArgs<IBoardsUser>>();

		/// <summary>
		/// При добавлении пользователя на доску
		/// 1 - пользователь
		/// 2 - ид доски
		/// </summary>
		public event Action<int, int> OnAddUserToBoard = delegate { };


		/// <summary>
		/// Пописчики:
		/// - обновление кеша задач
		/// </summary>
		public Listener<EventArgs<IBoardsColor>> OnUpdateColor = new Listener<EventArgs<IBoardsColor>>();
		public Listener<EventArgs<IBoardsColor>> OnDeleteColor = new Listener<EventArgs<IBoardsColor>>();
		public Listener<EventArgs<IBoardsColor>> OnCreateColor = new Listener<EventArgs<IBoardsColor>>();

		#endregion

		#region Boards

		/// <summary>
		/// Создание доски пользователем
		/// </summary>
		/// <param name="name"></param>
		/// <param name="description"></param>
		/// <param name="user">Кто создает доску</param>
		/// <param name="refreshPeriod"></param>
		/// <param name="organization"></param>
		/// <returns></returns>
		/// <exception cref="TariffException"></exception>
		public IBoard Create(string name, string description, IUser user, int? refreshPeriod = null, IOrganization organization = null)
		{
			CheckLimits(organization);

			using (TransactionScope scope = new TransactionScope())
			{
				// Создаем
				IBoard board = Repository.Boards.Create(
					name, description, refreshPeriod
					, organization != null ? organization.Id : (int?)null);

				// Добавляем владельца
				IBoardsUser boardsUser = Repository.BoardsUsers.AddUserToBoard(board.Id, user.Id, UserRole.All);

				// Добавляем статусы поумолчанию
				Repository.TasksStatuses.Create(board.Id, "Беклог", true, false, null, null);
				Repository.TasksStatuses.Create(board.Id, "Сделать", false, true, 10, 480);
				Repository.TasksStatuses.Create(board.Id, "Делается сейчас", false, true, 2, null);
				Repository.TasksStatuses.Create(board.Id, "Отдано на тестирование", false, false, 10, null);

				// Цвета поумолчанию
				AddColor(board.Id, "Первоочередное", "#D8004B");
				AddColor(board.Id, "Важное", "#FCD62D");
				AddColor(board.Id, "Обычные задачи", "#7BB28A", true);
				AddColor(board.Id, "Не срочное", "#1FACE1");

				// Создать проект
				Utility.Projects.Create(board.Id, "Проект", user);

				OnCreate.Invoke(new EventArgs<IBoardsUser>(boardsUser));
				scope.Complete();
				return board;
			}
		}

		private void CheckLimits(IOrganization organization)
		{
			// в личной организации можно создавать сколько угодно досок, 
			// поэтому, если организация не указана, лимиты не проверяем
			if (organization == null)
				return;

			int? count = Utility.Tariffs.GetAvailableBoardsCount(organization);
			if (count.HasValue && count <= 0)
				throw new TariffException("В компании " + organization.Name + " достигнут лимит досок.");
		}

		/// <summary>
		/// Получение досок
		/// Активные утвержденные доски
		/// </summary>
		public List<IBoard> GetByUser(int userId)
		{
			var key = GetAllBoardsOfUserCacheKey(userId);
			return Cache.Get(key, () => Repository.Boards.GetByUserId(userId).ToList());
		}

		public List<IBoard> GetByOrganization(int organizationId)
		{
			return Repository.Boards.GetByOrganization(organizationId).ToList();
		}

		/// <summary>
		/// Получает доску
		/// </summary>
		public IBoard Get(int boardId)
		{
			IEnumerable<KeyValuePair<CacheKey, string>> key = GetBoardCacheKey(boardId);
			IBoard board = Cache.Get(key, () => Repository.Boards.Get(boardId));
			return board;
		}

		public void Update(int boardId, string name, string description, int? refreshPeriod, IOrganization organization)
		{
			using (TransactionScope scope = new TransactionScope())
			{
				IBoard board = Repository.Boards.Get(boardId);
				int? organizationId = organization != null ? organization.Id : (int?)null;
				if (board.OrganizationId != organizationId)
					CheckLimits(organization);

				board.Name = name;
				board.Description = description;
				board.RefreshPeriod = refreshPeriod;
				if (board.OrganizationId.HasValue && board.OrganizationId != organizationId)
				{
					EmployeeSettings userSettings = Utility.Organizations.GetUserSettings(board.OrganizationId.Value, Utility.Users.CurrentUser.Id);
					if (!userSettings.Settings.IsAdmin)
						throw new InvalidOperationTimezException("Нет прав на изменение организации.");
				}
				board.OrganizationId = organizationId;

				Repository.SubmitChanges();
				OnUpdate.Invoke(new EventArgs<IBoard>(board));
				scope.Complete();
			}
		}

		/// <summary>
		/// Удаление доски
		/// </summary>
		public void Delete(int id)
		{
			using (TransactionScope scope = new TransactionScope())
			{
				// no subscribers
				OnDeleting.Invoke(new EventArgs<int>(id));

				Repository.Boards.Delete(id);

				// BoardsModel - очиска кеша доски и кеша учасников на ней
				OnDelete.Invoke(new EventArgs<int>(id));

				scope.Complete();
			}
		}

		#endregion

		#region Users on boards

		/// <summary>
		/// Добавление на доску
		/// </summary>
		public bool AddUserToBoard(int boardId, int userId, UserRole role)
		{
			if (Repository.BoardsUsers.GetBoardsUsers(boardId, userId) != null)
				return false;

			Repository.BoardsUsers.AddUserToBoard(boardId, userId, role);

			OnAddUserToBoard(userId, boardId);

			return true;
		}

		/// <summary>
		/// Добавляет исполнителя
		/// </summary>
		public void AddExecutorToBoard(int boardId, int userId)
		{
			AddUserToBoard(boardId, userId, UserRole.Executor);
		}

		/// <summary>
		/// Учасники на доске, включая неактивных
		/// </summary>
		public List<UserSettings> GetParticipants(int boardId)
		{
			var key = Cache.GetKeys(
					CacheKey.Board, boardId,
					CacheKey.Participant, CacheKey.All);

			return Cache.Get(key, () => Repository.BoardsUsers.GetUsersSettings(boardId).ToList());
		}

		public UserSettings GetUserSettings(int boardId, int userId)
		{
			return GetParticipants(boardId).FirstOrDefault(x => x.User.Id == userId);
		}

		/// <summary>
		/// Исполнители для текущего пользователя
		/// </summary>
		public List<IUser> GetAllExecutorsOnBoard(int boardId)
		{
			int userId = Utility.Authentication.UserId;

			UserSettings settings = GetUserSettings(boardId, userId);

			UserRole role = settings.Settings.GetUserRole();
			bool notPureExecutor = role.HasAnyFlag(~UserRole.Executor);

			// для исполнителя отображать только себя, что бы нельзя было создать недоступную задачу
			// остальные роли видят всех исполнителей
			List<IUser> users = notPureExecutor // ownerOrCustomer || 
				? GetParticipants(boardId).Where(x => x.Settings.IsActive && x.Settings.UserRole.HasTheFlag((int)UserRole.Executor)).Select(x => x.User).ToList()
				: new List<IUser>(1); // не отдаем других исполнителей "чистому исполнителю"

			// вседа добавляем себя в список, что бы можно было добавлять себя исполнителем!
			if (Utility.Authentication.IsAuthenticated && !users.Any(x => x.Id == Utility.Authentication.UserId))
				users = new List<IUser>(users) { Utility.Users.CurrentUser };

			return users;
		}

		/// <summary>
		/// Исполнители для текущего пользователя для попапа и контекстного меню
		/// </summary>
		public List<IUser> GetExecutorsToAssing(int boardId)
		{
			UserSettings settings = GetUserSettings(boardId, Utility.Authentication.UserId);
			UserRole role = settings.Settings.GetUserRole();
			bool ownerOrCustomer = role.HasAnyFlag(UserRole.Customer | UserRole.Owner);

			return ownerOrCustomer
					? GetAllExecutorsOnBoard(boardId)
					: new List<IUser>(1) { Utility.Users.CurrentUser };
		}

		/// <summary>
		/// Обновление настроек пользователя на досте
		/// </summary>
		/// <param name="boardId"></param>
		/// <param name="userId"></param>
		/// <param name="isActive">null = значение не меняется</param>
		public IBoardsUser UpdateUserOnBoard(int boardId, int userId, bool isActive)
		{
			using (var scope = new TransactionScope())
			{
				IBoardsUser boardUser = Repository.BoardsUsers.GetBoardsUsers(boardId, userId);
				boardUser.IsActive = isActive;
				Repository.SubmitChanges();

				OnUpdateUserOnBoard.Invoke(new EventArgs<IBoardsUser>(boardUser));

				scope.Complete();

				return boardUser;
			}
		}

		public void UpdateRole(int boardId, int userId, UserRole role)
		{
			IBoardsUser boardUser = Repository.BoardsUsers.GetBoardsUsers(boardId, userId);

			if (boardUser.UserRole != (int)role)
			{
				if (boardUser.UserRole.HasTheFlag((int)UserRole.Owner) && !role.HasTheFlag(UserRole.Owner))
				{
					// Если пользователя лишают админских прав, нужно проверить, есть ли еще други админы
					if (Repository.BoardsUsers.AdminsCount(boardId) == 1)
						throw new NeedAdminException("На доске должен быть хотя бы один администратор");
				}

				boardUser.UserRole = (int)role;
				Repository.SubmitChanges();

				OnUpdateUserOnBoard.Invoke(new EventArgs<IBoardsUser>(boardUser));
			}
		}

		/// <summary>
		/// Исключение пользователя с доски
		/// </summary>
		/// <exception cref="NeedAdminException">На доске должен оставаться хотя бы один админ</exception>
		public void RemoveUserFromBoard(IBoard board, int userId)
		{
			List<UserSettings> settings = GetParticipants(board.Id);
			UserSettings user = settings.FirstOrDefault(x => x.User.Id == userId);

			if (user != null)
			{
				if (user.Settings.UserRole.HasTheFlag((int)UserRole.Owner))
				{
					int count = settings.Count(x => x.Settings.UserRole.HasTheFlag((int)UserRole.Owner));
					if (count < 2)
						throw new NeedAdminException("Доске \"" + board.Name + "\" требуется хотя бы один админ.");
				}

				bool hasTasks = Repository.Tasks.HasTasks(board.Id, userId);
				if (hasTasks)
					throw new TasksExistsException(user.User, board);

				using (TransactionScope scope = new TransactionScope())
				{
					IBoardsUser boardsUser = Repository.BoardsUsers.Delete(board.Id, userId);

					// TasksModel - Очистка кеша задач
					// BoardsModel - Очистка кеша досок
					OnRemoveUserFromBoard(boardsUser);

					scope.Complete();
				}
			}
		}

		#endregion

		#region Colors

		public List<IBoardsColor> GetColors(int boardId)
		{
			var key = ColorsCacheKey(boardId);
			return Cache.Get(key, () => Repository.BoardsColors.GetColors(boardId).ToList());
		}

		public IBoardsColor GetColor(int boardId, int colorId)
		{
			return GetColors(boardId).FirstOrDefault(x => x.Id == colorId);
		}

		public void DeleteColor(int colorId)
		{
			IBoardsColor boardsColor = Repository.BoardsColors.DeleteColor(colorId);
			if (boardsColor != null)
			{
				OnDeleteColor.Invoke(new EventArgs<IBoardsColor>(boardsColor));
			}
		}

		public IBoardsColor AddColor(int boardId, string name, string color)
		{
			IBoardsColor boardsColor = AddColor(boardId, name, color, false);
			OnCreateColor.Invoke(new EventArgs<IBoardsColor>(boardsColor));
			return boardsColor;
		}

		IBoardsColor AddColor(int boardId, string name, string color, bool isDefault)
		{
			return Repository.BoardsColors.CreateColor(boardId, name, color, isDefault);
		}

		/// <summary>
		/// Обновление приоритета
		/// </summary>
		public IBoardsColor UpdateColor(int colorId, string name, string colorHEX, bool isDefault)
		{
			bool updated = false;
			var color = Repository.BoardsColors.GetColor(colorId);
			bool colorChanged = color.Color != colorHEX;

			using (TransactionScope scope = new TransactionScope())
			{
				if (color.Name != name || colorChanged)
				{

					color.Name = name;
					color.Color = colorHEX;
					Repository.SubmitChanges();

					//if (colorChanged)
					//{
					// Обновляем задачи с этим цветом
					Repository.Tasks.UpdateColor(color);
					//}

					updated = true;
				}

				if (isDefault)
				{
					Repository.BoardsColors.MakeDefault(color);
					updated = true;
				}

				if (updated)
					OnUpdateColor.Invoke(new EventArgs<IBoardsColor>(color));

				scope.Complete();
				return color;
			}
		}

		/// <summary>
		/// Задает порядок цветов на доске
		/// </summary>
		public void SetColorsOrder(int boardId, IEnumerable<int> newOrder)
		{
			IBoardsColor[] colors = Repository.BoardsColors.GetColors(boardId).ToArray();
			Repository.SetOrder(newOrder, colors,
				color =>
				{
					Repository.Tasks.UpdateColorPosition(color);
					OnUpdateColor.Invoke(new EventArgs<IBoardsColor>(color));
				});
		}

		#endregion
	}
}
