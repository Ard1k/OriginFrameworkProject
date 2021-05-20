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
using static OriginFramework.OfwFunctions;

namespace OriginFramework.Scripts
{
	public abstract class ESXFactionScriptBase : ESXScriptBase
	{
		protected FactionDefinitionBag factionDefinition = null;
		protected object factionData = null;
		protected event EventHandler DataUpdated;

		public FactionDataBag FactionBaseData { get { return factionData as FactionDataBag; } }
		private List<int> staticBlips = new List<int>();
		private Dictionary<Guid, int> dynamicBlips = new Dictionary<Guid, int>();

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

			RefreshStaticBlips();
			RefreshStaticBlips();
			DataUpdated?.Invoke(this, new EventArgs());

			JobChanged += ESXFactionScriptBase_JobChanged;
		}

		private async void ESXFactionScriptBase_JobChanged(object sender, EventArgs e)
		{
			if (DoesPlayerHaveJob())
			{
				var data = await OfwFunctions.ServerAsyncCallbackToSync<string>("ofw_factions:GetFactionData", (int)factionDefinition.Faction);

				if (data != null)
					factionData = JsonConvert.DeserializeObject(data, factionDefinition.FactionDataType);

				RefreshStaticBlips();
				RefreshStaticBlips();
				DataUpdated?.Invoke(this, new EventArgs());
			}
			else
			{
				ClearStaticBlips();
				ClearDynamicBlips(true);
			}
		}

		protected async override void OnResourceStop(string resourceName)
		{
			base.OnResourceStop(resourceName);
			if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

			await ClearStaticBlips();
			await ClearDynamicBlips(true);
		}

		protected async virtual void FactionDataUpdated(int faction, string data)
		{
			if ((int)factionDefinition.Faction != faction)
				return;

			if (data != null)
				factionData = JsonConvert.DeserializeObject(data, factionDefinition.FactionDataType);

			await RefreshDynamicBlips();
			DataUpdated?.Invoke(this, new EventArgs());
		}

		protected bool DoesPlayerHaveJob()
		{
			if (PlayerJob != null && PlayerJob.name == factionDefinition.JobName)
				return true;

			return false;
		}

		protected bool IsPlayerBoss()
		{
			if (PlayerJob != null && PlayerJob.name == factionDefinition.JobName && (PlayerJob.grade_name == "boss" || PlayerJob.grade_name == "viceboss"))
				return true;

			return false;
		}

		private async Task RefreshStaticBlips()
		{
			if (!DoesPlayerHaveJob())
				return;

			await ClearStaticBlips();

			if (FactionBaseData != null && FactionBaseData.StaticBlips != null)
			{
				for (int i = 0; i < FactionBaseData.StaticBlips.Count; i++)
				{
					var b = FactionBaseData.StaticBlips[i];

					var blip = OfwFunctions.CreateBlip(new Vector3(b.Pos.X, b.Pos.Y, b.Pos.Z), b.DisplayName, b.BlipSprite, b.BlipColor, 1f);
					staticBlips.Add(blip);
				}
			}
		}

		private async Task ClearStaticBlips()
		{
			if (staticBlips.Count > 0)
			{
				for (int i = staticBlips.Count - 1; i >= 0 ; i--)
				{
					if (DoesBlipExist(staticBlips[i]))
					{
						int b = staticBlips[i];
						RemoveBlip(ref b);
					}
				}
			}
		}

		private async Task RefreshDynamicBlips()
		{
			if (!DoesPlayerHaveJob())
				return;

			await ClearDynamicBlips(false);

			if (FactionBaseData != null && FactionBaseData.DynamicBlips != null)
			{
				for (int i = 0; i < FactionBaseData.DynamicBlips.Count; i++)
				{
					var b = FactionBaseData.DynamicBlips[i];

					if (dynamicBlips.ContainsKey(b.SyncId))
						continue;

					var blip = OfwFunctions.CreateBlip(new Vector3(b.Pos.X, b.Pos.Y, b.Pos.Z), b.DisplayName, b.BlipSprite, b.BlipColor, 1f);
					dynamicBlips.Add(b.SyncId, blip);
				}
			}
		}

		private async Task ClearDynamicBlips(bool clearAll)
		{
			if (dynamicBlips.Count > 0)
			{
				for (int i = dynamicBlips.Count - 1; i >= 0; i--)
				{
					var kvp = dynamicBlips.ElementAt(i);
					if (clearAll || FactionBaseData == null || FactionBaseData.DynamicBlips == null || !FactionBaseData.DynamicBlips.Any(a => a.SyncId == kvp.Key))
					{
						if (DoesBlipExist(kvp.Value))
						{
							int b = kvp.Value;
							RemoveBlip(ref b);
						}

						dynamicBlips.Remove(kvp.Key);
					}
				}
			}
		}
	}
}
