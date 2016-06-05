using Common.Alias;

namespace Timez.Entities
{
    /// <summary>
    /// Способы сотрировки задач
    /// </summary>
    public enum TasksSortType
    {
        [Alias("По имени")]
        ByName,

        [Alias("По приоритету")]
        ByColor,

        [Alias("По проекту")]
        ByProject,

        [Alias("По исполнителю")]
        ByExecutor,

        [Alias("По статусу")]
        ByStatus,

        [Alias("По времени создания")]
        ByCreationDate,

        [Alias("По времени изменения статуса")]
        ByStatusChangeDateTime,

        [Alias("По планируемой длительности")]
        ByPlanningTime
    }
}
