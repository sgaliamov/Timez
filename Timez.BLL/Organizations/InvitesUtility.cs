using System;
using System.Linq;
using System.Collections.Generic;
using System.Transactions;
using Timez.Entities;

namespace Timez.BLL.Organizations
{
    public class InvitesUtility : BaseUtility<InvitesUtility>
    {
        public Listener<EventArgs<IOrganization>> OnRefreshInviteCode = new Listener<EventArgs<IOrganization>>();
        public Listener<EventArgs<IOrganizationUser>> OnAcceptInvite = new Listener<EventArgs<IOrganizationUser>>();

        /// <summary>
        /// Код инвайта для нового пользователя
        /// </summary>
        public string CreateNewInvite(int organizationId, string email, int inviterId)
        {
            IUsersInvite invite = Repository.Invites.CreateNewInvite(organizationId, email, inviterId);
            return invite.InviteCode;
        }

        public List<IUsersInvite> GetInvites(int organizationId)
        {
            return Repository.Invites.GetInvites(organizationId).ToList();
        }

        /// <summary>
        /// Удаление старых инвайтов
        /// </summary>
        public void RemoveOldInvites(int days)
        {
            Repository.Invites.RemoveOldInvites(days);
        }        

        public IUsersInvite GetInvite(string inviteCode)
        {
            return Repository.Invites.GetInvite(inviteCode);
        }

        /// <summary>
        /// Обновление кода для ссылки-приглашения
        /// </summary>
        public string RefreshInviteCode(int organizationId)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                IOrganization organization = Repository.Organizations.Get(organizationId);
                organization.InviteCode = Guid.NewGuid().ToString().ToUpper();
                Repository.SubmitChanges();

                OnRefreshInviteCode.Invoke(new EventArgs<IOrganization>(organization));

                scope.Complete();

                return organization.InviteCode;
            }
        }

        public void AcceptInvite(int organizationId, int userId)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                IOrganizationUser user = Repository.Organizations.UpdateUser(organizationId, userId, EmployeeRole.Employee, true);
                // TODO: удалить старый ключ приглашения UsersInvite

                OnAcceptInvite.Invoke(new EventArgs<IOrganizationUser>(user));

                scope.Complete();
            }
        }        
    }
}
