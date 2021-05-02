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


namespace OriginFramework
{
  public class LuxuryCarDelivery : BaseScript
  {
    private Dictionary<string, bool> JobNpcList = new Dictionary<string, bool> { { "LuxuryCarDelivery_01", false }, { "kokos", false } };
    private LCDJobStateBag JobState = null;
    private List<int> blips = new List<int>();
    private string InteractionKeyString = " ~INPUT_CONTEXT~ ";
    public static Control InteractionKey { get; private set; } = Control.Context;

    public LuxuryCarDelivery()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
      EventHandlers["ofw_lcd:NewJobStateSent"] += new Action<string>(NewJobStateSent);
      EventHandlers["ofw_lcd:TryRestoreJobState"] += new Action(TryRestoreJobState);
      EventHandlers["ofw_lcd:VehicleDeliveredUpdate"] += new Action<string>(VehicleDeliveredUpdate);
      EventHandlers["ofw_lcd:JobFinishedUpdate"] += new Action<int>(JobFinishedUpdate);
      EventHandlers["ofw_lcd:JobCancelledUpdate"] += new Action(JobCancelledUpdate);
    }


    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      AddTextEntry("OFW_LCD_DELIVER", $"Stiskni {InteractionKeyString} pro interakci");
      AddTextEntry("OFW_LCD_BADVEH", $"Kamo, tohle neni to auto!");

      while (NPCClient.NPCs == null)
        await Delay(0);

      foreach (var n in NPCClient.NPCs)
      {
        switch (n.UniqueName)
        {
          case "LuxuryCarDelivery_01":
            n.OnInteraction = LuxuryCarDelivery_01_Interaction;
            break;
          default: break;
        }
      }

      TriggerEvent("ofw_lcd:TryRestoreJobState");

      Tick += VehicleDistanceChecker;
      Tick += CheckEntityBlips;
      Tick += VehicleDeliveryHandler;
    }

    private async void TryRestoreJobState()
    {
      string ret = null;
      bool completed = false;
      Func<string, bool> CallbackFunction = (data) =>
      {
        ret = data;
        completed = true;
        return true;
      };

      BaseScript.TriggerServerEvent("ofw_lcd:GetJobStateToRestore", CallbackFunction);

      while (!completed)
      {
        await Delay(0);
      }

      if (ret != null)
      {
        JobState = JsonConvert.DeserializeObject<LCDJobStateBag>(ret);
        RefreshBlipsFromJobstate(JobState);
      }
    }

    private async Task CheckEntityBlips()
    {
      if (JobState != null && JobState.TargetVehicles != null)
      {
        for (int i = 0; i < JobState.TargetVehicles.Length; i++)
        {
          var v = JobState.TargetVehicles[i];

          if (v.Delivered || v.NetID <= 0)
            continue;

          if (NetworkDoesNetworkIdExist(v.NetID) && NetworkDoesEntityExistWithNetworkId(v.NetID))
          {
            var entid = NetworkGetEntityFromNetworkId(v.NetID);
            if (GetBlipFromEntity(entid) > 0)
              continue;
            
            OfwFunctions.CreateVehicleBlip(entid, v.Name, 596, 1, 1f);
          }
        }
      }
    }

    private async Task VehicleDeliveryHandler()
    {
      if (JobState != null && JobState.CurrentState == LCDState.VehicleHunt && JobState.TargetVehicles != null && JobState.DeliverySpot != null)
      {
        var pedid = Game.PlayerPed.Handle;
        var vehid = GetVehiclePedIsIn(pedid, true);

        if (Vector3.Distance(Game.PlayerPed.Position, new Vector3(JobState.DeliverySpot.X, JobState.DeliverySpot.Y, JobState.DeliverySpot.Z)) < 50f)
          DrawMarker(25, JobState.DeliverySpot.X, JobState.DeliverySpot.Y, JobState.DeliverySpot.Z + 0.1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0f, 7f, 7f, 7f, 0, 255, 0, 90, false, false, 2, false, null, null, false);

        if (vehid > 0)
        {
          var veh = new Vehicle(vehid);
          if (Vector3.Distance(veh.Position, new Vector3(JobState.DeliverySpot.X, JobState.DeliverySpot.Y, JobState.DeliverySpot.Z)) < 5)
          {
            var vehNetID = NetworkGetNetworkIdFromEntity(vehid);
            var missionVehicle = JobState.TargetVehicles.Where(v => v.NetID == vehNetID).FirstOrDefault();
            if (missionVehicle != null)
            {
              DisplayHelpTextThisFrame("OFW_LCD_DELIVER", false);
              if (IsControlJustPressed(0, (int)InteractionKey))
              {
                bool ret = false;
                bool completed = false;
                Func<bool, bool> CallbackFunction = (data) =>
                {
                  ret = data;
                  completed = true;
                  return true;
                };

                BaseScript.TriggerServerEvent("ofw_lcd:DeliverVehicle", missionVehicle.NetID, CallbackFunction);

                while (!completed)
                {
                  await Delay(0);
                }

                if (ret == true)
                {
                  missionVehicle.Delivered = true;
                }
                else
                {
                  Notify.Info("Vozidlo se nepodarilo dorucit");
                }
              }
            }
            else
            {
              DisplayHelpTextThisFrame("OFW_LCD_BADVEH", false);
            }
          }
        }
      }
    }

    private async void VehicleDeliveredUpdate(string identifier)
    {
      if (JobState != null && JobState.TargetVehicles != null)
      {
        var vehbag = JobState.TargetVehicles.Where(v => v.Identifier == identifier).FirstOrDefault();

        if (vehbag != null)
        {
          vehbag.Delivered = true;
          Notify.Success($"Vozidlo {vehbag.Name} bylo doruceno!");
        }
      }
    }

    private async void JobFinishedUpdate(int reward)
    {
      var scaleform = RequestScaleformMovie("MP_BIG_MESSAGE_FREEMODE");

      while (!HasScaleformMovieLoaded(scaleform))
        await Delay(0);

      BeginScaleformMovieMethod(scaleform, "SHOW_SHARD_WASTED_MP_MESSAGE");
      PushScaleformMovieMethodParameterString("Job Done");
      PushScaleformMovieMethodParameterString($"Odmena: {reward}$");
      EndScaleformMovieMethod();

      float time = 0;

      JobState = null;
      ClearBlips();

      while (time <= 5000)
      {
        time += (GetFrameTime() * 1000);
        DrawScaleformMovieFullscreen(scaleform, 0, 255, 0, 255, 0);
        await Delay(0);
      }

      SetScaleformMovieAsNoLongerNeeded(ref scaleform);
    }

    private async void JobCancelledUpdate()
    {
      var scaleform = RequestScaleformMovie("MP_BIG_MESSAGE_FREEMODE");

      while (!HasScaleformMovieLoaded(scaleform))
        await Delay(0);

      BeginScaleformMovieMethod(scaleform, "SHOW_SHARD_WASTED_MP_MESSAGE");
      PushScaleformMovieMethodParameterString("Job Failed");
      PushScaleformMovieMethodParameterString($"Posral ses a nedokoncil si ukol!");
      EndScaleformMovieMethod();

      float time = 0;

      JobState = null;
      ClearBlips();

      while (time <= 5000)
      {
        time += (GetFrameTime() * 1000);
        DrawScaleformMovieFullscreen(scaleform, 255, 0, 0, 255, 0);
        await Delay(0);
      }

      SetScaleformMovieAsNoLongerNeeded(ref scaleform);
    }

    private void LuxuryCarDelivery_01_Interaction(int pid, string pname)
    {
      var localPlayers = Main.LocalPlayers.ToList();

      var dynMenuItems = new List<DynamicMenuItem>();

      string npcTalk = $"{pname} rika: Co chces? Praci? Neco bych mel, ale neni to nic pro padavky. Myslis, ze na to mas?";
      if (JobState != null)
      {
        switch (JobState.CurrentState)
        {
          case LCDState.VehicleHunt: npcTalk = $"Mas pro me ty auta? Jestli ne, tak co po mne chces?"; break;
        }
      }

      if (JobState == null)
      {
        dynMenuItems.Add(new DynamicMenuItem
        {
          TextLeft = $"Jasne, du do toho!",
          TextDescription = npcTalk,
          OnClick = () =>
          {
            var ped = new Ped(pid);
            if (GroupManager.CheckGroupInQuestDistance(Game.PlayerPed.Position))
            {
              TriggerServerEvent("ofw_lcd:StartJob");
            }
            MenuController.CloseAllMenus();
          }
        });
      }

      if (JobState != null && JobState.TargetVehicles != null && !JobState.TargetVehicles.Any(v => v.Delivered == false))
      {
        dynMenuItems.Add(new DynamicMenuItem
        {
          TextLeft = $"Jasne, mas je v garazi!",
          TextDescription = npcTalk,
          OnClick = () => {
            TriggerServerEvent("ofw_lcd:JobFinished");
            MenuController.CloseAllMenus();
          }
        });
      }

      if (JobState != null)
      {
        dynMenuItems.Add(new DynamicMenuItem
        {
          TextLeft = $"Seru na to, sem sracka!",
          TextDescription = npcTalk,
          OnClick = () => {
            TriggerServerEvent("ofw_lcd:JobCancelled");
            MenuController.CloseAllMenus();
          }
        });
      }

      dynMenuItems.Add(new DynamicMenuItem
      {
        TextLeft = $"Nic nechci!",
        TextDescription = npcTalk,
        OnClick = () => {
          MenuController.CloseAllMenus();
        }
      });

      var dynMenuDef = new DynamicMenuDefinition
      {
        MainName = pname,
        Name = $"Co reknes?",
        Items = dynMenuItems
      };

      var dynMenu = new DynamicMenu(dynMenuDef);
      MenuController.CloseAllMenus();
      dynMenu.Menu.OpenMenu();
    }

    private async void NewJobStateSent(string sstate)
    {
      Debug.WriteLine(sstate);

      JobState = JsonConvert.DeserializeObject<LCDJobStateBag>(sstate);
      RefreshBlipsFromJobstate(JobState);

      if (JobState.CurrentState == LCDState.VehicleHunt)
      {
        var scaleform = RequestScaleformMovie("MP_BIG_MESSAGE_FREEMODE");

        while (!HasScaleformMovieLoaded(scaleform))
          await Delay(0);

        BeginScaleformMovieMethod(scaleform, "SHOW_SHARD_WASTED_MP_MESSAGE");
        PushScaleformMovieMethodParameterString("job started");
        PushScaleformMovieMethodParameterString($"Dovez pozadovana auta");
        EndScaleformMovieMethod();

        float time = 0;
        while (time <= 5000)
        {
          time += (GetFrameTime() * 1000);
          DrawScaleformMovieFullscreen(scaleform, 255, 255, 255, 255, 0);
          await Delay(0);
        }

        SetScaleformMovieAsNoLongerNeeded(ref scaleform);
      }
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      ClearBlips();
    }

    private async Task VehicleDistanceChecker()
    {
      await Delay(1000);
      if (JobState == null || JobState.TargetVehicles == null)
        return;
      var pos = Game.PlayerPed.Position;

      for (int i = 0; i < JobState.TargetVehicles.Length; i++)
      {
        var v = JobState.TargetVehicles[i];
        if (v.NetID > 0 || v.Delivered)
          continue;

        var carPos = new Vector3(v.Position.X, v.Position.Y, v.Position.Z);
        var dist = Vector3.Distance(pos, carPos);
        if (dist < 200f)
        {
          int netID = -1;
          bool completed = false;
          Func<int, bool> CallbackFunction = (data) =>
          {
            netID = data;
            completed = true;
            return true;
          };

          Debug.WriteLine("Triggering spawn");
          var blockingEnt = VehicleClient.GetParkingSpotBlockingEntity(carPos, v.Position.Heading);
          int blockingNetID = -1;
          if (blockingEnt > 0)
            blockingNetID = NetworkGetNetworkIdFromEntity(blockingNetID);
          TriggerServerEvent("ofw_lcd:SpawnJobCar", v.Identifier, blockingNetID, CallbackFunction);

          while (!completed)
          {
            await Delay(0);
          }

          Debug.WriteLine("Spawncar netID returned: " + netID);
          if (netID > 0)
          {
            v.NetID = netID;
            if (v.StaticBlip > 0 && DoesBlipExist(v.StaticBlip))
            {
              var blp = v.StaticBlip;
              RemoveBlip(ref blp);
            }

            var entid = NetworkGetEntityFromNetworkId(netID);
            OfwFunctions.CreateVehicleBlip(entid, v.Name, 596, 1, 1f);
            v.HasEntityBlip = true;
          }
        }
      }
    }

    private void RefreshBlipsFromJobstate(LCDJobStateBag state)
    {
      ClearBlips();

      if (JobState != null && JobState.TargetVehicles != null)
      {
        foreach (var v in JobState.TargetVehicles)
        {
          //blips.Add(OfwFunctions.CreateRadiusBlip(new Vector3(v.Position.X, v.Position.Y, v.Position.Z), 2f, 1));
          if (v.NetID <= 0 && !v.Delivered)
          {
            var bc = OfwFunctions.CreateBlip(new Vector3(v.Position.X, v.Position.Y, v.Position.Z), v.Name, 596, 1, 1f);
            blips.Add(bc);
            v.StaticBlip = bc;
          }
          else if (!v.Delivered)
          {
            TriggerEvent("ofw_lcd:DrawEntityBlip", v.NetID);
          }
        }
      }

      if (JobState != null && JobState.DeliverySpot != null)
        blips.Add(OfwFunctions.CreateBlip(new Vector3(JobState.DeliverySpot.X, JobState.DeliverySpot.Y, JobState.DeliverySpot.Z), "Delivery", 103, 3, 1f));
    }

    private void ClearBlips()
    {
      if (blips.Count > 0)
      {
        for (int i = 0; i < blips.Count; i++)
        {
          if (DoesBlipExist(blips[i]))
          {
            int b = blips[i];
            RemoveBlip(ref b);
          }
        }
      }
    }
  }
}
