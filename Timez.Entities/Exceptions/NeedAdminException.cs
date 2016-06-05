namespace Timez.Entities
{
    /// <summary>
    /// Требуется админ
    /// </summary>
    public class NeedAdminException : TimezException
    {
        public NeedAdminException(string message) : base(message) { }

        /// <summary>
        /// Не логируется
        /// </summary>
        public override bool Logging { get { return false; } }
    }
}