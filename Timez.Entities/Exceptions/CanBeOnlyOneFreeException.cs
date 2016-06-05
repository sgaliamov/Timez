namespace Timez.Entities
{
    public class CanBeOnlyOneFreeException : TimezException
    {
        public CanBeOnlyOneFreeException(IUser user)
            : base("Пользователь " + user.Nick + " может состоять только в одной бесплатной организации.") { }

        public override bool Logging { get { return false; } }
    }
}
