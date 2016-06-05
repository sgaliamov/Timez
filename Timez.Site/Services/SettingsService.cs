using System.Configuration;
using Common.Extentions;
using Timez.BLL;

namespace Timez.Services
{
	public class SettingsService : ISettingsService
	{
		public string GoogleAppId { get { return ConfigurationManager.AppSettings["GoogleAppId"]; } }
		public string VKontakteAppId { get { return ConfigurationManager.AppSettings["VKontakteAppId"]; } }
		public string VKontakteSecureKey { get { return ConfigurationManager.AppSettings["VKontakteSecureKey"]; } }
		public string FacebookAppId { get { return ConfigurationManager.AppSettings["FacebookAppId"]; } }

		public int LastNewsOnPage { get { return ConfigurationManager.AppSettings["LastNewsOnPage"].TryToInt() ?? 3; } }

		public string ConnectionString { get { return ConfigurationManager.ConnectionStrings["TimezConnectionString"].ConnectionString; } }
	}
}