using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web;

namespace Common.Extentions
{
    /// <summary>
    /// Расширения работы со строками
    /// </summary>
    public static class StringExtentions
    {
        /// <summary>
        /// Преобразование в инт
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int ToInt(this string str)
        {
            return int.Parse(str);
        }

        public static bool ToBool(this string str)
        {
            return bool.Parse(str);
        }

        public static int? TryToInt(this string str)
        {
            int res;
            return int.TryParse(str, out res) ? (int?)res : null;
        }

        public static int TryToInt(this string str, int defaultValue)
        {
            int res;
            return int.TryParse(str, out res) ? res : defaultValue;
        }

        /// <summary>
        /// Преобразование строки к перечеслению
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this string str)
        {
            return (T)Enum.Parse(typeof(T), str, true);
        }

        /// <summary>
        /// Обертка над string.Format
        /// </summary>
        /// <param name="str"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string Params(this string str, params object[] values)
        {
            return string.Format(str, values);
        }

        public static string Add(this string str, string add)
        {
            return string.Concat(str, add);
        }

        /// <summary>
        /// Проверка сроки на пустоту
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Обертка над HttpUtility.UrlDecode
        /// Преобразовывает строку, зашифрованную для передачи по URL-адресу, в расшифрованную строку.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UrlDecode(this string str)
        {
            return HttpUtility.UrlDecode(str);
        }

        /// <summary>
        /// Обертка над HttpUtility.UrlEncode
        /// Использовать при рендеринге
        /// Преобразует строку в строку формата HTML.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UrlEncode(this string str)
        {
            return HttpUtility.UrlEncode(str);
        }

        /// <summary>
        /// Обертка над HttpUtility.HtmlDecode
		/// Использовать перед сохранением в БД, чтобы вернуть оригинальный текст
        /// </summary>
        public static string HtmlDecode(this string str)
        {
            return HttpUtility.HtmlDecode(str);
        }

        /// <summary>
        /// Обертка над HttpUtility.HtmlEncode
		/// Использовать при выводе на странице из БД
		/// Безопасный для отображения в браузере режим
        /// </summary>
        public static string HtmlEncode(this string str)
        {
            return HttpUtility.HtmlEncode(str);
        }

        /// <summary>
        /// заменяет \n на br
        /// вставляет ссылки
        /// </summary>
        public static string ToHtml(this string str)
        {
            return MakeLink(str.Replace(Environment.NewLine, "<br />"));
        }

        /// <summary>
        /// Делает ссылки ссылками
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        static string MakeLink(string txt)
        {

			// todo: bug with string: 4. Приведенные ниже ссылки требуется закрыть тегами <noindex> и прописать атрибут rel=”nofollow”:  Страница Ссылка  http://www.mtk-fortuna.ru/News2_47.aspx http://www.fasttec.ru  http://www.mtk-fortuna.ru/News131_84.aspx http://www.fasttec.ru  http://www.mtk-fortuna.ru/News131_20.aspx http://www.gks.ru  http://www.mtk-fortuna.ru/News131_64.aspx http://www.metalinfo.ru  http://www.mtk-fortuna.ru/News2_105.aspx http://www.metalinfo.ru/emagazine/2012/0708/#/60/  http://www.mtk-fortuna.ru/News131_115.aspx http://www.metalinfo.ru/ru/magazine/rate/2012/2012_1  http://www.mtk-fortuna.ru/News131_76.aspx http://www.mmk-metiz.ru/  http://www.mtk-fortuna.ru/News131_38.aspx http://www.mmk-metiz.ru/  http://www.mtk-fortuna.ru/News131_92.aspx http://www.mmk-metiz.ru/  http://www.mtk-fortuna.ru/Providers.aspx http://www.mtk.mhost.ru/Belorussia.aspx  http://www.mtk-fortuna.ru/Providers.aspx http://www.mtk.mhost.ru/Russia1.aspx  http://www.mtk-fortuna.ru/Providers.aspx http://www.mtk.mhost.ru/Ukraine.aspx  http://www.mtk-fortuna.ru/News131_45.aspx http://www.rbc.ru  http://www.mtk-fortuna.ru/News131_63.aspx http://www.rmz.by  http://www.mtk-fortuna.ru/News131_65.aspx http://www.rmz.by  http://www.mtk-fortuna.ru/News131_43.aspx http://www.rmz.by  http://www.mtk-fortuna.ru/News131_68.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_86.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_33.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_31.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_98.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_22.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_53.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_37.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_40.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_56.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_73.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_39.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News2_94.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_91.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News2_29.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_36.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_55.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_35.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_74.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_21.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_54.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_85.aspx http://www.rosmetiz.ru  http://www.mtk-fortuna.ru/News131_78.aspx http://www.rusmet.ru  http://www.mtk-fortuna.ru/News131_72.aspx http://www.rusmet.ru  http://www.mtk-fortuna.ru/Service9_45.aspx http://www.rusmet.ru  http://www.mtk-fortuna.ru/News131_52.aspx http://www.severstal.ru  http://www.mtk-fortuna.ru/Russia1.aspx http://www.severstalmetiz.com/rus/       
			//Regex regx = new Regex("http://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);

			//MatchCollection mactches = regx.Matches(txt);

			//foreach (Match match in mactches)
			//{
			//	txt = txt.Replace(match.Value, "<a href='" + match.Value + "'>" + match.Value + "</a>");
			//}

            return txt;
        }

        /// <summary>
        /// Получение MD5
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetMD5Hash(this string input)
        {
            MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            return s.ToString();
        }

        /// <summary>
        /// Преобразует строку в коллекцию типа R, в которой будут объекты типа Т
        /// Разбивает с флагом StringSplitOptions.RemoveEmptyEntries
        /// </summary>
        public static R Split<R, T>(this string str, params char[] separator)
            where R : ICollection<T>, new()
        {
            // TODO: теоретически, если строка пустая, то и результат должен быть пустой
			// но, видимо, где-то эта логика не нужна. следать чтоб было логичным
            //if (str == null)
            //    return null;

            if (str.IsNullOrEmpty())
                return default(R);

            R res = new R();

            foreach (var item in str.Split(separator, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!item.IsNullOrEmpty())
                {
                    T tag = item.Trim().To<T>();
                    if (tag != null && !res.Contains(tag))
                        res.Add(tag);
                }
            }

            return res.Count == 0 ? default(R) : res;
        }

        /// <summary>
        /// Обрезает текст до заданной длины
        /// </summary>
        public static string CutText(this string text, int count)
        {
            if (text.Length <= count)
            {
                return text;
            }

            return string.Concat(text.Substring(0, count), "…");
        }

        /// <summary>
        /// Является ли строка имейл адресом
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsValidEmail(this string str)
        {
            //const string pattern = @"^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,3})$";

            const string pattern = @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))" +
                                   @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$";

            return !string.IsNullOrWhiteSpace(str) && Regex.IsMatch(str, pattern);
        }
    }
}
