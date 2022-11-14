using CitizenFX.Core;
using CitizenFX.Core.UI;
using Newtonsoft.Json;
using OriginFramework.Menus;
using OriginFrameworkData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static OriginFramework.OfwFunctions;

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

    #region EntityInfoDraw
    public static bool IsEntityInfoEnabled { get; set; } = GetResourceKvpInt(KvpManager.getKvpString(KvpEnum.MiscDrawEntityInfo)) > 0;

    private bool stopPropsLoop = false;
    private bool stopVehiclesLoop = false;
    private bool stopPedsLoop = false;
    private List<Prop> props = new List<Prop>();
    private List<Vehicle> vehicles = new List<Vehicle>();
    private List<Ped> peds = new List<Ped>();
    internal static float entityRange = 2000f;

    private async Task EntityInfoRefresh()
    {
      const int delay = 200;
      if (IsEntityInfoEnabled)
      {
        var pp = Game.PlayerPed.Position;
        stopPropsLoop = true;
        props = World.GetAllProps().Where(e => e.IsOnScreen && e.Position.DistanceToSquared(pp) < entityRange).ToList();
        stopPropsLoop = false;

        await Delay(delay);

        stopPedsLoop = true;
        peds = World.GetAllPeds().Where(e => e.IsOnScreen && e.Position.DistanceToSquared(pp) < entityRange).ToList();
        stopPedsLoop = false;

        await Delay(delay);

        stopVehiclesLoop = true;
        vehicles = World.GetAllVehicles().Where(e => e.IsOnScreen && e.Position.DistanceToSquared(pp) < entityRange).ToList();
        stopVehiclesLoop = false;

        await Delay(delay);
      }
      else
      {
        await Delay(1000);
      }
    }

    private async Task EntityInfoDrawing()
    {
      if (!IsEntityInfoEnabled)
      {
        await Delay(1000);
        return;
      }

      foreach (Vehicle v in vehicles)
      {
        if (stopVehiclesLoop)
        {
          break;
        }

        if (v.IsOnScreen)
        {
          int model = GetEntityModel(v.Handle);
          var isNetworked = NetworkGetEntityIsNetworked(v.Handle);
          int netID = isNetworked ? NetworkGetNetworkIdFromEntity(v.Handle) : -1;
          int netOwnerLocalId = NetworkGetEntityOwner(v.Handle);
          int playerServerId = -1;
          string playerName = "---";
          if (netOwnerLocalId != 0)
          {
            playerServerId = GetPlayerServerId(netOwnerLocalId);
            playerName = GetPlayerName(netOwnerLocalId);
          }

          string dstr = null;
          if (isNetworked)
            dstr = $"~o~ID {v.Handle}~n~Hash {model}~n~NetID {netID}~n~Owner ID {playerServerId} ({playerName})";
          else
            dstr = $"~o~ID {v.Handle}~n~Hash {model}~n~NOT NETWORKED";

          SetDrawOrigin(v.Position.X, v.Position.Y, v.Position.Z, 0);
          TextUtils.DrawTextOnScreen(dstr, 0f, 0f, 0.3f, Alignment.Center, 0);
          ClearDrawOrigin();
        }
      }

      foreach (Prop p in props)
      {
        if (stopPropsLoop)
        {
          break;
        }

        if (p.IsOnScreen)
        {
          int model = GetEntityModel(p.Handle);
          var isNetworked = NetworkGetEntityIsNetworked(p.Handle);
          int netID = isNetworked ? NetworkGetNetworkIdFromEntity(p.Handle) : -1;
          int netOwnerLocalId = NetworkGetEntityOwner(p.Handle);
          int playerServerId = -1;
          string playerName = "---";
          if (netOwnerLocalId != 0)
          {
            playerServerId = GetPlayerServerId(netOwnerLocalId);
            playerName = GetPlayerName(netOwnerLocalId);
          }

          string dstr = null;
          if (isNetworked)
            dstr = $"~c~ID {p.Handle}~n~Hash {model}~n~NetID {netID}~n~Owner ID {playerServerId} ({playerName})";
          else
            dstr = $"~c~ID {p.Handle}~n~Hash {model}~n~NOT NETWORKED";

          SetDrawOrigin(p.Position.X, p.Position.Y, p.Position.Z, 0);
          TextUtils.DrawTextOnScreen(dstr, 0f, 0f, 0.3f, Alignment.Center, 0);
          ClearDrawOrigin();
        }
      }

      foreach (Ped p in peds)
      {
        if (stopPedsLoop)
        {
          break;
        }

        if (p.IsOnScreen)
        {
          int model = GetEntityModel(p.Handle);
          var isNetworked = NetworkGetEntityIsNetworked(p.Handle);
          int netID = isNetworked ? NetworkGetNetworkIdFromEntity(p.Handle) : -1;
          var controllingPlayerIndex = NetworkGetPlayerIndexFromPed(p.Handle);
          var controllingPlayerServerID = GetPlayerServerId(controllingPlayerIndex);
          var controllingPlayerString = controllingPlayerServerID > 0 ? controllingPlayerServerID.ToString() : "---";
          int netOwnerLocalId = NetworkGetEntityOwner(p.Handle);
          int playerServerId = -1;
          string playerName = "---";
          if (netOwnerLocalId != 0)
          {
            playerServerId = GetPlayerServerId(netOwnerLocalId);
            playerName = GetPlayerName(netOwnerLocalId);
          }

          string dstr = null;
          if (isNetworked)
            dstr = $"~p~ID {p.Handle}~n~Hash {model}~n~NetID {netID}~n~PID {controllingPlayerString}~n~Owner ID {playerServerId} ({playerName})";
          else
            dstr = $"~p~ID {p.Handle}~n~Hash {model}~n~NOT NETWORKED";

          SetDrawOrigin(p.Position.X, p.Position.Y, p.Position.Z + 1f, 0);
          TextUtils.DrawTextOnScreen(dstr, 0f, 0f, 0.3f, Alignment.Center, 0);
          ClearDrawOrigin();
        }
      }
    }
    #endregion

    #region closest veh trunk state
    public static bool IsClosestVehicleTrunkInfoEnabled { get; set; } = GetResourceKvpInt(KvpManager.getKvpString(KvpEnum.ShowClosesVehicleTrunkInfo)) > 0;
    private int closestVehicle = -1;

    private async Task ClosestVehRefresh()
    {
      if (!IsClosestVehicleTrunkInfoEnabled)
      {
        await Delay(2000);
        return;
      }

      var p = Game.PlayerPed.Position;
      var vehs = World.GetAllVehicles();
      float closest = 999999999999f;
      bool isAnyFound = false;

      if (vehs != null)
      {
        foreach (var veh in vehs)
        {
          var dist = Vector3.Distance(p, veh.Position);
          //Debug.WriteLine($"veh: {veh.Handle} distance: {dist}");
          if (dist < 50 && dist < closest)
          {
            closest = dist;
            closestVehicle = veh.Handle;
            isAnyFound = true;
          }
        }
      }

      if (!isAnyFound)
        closestVehicle = -1;

      await Delay(500);
    }

    private async Task ClosestVehTrunkShowMarker()
    {
      if (!IsClosestVehicleTrunkInfoEnabled)
      {
        await Delay(2000);
        return;
      }

      if (closestVehicle > 0)
      {
        var veh = (Vehicle)Vehicle.FromHandle(closestVehicle);
        if (veh != null)
        {
          DrawMarker(0, veh.Position.X, veh.Position.Y, veh.Position.Z + 2f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0f, 0.7f, 0.7f, 0.7f, 255, 128, 0, 100, true, false, 2, false, null, null, false);

          //doorIndex: 0 = Front Left 1 = Front Right 2 = Back Left 3 = Back Right 4 = Hood 5 = Trunk 6 = Trunk2

          bool hasDoor = veh.Doors.HasDoor(VehicleDoorIndex.Trunk);
          var door = veh.Doors[VehicleDoorIndex.Trunk];
          int color_red = 0;
          int color_green = 0;
          if (!hasDoor || door.IsOpen || door.IsBroken)
          {
            color_red = 0;
            color_green = 255;
          }
          else
          {
            color_red = 255;
            color_green = 0;
          }

          var x = veh.Position.X;
          var y = veh.Position.Y;
          var z = veh.Position.Z;
          
          if (hasDoor)
          {
            var bootBone = veh.Bones["boot"];
            x = bootBone.Position.X;
            y = bootBone.Position.Y;
            z = bootBone.Position.Z;
          }
          DrawMarker(0, x, y, z + 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0f, 0.4f, 0.4f, 0.4f, color_red, color_green, 0, 255, true, false, 2, false, null, null, false);
        }
      }
    }
    #endregion

    #region God mode
    public static bool IsPlayerGodModeEnabled = false;

    public static void TogglePlayerGodMode()
    {
      IsPlayerGodModeEnabled = !IsPlayerGodModeEnabled;
      SetEntityInvincible(Game.PlayerPed.Handle, IsPlayerGodModeEnabled);
    }
    #endregion

    public Misc()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.Misc))
        return;

      EventHandlers["ofw_misc:CopyStringToClipboard"] += new Action<string>(CopyStringToClipboard);

      Tick += EntityInfoRefresh;
      Tick += EntityInfoDrawing;

      Tick += ClosestVehRefresh;
      Tick += ClosestVehTrunkShowMarker;

      if (SettingsManager.Settings.LoadHeistIsland)
      {
        Debug.WriteLine("Island loading enabled");
        Tick += IslandLoader;
      }

      InternalDependencyManager.Started(eScriptArea.Misc);
    }

    private async void CopyStringToClipboard(string toCopy)
    {
      var message = new
      {
        type = "copyDataMessage",
        message = toCopy
      };
      SendNuiMessage(JsonConvert.SerializeObject(message));

      Notify.Info("Copied to clipboard!");
    }

  }
}
