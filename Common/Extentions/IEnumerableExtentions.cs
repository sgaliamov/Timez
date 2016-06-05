using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Extentions
{
	/// <summary>
	/// Расширения над перечистелниями
	/// </summary>
	public static class IEnumerableExtentions
	{
		/// <summary>
		/// Объединяет перечисление в сроку разделенную сепараторами
		/// </summary>
		/// <returns></returns>
		public static string ToString<T>(this IEnumerable<T> collection, char separator)
		{
			if (collection == null || !collection.Any())
				return string.Empty;

			StringBuilder sb = new StringBuilder();
			foreach (var item in collection)
			{
				sb.AppendFormat("{0}{1}", item.ToString(), separator);
			}
			sb.Length -= 1;
			return sb.ToString();
		}


		/// <summary>
		/// Преобразование в строку коллекции обрабонанной селектором
		/// </summary>
		/// <typeparam name="TSource">Тип элементов коллекции</typeparam>
		/// <typeparam name="TResult">Тип поля в селекторе, которое будет добавленно в строку</typeparam>
		/// <param name="collection">коллекция</param>
		/// <param name="selector">селектор доставания поля, которое будет добавленно в строку</param>
		/// <param name="separator">разделитель, которым будут отделены элементы</param>
		/// <returns>строка</returns>
		public static string ToString<TSource, TResult>(this IEnumerable<TSource> collection, Func<TSource, TResult> selector, string separator)
		{
			if (collection == null)
				return string.Empty;

			StringBuilder sb = new StringBuilder();

			foreach (TResult item in collection.Select(selector))
			{
				sb.AppendFormat("{0}{1}", item.ToString(), separator);
			}
			return sb.ToString().Trim(separator.ToArray());
		}

		/// <summary>
		/// Пейджирование данных
		/// </summary>
		/// <param name="page">Страница, начиная с 1</param>
		/// <param name="itemsOnPage">Количество элементов на странице</param>
		/// <param name="collection">Вся коллекция</param>
		/// <param name="total">Всего элементов в коллекции</param>
		public static IQueryable<T> GetPaged<T>(this IQueryable<T> collection, int page, int itemsOnPage, out int total)
		{
			total = collection.Count();

			int skip = itemsOnPage * (page - 1);
			if (skip >= total)
				skip = 0;

			return total > itemsOnPage
				? collection.Skip(skip).Take(itemsOnPage)
				: collection;
		}
	}
}
