using System;
using Common.Alias;

namespace Timez.Entities
{
    [Flags]
    public enum EventType
    {
        [Alias("Добавление")]
        CreateTask = 1,

        [Alias("Обновление")]
        Update = CreateTask << 1,

        [Alias("Удаление")]
        Delete = CreateTask << 2,

        [Alias("Предупреждение")]
        Warning = CreateTask << 3,

        [Alias("Ошибка")]
        Error = CreateTask << 4,

        [Alias("Превышение лимита задач")]
        CountLimitIsReached = CreateTask << 5,

        [Alias("Превышение времени, сумма задач в колонке более {0} минут")]
        PlanningTimeIsExceeded = CreateTask << 6,

        [Alias("Привязка задачи к пользователю")]
        TaskAssigned = CreateTask << 7,

        [Alias("Изменение приоритета задачи")]
        TaskColorChanged = CreateTask << 8,

        [Alias("Изменение планируемого времени на задачу")]
        PlaningTimeChanged = CreateTask << 9,

        [Alias("Изменение проекта")]
        ProjectChanged = CreateTask << 10,

        [Alias("Архивирование задачи")]
        TaskToArchive = CreateTask << 11,

        [Alias("Восстановление задачи")]
        TaskRestore = CreateTask << 12,
        
        All = int.MaxValue
    }
}
