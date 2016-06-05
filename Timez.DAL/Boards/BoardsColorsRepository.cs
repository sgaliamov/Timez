using System;
using System.Data.Linq;
using System.Linq;
using Timez.DAL.DataContext;
using Timez.Entities;

namespace Timez.DAL.Boards
{
    public interface IBoardsColorsRepository
    {
        IOrderedQueryable<IBoardsColor> GetColors(int boardId);

        /// <summary>
        /// Удаление приоритета
        /// При удалении в тригере всем задачам с данным проиоритетам назначается дефолтный цвет
        /// </summary>
		IBoardsColor DeleteColor(int colorId);

        IBoardsColor CreateColor(int boardId, string name, string color, bool isDefault);
        IBoardsColor GetColor(int colorId);

        /// <summary>
        /// Делает цвет цветом поумолчанию
        /// </summary>
        void MakeDefault(IBoardsColor color);
    }

    /// <summary>
    /// Работа с цветами
    /// </summary>
    class BoardsColorsRepository : BaseRepository<BoardsColorsRepository>, IBoardsColorsRepository
    {
        public IOrderedQueryable<IBoardsColor> GetColors(int boardId)
        {
            return _GetColors(_Context, boardId);
        }
        private static readonly Func<TimezDataContext, int, IOrderedQueryable<BoardsColor>> _GetColors =
            CompiledQuery.Compile((TimezDataContext ctx, int boardId) =>
                                  ctx.BoardsColors
                                      .Where(x => x.BoardId == boardId)
                                      .OrderBy(x => x.Position));

        /// <summary>
        /// Удаление приоритета
        /// При удалении в тригере всем задачам с данным проиоритетам назначается дефолтный цвет
        /// </summary>
        public IBoardsColor DeleteColor(int colorId)
        {
            BoardsColor color = _Context.BoardsColors.FirstOrDefault(b => b.Id == colorId);

            if (color != null && !color.IsDefault)
            {
                _Context.BoardsColors.DeleteOnSubmit(color);
                _Context.SubmitChanges();
				return color;
            }

            return null;
        }

        public IBoardsColor CreateColor(int boardId, string name, string color, bool isDefault)
        {
            var newColor = new BoardsColor
            {
                BoardId = boardId,
                Color = color,
                Name = name,
                IsDefault = isDefault
            };

            _Context.BoardsColors.InsertOnSubmit(newColor);
            _Context.SubmitChanges();

            // так как страбатывает тригер задающий позицию, обновляем позицию вручную
            return new TimezBoardsColor(newColor) { Position = newColor.Id };
        }

        public IBoardsColor GetColor(int colorId)
        {
            return _GetColor(_Context, colorId);
        }
        static readonly Func<TimezDataContext, int, BoardsColor> _GetColor =
            CompiledQuery.Compile((TimezDataContext ctx, int colorId) =>
                ctx.BoardsColors.FirstOrDefault(b => b.Id == colorId));

        /// <summary>
        /// Делает цвет цветом поумолчанию
        /// </summary>
        public void MakeDefault(IBoardsColor color)
        {
            // TODO: какая-то хенря.
            // если все сделать через нейтивный скл, то контекст становится невалидным, по этому часть через SubmitChanges
            _Context.ExecuteCommand(@"UPDATE BoardsColors SET IsDefault = 0 WHERE BoardId = {0}", color.BoardId);
            color.IsDefault = true;
            _Context.SubmitChanges();
        }
    }
}
