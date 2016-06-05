namespace Timez.Entities
{
    public class TariffException : TimezException
    {
        public TariffException(string message) : base(message) { }
        public override bool Logging { get { return false; } }
    }
}