using System;
using System.Linq;
using Timez.Entities;
using Timez.DAL.DataContext;
using System.Data.Linq;


namespace Timez.DAL.Users
{
    public interface IUsersRepository
    {
        /// <summary>
        /// Пользователь по ИДу
        /// </summary>
        IUser Get(int id);

        /// <summary>
		/// Пользователь по валидному мылу
        /// </summary>
        IUser GetByEmail(string email);

        /// <summary>
        /// Пользователь по логину
        /// </summary>
        IUser GetByLogin(string login);

        /// <summary>
        /// Пользователь по коду подтверждения регистрации
        /// </summary>
        IUser GetByCode(string code);

        IQueryable<IUser> GetByProject(int projectId);

        /// <summary>
        /// Создание пользователя
        /// </summary>
        IUser Add(string nick, string login, string password, string email, TimeSpan timeZone, RegistrationType type);

        /// <summary>
        /// Удаление пользователя из БД по мылу, если таковой есть
        /// </summary>
        void Delete(string login);

        /// <summary>
        /// Удаление просроченых неподтвержденных пользователей
        /// </summary>
        IUser[] RemoveUnconfirmed(double days);
    }

    /// <summary>
    /// Работа с пользователями
    /// </summary>
    class UsersRepository : BaseRepository<UsersRepository>, IUsersRepository
    {
        #region Gets
        /// <summary>
        /// Пользователь по ИДу
        /// </summary>
        public IUser Get(int id)
        {
            return _GetById(_Context, id);
        }
        static readonly Func<TimezDataContext, int, User> _GetById =
            CompiledQuery.Compile((TimezDataContext ctx, int id) => ctx.Users.FirstOrDefault(u => u.Id == id));

        /// <summary>
        /// Пользователь по валидному мылу
        /// </summary>
        public IUser GetByEmail(string email)
        {
            return _GetByEmail(_Context, email.Trim().ToUpper());
        }
        static readonly Func<TimezDataContext, string, User> _GetByEmail =
            CompiledQuery.Compile((TimezDataContext ctx, string email) 
				=> ctx.Users.FirstOrDefault(u => !u.EmailChangeDate.HasValue && u.EMail.ToUpper() == email));

        /// <summary>
        /// Пользователь по логину
        /// </summary>
        public IUser GetByLogin(string login)
        {
            return _GetByLogin(_Context, login.Trim().ToUpper());
        }
        static readonly Func<TimezDataContext, string, User> _GetByLogin =
            CompiledQuery.Compile((TimezDataContext ctx, string login) => ctx.Users.FirstOrDefault(u => u.Login.ToUpper() == login));

        /// <summary>
        /// Пользователь по коду подтверждения регистрации
        /// </summary>
        public IUser GetByCode(string code)
        {
            return _GetByCode(_Context, code);
        }
        static readonly Func<TimezDataContext, string, User> _GetByCode =
            CompiledQuery.Compile((TimezDataContext ctx, string code) => ctx.Users.FirstOrDefault(u => u.ConfimKey == code));

        public IQueryable<IUser> GetByProject(int projectId)
        {
            return _GetByProject(_Context, projectId);
        }
        static readonly Func<TimezDataContext, int, IQueryable<User>> _GetByProject =
        CompiledQuery.Compile((TimezDataContext dataContext, int projectId) =>
                from u in dataContext.Users
                join up in dataContext.ProjectsUsers
                on u.Id equals up.UserId
                where up.ProjectId == projectId
                select u);

        #endregion

        /// <summary>
        /// Создание пользователя
        /// </summary>
		public IUser Add(string nick, string login, string password, string email, TimeSpan timeZone, RegistrationType type)
        {
            // Создаем пользователя
            User user = new User
            {
                Password = password,
                EMail = email.Trim(),
                IsConfirmed = false,
                RegistrationDate = DateTimeOffset.Now,
                TimeZone = (int)timeZone.TotalMinutes,
                Nick = nick,
                ConfimKey = Guid.NewGuid().ToString(),
                RegistrationType = (int)type,
				Login = login,
				EmailChangeDate = null
            };

            _Context.Users.InsertOnSubmit(user);
            _Context.SubmitChanges();

            return user;
        }

        #region Deletes
        /// <summary>
        /// Удаление пользователя из БД по мылу, если таковой есть
        /// </summary>
        public void Delete(string login)
        {
            User u = _GetByLogin(_Context, login);
            if (u == null) return;

            _Context.Users.DeleteOnSubmit(u);
            _Context.SubmitChanges();
        }

        /// <summary>
        /// Удаление просроченых неподтвержденных пользователей
        /// </summary>
        public IUser[] RemoveUnconfirmed(double days)
        {
            User[] users = _Context.Users
                .Where(x => !x.IsConfirmed && x.RegistrationDate < DateTime.Now.AddDays(-days))
                .ToArray();

            _Context.Users.DeleteAllOnSubmit(users);
            _Context.SubmitChanges();

            return users;
        }

        #endregion
    }
}
