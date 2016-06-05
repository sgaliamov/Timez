using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using Common.Extentions;
using Timez.Controllers.Base;
using Timez.Entities;

namespace Timez.Controllers
{
	[Authorize]
	public class OrganizationController : BaseController
	{
		#region Организации

		/// <summary>
		/// Страница огранизации
		/// </summary>
		[OrganizationPermission(EmployeeRole.Administrator)]
		public ViewResult Index(int id)
		{
			IOrganization organization = Utility.Organizations.Get(id);
			ViewData.Model = organization;

			int? availableBoarsdCount = Utility.Tariffs.GetAvailableBoardsCount(organization);
			ViewData.Add("availableBoarsdCount", availableBoarsdCount);

			return View();
		}

		/// <summary>
		/// Создание/Редактирования организации
		/// </summary>
		[HttpPost]
		[OrganizationPermission(ResultType.JsonError, EmployeeRole.Administrator)]
		public PartialViewResult Edit(int? id, FormCollection collection)
		{
			string name = collection["Name"];
			int? tariffId = collection["TariffId"].TryToInt();

			if (name.IsNullOrEmpty())
				ModelState.AddModelError("Name", "Название обязательно.");

			if (!tariffId.HasValue)
				ModelState.AddModelError("tariffId", "Вы не указали тариф.");

			if (ModelState.IsValid && tariffId.HasValue)
			{
				try
				{
					if (id.HasValue)
					{
						Utility.Organizations.Update(id.Value, name, tariffId.Value);
					}
					else
					{
						Utility.Organizations.Create(name, tariffId.Value);
					}
				}
				catch (CanBeOnlyOneFreeException exception)
				{
					ModelState.AddModelError(string.Empty, exception);
				}
			}

			return UserOrganizations();
		}

		[OrganizationPermission(ResultType.JsonError, EmployeeRole.Administrator)]
		public PartialViewResult Edit(int? id)
		{
			if (id.HasValue)
			{
				ViewData.Model = Utility.Organizations.Get(id.Value);
			}

			List<ITariff> tariffs = Utility.Tariffs.GetTariffs();
			ViewData.Add("tariffs", tariffs);

			return PartialView("Edit");
		}

		[OrganizationPermission(ResultType.String, EmployeeRole.Administrator)]
		public PartialViewResult Delete(int id)
		{
			Utility.Organizations.Delete(id);
			return UserOrganizations();
		}

		/// <summary>
		/// Организации текущего пользователя
		/// </summary>
		/// <returns></returns>
		public PartialViewResult UserOrganizations()
		{
			List<EmployeeSettings> organizations = Utility.Organizations.GetByUser(Utility.Authentication.UserId);
			ViewData.Model = organizations;

			return PartialView("UserOrganizations");
		}

		#endregion

		#region Сотрудники

		/// <summary>
		/// Список сотрудников
		/// </summary>
		/// <param name="id">ид организации</param>
		/// <returns>PartialView</returns>
		[OrganizationPermission(ResultType.JsonError, EmployeeRole.Belong)]
		public PartialViewResult EmployeeList(int id)
		{
			List<EmployeeSettings> settings = Utility.Organizations.GetEmployees(id);
			ViewData.Model = settings;

			return PartialView("EmployeeList");
		}

		/// <summary>
		/// Форма редактированиея
		/// </summary>
		/// <param name="organizationId"></param>
		/// <param name="id">ид пользователя</param>
		/// <returns></returns>
		[HttpGet]
		[OrganizationPermission("organizationId", ResultType.JsonError, EmployeeRole.Administrator)]
		public PartialViewResult EmployeeEdit(int organizationId, int id)
		{
			EmployeeSettings settings = Utility.Organizations.GetUserSettings(organizationId, id);
			ViewData.Model = settings;
			return PartialView();
		}

		/// <summary>
		/// Редактирование учасников
		/// </summary>
		[HttpPost]
		[OrganizationPermission("organizationId", ResultType.JsonError, EmployeeRole.Administrator)]
		public PartialViewResult EmployeeEdit(int organizationId, int id, FormCollection collection)
		{
			try
			{
				if (collection.AllKeys.Contains("save"))
				{
					EmployeeRole role = (EmployeeRole)collection["role"].ToInt();
					Utility.Organizations.UpdateRole(organizationId, id, role);

				}
				else if (collection.AllKeys.Contains("delete"))
				{
					Utility.Organizations.RemoveUser(organizationId, id);
				}
			}
			catch (NeedAdminException ex)
			{
				string message = "Пользователь не может покинуть огранизацию"
					+ Environment.NewLine + ex.Message
					+ Environment.NewLine + "Назначте на другого пользователя администратором этой доски.";
				ModelState.AddModelError("NeedAdminException", message);
			}
			catch (TasksExistsException ex)
			{
				string message = "Пользователь не может покинуть огранизацию"
					+ Environment.NewLine + ex.Message
					+ Environment.NewLine + "Переназначте задачи на другого пользователя.";
				ModelState.AddModelError("TasksExistsException", message);
			}

			return EmployeeList(organizationId);
		}

		/// <summary>
		/// Выход из огранизации
		/// </summary>
		/// <param name="id">Организации</param>
		/// <returns></returns>
		[OrganizationPermission(ResultType.JsonError, EmployeeRole.Belong)]
		public PartialViewResult Leave(int id)
		{
			try
			{
				Utility.Organizations.Leave(id);
			}
			catch (NeedAdminException ex)
			{
				ModelState.AddModelError("NeedAdminException", ex);
			}
			catch (TasksExistsException ex)
			{
				string message = "Пользователь не может покинуть огранизацию"
					+ Environment.NewLine + ex.Message
					+ Environment.NewLine + "Переназначте задачи на другого пользователя.";
				ModelState.AddModelError("TasksExistsException", message);
			}

			return UserOrganizations();
		}

		#endregion
	}
}