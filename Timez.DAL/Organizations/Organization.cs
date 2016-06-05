using Common.Extentions;
using Timez.Entities;

namespace Timez.DAL.DataContext
{
	internal partial class Organization : IOrganization { }

	internal partial class Tariff : ITariff
	{
		public TariffFlags GetFlags()
		{
			return (TariffFlags)Flags;
		}

		public bool IsFree()
		{
			return Flags.HasTheFlag((int)TariffFlags.IsFree);
		}
	}
}
