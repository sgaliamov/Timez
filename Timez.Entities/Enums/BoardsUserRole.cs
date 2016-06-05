using System;

namespace Timez.Entities
{
    /// <summary>
    /// Роли пользователя на доске
    /// </summary>
    [Flags]
    public enum UserRole
    {
        Belong = 0, // Пользователь имеет отношение к доске
        Executor = 1, // Исполнитель
        Owner = 2, // Владелец
        Customer = 4, // Заказчик
        Observer = 8, // Наблюдатель

        All = 0x7fffffff, // int.Max

        ExecutorAndOwnerAndCustomer = Customer | Executor | Owner
    }

    public static class UserRoleExtentions
    {
        public static bool HasTheFlag(this UserRole value, UserRole flag)
        {
            return (value & flag) == flag;
        }

        public static bool HasAnyFlag(this UserRole value, UserRole flag)
        {
            return (value & flag) > 0;
        }

        public static bool HasTheFlag(this UserRole? value, UserRole flag)
        {
			return value.HasValue && value.Value.HasTheFlag(flag);
        }

        public static bool HasAnyFlag(this UserRole? value, UserRole flag)
        {
            return value.HasValue && value.Value.HasAnyFlag(flag);
        }
    }
}
