using System;
using System.Globalization;
using log4net;

namespace Common
{
	public static class Log
	{
		/// <summary>
		///     Логирование ошибок
		/// </summary>
		/// <param name="ex"></param>
		public static void Exception(Exception ex)
		{
			// TODO: иногда происходит ошибка - попытка обратиться к выгруженному AppDomain

			var logger = LogManager.GetLogger("Log");

			while(ex != null)
			{
				logger.Error("Ошибка [" + DateTime.Now.ToString(CultureInfo.InvariantCulture) + "]", ex);
				ex = ex.InnerException;
			}
		}
	}
}