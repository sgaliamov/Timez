using System;
using System.Collections.Generic;
using System.Threading;
using Common;
using Timez.Entities;

namespace Timez.BLL
{
	/// <summary>
	/// Делегат для событий над сущностями
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="entity"></param>
	public delegate void EntityEventHandler<in T>(T entity);
	public delegate void UpdateEntityEventHandler<in T>(T oldEntity, T newEntity);
	public delegate void TaskErrorEventHandler(ITask task, int count);

	public sealed class UpdateEventArgs<T> : EventArgs
	{
		public UpdateEventArgs() { }
		public UpdateEventArgs(T oldData, T newData)
		{
			OldData = oldData;
			NewData = newData;
		}

		public T OldData { get; set; }
		public T NewData { get; set; }
	}

	public sealed class EventArgs<T> : EventArgs
	{
		public EventArgs() { }
		public EventArgs(T data)
		{
			Data = data;
		}
		public T Data { get; set; }
	}

	/// <summary>
	/// Замена обычному ивенту, так как нужно продолжать исполение остальных подписчиков при выпадении эксепшена у одного из них
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class Listener<T>
		where T : EventArgs, new()
	{
		readonly ReaderWriterLockSlim _Locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

		readonly List<EventHandler<T>> _Actions = new List<EventHandler<T>>();

		public void Add(EventHandler<T> action)
		{
			_Locker.EnterWriteLock();
			_Actions.Add(action);
			_Locker.ExitWriteLock();
		}

		public static Listener<T> operator +(Listener<T> list, EventHandler<T> action)
		{
			list.Add(action);
			return list;
		}

		public void Remove(EventHandler<T> action)
		{
			_Locker.EnterWriteLock();
			_Actions.Remove(action);
			_Locker.ExitWriteLock();
		}

		public void Invoke(T args)
		{
			Invoke(this, args);
		}

		public void Invoke(object sender, T args)
		{
			_Locker.EnterReadLock();
			foreach (var item in _Actions)
			{
				try
				{
					item(sender, args);
				}
				catch (TimezException ex)
				{
					// Можно ли пропустить ошибку                        
					if (ex.ReThrow)
					{
						// Тут не логируем так как логирование произойдет в Global.asax, если нужно
						throw;
					}

					if (ex.Logging)
						Log.Exception(ex);
				}

			}
			_Locker.ExitReadLock();
		}

		/// <summary>
		/// Удаляет всех подписчиков
		/// </summary>
		public void Clear()
		{
			_Locker.EnterWriteLock();
			_Actions.Clear();
			_Locker.ExitWriteLock();
		}
	}
}
