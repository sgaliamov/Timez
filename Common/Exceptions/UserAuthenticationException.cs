using System;
using System.Runtime.Serialization;

namespace Common.Exceptions
{
	/// <summary>
	/// Случается, когда пытаемся использовать функции, 
	/// доступные только для аутентифицированных пользователей
	/// </summary>
	[DataContract]
	public sealed class UserAuthenticationException : Exception
	{
		public UserAuthenticationException() : base("Пользователь не авторизован!") { }
	}
}
