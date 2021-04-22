using CitizenFX.Core;
using MenuAPI;
using OriginFramework.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
	public class Misc : BaseScript
	{
		#region island loader
    public bool islandLoaded = false;

    private async Task IslandLoader()
    {
      var playerpos = GetEntityCoords(Game.PlayerPed.Handle, true);
      var islandpos = new Vector3(4840.571f, -5174.425f, 2.0f);

      var dist = Vector3.Distance(playerpos, islandpos);

      if (dist < 2000f && !islandLoaded)
      {
        CitizenFX.Core.Native.Function.Call(CitizenFX.Core.Native.Hash._SET_ISLAND_HOPPER_ENABLED, "HeistIsland", true);
        CitizenFX.Core.Native.Function.Call(CitizenFX.Core.Native.Hash._SET_TOGGLE_MINIMAP_HEIST_ISLAND, true);
        CitizenFX.Core.Native.Function.Call(CitizenFX.Core.Native.Hash._SET_AI_GLOBAL_PATH_NODES_TYPE, true);
        SetScenarioGroupEnabled("Heist_Island_Peds", true);
        SetAudioFlag("PlayerOnDLCHeist4Island", true);
        SetAmbientZoneListStatePersistent("AZL_DLC_Hei4_Island_Zones", true, true);
        SetAmbientZoneListStatePersistent("AZL_DLC_Hei4_Island_Disabled_Zones", false, true);

        islandLoaded = true;
      }
      else if (dist > 2000f && islandLoaded)
      {
        CitizenFX.Core.Native.Function.Call(CitizenFX.Core.Native.Hash._SET_ISLAND_HOPPER_ENABLED, "HeistIsland", false);
        CitizenFX.Core.Native.Function.Call(CitizenFX.Core.Native.Hash._SET_TOGGLE_MINIMAP_HEIST_ISLAND, false);
        CitizenFX.Core.Native.Function.Call(CitizenFX.Core.Native.Hash._SET_AI_GLOBAL_PATH_NODES_TYPE, false);
        SetScenarioGroupEnabled("Heist_Island_Peds", false);
        SetAudioFlag("PlayerOnDLCHeist4Island", false);
        SetAmbientZoneListStatePersistent("AZL_DLC_Hei4_Island_Zones", false, true);
        SetAmbientZoneListStatePersistent("AZL_DLC_Hei4_Island_Disabled_Zones", true, true);

        islandLoaded = false;
      }

      await Delay(1000);
    }
    #endregion

    #region dynamic menu cleanup
    private float timeMinuteCounter = 0;

    private async Task DynamicMenuCleanup()
    {
      timeMinuteCounter += (GetFrameTime() * 1000);
      if (timeMinuteCounter > 60000)
      {
        timeMinuteCounter = 0;

        if (!MenuController.IsAnyMenuOpen() && DynamicMenu.OwnedMenus.Count > 0)
        {
          //lock fivem nedava, chci se vyhnout vykydleni menu, ktery zrovna vytvarim
          var localMenus = DynamicMenu.OwnedMenus;
          DynamicMenu.OwnedMenus = new List<Menu>();

          foreach (var it in localMenus)
            MenuController.Menus.Remove(it);
        }
      }

      await Delay(1000); //zkontrolovat
    }
    #endregion

    public Misc()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        Delay(0);

      Tick += DynamicMenuCleanup;

      if (SettingsManager.Settings.LoadHeistIsland)
      {
        Debug.WriteLine("Island loading enabled");
        Tick += IslandLoader;
      }
    }

    
  }
}
