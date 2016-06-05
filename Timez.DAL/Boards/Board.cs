using Timez.Entities;

namespace Timez.DAL.DataContext
{
    internal partial class Board : IBoard { }

    internal partial class BoardsColor : IBoardsColor { }

	internal partial class BoardsUser : IBoardsUser
	{

		public UserRole GetUserRole()
		{
			return (UserRole)UserRole;
		}
	}
}
