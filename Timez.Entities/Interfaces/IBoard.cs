using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Timez.Entities
{
    public interface IBoard : IId
    {
        [DisplayName("Описание")]
        string Description { get; set; }

        [DisplayName("Название")]
        [Required(ErrorMessage = "Название обязательно")]
        string Name { get; set; }

        [DisplayName("Переодичность автоматического обновления")]
        [Range(10, int.MaxValue, ErrorMessage = "Минимальное значение: 10 минут")]
        int? RefreshPeriod { get; set; }

        [DisplayName("Организация")]
        int? OrganizationId { get; set; }
    }
}
