using System;

namespace Common.Exceptions
{
    /// <summary>
    /// Доступ запрещен
    /// </summary>
    public sealed class AccessDeniedException : Exception
    {
        public override string Message
        {
            get
            {
                return "Доступ запрещен";
            }
        }
    }
}
