using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
	public class LCDJobStateBag
	{
		public int[] PlayersOnJob { get; set; }
		public LCDState CurrentState { get; set; }
		public LCDTargetVehicleBag[] TargetVehicles { get; set; }
		public LCDLocationGuard[] Guards { get; set; }
		public PosBag DeliverySpot { get; set; }
		public int RewardPerCar { get; set; }
		public LCDMissionDefinitionBag MissionDefinition { get; set; }
	}

	public class LCDMissionDefinitionBag
	{
		public LCDTargetVehicleBag[] Targets { get; set; }
		public LCDLocationGuard[] Guards { get; set; }
		public PosBag DeliverySpot { get; set; }
		public int RewardPerCar { get; set; }
		public DateTime? LastRun { get; set; }
		public bool IsActive { get; set; }
	}

	public class LCDTargetVehicleBag
	{
		public string Name { get; set; }
		public string Identifier { get; set; }
		public PosBag Position { get; set; }
		public string ModelName { get; set; }
		public int NetID { get; set; } = -1;
		public int StaticBlip { get; set; }
		public bool HasEntityBlip { get; set; }
		public bool Delivered { get; set; }
	}

	public class LCDLocationGuard
	{
		public int NetID { get; set; }
		public string Identifier { get; set; }
		public PosBag Position { get; set; }
		public string ModelName { get; set; }
		public string WeaponModelName { get; set; }
	}

	public enum LCDState : int
	{
		None = 0,
		VehicleHunt = 1
	}
}
