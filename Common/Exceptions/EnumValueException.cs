using System;
using Common.Extentions;

namespace Common.Exceptions
{
    /// <summary>
    /// Исключение для недопустимых значений в типе
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ValueException<T> : Exception
    {
        /// <summary>
        /// Значение
        /// </summary>
        readonly T _Value;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="value"></param>
        public ValueException(T value)
        {
            _Value = value;
        }

        /// <summary>
        /// Вывод ошибки
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Недопустиоме значение {0} в типе данных {1}".Params(typeof(T).ToString(), _Value);
        }
    }
}
