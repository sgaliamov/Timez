namespace Timez.Entities
{
    /// <summary>
    /// Инетфейс классов и идом
    /// </summary>
    public interface IId
    {
        int Id { get; }
    }

    /// <summary>
    /// Инетфейс классов с упорядочиванием
    /// </summary>
    public interface IPosition : IId
    {
        int Position { get; set; }
    }
}
