using System;

namespace Common
{
    /// <summary>
    /// Самые общие методы
    /// </summary>
    public static class Utility
    {

        // Можно передавать в функцию анонимные типы, пользоваться рефлекшном
        //Func(new { Desc = description, Tags = tags });
        //private void Func(object obj)
        //{
        //    var pis = obj.GetType().GetProperties();
        //    foreach (var pi in pis)
        //    {
        //        var val = pi.GetValue(obj, null);
        //    }
        //}

        #region Обертка над блоком try catch, осуществляется логирование
        public static T Try<T>(Func<T> f) where T : class
        {
            try { return f(); }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
            return null;
        }
        public static void Try(Action f)
        {
            try { f(); }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
        #endregion

        //public static Color GetRandomColor()
        //{
        //    KnownColor[] colors = (KnownColor[])Enum.GetValues(typeof(KnownColor));
        //    Random random = new Random();
        //    return Color.FromKnownColor(colors[random.Next(colors.Length)]);
        //}
    }
}
