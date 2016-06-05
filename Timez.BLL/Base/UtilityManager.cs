using System;
using System.Runtime.ConstrainedExecution;
using Timez.BLL.Boards;
using Timez.BLL.EventHistory;
using Timez.BLL.Organizations;
using Timez.BLL.Projects;
using Timez.BLL.Tasks;
using Timez.BLL.Texts;
using Timez.BLL.Users;
using Timez.DAL.DataContext;

namespace Timez.BLL
{
    /// <summary>
    /// Модель сайта
    /// </summary>
    public sealed class UtilityManager : CriticalFinalizerObject, IDisposable
    {
        /// <summary>
        /// Доступ к репозиториям данных
        /// </summary>
        internal Repositories Repository { get; private set; }
        internal ICacheService CacheUtility { get; private set; }
        public IAuthenticationService Authentication { get; private set; }
		public ISettingsService Settings { get; private set; }

		public UtilityManager(ICacheService cacheUtility, IAuthenticationService authenticationService, ISettingsService settings)
        {
            CacheUtility = cacheUtility;
            Authentication = authenticationService;
			Settings = settings;
			Repository = new Repositories(settings.ConnectionString);

			// Утилиты нужно создавать все, так как они могут использовать события друг друга, 
			// если подписчика не создавать, то подписанные события не стработают            
			// Утилиты подписываются на события менеджера, что бы знать когда инициализоваться и освобождать ресурсы

            Tasks = TasksUtility.Create(this);
            Users = UsersUtility.Create(this);
            Boards = BoardsUtility.Create(this);
            Projects = ProjectsUtility.Create(this);
            Statuses = TasksStatusesUtility.Create(this);
            Events = EventHistoryUtility.Create(this);
            Comments = CommentsUtility.Create(this);
            Invites = InvitesUtility.Create(this);
            Organizations = OrganizationsUtility.Create(this);
            Tariffs = TariffUtility.Create(this);
			Texts = TextsUtility.Create(this);

            OnInited();
        }        

        #region Утилиты

        public TasksUtility Tasks { get; private set; }
        public UsersUtility Users { get; private set; }
        public BoardsUtility Boards { get; private set; }
        public ProjectsUtility Projects { get; private set; }
        public TasksStatusesUtility Statuses { get; private set; }
        public EventHistoryUtility Events { get; private set; }
        public CommentsUtility Comments { get; private set; }
        public OrganizationsUtility Organizations { get; private set; }
        public InvitesUtility Invites { get; private set; }
        public TariffUtility Tariffs { get; private set; }
		public TextsUtility Texts { get; private set; }

        #endregion

        /// <summary>
        /// Когда менеджер инициализировался
        /// </summary>
        internal event Action OnInited = delegate { };

        /// <summary>
        /// Когда менеджер диспоузится
        /// </summary>
        internal event Action OnDispose = delegate { };

        public void Dispose()
        {
            OnDispose();
            Repository.Dispose();
        }
    }
}
