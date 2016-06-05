using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Timez.Controllers;
using Timez.DAL.DataContext;
using Timez.Entities;
using Timez.Helpers;

namespace Timez.Test
{
	/// <summary>
	/// Summary description for Main
	/// </summary>
	[TestClass]
	public class Organization
	{
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		const string Email0 = "test0@test.ru";
		const string Email1 = "test1@test.ru";
		const string Email2 = "test2@test.ru";
		const string Email3 = "test3@test.ru";
		const string Email4 = "test4@test.ru";
		const string Email5 = "test5@test.ru";
		const string Email6 = "test6@test.ru";
		const string Email7 = "test7@test.ru";
		const string Email8 = "test8@test.ru";
		const string Email9 = "test9@test.ru";

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize]
		public static void ClassInitialize(TestContext testContext)
		{
			ClassCleanup();
		}

		//
		// Use ClassCleanup to run code after all tests in a class have run
		[ClassCleanup]
		public static void ClassCleanup()
		{
			Repositories repositories = new Repositories(ConfigurationManager.ConnectionStrings["TimezConnectionString"].ConnectionString);
			try { repositories.Users.Delete(Email3); }
			catch { }
			try { repositories.Users.Delete(Email4); }
			catch { }
			try { repositories.Users.Delete(Email1); }
			catch { }
			try { repositories.Users.Delete(Email2); }
			catch { }
			try { repositories.Users.Delete(Email0); }
			catch { }

			try { repositories.Users.Delete(Email5); }
			catch { }
			try { repositories.Users.Delete(Email6); }
			catch { }
			try { repositories.Users.Delete(Email7); }
			catch { }
			try { repositories.Users.Delete(Email8); }
			catch { }
			try { repositories.Users.Delete(Email9); }
			catch { }
		}

		#endregion

		[TestMethod]
		public void OrganizationTest()
		{
			FormCollection collection = new FormCollection();
			ViewResult result;
			RedirectToRouteResult redirectToRouteResult;

			OrganizationController organizationController = Base.GetController<OrganizationController>();
			PartialViewResult partialViewResult = organizationController.Edit(null);

			List<ITariff> tariffs = partialViewResult.ViewData.Get<List<ITariff>>("tariffs");
			Assert.IsNotNull(tariffs);
			ITariff freeTariff = tariffs.Single(x => x.IsFree());
			Assert.IsNotNull(freeTariff);
			Main.Registation(Email0);

			// создали тестовую организацию
			collection.Clear();
			organizationController = Base.GetController<OrganizationController>();
			collection.Add("Name", "test");
			collection.Add("TariffId", freeTariff.Id.ToString());
			partialViewResult = organizationController.Edit(null, collection);
			IOrganization organization = ((List<EmployeeSettings>)partialViewResult.Model).Single().Organization;
			Assert.IsNotNull(organization);
			int organizationId = organization.Id;

			#region Приглашения

			InviteController inviteController = Base.GetController<InviteController>();

			// через мыло незареганного
			collection.Clear();
			collection.Add("Email", Email3);
			inviteController.NewInvite(organizationId, collection);
			List<IUsersInvite> invites = inviteController.Utility.Invites.GetInvites(organizationId);
			IUsersInvite invite = invites.FirstOrDefault(x => x.EMail.ToUpper() == Email3.ToUpper());
			Assert.IsNotNull(invite);
			Main.Registation(Email3, out result, out redirectToRouteResult, invite.InviteCode);
			ViewResultBase resultBase = organizationController.EmployeeList(organizationId);
			EmployeeSettings emeil3User = (resultBase.Model as List<EmployeeSettings>).FirstOrDefault(x => x.User.EMail.ToUpper() == Email3.ToUpper());
			Assert.IsNotNull(emeil3User);
			collection.Clear();
			collection.Add("delete", "true");
			organizationController.EmployeeEdit(organizationId, emeil3User.User.Id, collection);

			// через мыло зареганного
			inviteController.Dispose();
			inviteController = Base.GetController<InviteController>();
			collection.Clear();
			collection.Add("Email", Email3);
			inviteController.NewInvite(organizationId, collection);
			invites = organizationController.Utility.Invites.GetInvites(organizationId);
			invite = invites.FirstOrDefault(x => x.EMail.ToUpper() == Email3.ToUpper());
			Assert.IsNotNull(invite);
			Base.GetController<AdminController>().ClearCache();
			resultBase = organizationController.EmployeeList(organizationId);
			emeil3User = (resultBase.Model as List<EmployeeSettings>).FirstOrDefault(x => x.User.EMail.ToUpper() == Email3.ToUpper());
			Assert.IsNotNull(emeil3User);
			var userController = Base.GetController<UserController>();
			userController.Login(null, Email3, Email3, true, null);
			inviteController.AcceptInvite(organizationId);
			emeil3User = (resultBase.Model as List<EmployeeSettings>).FirstOrDefault(x => x.User.EMail.ToUpper() == Email3.ToUpper());
			Assert.IsTrue(emeil3User.Settings.UserRole == (int)EmployeeRole.Employee);

			// через ссылку незареганного пользователя
			userController.Dispose();
			userController = Base.GetController<UserController>();
			userController.SignOut();
			redirectToRouteResult = (RedirectToRouteResult)userController.Invite(organization.InviteCode);
			Assert.IsTrue(redirectToRouteResult.RouteValues["action"].ToString() == "Register");
			collection.Clear();
			Main.Registation(Email4, out result, out redirectToRouteResult, organization.InviteCode);
			resultBase = inviteController.List();
			var organizations = (IEnumerable<IOrganization>)resultBase.Model;
			IOrganization first = organizations.FirstOrDefault();
			Assert.IsTrue(first != null && first.Id == organizationId);
			inviteController.AcceptInvite(first.Id);
			resultBase = organizationController.EmployeeList(organizationId);
			EmployeeSettings emeil4User = ((List<EmployeeSettings>)resultBase.Model).FirstOrDefault(x => x.User.EMail.ToUpper() == Email4.ToUpper());
			Assert.IsNotNull(emeil4User);
			collection.Clear();
			collection.Add("delete", "true");
			organizationController.EmployeeEdit(organizationId, emeil4User.User.Id, collection);

			// через ссылку зареганного пользователя 
			userController.Dispose();
			userController = Base.GetController<UserController>();
			redirectToRouteResult = (RedirectToRouteResult)userController.Invite(organization.InviteCode);
			Assert.IsTrue(redirectToRouteResult.RouteValues["action"].ToString() == "Index");
			collection.Clear();
			inviteController.Dispose();
			inviteController = Base.GetController<InviteController>();
			resultBase = inviteController.List();
			organizations = (IEnumerable<IOrganization>)resultBase.Model;
			first = organizations.FirstOrDefault();
			Assert.IsTrue(first != null && first.Id == organizationId);
			inviteController.AcceptInvite(first.Id);
			resultBase = organizationController.EmployeeList(organizationId);
			emeil4User = (resultBase.Model as List<EmployeeSettings>).FirstOrDefault(x => x.User.EMail.ToUpper() == Email4.ToUpper());
			Assert.IsNotNull(emeil4User);

			#endregion

			#region Удаление

			userController.Dispose();
			userController = Base.GetController<UserController>();
			userController.Login(null, Email3, Email3, true, null);

			BoardsController boardsController = Base.GetController<BoardsController>();
			collection.Clear();
			collection.Add("name", "t1");
			collection.Add("OrganizationId", organizationId.ToString());
			boardsController.Create(collection);

			collection.Clear();
			collection.Add("name", "t2");
			collection.Add("OrganizationId", organizationId.ToString());
			boardsController.Create(collection);

			collection.Clear();
			organizationController = Base.GetController<OrganizationController>();
			organizationController.Delete(organizationId);

			boardsController = Base.GetController<BoardsController>();
			partialViewResult = boardsController.List(null);
			Assert.IsFalse((partialViewResult.Model as List<IBoard>).Single().OrganizationId.HasValue); // остается только личная доска

			#endregion
		}
	}
}

