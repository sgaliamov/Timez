using System;

namespace Common.Extentions
{
    /// <summary>
    /// Экстренш работы с объектами
    /// </summary>
    public static class ObjectExtention
    {
        /// <summary>
        /// Конвертация объекта в тип
        /// </summary>
        /// <typeparam name="T">требуемы тип</typeparam>
        /// <param name="o">любой объект</param>
        /// <returns>сконвеченный тип</returns>
        public static T To<T>(this object o)
        {
            Type type = typeof(T);
            return (T)Convert.ChangeType(o, type);
        }

        public static T To<T>(this object o, T value)
        {
            try
            {
                return o == null ? value : (T)Convert.ChangeType(o, typeof(T));
            }
            catch
            {
                return value;
            }
        }

        /// <summary>
        /// Конвертация объекта в нулэйбл тип
        /// </summary>
        /// <typeparam name="T">требуемы тип</typeparam>
        /// <param name="o">любой объект</param>
        /// <returns>сконвеченный тип</returns>
        public static T? ToNullable<T>(this object o)
            where T : struct
        {
            if (o == null || string.Empty.Equals(o))
                return null;

            try
            {
                return (T)Convert.ChangeType(o, typeof(T));
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }

            return (T?)null;
        }
    }
}
