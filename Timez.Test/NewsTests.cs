using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Timez.Models;

namespace Timez.Test
{
	/// <summary>
	/// Summary description for News
	/// </summary>
	[TestClass]
	public class NewsTests
	{
		public NewsTests()
		{
			//
			// TODO: Add constructor logic here
			//
		}

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
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void News_Brief()
		{
			News news = new News();
			news.Content = "text start <cut>my cut</cut> text end";
			Assert.IsTrue(news.Brief == "my cut");

			news.Content = "<cut>my cut</cut>text start text end";
			Assert.IsTrue(news.Brief == "my cut");

			news.Content = "text start text end <cut>my cut</cut>";
			Assert.IsTrue(news.Brief == "my cut");

			news.Content = "text start text end <cut>my cut";
			Assert.IsFalse(news.Brief == "text start…");
		}
	}
}
