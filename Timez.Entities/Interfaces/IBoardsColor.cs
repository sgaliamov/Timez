using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Timez.Entities
{
    public interface IBoardsColor : IPosition
    {
        int BoardId { get; }

        [DisplayName("Цвет")]
        string Color { get; set; }
        
        [DisplayName("Название")]
        [Required(ErrorMessage = "Название обязательно")]
        string Name { get; set; }
        
        [DisplayName("Использовать поумолчанию")]
        bool IsDefault { get; set; }
    }
}
