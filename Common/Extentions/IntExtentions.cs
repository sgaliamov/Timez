namespace Common.Extentions
{
	public static class IntExtentions
	{
		/// <summary>
		/// Есть флаг flag в value
		/// </summary>
		public static bool HasTheFlag(this int value, int flag)
		{
			return (value & flag) == flag;
		}

		/// <summary>
		/// Есть любой из флагов flag в value
		/// 0101.HasAnyFlag(0100) == true
		/// 0101.HasAnyFlag(1010) == false
		/// </summary>
		public static bool HasAnyFlag(this int value, int flag)
		{
			return (value & flag) > 0;
		}
	}
}
