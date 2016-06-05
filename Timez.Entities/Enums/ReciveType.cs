using System;

namespace Timez.Entities
{
    /// <summary>
    /// Типы рассылки
    /// </summary>
    [Flags]
    public enum ReciveType : int
    {
        NotDefined = 0, // Не определена
        
        TaskAssigned = 1, // На пользователя назначена задача
        TaskStatusChanged = 2, // Какая-то задача в проекте на доске поменяла статус
        TaskCreated = 4, // На доске в проекте создана задача

        All = TaskAssigned | TaskStatusChanged | TaskCreated // Все уведомления
    }
}
