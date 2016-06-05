namespace Timez.Entities
{
    public interface ITariff
    {
        int Id { get; set; }
        string Name { get; set; }
        decimal Price { get; set; }
        int? BoardsCount { get; set; }
        int? ProjectsPerBoard { get; set; }
        int? EmployeesCount { get; set; }
        int Flags { get; set; }

        TariffFlags GetFlags();
        bool IsFree();
    }

    public enum TariffFlags
    {
        None = 0,

        EnableCss = 1,
        EnableLogo = 1 << 1,
        EnableDomain = 1 << 2,
        
        /// <summary>
        /// Бесплатный тариф
        /// </summary>
        IsFree = 1 << 3,
    }
}