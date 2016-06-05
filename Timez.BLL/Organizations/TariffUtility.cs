using System.Collections.Generic;
using System.Linq;
using Timez.Entities;

namespace Timez.BLL.Organizations
{
    /// <summary>
    /// Все что завязано на тарифах
    /// </summary>
    public class TariffUtility : BaseUtility<TariffUtility>
    {
        public List<ITariff> GetTariffs()
        {
            var key = Cache.GetKeys(CacheKey.Tariff, CacheKey.All);
            return Cache.Get(key, () => Repository.Organizations.GetTariffs().ToList());
        }

        public ITariff GetTariff(int tariffId)
        {
            return GetTariffs().FirstOrDefault(x => x.Id == tariffId);
        }

        /// <summary>
        /// Сколько пользователей можно еще добавить
        /// </summary>
        public int? GetAvailableUsersCount(IOrganization organization)
        {
            ITariff tariff = GetTariff(organization.TariffId);
            if (tariff.EmployeesCount.HasValue)
            {
                List<EmployeeSettings> employees = Utility.Organizations.GetEmployees(organization.Id);
                return tariff.EmployeesCount.Value - employees.Count;
            }
            return null;
        }

        public int? GetAvailableBoardsCount(IOrganization organization)
        {
            ITariff tariff = GetTariff(organization.TariffId);
            if (tariff.BoardsCount.HasValue)
            {
                List<IBoard> boards = Utility.Boards.GetByOrganization(organization.Id);
                return tariff.BoardsCount.Value - boards.Count;
            }
            return null;
        }

        public int? GetAvailableProjectsCount(int boardId)
        {
            IBoard board = Utility.Boards.Get(boardId);
            if (!board.OrganizationId.HasValue)
                return null;

            IOrganization organization = Utility.Organizations.Get(board.OrganizationId.Value);
            ITariff tariff = GetTariff(organization.TariffId);
            if (tariff.ProjectsPerBoard.HasValue)
            {
                List<IProject> projects = Utility.Projects.GetByBoard(boardId);
                return tariff.ProjectsPerBoard.Value - projects.Count;
            }
            return null;
        }
    }
}
