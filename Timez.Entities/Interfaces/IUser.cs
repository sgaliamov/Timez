using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Timez.Entities
{
    public interface IUser : IId
    {
        string ConfimKey { get; set; }

        [DisplayName("EMail для уведомлений")]
        [Required(ErrorMessage = "EMail для уведомлений обязателен")]
        string EMail { get; set; }

        bool IsAdmin { get; set; }
        bool IsConfirmed { get; set; }

        [DisplayName("Отображаемое имя")]
        [Required(ErrorMessage = "Имя обязательно")]
        string Nick { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        string Password { get; set; }

        DateTimeOffset RegistrationDate { get; set; }
        int TimeZone { get; set; }
        bool RecievOwnEvents { get; set; }
        
        RegistrationType GetRegistrationType();
        string Login { get; set; }

		/// <summary>
		/// Время изменения имейла
		/// Если оно задано, значит имейл невалидный.
		/// Обычно равно нулю.
		/// </summary>
		DateTimeOffset? EmailChangeDate { get; set; }
    }
}
