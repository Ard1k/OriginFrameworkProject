using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
	public class FactionDefinitionBag
	{
		public Type FactionDataType { get; set; }
		public string DBDataKey { get; set; }
		public string JobName { get; set; }
		public eFaction Faction { get; set; }

		public static FactionDefinitionBag getDefinition(eFaction faction)
		{
			switch (faction)
			{
				case eFaction.OilIndustries:
					return new FactionDefinitionBag { Faction = faction, JobName = "oilindustries", DBDataKey = "oilindustries", FactionDataType = typeof(OilIndustriesFactionDataBag) };
				default:
					return null;
			}
		}
	}

	public enum eFaction : int
	{
		OilIndustries = 1
	}
}
