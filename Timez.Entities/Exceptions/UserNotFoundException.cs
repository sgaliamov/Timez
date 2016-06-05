namespace Timez.Entities
{
    /// <summary>
    /// Пользователь не найден
    /// </summary>
    public sealed class UserNotFoundException : TimezException
    {
        public UserNotFoundException()
            : base("Пользователь не найден") { }

        public override bool Logging { get { return true; } }
    }
}

