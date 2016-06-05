using System;
using System.Collections.Generic;
using Timez.Entities;

namespace Timez.Controllers.Base
{
    /// <summary>
    /// Атрибут для проверки разрешений на доске
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class PermissionAttribute : Attribute
    {
        const string DefaultBoardIdParamName = "id";

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="boardIdParamName">наименование параметра ида доски</param>
        /// <param name="taskIdParamName">наименование параметра ида задачи</param>
        /// <param name="isArchiveParamName"></param>
        /// <param name="resultType">Возращать пустое представление, если нет доступа</param>
        /// <param name="roles">Набор каких прав нужно иметь</param>
        public PermissionAttribute(string boardIdParamName, string taskIdParamName, string isArchiveParamName, ResultType resultType, params UserRole[] roles)
        {
            Roles = roles;

            BoardIdParamName = boardIdParamName;
            TaskIdParamName = taskIdParamName;
            IsArchiveParamName = isArchiveParamName;

            Type = resultType;
        }

        public PermissionAttribute(string boardIdParamName, string taskIdParamName, ResultType resultType, params UserRole[] roles)
            : this(boardIdParamName, taskIdParamName, null, resultType, roles) { }

        public PermissionAttribute(string boardIdParamName, string taskIdParamName, params UserRole[] roles)
            : this(boardIdParamName, taskIdParamName, null, ResultType.View, roles) { }

        public PermissionAttribute(string boardIdParamName, params UserRole[] roles)
            : this(boardIdParamName, null, null, ResultType.View, roles) { }

        public PermissionAttribute(ResultType resultType, params UserRole[] roles)
            : this(DefaultBoardIdParamName, null, null, resultType, roles) { }

        public PermissionAttribute(params UserRole[] roles)
            : this(DefaultBoardIdParamName, null, null, ResultType.View, roles) { }

        public string IsArchiveParamName { get; private set; }
        public ResultType Type { get; private set; }
        public string TaskIdParamName { get; private set; }
        public string BoardIdParamName { get; private set; }
        public IEnumerable<UserRole> Roles { get; private set; }
    }

    /// <summary>
    /// Для проверки прав в организации
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class OrganizationPermissionAttribute : Attribute
    {
        const string DefaultIdParamName = "id";

        public OrganizationPermissionAttribute(string idParamName, ResultType resultType, params EmployeeRole[] roles)
        {
            Roles = roles;
            ResultType = resultType;
            IdParamName = idParamName;
        }
        public OrganizationPermissionAttribute(ResultType resultType, params EmployeeRole[] roles)
            : this(DefaultIdParamName, resultType, roles) { }
        public OrganizationPermissionAttribute(params EmployeeRole[] roles)
            : this(DefaultIdParamName, ResultType.View, roles) { }

        public ResultType ResultType { get; private set; }
        public string IdParamName { get; private set; }
        public IEnumerable<EmployeeRole> Roles { get; private set; }
    }

    public enum ResultType
    {
        View,
        JsonError,
        Empty,
        String
    }

    public class JsonMessage
    {
        public object Error;
        public object Data;
        public string Message;
    }
}