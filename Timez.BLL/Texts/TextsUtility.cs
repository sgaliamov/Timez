using System;
using System.Collections.Generic;
using System.Linq;
using Timez.Entities;

namespace Timez.BLL.Texts
{
	public sealed class TextsUtility : BaseUtility<TextsUtility>
	{
		public TextsCollection Get(TextType type, int page, int count, bool? isVisible = null)
		{
			IEnumerable<KeyValuePair<CacheKey, string>> key = Cache.GetKeys(
				CacheKey.Text, type
				, CacheKey.Page, page
				, CacheKey.Count, count
				, CacheKey.Visible, isVisible);

			return Cache.Get(key,
			() =>
			{
				int total;
				IQueryable<IText> texts = Repository.Texts.Get(type, page, count, out total, isVisible);
				// Нужен контейнер, так как нужно вернуть общее количество из кеша
				TextsCollection result = new TextsCollection
											{
												Collection = texts.ToList(),
												Total = total,
												Page = page,
												ItemsOnPage = count
											};
				return result;
			});
		}

		/// <summary>
		/// Контейнер текстов
		/// </summary>
		public class TextsCollection
		{
			/// <summary>
			/// Коллекция текстов
			/// </summary>
			public List<IText> Collection { get; set; }

			/// <summary>
			/// Всего элементов в репозитории
			/// </summary>
			public int Total { get; set; }

			/// <summary>
			/// Текущая страница
			/// </summary>
			public int Page { get; set; }

			public int ItemsOnPage { get; set; }
		}

		public IText Get(int id)
		{
			IEnumerable<KeyValuePair<CacheKey, string>> key = Cache.GetKeys(CacheKey.Text, id);
			return Cache.Get(key, () => Repository.Texts.Get(id));
		}

		public void Delete(int id)
		{
			IText text = Repository.Texts.Delete(id);

			Cache.Clear(Cache.GetKeys(CacheKey.Text, id));
			Cache.Clear(Cache.GetKeys(CacheKey.Text, text.Type));
		}

		public IText Save(int? id, string title, string content, TextType type, bool isVisible)
		{
			IText text = Repository.Texts.Save(id, title, content, isVisible, type);

			if (id.HasValue)
				Cache.Clear(Cache.GetKeys(CacheKey.Text, id));
			Cache.Clear(Cache.GetKeys(CacheKey.Text, type));

			return text;
		}
	}
}
