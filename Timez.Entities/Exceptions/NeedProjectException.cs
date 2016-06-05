namespace Timez.Entities
{
    /// <summary>
    /// Требуется проект
    /// </summary>
    public class NeedProjectException : TimezException
    {
        public NeedProjectException(string message) : base(message) { }

        /// <summary>
        /// Не логируется
        /// </summary>
        public override bool Logging { get { return false; } }
    }
}