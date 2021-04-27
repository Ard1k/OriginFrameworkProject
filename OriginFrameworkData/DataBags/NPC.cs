using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
	public class NPCDefinitionBag
	{
		public int SpawnedNetID { get; set; }
		public string UniqueName { get; set; }
		public string VisibleName { get; set; }
		public string ModelName { get; set; }
		public string WeaponModelName { get; set; }
		public int RespawnTimeMinutes { get; set; }
		public DateTime? NextRespawn { get; set; }
		public bool IsPositionFrozen { get; set; }
		public bool IsInvincible { get; set; }
		public bool CanDropWeapon { get; set; }
		public bool HasNoColissions { get; set; }
		public bool HasWeapon { get { return !String.IsNullOrEmpty(WeaponModelName); } }
		public NPCPosBag Position { get; set; }
		public bool IsSpawning { get; set; }
		public int Group { get; set; } = -1;
		public Action<int, string> OnInteraction { get; set; }
	}

	public class NPCPosBag
	{
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }
		public float Heading { get; set; }
	}
}
