using Timez.Entities;

namespace Timez.BLL.Projects
{
    public sealed partial class ProjectsUtility
    {
        public ProjectsUtility()
        {
            OnCreate += (s, e) =>
            {
                IProject project = e.Data;
                var key = Cache.GetKeys(
                    CacheKey.Board, project.BoardId,
                    CacheKey.Project, CacheKey.All);
                Cache.Clear(key);
            };

            OnUpdate += (s, e) =>
            {
                IProject project = e.Data;
                Get(project.BoardId, project.Id).Name = project.Name;
            };

            OnDelete += (s, e) =>
            {
                IProject project = e.Data;

                var key = Cache.GetKeys(
                    CacheKey.Board, project.BoardId,
                    CacheKey.Project, CacheKey.All);
                Cache.Clear(key);
            };

            OnUpdateUserSettings += (s, e) =>
            {
                IProjectsUser projectsUser = e.Data;
                var key = Cache.GetKeys(
                    CacheKey.User, projectsUser.UserId,
                    CacheKey.ProjectsUser, CacheKey.All);
                Cache.Clear(key);
            };
        }
    }
}