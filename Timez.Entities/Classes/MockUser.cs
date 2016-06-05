using System;

namespace Timez.Entities.Classes
{
	public class MockUser : IUser
	{
		#region IUser

		public int Id { get; private set; }
		public string ConfimKey { get; set; }
		public string EMail { get; set; }
		public bool IsAdmin { get; set; }
		public bool IsConfirmed { get; set; }
		public string Nick { get; set; }
		public string Password { get; set; }
		public DateTimeOffset RegistrationDate { get; set; }
		public int TimeZone { get; set; }
		public bool RecievOwnEvents { get; set; }

		public RegistrationType GetRegistrationType()
		{
			return registrationType;
		}
		private readonly RegistrationType registrationType;

		public string Login { get; set; }
		public DateTimeOffset? EmailChangeDate { get; set; }

		#endregion

		public MockUser(IUser user)
		{
			Id = user.Id;
			ConfimKey = user.ConfimKey;
			EMail = user.EMail;
			IsAdmin = user.IsAdmin;
			IsConfirmed = user.IsConfirmed;
			Nick = user.Nick;
			Password = user.Password;
			RegistrationDate = user.RegistrationDate;
			TimeZone = user.TimeZone;
			RecievOwnEvents = user.RecievOwnEvents;
			Login = user.Login;
			EmailChangeDate = user.EmailChangeDate;
			registrationType = user.GetRegistrationType();
		}
	}
}
