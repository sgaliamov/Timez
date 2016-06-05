using System;
using System.Data.Linq;
using System.Linq;
using Timez.DAL.DataContext;
using Timez.Entities;

namespace Timez.DAL.Organizations
{
    public interface IOrganizationsRepository
    {
        IQueryable<EmployeeSettings> GetByUser(int userId, bool? isApproved);
        IOrganization Get(int id);
        IOrganization Create(string name, int tariffId, bool isFree);
        void Delete(int id);
        IOrganizationUser AddUser(int organizationId, int userId, EmployeeRole role, bool isApproved);
        IOrganizationUser UpdateUser(int organizationId, int userId, EmployeeRole role, bool isApproved);
        void RemoveUser(int organizationId, int userId);
        void RemoveUser(IOrganizationUser organizationUser);
        IQueryable<IOrganizationUser> GetUsers(int id);

        /// <summary>
        /// Запись IOrganizationUser
        /// </summary>
        IOrganizationUser GetOrganizationUser(int organizationId, int userId);

        IOrganization GetByInviteCode(string inviteCode);

        /// <summary>
        /// Все записи IOrganizationUser организации
        /// </summary>
        IQueryable<IOrganizationUser> GetOrganizationUsers(int organizationId);

        /// <summary>
        /// Настройки пользователя в огранизации
        /// </summary>
        IQueryable<EmployeeSettings> GetEmployeeSettings(int organizationId);

        /// <summary>
        /// Количество админов в организации
        /// </summary>
        int AdminsCount(int organizationId);

        IQueryable<ITariff> GetTariffs();
    }

    class OrganizationsRepository : BaseRepository<OrganizationsRepository>, IOrganizationsRepository
    {
        #region Организации

        public IOrganization GetByInviteCode(string inviteCode)
        {
            return _GetByInviteCode(_Context, inviteCode);
        }
        static readonly Func<TimezDataContext, string, Organization> _GetByInviteCode =
            CompiledQuery.Compile((TimezDataContext ctx, string inviteCode) =>
                ctx.Organizations.FirstOrDefault(u => u.InviteCode == inviteCode));

        public IQueryable<EmployeeSettings> GetByUser(int userId, bool? isApproved)
        {
            return _GetByUser(_Context, userId, isApproved);
        }
        static readonly Func<TimezDataContext, int, bool?, IQueryable<EmployeeSettings>> _GetByUser =
            CompiledQuery.Compile((TimezDataContext dataContext, int userId, bool? isApproved) =>
                from o in dataContext.Organizations
                join ou in dataContext.OrganizationUsers
                on o.Id equals ou.OrganizationId
                join u in dataContext.Users
                on ou.UserId equals u.Id
                where ou.UserId == userId
                && (!isApproved.HasValue || ou.IsApproved == isApproved.Value)
                select new EmployeeSettings { User = u, Settings = ou, Organization = o });

        public IOrganization Get(int id)
        {
            return _Get(_Context, id);
        }
        static readonly Func<TimezDataContext, int, Organization> _Get =
            CompiledQuery.Compile((TimezDataContext dataContext, int id) =>
                (from o in dataContext.Organizations
                 where o.Id == id
                 select o).FirstOrDefault());

        public IOrganization Create(string name, int tariffId, bool isFree)
        {
            Organization organization =
                new Organization
                {
                    Name = name,
                    TariffId = tariffId,
                    IsFree = isFree,
                    InviteCode = Guid.NewGuid().ToString().ToUpper()
                };
            _Context.Organizations.InsertOnSubmit(organization);
            _Context.SubmitChanges();

            return organization;
        }

        public void Delete(int id)
        {
            Organization organization = _Get(_Context, id);
            _Context.Organizations.DeleteOnSubmit(organization);
            _Context.SubmitChanges();
        }

        #endregion

        #region Сотрудники

        public IOrganizationUser AddUser(int organizationId, int userId, EmployeeRole role, bool isApproved)
        {
            OrganizationUser organizationUser = new OrganizationUser
            {
                IsApproved = isApproved,
                OrganizationId = organizationId,
                UserId = userId,
                UserRole = (int)role
            };
            _Context.OrganizationUsers.InsertOnSubmit(organizationUser);
            _Context.SubmitChanges();

            return organizationUser;
        }

        public IOrganizationUser UpdateUser(int organizationId, int userId, EmployeeRole role, bool isApproved)
        {
            OrganizationUser organizationUser = _GetOrganizationUser(_Context, organizationId, userId);
            organizationUser.IsApproved = isApproved;
            organizationUser.UserRole = (int)role;
            _Context.SubmitChanges();

            return organizationUser;
        }

        public void RemoveUser(int organizationId, int userId)
        {
            OrganizationUser organizationUser = _GetOrganizationUser(_Context, organizationId, userId);

            RemoveUser(organizationUser);
        }

        public void RemoveUser(IOrganizationUser organizationUser)
        {
            _Context.OrganizationUsers.DeleteOnSubmit((OrganizationUser)organizationUser);
            _Context.SubmitChanges();
        }

        public IQueryable<IOrganizationUser> GetUsers(int id)
        {
            return _Context.OrganizationUsers
                .Where(x => x.OrganizationId == id);
        }

        /// <summary>
        /// Запись IOrganizationUser
        /// </summary>
        public IOrganizationUser GetOrganizationUser(int organizationId, int userId)
        {
            return _GetOrganizationUser(_Context, organizationId, userId);
        }
        static readonly Func<TimezDataContext, int, int, OrganizationUser> _GetOrganizationUser =
            CompiledQuery.Compile((TimezDataContext dataContext, int organizationId, int userId) =>
               dataContext.OrganizationUsers.FirstOrDefault(x => x.OrganizationId == organizationId && x.UserId == userId));

        /// <summary>
        /// Все записи IOrganizationUser организации
        /// </summary>
        public IQueryable<IOrganizationUser> GetOrganizationUsers(int organizationId)
        {
            return _GetOrganizationUsers(_Context, organizationId);
        }
        static readonly Func<TimezDataContext, int, IQueryable<OrganizationUser>> _GetOrganizationUsers =
            CompiledQuery.Compile((TimezDataContext ctx, int organizationId) =>
                from bu in ctx.OrganizationUsers
                where bu.OrganizationId == organizationId
                select bu);

        /// <summary>
        /// Настройки пользователя в огранизации
        /// </summary>
        public IQueryable<EmployeeSettings> GetEmployeeSettings(int organizationId)
        {
            return _GetEmployeeSettings(_Context, organizationId);
        }
        static readonly Func<TimezDataContext, int, IQueryable<EmployeeSettings>> _GetEmployeeSettings =
            CompiledQuery.Compile((TimezDataContext dataContext, int organizationId) =>
                from ou in dataContext.OrganizationUsers
                join u in dataContext.Users
                on ou.UserId equals u.Id
                join o in dataContext.Organizations
                on ou.OrganizationId equals o.Id
                where ou.OrganizationId == organizationId
                select new EmployeeSettings { User = u, Settings = ou, Organization = o });

        /// <summary>
        /// Количество админов в организации
        /// </summary>
        public int AdminsCount(int organizationId)
        {
            return _Context.OrganizationUsers
                .Count(x => x.OrganizationId == organizationId
                   && (x.UserRole & (int)EmployeeRole.Administrator) == (int)EmployeeRole.Administrator);
        }

        #endregion

        /// <summary>
        /// Все тарифы
        /// </summary>
        /// <returns></returns>
        public IQueryable<ITariff> GetTariffs()
        {
            return _Context.Tariffs;
        }
    }
}
