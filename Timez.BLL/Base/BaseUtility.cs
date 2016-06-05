using System;
using System.Transactions;
using Timez.DAL.DataContext;

namespace Timez.BLL
{
	/// <summary>
	/// Базовый класс для утилит
	/// </summary>
	public abstract class BaseUtility<T> where T : BaseUtility<T>, new()
	{
		protected UtilityManager Utility { get; private set; }
		/// <summary>
		/// Доступ к репозиториям данных
		/// </summary>
		protected Repositories Repository { get; private set; }

		/// <summary>
		/// Общий кеш, не зависит от утилит
		/// </summary>
		protected ICacheService Cache { get; private set; }
		protected IAuthenticationService Authentication { get; private set; }		

		/// <summary>
		/// Виртуальный конструктор утилит
		/// </summary>
		internal static T Create(UtilityManager utility)
		{
			T t = new T
			{
				Utility = utility,
				Repository = utility.Repository,
				Cache = utility.CacheUtility,
				Authentication = utility.Authentication
			};

			// Пописываемся на события менеджера, что бы автоматически вызывались инициализации и освобождения ресурсов
			t.Utility.OnInited += t.Init;
			t.Utility.OnDispose += t._Free;

			return t;
		}

		/// <summary>
		/// Дополнительная инициализация, если требуется
		/// </summary>
		internal virtual void Init() { }

		/// <summary>
		/// Если в Init были захвачены какие то ресурсы, то клиентскому коду их нужно будет освобождать сдесь
		/// </summary>
		internal virtual void Free() { }

		/// <summary>
		/// Служебное освобождение ресурсов
		/// </summary>
		void _Free()
		{
			Utility.OnInited -= Init;
			Utility.OnDispose -= _Free;

			Free();
		}
	}
}
