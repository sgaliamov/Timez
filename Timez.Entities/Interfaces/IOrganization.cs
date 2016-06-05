using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Timez.Entities
{
	public interface IOrganization
	{
		int Id { get; set; }
		[DisplayName("Название организации")]
		[Required(ErrorMessage = "Название обязательно")]
		string Name { get; set; }

		[DisplayName("Тариф")]
		[Required(ErrorMessage = "Выберите тариф")]
		int TariffId { get; set; }
		bool IsFree { get; set; }
		string Css { get; set; }
		string Logo { get; set; }
		decimal Money { get; set; }
		DateTimeOffset? WithdrawalDate { get; set; }
		bool IsBlocked { get; set; }
		string InviteCode { get; set; }
	}

	public interface IOrganizationUser
	{
		int Id { get; set; }
		int UserId { get; set; }
		int OrganizationId { get; set; }
		bool IsApproved { get; set; }
		int UserRole { get; set; }
		EmployeeRole GetUserRole();
		bool IsAdmin { get; }
	}
}