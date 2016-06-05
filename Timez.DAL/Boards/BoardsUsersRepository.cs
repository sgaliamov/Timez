using System;
using System.Linq;
using Timez.Entities;
using Timez.DAL.DataContext;
using System.Data.Linq;

namespace Timez.DAL.Boards
{
    public interface IBoardsUsersRepository
    {
        /// <summary>
        /// Добавляем пользователя на доску
        /// </summary>
		IBoardsUser AddUserToBoard(int boardId, int userId, UserRole role);

        IQueryable<UserSettings> GetUsersSettings(int boardId);
        IBoardsUser GetBoardsUsers(int boardId, int userId);

        /// <summary>
        /// Удаление с доски
        /// </summary>        
        IBoardsUser Delete(int boardId, int userId);

        int AdminsCount(int boardId);
    }

    /// <summary>
    /// Настрока пользователей на досках
    /// </summary>
    class BoardsUsersRepository : BaseRepository<BoardsUsersRepository>, IBoardsUsersRepository
    {
        /// <summary>
        /// Добавляем пользователя на доску
        /// </summary>
		public IBoardsUser AddUserToBoard(int boardId, int userId, UserRole role)
        {
            BoardsUser boardsUser
                = new BoardsUser
                {
                    UserId = userId,
                    IsActive = true,
                    BoardId = boardId,
                    UserRole = (int)role
                };

            _Context.BoardsUsers.InsertOnSubmit(boardsUser);
            _Context.SubmitChanges();

			return boardsUser;
        }

        public IQueryable<UserSettings> GetUsersSettings(int boardId)
        {
            return _GetUsersSettings(_Context, boardId);
        }
        static readonly Func<TimezDataContext, int, IQueryable<UserSettings>> _GetUsersSettings =
            CompiledQuery.Compile((TimezDataContext ctx, int boardId) =>
                from bu in ctx.BoardsUsers
                join u in ctx.Users
                on bu.UserId equals u.Id
                where bu.BoardId == boardId
				orderby u.Nick
                select new UserSettings { User = u, Settings = bu }
            );

        public IBoardsUser GetBoardsUsers(int boardId, int userId)
        {
            return _GetBoardsUser(_Context, boardId, userId);
        }
        static readonly Func<TimezDataContext, int, int, BoardsUser> _GetBoardsUser =
            CompiledQuery.Compile((TimezDataContext ctx, int boardId,int userId) =>
                (from bu in ctx.BoardsUsers
                 where bu.BoardId == boardId
                 && bu.UserId == userId
                 select bu).FirstOrDefault()
            );

        /// <summary>
        /// Удаление с доски
        /// </summary>        
        public IBoardsUser Delete(int boardId, int userId)
        {
            BoardsUser bu = _GetBoardsUser(_Context, boardId, userId);
            return Delete(bu);
        }
        IBoardsUser Delete(IBoardsUser boardUser)
        {
            // Заглушка с копией данных
            TimezBoardsUser dummy = new TimezBoardsUser(boardUser);

            _Context.BoardsUsers.DeleteOnSubmit((BoardsUser)boardUser);
            _Context.SubmitChanges();

            return dummy;
        }

        public int AdminsCount(int boardId)
        {
            return _Context.BoardsUsers.Count(
                x => x.BoardId == boardId && (x.UserRole & (int)UserRole.Owner) == (int)UserRole.Owner);
        }
    }
}
