using System;

namespace Timez.Entities
{
    public abstract class TimezException : Exception
    {
        protected TimezException() { }
        protected TimezException(string message) : base(message) { }

        /// <summary>
        /// Нужно ли логировать ошибку
        /// </summary>
        public abstract bool Logging { get; }

        /// <summary>
        /// Будет ли проброшен эксепшн в Listener
        /// false - Только для случаев, когда пользователю нужно выдать предупреждение и продолжить обрабатывать подписчиков в Listener, 
        /// это не ошибка логики, а ошибка рабочего процесса.
        /// </summary>
        public virtual bool ReThrow { get { return true; } }
    }
}
