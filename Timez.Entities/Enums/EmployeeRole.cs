using System;

namespace Timez.Entities
{
    /// <summary>
    /// Роли пользователя в организации
    /// </summary>
    [Flags]
    public enum EmployeeRole
    {
        Belong = 0,
        Employee = 1,
        Administrator = 2
    }

    public static class EmployeeRoleExtentions
    {
        public static bool HasTheFlag(this EmployeeRole value, EmployeeRole flag)
        {
            return (value & flag) == flag;
        }

        public static bool HasAnyFlag(this EmployeeRole value, EmployeeRole flag)
        {
            return (value & flag) > 0;
        }
    }
}