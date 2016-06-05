using System;
using System.Collections;
using System.Web.Caching;

namespace Common.Extentions
{
    /// <summary>
    /// Класс работы с кешем
    /// </summary>
    public static class CacheExtentions
    {
        #region Геты

        /// <summary>
        /// Полчение данных из кеша
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="cache"></param>
        /// <param name="key">Ключ кеша</param>
        /// <param name="func">функция которая должна заполнять кеш</param>
        /// <returns>данные их кеша</returns>
        public static T Get<T>(this Cache cache, string key, Func<T> func) where T : class
        {
            if (cache[key] == null)
            {
                T val = func();
                if (val != null)
                {
                    cache[key] = val;
                    return val;
                }
            }
            return (T)cache[key];
        }

        public static T Get<T>(this Cache cache, string key, DateTime absoluteExpiration, Func<T> func)
        {
            if (cache[key] == null)
            {
                T value = func();
                if (value != null)
                {
                    cache.Insert(key, value, null, absoluteExpiration, Cache.NoSlidingExpiration);
                    return value;
                }
            }
            return (T)cache[key];
        }

        #endregion

        #region Очистка

        /// <summary>
        /// Очищает кеш по ключам начинающихся с startsWith
        /// </summary>
        /// <param name="cache">cache</param>
        /// <param name="startsWith">startsWith</param>
        public static void Clear(this Cache cache, string startsWith)
        {
            foreach (DictionaryEntry e in cache)
            {
                if (e.Key.ToString().StartsWith(startsWith))
                    cache.Remove(e.Key.ToString());
            }
        }

        /// <summary>
        /// Очистка всего кеша
        /// </summary>
        /// <param name="cache"></param>
        public static int ClearAll(this Cache cache)
        {
            int i = 0;
            foreach (DictionaryEntry e in cache)
            {
                cache.Remove(e.Key.ToString());
                i++;
            }
            return i;
        }

        #endregion

        #region Сеты

        /// <summary>
        /// Устанавливает значение для кеша
        /// </summary>
        public static void Set(this Cache cache, string key, object value)
        {
            if (value == null)
            {
                // Очистка кеша
                cache.Remove(key);
                return;
            }

            cache[key] = value;
        }

        public static void Set(this Cache cache, string key, object value, DateTime absoluteExpiration)
        {
            if (value == null)
            {
                // Очистка кеша
                cache.Remove(key);
                return;
            }

            cache.Insert(key, value, null, absoluteExpiration, Cache.NoSlidingExpiration);
        }

        #endregion
    }
}
