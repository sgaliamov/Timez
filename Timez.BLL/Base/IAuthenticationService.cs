namespace Timez.BLL
{
    /// <summary>
    /// Интерфейс аутентификации
    /// </summary>
    public interface IAuthenticationService
    {
		/// <summary>
		/// Вход
		/// </summary>
		void SignIn(int userId, bool createPersistentCookie);

		/// <summary>
		/// Разлогинивание
		/// </summary>
		/// <returns></returns>
		void SignOut();

		/// <summary>
		/// Авторизован ли пользоватейль
		/// </summary>
        bool IsAuthenticated { get; }

		/// <summary>
		/// ИД текущего пользователя
		/// </summary>
        int UserId { get; }
    }
}