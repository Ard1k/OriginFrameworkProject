using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
	public class VehiclePropertiesBag
	{
		public int model { get; set; }
		public string plate { get; set; }
		public int? plateIndex { get; set; }
		public float? bodyHealth { get; set; }
		public float? engineHealth { get; set; }
		public float? fuelLevel { get; set; }
		public float? dirtLevel { get; set; }
		public int? color1 { get; set; }
		public int? color2 { get; set; }
		public int? pearlescentColor { get; set; }
		public int? wheelColor { get; set; }
		public int? wheels { get; set; }
		public int? windowTint { get; set; }
		public int? xenonColor { get; set; }
		public List<bool> neonEnabled { get; set; }
		public List<int> neonColor { get; set; }
		public Dictionary<string, bool> extras { get; set; }
		public List<int> tyreSmokeColor { get; set; }

		public int? modSpoilers { get; set; }
		public int? modFrontBumper { get; set; }
		public int? modRearBumper { get; set; }
		public int? modSideSkirt { get; set; }
		public int? modExhaust { get; set; }
		public int? modFrame { get; set; }
		public int? modGrille { get; set; }
		public int? modHood { get; set; }
		public int? modFender { get; set; }
		public int? modRightFender { get; set; }
		public int? modRoof { get; set; }

		public int? modEngine { get; set; }
		public int? modBrakes { get; set; }
		public int? modTransmission { get; set; }
		public int? modHorns { get; set; }
		public int? modSuspension { get; set; }
		public int? modArmor { get; set; }

		public bool? modTurbo { get; set; }
		public bool? modSmokeEnabled { get; set; }
		public bool? modXenon { get; set; }

		public int? modFrontWheels { get; set; }
		public int? modBackWheels { get; set; }

		public int? modPlateHolder { get; set; }
		public int? modVanityPlate { get; set; }
		public int? modTrimA { get; set; }
		public int? modOrnaments { get; set; }
		public int? modDashboard { get; set; }
		public int? modDial { get; set; }
		public int? modDoorSpeaker { get; set; }
		public int? modSeats { get; set; }
		public int? modSteeringWheel { get; set; }
		public int? modShifterLeavers { get; set; }
		public int? modAPlate { get; set; }
		public int? modSpeakers { get; set; }
		public int? modTrunk { get; set; }
		public int? modHydrolic { get; set; }
		public int? modEngineBlock { get; set; }
		public int? modAirFilter { get; set; }
		public int? modStruts { get; set; }
		public int? modArchCover { get; set; }
		public int? modAerials { get; set; }
		public int? modTrimB { get; set; }
		public int? modTank { get; set; }
		public int? modWindows { get; set; }
		public int? modLivery { get; set; }

		public bool[] windows { get; set; }
		public bool[] tyres { get; set; }
		public bool[] doors { get; set; }
	}
}
