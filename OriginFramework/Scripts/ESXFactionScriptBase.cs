using CitizenFX.Core;
using MenuAPI;
using Newtonsoft.Json;
using OriginFramework.Menus;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework.Scripts
{
	public abstract class ESXFactionScriptBase : ESXScriptBase
	{
		protected FactionDefinitionBag factionDefinition = null;
		protected object factionData = null;
		public event EventHandler DataUpdated;

		public ESXFactionScriptBase(eFaction faction)
		{
			factionDefinition = FactionDefinitionBag.getDefinition(eFaction.OilIndustries);
			if (factionDefinition == null)
				throw new Exception("Faction definition does not exist!");

			EventHandlers["ofw_factions:FactionDataUpdated"] += new Action<int, string>(FactionDataUpdated);
		}

		protected async override void OnClientResourceStart(string resourceName)
		{
			base.OnClientResourceStart(resourceName);
			if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

			var data = await OfwFunctions.ServerAsyncCallbackToSync<string>("ofw_factions:GetFactionData", (int)factionDefinition.Faction);

			if (data != null)
				factionData = JsonConvert.DeserializeObject(data, factionDefinition.FactionDataType);

			DataUpdated?.Invoke(this, new EventArgs());
		}

		protected async virtual void FactionDataUpdated(int faction, string data)
		{
			if ((int)factionDefinition.Faction != faction)
				return;

			if (data != null)
				factionData = JsonConvert.DeserializeObject(data, factionDefinition.FactionDataType);

			DataUpdated?.Invoke(this, new EventArgs());
		}

		protected bool DoesPlayerHaveJob()
		{
			if (PlayerJob != null && PlayerJob.name == factionDefinition.JobName)
				return true;

			return false;
		}
	}
}
