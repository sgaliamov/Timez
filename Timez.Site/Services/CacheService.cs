using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using Common.Extentions;
using Timez.BLL;
using CacheKeyValue = System.Collections.Generic.KeyValuePair<Timez.BLL.CacheKey, string>;
using CacheKeyCollection = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<Timez.BLL.CacheKey, string>>;

namespace Timez.Utilities
{
	/// <summary>
	/// Работа кешем
	/// </summary>
	public sealed class CacheService : ICacheService
	{
		#region Stuff

		/// <summary>
		/// Сам кеш
		/// </summary>
		private static readonly System.Web.Caching.Cache _Cache = HttpRuntime.Cache;

		/// <summary>
		/// Время жизни кеша в минутах
		/// </summary>
		private const double UsersCacheLifeTime = 60.0;

		#endregion

		#region Keys

		/// <summary>
		/// Префикс, чтоб отличать наш кеш, от других
		/// </summary>
		internal const string Prefix = "Timez-";

		/// <summary>
		/// Ключ кеширования
		/// </summary>
		private static string GetKeys(CacheKeyCollection keys)
		{
			string key = Prefix + keys
									.OrderBy(x => x.Key).ThenBy(x => x.Value)
									.Distinct()
									.Select(x => x.Key.ToString() + ":" + x.Value)
									.Aggregate((x, y) => x + "," + y);

			return key;
		}

		#endregion

		#region ICacheUtility

		CacheKeyCollection ICacheService.GetKeys(params object[] keys)
		{
			Contract.Assert(keys.Length % 2 == 0);

			HashSet<CacheKeyValue> pairs = new HashSet<CacheKeyValue>();
			for (int i = 0; i < keys.Length; i += 2)
				pairs.Add(new CacheKeyValue((CacheKey)keys[i], (keys[i + 1] ?? "").ToString()));

			return pairs;
		}

		/// <summary>
		/// Кеш текущего пользователя
		/// </summary>
		T ICacheService.Get<T>(CacheKeyCollection keys, Func<T> func)
		{
			string key = GetKeys(keys);
			return _Cache.Get(key, DateTime.Now.AddMinutes(UsersCacheLifeTime), func);
		}

		object ICacheService.Get(CacheKeyCollection keys)
		{
			string key = GetKeys(keys);
			return _Cache[key];
		}

		void ICacheService.Set(CacheKeyCollection keys, object value)
		{
			string key = GetKeys(keys);
			_Cache.Set(key, value, DateTime.Now.AddMinutes(UsersCacheLifeTime));
		}

		/// <summary>
		/// Очищает кеш, где ключи содеражат все пары из коллекции
		/// </summary>
		void ICacheService.Clear(CacheKeyCollection collection)
		{
			foreach (DictionaryEntry item in _Cache)
			{
				string key = item.Key as string;
				if (key != null && key.StartsWith(Prefix))
				{
					CacheKeyValue[] pairs = key
						.Substring(Prefix.Length)
						.Split(',')
						.Select(x =>
									{
										string[] split = x.Split(':');
										CacheKey cacheKey = split[0].ToEnum<CacheKey>();
										return new CacheKeyValue(cacheKey, split[1]);
									})
						.ToArray();

					if (!collection.Except(pairs).Any())
						_Cache.Remove(key);
				}
			}
		}

		public int ClearAll()
		{
			return _Cache.ClearAll();
		}

		#endregion
	}
}