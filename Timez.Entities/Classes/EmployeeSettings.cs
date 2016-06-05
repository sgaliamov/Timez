namespace Timez.Entities
{
    /// <summary>
    /// Настройки пользователя на в организации
    /// </summary>
    public sealed class EmployeeSettings
    {
        /// <summary>
        /// Пользователь
        /// </summary>
        public IUser User { get; set; }

        /// <summary>
        /// Настрока
        /// </summary>
        public IOrganizationUser Settings { get; set; }

        /// <summary>
        /// Огранизация
        /// </summary>
        public IOrganization Organization { get; set; }
    }
}
