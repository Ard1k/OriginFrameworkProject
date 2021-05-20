using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
	public class OilIndustriesFactionDataBag
	{
		public int ProcessedOil { get; set; } = 0;
		[JsonIgnore]
		public List<int> Cisterns { get; set; } = new List<int>();
	}

	public static class OilIndustriesSaticData
	{
		public static PosBag CisternSpawnpoint { get; } = new PosBag { X= 293.5926f, Y= -3086.945f, Z= 4.9899f, Heading= 177.2754f };
		public static PosBag TruckSpawnpoint { get; } = new PosBag { X= 293.3773f, Y= -3106.5332f, Z= 4.9002f, Heading= 175.6817f };
	}
}
