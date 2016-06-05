using System.Configuration;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Timez.Controllers;
using Timez.DAL.DataContext;
using Common.Extentions;

namespace Timez.Test
{
	/// <summary>
	/// Summary description for Main
	/// </summary>
	[TestClass]
	public class Board
	{
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

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
			try { repositories.Users.Delete(Email0); }
			catch { }
		}

		#endregion

		const string Email0 = "test0@test.ru";

		[TestMethod]
		public void BoardTest()
		{
			ViewResult result;
			RedirectToRouteResult redirectToRouteResult;
			BoardsController boardsController = Base.GetController<BoardsController>();

			Main.Registation(Email0, out result, out redirectToRouteResult, null);
			boardsController.Create();

			FormCollection collection = new FormCollection();
			collection["name"] = "test";
			RedirectToRouteResult routeResult = boardsController.Create(collection) as RedirectToRouteResult;
			int boardId = (int)routeResult.RouteValues["id"];

			boardsController = Base.GetController<BoardsController>();
			boardsController.Delete(boardId);
			var board = boardsController.Utility.Boards.Get(boardId);
			Assert.IsNull(board);
		}
	}
}

