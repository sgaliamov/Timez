using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Common.Extentions;
using Timez.Entities;

namespace Timez.BLL.Projects
{
    public sealed partial class ProjectsUtility : BaseUtility<ProjectsUtility>
    {
        /// <summary>
        /// Очистка:
        /// - задач из кеша
        /// - список проектов на доске
        /// </summary>
        public Listener<EventArgs<IProject>> OnDelete = new Listener<EventArgs<IProject>>();
        public Listener<EventArgs<IProject>> OnUpdate = new Listener<EventArgs<IProject>>();
        public Listener<EventArgs<IProject>> OnCreate = new Listener<EventArgs<IProject>>();

        /// <summary>
        /// Установка рассылки для пользователя в проекте
        /// </summary>
        public Listener<EventArgs<IProjectsUser>> OnUpdateUserSettings = new Listener<EventArgs<IProjectsUser>>();

        public List<IProject> GetByBoard(int boardId)
        {
            var key = Cache.GetKeys(
                CacheKey.Board, boardId,
                CacheKey.Project, CacheKey.All);

            return Cache.Get(key, () => Repository.Projects.GetByBoard(boardId).ToList());
        }

        public List<IProject> GetByUser(int userId)
        {
            return Repository.Projects.GetByUser(userId).ToList();
        }

        public IProject Get(int boardId, int projectId)
        {
            return GetByBoard(boardId).First(x => x.Id == projectId);
        }

        /// <summary>
        /// Создаем проект и подписывает создателя на все события на доске
        /// </summary>
        public IProject Create(int boardId, string name, IUser creatorUser)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                int? projectsCount = Utility.Tariffs.GetAvailableProjectsCount(boardId);
                if (projectsCount.HasValue && projectsCount.Value <= 0)
                    throw new TariffException("Достигнут лимит количества проектов");

                IProject prj = Repository.Projects.Create(boardId, name);

                //в зависимости от валидности мыла по разному подписываем на событи + при регистрации через ФБ не всегда валидировать мыло сразу
                ReciveType reciveType = creatorUser.EMail.IsValidEmail()
                                            ? ReciveType.All
                                            : ReciveType.NotDefined;

                Repository.Projects.AddProjectsUser(prj.Id, creatorUser.Id, reciveType);

                OnCreate.Invoke(new EventArgs<IProject>(prj));

                scope.Complete();

                return prj;
            }
        }

        /// <summary>
        /// Удаление проекта с доски
        /// </summary>        
        void Delete(IProject project)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                int count = Repository.Projects.GetByBoard(project.BoardId).Count();
                if (count == 1)
                    throw new NeedProjectException("На доске должен оставаться хотя бы один проект.");

                Repository.Projects.Delete(project.Id);
                
                OnDelete.Invoke(new EventArgs<IProject>(project));

                scope.Complete();
            }
        }

        public void Delete(int boardId, int projId)
        {
            IProject project = Get(boardId, projId);
            Delete(project);
        }

        /// <summary>
        /// Обновление имени
        /// </summary>
        public IProject Update(int projId, string name)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                IProject proj = Repository.Projects.Get(projId);
                proj.Name = name;
                Repository.SubmitChanges();

                // Обновляем в задачах
                Repository.Tasks.UpdateProjectName(proj);

                // обновление в кеше задач названий проектов
                OnUpdate.Invoke(new EventArgs<IProject>(proj));

                scope.Complete();

                return proj;
            }
        }

        #region Настройки рассылки
        public List<IProjectsUser> GetSettings(int userId)
        {
            var key = Cache.GetKeys(
                CacheKey.User, userId,
                CacheKey.ProjectsUser, CacheKey.All);

            return Cache.Get(key, () => Repository.Projects.GetProjectsUsers(userId).ToList());
        }

        /// <summary>
        /// Установка рассылки для пользователя в проекте
        /// </summary>
        public void Update(int projId, int userId, ReciveType type)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                var setting = Repository.Projects.GetProjectsUser(projId, userId);
                if (setting == null)
                {
                    Repository.Projects.AddProjectsUser(projId, userId, type);
                }
                else
                {
                    if (setting.ReciveEMail != (int) type)
                    {
                        setting.ReciveEMail = (int) type;
                        Repository.SubmitChanges();
                    }
                }

                OnUpdateUserSettings.Invoke(new EventArgs<IProjectsUser>(setting));

                scope.Complete();
            }
        }         
        #endregion
    }
}
