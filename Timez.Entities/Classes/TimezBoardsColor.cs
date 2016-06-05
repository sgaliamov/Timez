namespace Timez.Entities
{
    public class TimezBoardsColor : IBoardsColor // !!! ОБНОВИ КОНСТРУКТОР ПОСЛЕ ИЗМЕНЕНИЯ ИНТЕРФЕЙСА !!!
    {
        public TimezBoardsColor() { }
        public TimezBoardsColor(IBoardsColor color)
        {
            Id = color.Id;
            Position = color.Position;
            BoardId = color.BoardId;
            Color = color.Color;
            Name = color.Name;
            IsDefault = color.IsDefault;
        }

        public int Id { get; private set; }
        public int Position { get; set; }
        public int BoardId { get; private set; }
        public string Color { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
    }
}