using System;
using System.Data.Linq;
using System.Linq;
using Timez.DAL.DataContext;
using Timez.Entities;

namespace Timez.DAL.Projects
{
    public interface IProjectsRepository
    {
        IOrderedQueryable<IProject> GetByBoard(int boardId);

        /// <summary>
        /// Создание
        /// </summary>
        IProject Create(int boardId, string name);

        /// <summary>
        /// Настройки пользоватлей проектов на доске
        /// </summary>        
        IQueryable<IProjectsUser> GetProjectsUsers(int userId);

        /// <summary>
        /// Настройки проекта у пользователя
        /// </summary>
        IProjectsUser GetProjectsUser(int projId, int userId);

        /// <summary>
        /// Удаление проекта
        /// </summary>
        /// <returns></returns>
        void Delete(int projId);

        /// <summary>
        /// bla
        /// </summary>
        /// <returns></returns>
        IProject Get(int projId);

        /// <summary>
        /// bla
        /// </summary>
        /// <returns></returns>
        IProjectsUser AddProjectsUser(int projId, int userId, Entities.ReciveType type);

        /// <summary>
        /// Все проекты пользователя на всех досках
        /// </summary>
        IQueryable<IProject> GetByUser(int userId);
    }

    class ProjectsRepository : BaseRepository<ProjectsRepository>, IProjectsRepository
    {
        public IOrderedQueryable<IProject> GetByBoard(int boardId)
        {
            return _GetByBoard(_Context, boardId);
        }
        static readonly Func<TimezDataContext, int, IOrderedQueryable<Project>> _GetByBoard =
            CompiledQuery.Compile((TimezDataContext ctx, int boardId) => ctx.Projects
                .Where(p => p.BoardId == boardId)
                .OrderBy(p => p.Name));

        /// <summary>
        /// Создание
        /// </summary>
        public IProject Create(int boardId, string name)
        {
            Project proj = new Project
            {
                Name = name,
                BoardId = boardId
            };

            _Context.Projects.InsertOnSubmit(proj);
            _Context.SubmitChanges();

            return proj;
        }

        /// <summary>
        /// Настройки пользоватлей проектов на доске
        /// </summary>        
        public IQueryable<IProjectsUser> GetProjectsUsers(int userId)
        {
            return _GetProjectsUsers(_Context, userId);
        }
        static readonly Func<TimezDataContext, int, IQueryable<ProjectsUser>> _GetProjectsUsers =
            CompiledQuery.Compile((TimezDataContext ctx, int userId) =>
                    from pu in ctx.ProjectsUsers
                    where pu.UserId == userId
                    select pu);

        /// <summary>
        /// Настройки проекта у пользователя
        /// </summary>
        public IProjectsUser GetProjectsUser(int projId, int userId)
        {
            var result = _GetProjectsUser(_Context, projId, userId);

            return result;
        }
        static readonly Func<TimezDataContext, int, int, ProjectsUser> _GetProjectsUser =
            CompiledQuery.Compile((TimezDataContext ctx, int projId, int userId) =>
                    (from pu in ctx.ProjectsUsers
                     where pu.ProjectId == projId
                     && pu.UserId == userId
                     select pu).FirstOrDefault());

        /// <summary>
        /// Удаление проекта
        /// </summary>
        /// <returns></returns>
        public void Delete(int projId)
        {
            Project proj = _Get(_Context, projId);
            //TimezProject ret = null;
            if (proj != null)
            {
                //ret = new TimezProject(proj);
                _Context.Projects.DeleteOnSubmit(proj);
                _Context.SubmitChanges();
            }
            //return ret;
        }

        /// <summary>
        /// bla
        /// </summary>
        /// <returns></returns>
        public IProject Get(int projId)
        {
            return _Get(_Context, projId);
        }
        static readonly Func<TimezDataContext, int, Project> _Get =
            CompiledQuery.Compile((TimezDataContext ctx, int projId) =>
                    ctx.Projects.FirstOrDefault(p => p.Id == projId));

        /// <summary>
        /// bla
        /// </summary>
        /// <returns></returns>
        public IProjectsUser AddProjectsUser(int projId, int userId, ReciveType type)
        {
            var proj = _Get(_Context, projId);
            ProjectsUser pu = new ProjectsUser
            {
                ProjectId = projId,
                BoardId = proj.BoardId,
                ReciveEMail = (int)type,
                UserId = userId
            };

            _Context.ProjectsUsers.InsertOnSubmit(pu);
            _Context.SubmitChanges();

            return pu;
        }

        /// <summary>
        /// Все проекты пользователя на всех досках
        /// </summary>
        public IQueryable<IProject> GetByUser(int userId)
        {
            return _GetByUser(_Context, userId);
        }
        static readonly Func<TimezDataContext, int, IQueryable<Project>> _GetByUser =
            CompiledQuery.Compile((TimezDataContext ctx, int userId) =>
                from p in ctx.Projects
                join pu in ctx.ProjectsUsers
                on p.Id equals pu.ProjectId
                where pu.UserId == userId
                select p);
    }
}
