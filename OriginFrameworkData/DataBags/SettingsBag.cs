using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
	public class SettingsBag
	{
		public int MenuKey { get; set; } = 344;
		public bool MenuAlignRight { get; set; } = false;
		public bool LoadHeistIsland { get; set; } = false;
		public bool TestJobActive { get; set; } = false;
		public NPCDefinitionBag[] NPCs { get; set; } = new NPCDefinitionBag[0];
		public float NPCRespawnDistance { get; set; } = 20f;
		public LCDMissionDefinitionBag[] LCDMissions { get; set; }
		public int GarageRecoverPrice { get; set; } = 1000;
		public GarageBag[] Garages { get; set; }

		public SettingsBag() { }
	}
}
