using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using Timez.DAL.DataContext;
using Timez.Entities;

namespace Timez.DAL.Organizations
{
    public interface IInvitesRepository
    {        
        IUsersInvite GetInvite(string inviteCode);

        /// <summary>
        /// Создание инвайта для регистрации нового пользователя
        /// </summary>
        IUsersInvite CreateNewInvite(int organizationId, string email, int inviterId);

        IQueryable<IUsersInvite> GetInvites(int organizationId);
        void RemoveOldInvites(int days);
    }

    class InvitesRepository : BaseRepository<InvitesRepository>, IInvitesRepository
    {
        public IUsersInvite GetInvite(string inviteCode)
        {
            return _Context.UsersInvites.FirstOrDefault(i => i.InviteCode.ToUpper() == inviteCode.ToUpper());
        }

        /// <summary>
        /// Создание инвайта для регистрации нового пользователя
        /// inviterId - тот кто приглашает
        /// email - имейл приглашаемого, может быть еще не зарегестрирован
        /// </summary>
        public IUsersInvite CreateNewInvite(int organizationId, string email, int inviterId)
        {
            UsersInvite invite = new UsersInvite
            {
                EMail = email,
                UserId = inviterId,
                InviteCode = Guid.NewGuid().ToString().ToUpper(),
                DateTime = DateTimeOffset.Now,
                OrganizationId = organizationId
            };

            _Context.UsersInvites.InsertOnSubmit(invite);
            _Context.SubmitChanges();

            return invite;
        }

        public IQueryable<IUsersInvite> GetInvites(int organizationId)
        {
            return _Context.UsersInvites.Where(x => x.OrganizationId == organizationId);
        }

        /// <summary>
        /// Устаревшие приглашения
        /// </summary>        
        static readonly Func<TimezDataContext, int, IQueryable<UsersInvite>> _GetOldInvites =
            CompiledQuery.Compile((TimezDataContext ctx, int days) =>
                ctx.UsersInvites.Where(x => x.DateTime.AddDays(days) < DateTimeOffset.Now));

        public void RemoveOldInvites(int days)
        {
            UsersInvite[] invites = _GetOldInvites(_Context, days).ToArray();
            _Context.UsersInvites.DeleteAllOnSubmit(invites);
            _Context.SubmitChanges();
        }

    }
}
