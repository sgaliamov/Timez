using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Transactions;
using Common;
using Timez.DAL.Boards;
using Timez.DAL.Organizations;
using Timez.DAL.Projects;
using Timez.DAL.Tasks;
using Timez.DAL.Texts;
using Timez.DAL.Users;
using Timez.Entities;

namespace Timez.DAL.DataContext
{
    // TODO: extract an interface
    /// <summary>
    /// Service locator of repositories
    /// </summary>
    public class Repositories : CriticalFinalizerObject, IDisposable
    {
        #region Stuff
        private readonly TimezDataContext _Context;

        public void Dispose()
        {
            _Context.Dispose();
        }

        /// <summary>
        /// Установка упорядоченности
        /// </summary>
        /// <param name="newOrder">иды сущностей в нужной последовательности</param>
        /// <param name="entities">упорядочиваемые сущности</param>
        /// <param name="onUpdate"></param>
        public void SetOrder<T>(IEnumerable<int> newOrder, IEnumerable<T> entities, Action<T> onUpdate)
            where T : IPosition
        {
            // Порядок не задан, ничего не делаем
            if (newOrder == null || entities == null || !newOrder.Any() || !entities.Any())
                return;

            // Гарантируем что порядки у статусов были
            if (entities.Count() != newOrder.Count())
                throw new ArgumentException("Количество не совпадает.");

            if (entities.Select(s => s.Id).Except(newOrder).Count() != 0)
                throw new ArgumentException("Сортировка не возможна.");

            // Создаем список, в котором упорядочено по новому
            List<T> newList = newOrder.Select(item => entities.First(s => s.Id == item)).ToList();

            // Назначаем в этом списке старый порядок
            IEnumerator<int> oldOrdEnumerator = entities
                .Select(s => s.Position)
                .ToList() // !нужно обязательно, иначе oldOrdEnumerator.MoveNext() работает некорректно!
                .GetEnumerator();
            var enumerator = newList.GetEnumerator();

            using (TransactionScope scope = new TransactionScope())
            {
                while (oldOrdEnumerator.MoveNext() && enumerator.MoveNext())
                {
                    if (enumerator.Current.Position != oldOrdEnumerator.Current)
                    {
                        enumerator.Current.Position = oldOrdEnumerator.Current;
                        _Context.SubmitChanges();

                        onUpdate(enumerator.Current);
                    }
                }

                scope.Complete();
            }
        }

        public void SubmitChanges()
        {
            _Context.SubmitChanges();
        }

        #endregion

        #region Repositories
        public Repositories(string connection)
        {
            #region Data context creating
            // ReSharper disable UseObjectOrCollectionInitializer
            _Context = new TimezDataContext(connection);
            // ReSharper restore UseObjectOrCollectionInitializer
#if DEBUG
            _Context.CommandTimeout = 60 * 5;
            _Context.Log = new DebuggerWriter();
#endif
            #endregion

            Boards = BoardsRepository.Create(_Context);
            BoardsColors = BoardsColorsRepository.Create(_Context);
            BoardsUsers = BoardsUsersRepository.Create(_Context);
            Tasks = TasksRepository.Create(_Context);
            TasksStatuses = TasksStatusesRepository.Create(_Context);
            Users = UsersRepository.Create(_Context);
            Organizations = OrganizationsRepository.Create(_Context);
            Projects = ProjectsRepository.Create(_Context);
            Comments = CommentsRepository.Create(_Context);
            EventHistory = EventHistoryRepository.Create(_Context);
            Invites = InvitesRepository.Create(_Context);
			Texts = TextsRepository.Create(_Context);
        }

        public IBoardsUsersRepository BoardsUsers { get; private set; }
        public IBoardsColorsRepository BoardsColors { get; private set; }
        public IEventHistoryRepository EventHistory { get; private set; }
        public IInvitesRepository Invites { get; private set; }
        public IOrganizationsRepository Organizations { get; private set; }
        public IProjectsRepository Projects { get; private set; }
        public ICommentsRepository Comments { get; private set; }
        public ITasksRepository Tasks { get; private set; }
        public ITasksStatusesRepository TasksStatuses { get; private set; }
        public IUsersRepository Users { get; private set; }
        public IBoardsRepository Boards { get; private set; }
		public ITextsRepository Texts { get; private set; }

        #endregion

    }
}
