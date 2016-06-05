using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Timez.Entities;
using CacheKeyValue = System.Collections.Generic.KeyValuePair<Timez.BLL.CacheKey, string>;

namespace Timez.BLL.Organizations
{
    public sealed partial class OrganizationsUtility : BaseUtility<OrganizationsUtility>
    {
        public readonly Listener<EventArgs<IOrganizationUser>> OnEmployeeRemove = new Listener<EventArgs<IOrganizationUser>>();
        public readonly Listener<EventArgs<IOrganizationUser>> OnUpdateRole = new Listener<EventArgs<IOrganizationUser>>();
        public readonly Listener<EventArgs<IOrganizationUser>> OnAddEmployee = new Listener<EventArgs<IOrganizationUser>>();

        public readonly Listener<EventArgs<int>> OnDelete = new Listener<EventArgs<int>>();

        #region Organizations

        /// <summary>
        /// Настройки пользователя в организациях
        /// </summary>
        public List<EmployeeSettings> GetByUser(int userId, bool? isApproved = true)
        {
            return Repository.Organizations.GetByUser(userId, isApproved).ToList();
        }

        public IOrganization GetByInviteCode(string inviteCode)
        {
            return Repository.Organizations.GetByInviteCode(inviteCode);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IOrganization> GetToAppove(int userId)
        {
            return GetByUser(userId, false).Select(x => x.Organization);
        }

        public IOrganization Get(int id)
        {
            return Repository.Organizations.Get(id);
        }

        public IOrganization Create(string name, int tariffId)
        {
            ITariff tariff = Utility.Tariffs.GetTariff(tariffId);

            return Create(name, tariff, Authentication.UserId);
        }

        IOrganization Create(string name, ITariff tariff, int creatorId)
        {
            bool isFree = tariff.IsFree();
            if (isFree)
                CheckOneFree(creatorId);

            IOrganization organization = Repository.Organizations.Create(name, tariff.Id, isFree);

            Repository.Organizations.AddUser(organization.Id, creatorId, EmployeeRole.Administrator, true);

            return organization;
        }

        IOrganization Update(int id, string name, ITariff tariff)
        {
            IOrganization organization = Repository.Organizations.Get(id);

            bool isFree = tariff.IsFree();
            if (!organization.IsFree && isFree)
            {
                // при изменении тарифа на бесплатный
                // нужно проверять, состоят ли сотрудники в других бесплатных организациях
                Repository.Organizations.GetUsers(id)
                    .ToList()
                    .ForEach(x => CheckOneFree(x.UserId));
            }

            organization.Name = name;
            organization.TariffId = tariff.Id;
            organization.IsFree = isFree;
            Repository.SubmitChanges();

            return organization;
        }

        public IOrganization Update(int id, string name, int tariffId)
        {
            ITariff tariff = Utility.Tariffs.GetTariff(tariffId);

            IOrganization organization = Update(id, name, tariff);

            return organization;
        }

        /// <summary>
        /// Удаление огранизации
        /// </summary>
        public void Delete(int id)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                // Удаляем все доски перед удалением
                List<IBoard> boards = Utility.Boards.GetByOrganization(id);
                foreach (IBoard board in boards)
                    Utility.Boards.Delete(board.Id);

                Repository.Organizations.Delete(id);

                OnDelete.Invoke(this, new EventArgs<int>(id));

                scope.Complete();
            }
        }

        #endregion

        #region Users

        /// <summary>
        /// Может быть только одна бесплатная организация у пользователя
        /// </summary>
        /// <exception cref="CanBeOnlyOneFreeException"></exception>
        private void CheckOneFree(int userId)
        {
            List<EmployeeSettings> organizations = GetByUser(userId);
            CheckOneFree(organizations);
        }
        private static void CheckOneFree(IEnumerable<EmployeeSettings> organizations)
        {
            EmployeeSettings settings = organizations.FirstOrDefault(x => x.Organization.IsFree);
            if (settings != null)
            {
                IUser user = settings.User;
                throw new CanBeOnlyOneFreeException(user);
            }
        }

        /// <summary>
        /// Добавлени нового сотрудника в организацию
        /// пользователь должен подтвердить добавление
        /// </summary>
        /// <returns>
        /// true - добавлен
        /// false - уже был добавлен
        /// </returns>
        /// <exception cref="CanBeOnlyOneFreeException"></exception>
        /// <exception cref="TariffException"></exception>
        public bool AddUser(IOrganization organization, IUser user, EmployeeRole role = EmployeeRole.Employee, bool isApproved = false)
        {
            List<EmployeeSettings> organizations = GetByUser(user.Id);

            if (organization.IsFree)
                CheckOneFree(organizations);

            if (organizations.Any(x => x.Organization.Id == organization.Id))
                return false;

            int? usersCount = Utility.Tariffs.GetAvailableUsersCount(organization);
            if (usersCount.HasValue && usersCount.Value <= 0)
            {
                string message = 
                    "Пользователь " + user.Nick 
                    + " не может быть добавлен в огранизацию " + organization.Name + "." + Environment.NewLine
                    + "Превышен лимит количества пользователей в огранизации.";
                throw new TariffException(message);
            }

            using (TransactionScope scope = new TransactionScope())
            {
                IOrganizationUser organizationUser = Repository.Organizations.AddUser(organization.Id, user.Id, role, isApproved);

                OnAddEmployee.Invoke(new EventArgs<IOrganizationUser>(organizationUser));

                scope.Complete();
            }

            return true;
        }

        /// <summary>
        /// Исключение из организации
        /// </summary>
        public void RemoveUser(int organizationId, int userId)
        {
            // в организации должен остаться хотя бы один админ
            IOrganizationUser user = CheckLastAdmin(organizationId, userId);

            using (TransactionScope scope = new TransactionScope())
            {
                // Удаляем со всех досок в этой огранизации
                List<IBoard> boards = Utility.Boards.GetByOrganization(organizationId);
                foreach (IBoard board in boards)
                    Utility.Boards.RemoveUserFromBoard(board, userId);

                // Удяляем из огранизации
                Repository.Organizations.RemoveUser(user);

                OnEmployeeRemove.Invoke(new EventArgs<IOrganizationUser>(user));

                scope.Complete();
            }
        }

        /// <summary>
        /// Выход из огранизации текущего пользователя
        /// </summary>
        public void Leave(int organizationId)
        {
            RemoveUser(organizationId, Authentication.UserId);
        }

        /// <summary>
        /// Проверяет является ли userId последним админом
        /// Если является, то бросается эксепшн NeedAdminException
        /// </summary>
        /// <exception cref="NeedAdminException"></exception>
        /// <returns>Настройки пользователя в организации</returns>
        private IOrganizationUser CheckLastAdmin(int organizationId, int userId)
        {
            List<IOrganizationUser> list = Repository.Organizations.GetOrganizationUsers(organizationId).ToList();
            IOrganizationUser user = list.First(x => x.UserId == userId);
            EmployeeRole role = user.GetUserRole();
            if (role.HasTheFlag(EmployeeRole.Administrator))
            {
                int count = list.Count(x => x.GetUserRole().HasTheFlag(EmployeeRole.Administrator));
                if (count < 2)
                    throw new NeedAdminException("В организации должен быть хотя бы один администратор");
            }

            return user;
        }

        /// <summary>
        /// Сотрудники
        /// кешируется
        /// </summary>
        public List<EmployeeSettings> GetEmployees(int organizationId)
        {
            var key = GetKeyEmployee(organizationId);
            
            List<EmployeeSettings> settings = 
                Cache.Get(key, 
                    () => Repository.Organizations.GetEmployeeSettings(organizationId).ToList());

            return settings;
        }

        public EmployeeSettings GetUserSettings(int organizationId, int userId)
        {
            return GetEmployees(organizationId).FirstOrDefault(x => x.User.Id == userId);
        }

        public void UpdateRole(int organizationId, int userId, EmployeeRole role)
        {
            // Если пользователя лишают админских прав, нужно проверить, есть ли еще други админы
            IOrganizationUser user = CheckLastAdmin(organizationId, userId);

            if (user.UserRole != (int)role)
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    user.UserRole = (int)role;
                    Repository.SubmitChanges();

                    OnUpdateRole.Invoke(new EventArgs<IOrganizationUser>(user));

                    scope.Complete();
                }
                
            }
        }

        #endregion        
    }
}