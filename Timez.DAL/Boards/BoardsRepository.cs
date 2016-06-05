using System.Linq;
using Timez.Entities;
using System;
using Timez.DAL.DataContext;
using System.Data.Linq;

namespace Timez.DAL.Boards
{
    public interface IBoardsRepository
    {
        /// <summary>
		/// Получение для пользователя досок, на которых он активен
        /// </summary>
        IQueryable<IBoard> GetByUserId(int userId);

        IQueryable<IBoard> GetByOrganization(int organizationId);

        /// <summary>
        /// Доска по иду
        /// </summary>
        IBoard Get(int id);

        /// <summary>
        /// Создаем доску
        /// </summary>
        IBoard Create(string name, string description, int? refreshPeriod, int? organizationId);

        /// <summary>
        /// Удяляет доску
        /// </summary>
        void Delete(int id);

        /// <summary>
        /// Удаление досок без пользователей
        /// </summary>
        void DeleteEmpty();
    }

    class BoardsRepository : BaseRepository<BoardsRepository>, IBoardsRepository
    {
        /// <summary>
        /// Получение для пользователя досок, на которых он активен
        /// </summary>
        public IQueryable<IBoard> GetByUserId(int userId)
        {
            return _GetByUserId(_Context, userId);
        }
        static readonly Func<TimezDataContext, int, IQueryable<Board>> _GetByUserId =
            CompiledQuery.Compile((TimezDataContext ctx, int userId) =>
                    from b in ctx.Boards
                    join bu in ctx.BoardsUsers
                    on b.Id equals bu.BoardId
                    where bu.UserId == userId && bu.IsActive
                    orderby b.Name
                    select b
            );

        public IQueryable<IBoard> GetByOrganization(int organizationId)
        {
            return _GetByOrganization(_Context, organizationId);
        }
        static readonly Func<TimezDataContext, int, IQueryable<Board>> _GetByOrganization =
            CompiledQuery.Compile((TimezDataContext dataContext, int organizationId) =>
                    from b in dataContext.Boards
                    where b.OrganizationId == organizationId
                    orderby b.Name
                    select b);

        /// <summary>
        /// Доска по иду
        /// </summary>
        public IBoard Get(int id)
        {
            return _GetById(_Context, id);
        }
        static readonly Func<TimezDataContext, int, Board> _GetById =
            CompiledQuery.Compile((TimezDataContext ctx, int id) => ctx.Boards.FirstOrDefault(u => u.Id == id));

        /// <summary>
        /// Создаем доску
        /// </summary>
        public IBoard Create(string name, string description, int? refreshPeriod, int? organizationId)
        {
            Board board = new Board
            {
                Name = name,
                Description = description,
                RefreshPeriod = refreshPeriod,
                OrganizationId = organizationId
            };

            _Context.Boards.InsertOnSubmit(board);
            _Context.SubmitChanges();

            return board;
        }

        /// <summary>
        /// Удяляет доску
        /// </summary>
        public void Delete(int id)
        {
            Board board = _Context.Boards.FirstOrDefault(b => b.Id == id);
            if (board == null) return;

            // Делается триггером
            //// Нужно убрать дефлтный цвет, иначе база не дает удалять
            //var colors = BoardsColorsRepository.GetColors(DataContext, id).ToList();
            //var color = colors.FirstOrDefault(x => x.IsDefault);
            //if (color != null)
            //{
            //    color.IsDefault = false;
            //    DataContext.SubmitChanges();
            //}

            _Context.Boards.DeleteOnSubmit(board);
            _Context.SubmitChanges();
        }

        /// <summary>
        /// Удаление досок без пользователей
        /// </summary>
        public void DeleteEmpty()
        {
            _Context.ExecuteCommand(
                @"DELETE FROM dbo.Boards
                    FROM Boards 
                    LEFT JOIN BoardsUsers 
                    ON Boards.Id = BoardsUsers.BoardId
                    WHERE (BoardsUsers.UserId IS NULL)");
        }
    }
}
