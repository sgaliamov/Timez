using System.Collections.Generic;
using Timez.Entities;
using CacheKeyValue = System.Collections.Generic.KeyValuePair<Timez.BLL.CacheKey, string>;

namespace Timez.BLL.Organizations
{
    public sealed partial class OrganizationsUtility
    {
        IEnumerable<CacheKeyValue> GetKeyEmployee(int organizationId)
        {
            var key = Cache.GetKeys(
                CacheKey.Organization, organizationId,
                CacheKey.EmployeeSettings, CacheKey.All);

            return key;
        }

        public OrganizationsUtility()
        {
            OnUpdateRole += (s, e) =>
            {
                IOrganizationUser user = e.Data;

                var key = GetKeyEmployee(user.OrganizationId);
                Cache.Clear(key);
            };

            OnAddEmployee += (s, e) =>
            {
                IOrganizationUser user = e.Data;

                var key = GetKeyEmployee(user.OrganizationId);
                Cache.Clear(key);
            };

            OnEmployeeRemove += (s, e) =>
            {
                IOrganizationUser user = e.Data;

                var key = GetKeyEmployee(user.OrganizationId);
                Cache.Clear(key);

                key = Cache.GetKeys(CacheKey.User, user.UserId);
                Cache.Clear(key);
            };

            OnDelete.Add((s, e) =>
            {
                int organizationId = e.Data;
                var key = Cache.GetKeys(CacheKey.Organization, organizationId);
                Cache.Clear(key);
            });

        }

        internal override void Init()
        {
            Utility.Invites.OnRefreshInviteCode += (s, e) =>
            {
                IOrganization organization = e.Data;
                Get(organization.Id).InviteCode = organization.InviteCode;
            };

            Utility.Invites.OnAcceptInvite += (s, e) =>
            {
                var key = GetKeyEmployee(e.Data.OrganizationId);
                Cache.Clear(key);
            };
        }
    }
}
