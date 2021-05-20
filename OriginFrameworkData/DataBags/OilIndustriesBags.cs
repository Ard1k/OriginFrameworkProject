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
		public Dictionary<int, Cistern> Cisterns { get; set; } = new Dictionary<int, Cistern>();
		public List<int> Haulers { get; set; } = new List<int>();
	}

	public static class OilIndustriesSaticData
	{
		public static PosBag CisternSpawnpoint { get; } = new PosBag { X= 293.5926f, Y= -3086.945f, Z= 4.9899f, Heading= 177.2754f };
		public static PosBag TruckSpawnpoint { get; } = new PosBag { X= 293.3773f, Y= -3106.5332f, Z= 4.9002f, Heading= 175.6817f };
	}

	public class Cistern : NonItemStorageVehicle
	{
		public Cistern()
		{
			CanCarryObjects = new string[] { "rawoil", "processedoil", "gasfraction" };
			MaxCapacity = 20000;
		}
	}

	public abstract class NonItemStorageVehicle
	{
		public int NetID { get; set; }
		public string[] CanCarryObjects { get; set; }
		public string CurrentObject { get; set; }
		public string CurrentObjectName { get; set; }
		public int MaxCapacity { get; set; }
		public int FilledCapacity { get; set; }
		[JsonIgnore]
		public int AvailableCapacity { get { return MaxCapacity - FilledCapacity; } }
	}
}
