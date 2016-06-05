namespace Timez.Entities
{
	public interface IBoardsUser
	{
		int BoardId { get; set; }
		bool IsActive { get; set; }
		int UserId { get; set; }
		int UserRole { get; set; }

		UserRole GetUserRole();
	}

	public class TimezBoardsUser : IBoardsUser
	{
		public TimezBoardsUser(IBoardsUser boardsUser)
		{
			BoardId = boardsUser.BoardId;
			IsActive = boardsUser.IsActive;
			UserId = boardsUser.UserId;
			UserRole = boardsUser.UserRole;
		}

		public int BoardId { get; set; }
		public bool IsActive { get; set; }
		public int UserId { get; set; }
		public int UserRole { get; set; }

		public UserRole GetUserRole() { return (UserRole)UserRole; }
	}
}
