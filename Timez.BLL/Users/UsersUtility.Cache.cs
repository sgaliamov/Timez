using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Common.Extentions;
using Timez.Entities;
using CacheKeyValue = System.Collections.Generic.KeyValuePair<Timez.BLL.CacheKey, string>;

namespace Timez.BLL.Users
{
	public sealed partial class UsersUtility
	{
		IEnumerable<CacheKeyValue> GetCacheKey(int userId)
		{
			return Cache.GetKeys(CacheKey.User, userId);
		}

		public UsersUtility()
		{
			OnUpdate +=
			(s, e) =>
			{
				IUser user = e.NewData;
				Cache.Set(GetCacheKey(user.Id), user);
			};

			OnUpdateMailingAdderss +=
			(s, e) =>
			{
				IUser user = e.Data;
				Cache.Set(GetCacheKey(user.Id), user);
			};

			OnConfirmEmail +=
			(s, e) =>
			{
				IUser user = e.Data;
				Cache.Set(GetCacheKey(user.Id), user);
			};
		}
	}
}
