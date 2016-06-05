using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Timez.Entities
{
    public interface ITasksStatus : IPosition
    {
        int BoardId { get; set; }

        bool IsBacklog { get; set; }

        [DisplayName("Количество назанченных задач на пользователя")]
        [Range(1, int.MaxValue, ErrorMessage = "Количество задач должно быть больше нуля")]
        int? MaxTaskCountPerUser { get; set; }

        [DisplayName("Название статуса")]
        [Required(ErrorMessage = "Название обязательно")]
        string Name { get; set; }

        bool NeedTimeCounting { get; set; }

        [DisplayName("Планирование обязательно")]
        bool PlanningRequired { get; set; }

        [DisplayName("Максимальное количество планируемых минут на пользователя")]
        [Range(1, int.MaxValue, ErrorMessage = "Планируемое время должно быть больше нуля")]
        int? MaxPlanningTime { get; set; }
    }
}
