namespace Timez.BLL
{
	public interface ISettingsService
	{
		string GoogleAppId { get; }
		string VKontakteAppId { get; }
		string VKontakteSecureKey { get; }
		string FacebookAppId { get; }
		int LastNewsOnPage { get; }
		string ConnectionString { get; }
	}
}