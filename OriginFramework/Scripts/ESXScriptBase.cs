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
	public abstract class ESXScriptBase : BaseScript
	{
		protected dynamic ESX = null;
		protected dynamic PlayerJob = null;
		protected event EventHandler JobChanged;

		public ESXScriptBase()
		{
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
      EventHandlers["baseevents:onPlayerDied"] += new Action<dynamic, dynamic>(OnPlayerDied);
      EventHandlers["baseevents:onPlayerKilled"] += new Action<dynamic, dynamic>(OnPlayerDied);
			EventHandlers["esx:playerLoaded"] += new Action<dynamic>(EsxPlayerLoaded);
			EventHandlers["esx:setJob"] += new Action<dynamic>(EsxSetJob);
		}

    protected async virtual void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

			while (SettingsManager.Settings == null)
				await Delay(0);

			//while (ESX == null)
			//{
			//	TriggerEvent("esx:getSharedObject", new object[] { new Action<dynamic>(esx => { ESX = esx; }) });
			//	await Delay(0);
			//}

			//while (PlayerJob == null)
			//{
			//	try
			//	{
			//		PlayerJob = ESX.GetPlayerData().job;
			//	}
			//	catch	{ }

			//	if (PlayerJob == null)
			//		await Delay(100);
			//}
		}

    protected async virtual void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
    }

    protected async virtual void OnPlayerDied(dynamic u1, dynamic u2)
    {
    }

		private async void EsxPlayerLoaded(dynamic playerData)
		{
			try
			{
				PlayerJob = playerData.job;
			}
			catch 
			{
				Debug.WriteLine("Can't update player job on EsxPlayerLoaded");
			}
		}

		protected async virtual void EsxSetJob(dynamic newJob)
		{
			if (newJob == null)
			{
				Debug.WriteLine("Invalid job in esx:setJob");
				return;
			}

			PlayerJob = newJob;
			JobChanged?.Invoke(this, null);
		}

		protected async Task RegisterNpcOnInteraction(string uniqueNpcName, Action<int, string> interaction)
		{
			while (NPCClient.NPCs == null)
				await Delay(0);

			foreach (var n in NPCClient.NPCs)
			{
				if (n.UniqueName == uniqueNpcName)
				{
					n.OnInteraction = interaction;
				}
			}
		}
	}
}
