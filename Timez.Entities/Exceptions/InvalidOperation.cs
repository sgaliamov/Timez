
namespace Timez.Entities
{
    public class InvalidOperationTimezException : TimezException
    {
        public InvalidOperationTimezException(string message) : base(message) { }

        public override bool Logging { get { return true; } }
    }
}
