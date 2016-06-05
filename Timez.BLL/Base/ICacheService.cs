using System;
using System.Collections.Generic;

namespace Timez.BLL
{
    public interface ICacheService
    {
        T Get<T>(IEnumerable<KeyValuePair<CacheKey, string>> keys, Func<T> func);
        object Get(IEnumerable<KeyValuePair<CacheKey, string>> keys);
        void Set(IEnumerable<KeyValuePair<CacheKey, string>> keys, object value);
        void Clear(IEnumerable<KeyValuePair<CacheKey, string>> collection);
        IEnumerable<KeyValuePair<CacheKey, string>> GetKeys(params object[] keys);
    	int ClearAll();
	}
}