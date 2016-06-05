using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Common.Extentions;

namespace Timez.Helpers
{
    public static class ViewDataDictionaryExtention
    {
        /// <summary>
        /// Приведенные к типу Т данные из viewData
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewData"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(this ViewDataDictionary viewData, string key)
        {
            return (T)viewData[key];
        }

        /// <summary>
        /// Получение списка объектов
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewData"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetCollection<T>(this ViewDataDictionary viewData, string key)
        {
            return (IEnumerable<T>)viewData[key];
        }

        /// <summary>
        /// Если данные не добавленны или равны null вернется ifEmpty
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewData"></param>
        /// <param name="key"></param>
        /// <param name="ifEmpty"></param>
        /// <returns></returns>
        public static T Get<T>(this ViewDataDictionary viewData, string key, T ifEmpty)
        {
            return viewData.ContainsKey(key) && viewData[key] != null ? (T)viewData[key] : ifEmpty;
        }

        public static object Get(this ViewDataDictionary viewData, string key)
        {
            return viewData[key];
        }

        ///// <summary>
        ///// Метод которым нужно устанавливать данные в ViewData
        ///// </summary>
        //public static T Set<T>(this ViewDataDictionary viewData, string key, T value)
        //{
        //    if (viewData.ContainsKey(key))
        //    {
        //        throw new Exception("Ключ {0} для ViewData занят!".Params(key));
        //    }

        //    viewData[key] = value;

        //    return value;
        //}
    }
}
