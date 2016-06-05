using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using Common.Extentions;
using Timez.Controllers.Base;
using Timez.Entities;

namespace Timez.Controllers
{
	[Authorize]
	public sealed class BoardsController : BaseController
	{
		#region Доска

		/// <summary>
		/// Достки
		/// </summary>
		/// <returns></returns>
		public ViewResult Index()
		{
			return View();
		}

		/// <summary>
		/// Список досок пользователя
		/// </summary>
		/// <param name="id">Организация</param>
		[OrganizationPermission(EmployeeRole.Belong)]
		public PartialViewResult List(int? id)
		{
			IEnumerable<IBoard> boards = Utility.Boards.GetByUser(Utility.Authentication.UserId);
			if (id.HasValue)
				boards = boards.Where(x => x.OrganizationId == id.Value);
			ViewData.Model = boards.OrderBy(x => x.OrganizationId).ThenBy(x => x.Name).ToList();

			// Организации пользователя
			IEnumerable<IOrganization> organizations = Utility.Organizations
				.GetByUser(Utility.Authentication.UserId, null)
				.Select(x => x.Organization)
				.ToArray();
			ViewData.Add("organizations", organizations);

			return PartialView("List");
		}

		//
		// GET: /Boards/Details/5
		[Permission(UserRole.Belong)]
		public ViewResult Details(int id)
		{
			ViewData.Model = Utility.Boards.Get(id);
			return View();
		}

		[HttpGet]
		[OrganizationPermission(EmployeeRole.Administrator)]
		public ViewResult Create(int? id = null)
		{
			ViewData.Add("OrganizationId", id);
			EditDetails(null);
			return View("Create");
		}

		[HttpPost]
		[OrganizationPermission("organizationId", ResultType.View, EmployeeRole.Administrator)]
		public ActionResult Create(FormCollection collection, int? organizationId = null)
		{
			return EditDetails(null, collection);
		}

		[Permission(UserRole.Owner)]
		[HttpGet]
		public ViewResult Edit(int id)
		{
			EditDetails(id);
			return View("Edit");
		}

		[Permission(UserRole.Owner)]
		[HttpPost]
		public ActionResult Edit(int id, FormCollection collection)
		{
			return EditDetails(id, collection);
		}

		void EditDetails(int? id)
		{
			// нельзя редактировать организацию, если ты не админ
			bool editOrganization = true;
			if (id.HasValue)
			{
				IBoard board = Utility.Boards.Get(id.Value);
				ViewData.Model = board;

				if (board.OrganizationId.HasValue)
				{
					ViewData.Add("Organization", Utility.Organizations.Get(board.OrganizationId.Value));
					EmployeeSettings userSettings = Utility.Organizations.GetUserSettings(board.OrganizationId.Value, Utility.Users.CurrentUser.Id);
					editOrganization = userSettings.Settings.IsAdmin;
				}
			}

			ViewData.Add("EditOrganization", editOrganization);
			if (editOrganization)
			{
				// только там, где пользователь админ, он может привязать доску
				IEnumerable<IOrganization> organizations = Utility.Organizations
					.GetByUser(Utility.Authentication.UserId)
					.Where(x => x.Settings.GetUserRole().HasTheFlag(EmployeeRole.Administrator))
					.Select(x => x.Organization)
					.ToList();
				ViewData.Add("organizations", organizations);
			}
		}

		ActionResult EditDetails(int? id, FormCollection collection)
		{
			string name = collection["name"];
			string description = collection["description"];
			int? refreshPeriod = collection["RefreshPeriod"].TryToInt();
			int? organizationId = collection["OrganizationId"].TryToInt();

			if (name.IsNullOrEmpty())
				ModelState.AddModelError("Name", "Название обязательно.");

			if (ModelState.IsValid)
			{
				try
				{
					IOrganization organization = organizationId.HasValue
													 ? Utility.Organizations.Get(organizationId.Value)
													 : null;
					if (id.HasValue)
					{
						Utility.Boards.Update(id.Value, name, description, refreshPeriod, organization);
					}
					else
					{
						IBoard board = Utility.Boards.Create(name, description, Utility.Users.CurrentUser, refreshPeriod, organization);
						id = board.Id;
					}
				}
				catch (TariffException exception)
				{
					ModelState.AddModelError("TariffException", exception);
				}
				catch (InvalidOperationTimezException exception)
				{
					ModelState.AddModelError("InvalidOperationTimezException", exception);
				}
			}

			if (!ModelState.IsValid)
			{
				// don't redirect because it's needed to preserve the same ModelState
				return id.HasValue ? Edit(id.Value) : Create(organizationId);	
			}

			// in success just redirect to the boards edit page
			return RedirectToAction("Edit", new { id = id.Value });
		}

		/// <summary>
		/// Удаление доски
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[Permission(UserRole.Owner)]
		public RedirectToRouteResult Delete(int id)
		{
			Utility.Boards.Delete(id);
			return RedirectToAction("Index");
		}
		#endregion

		#region Цвета/Приоритеты

		[Permission(UserRole.Owner)]
		public PartialViewResult EditColorList(int id)
		{
			List<IBoardsColor> colors = Utility.Boards.GetColors(id);
			ViewData.Model = colors;
			return PartialView("EditColorList");
		}

		[HttpPost]
		[Permission(UserRole.Owner)]
		public void EditColorList(int id, FormCollection collection)
		{
			// Сортируем
			if (!collection["ColorsOrder"].IsNullOrEmpty())
			{
				List<int> newOrder = collection["ColorsOrder"].Replace("[]=color_", "")
					.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(o => o.ToInt())
					//.Except(toDelete)
					.ToList(); // Исключаем удаленных
				Utility.Boards.SetColorsOrder(id, newOrder);
			}
		}

		[Permission("boardId", UserRole.Owner)]
		public PartialViewResult DeleteColor(int boardId, [Bind(Prefix = "id")] int colorId)
		{
			Utility.Boards.DeleteColor(colorId);
			return EditColorList(boardId);
		}

		[Permission("boardId", UserRole.Owner)]
		public PartialViewResult EditColor(int boardId, int? id)
		{
			if (id.HasValue)
			{
				IBoardsColor color = Utility.Boards.GetColor(boardId, id.Value);
				ViewData.Add("Id", color.Id);
				ViewData.Add("Color", color.Color);
				ViewData.Model = color;
			}
			else
			{
				Random randomclr = new Random();
				Color color = Color.FromArgb(0, randomclr.Next(256), randomclr.Next(256), randomclr.Next(256));
				ViewData.Add("Color", "#" + color.Name);
			}
			return PartialView();
		}

		[Permission("boardId", UserRole.Owner)]
		[HttpPost]
		public PartialViewResult EditColor(int boardId, int? id, FormCollection collection)
		{
			var name = collection["Name"];
			var color = collection["Color"];
			bool idDefault = collection.AllKeys.Contains("IsDefault")
				&& GetChecked(collection, "IsDefault").First();

			if (id.HasValue)
			{
				Utility.Boards.UpdateColor(id.Value, name, color, idDefault);
			}
			else
			{
				Utility.Boards.AddColor(boardId, name, color);
			}
			return EditColorList(boardId);
		}

		#endregion
	}
}
