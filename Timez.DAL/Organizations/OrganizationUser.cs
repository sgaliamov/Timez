using Common.Extentions;
using Timez.Entities;

namespace Timez.DAL.DataContext
{
	internal partial class OrganizationUser : IOrganizationUser
	{
		public EmployeeRole GetUserRole()
		{
			return (EmployeeRole)UserRole;
		}

		public bool IsAdmin
		{
			get { return UserRole.HasTheFlag((int)EmployeeRole.Administrator); }
		}
	}
}