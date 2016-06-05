namespace Timez.Entities
{
    /// <summary>
    /// Настройки пользователя на доске
    /// </summary>
    public sealed class UserSettings
    {
        //public UserSettings() { }

        //public UserSettings(IUser user, IBoardsUser settings)
        //{
        //    User = user;
        //    Settings = settings;
        //}

        /// <summary>
        /// Пользователь
        /// </summary>
        public IUser User { get; set; }

        /// <summary>
        /// Настрока
        /// </summary>
        public IBoardsUser Settings { get; set; }
    }
}
