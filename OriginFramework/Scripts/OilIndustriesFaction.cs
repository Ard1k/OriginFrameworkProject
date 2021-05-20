using CitizenFX.Core;
using MenuAPI;
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
	public class OilIndustriesFaction : ESXFactionScriptBase
	{
    public OilIndustriesFactionDataBag FactionData { get { return factionData as OilIndustriesFactionDataBag; } }
    private Dictionary<int, int> cisternsBlips = new Dictionary<int, int>(); 

    public OilIndustriesFaction() : base(eFaction.OilIndustries)
		{
      DataUpdated += OilIndustriesFaction_DataUpdated;
		}

    protected async override void OnClientResourceStart(string resourceName)
    {
      base.OnClientResourceStart(resourceName);
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      await RegisterNpcOnInteraction("OilIndustries_Manager", OilIndustries_Manager_Interaction);
      await RegisterNpcOnInteraction("OilIndustries_GarageMister", OilIndustries_GarageMister_Interaction);

      Tick += HandleMarkerLocations;
      Tick += CisternsHandler;
    }

    private async void OilIndustriesFaction_DataUpdated(object sender, EventArgs e)
    {
      Debug.WriteLine("DataUpdated triggered");
    }

    private void OilIndustries_Manager_Interaction(int pid, string pname)
    {
      var localPlayers = Main.LocalPlayers.ToList();

      var dynMenuItems = new List<DynamicMenuItem>();

      if (DoesPlayerHaveJob())
      {
        dynMenuItems.Add(new DynamicMenuItem
        {
          TextLeft = $"Obstarat palivo",
          TextDescription = "Klienti po nas chteji palivo. Vem si cisternu a bez to obstarat.",
          OnClick = () =>
          {
            var ped = new Ped(pid);
            if (GroupManager.CheckGroupInQuestDistance(Game.PlayerPed.Position))
            {
              TriggerServerEvent("ofw_oilindustries:Test");
            }
            MenuController.CloseAllMenus();
          }
        });
        dynMenuItems.Add(new DynamicMenuItem
        {
          TextLeft = $"Ukoncit rozhovor",
          OnClick = () =>
          {
            MenuController.CloseAllMenus();
          }
        });
      }
      else
      {
        dynMenuItems.Add(new DynamicMenuItem
        {
          TextLeft = $"Ok",
          TextDescription = "Tebe neznam, vysmahni",
          OnClick = () => {
            MenuController.CloseAllMenus();
          }
        });
      }

      var dynMenuDef = new DynamicMenuDefinition
      {
        MainName = pname,
        Name = $"Oil Industries",
        Items = dynMenuItems
      };

      var dynMenu = new DynamicMenu(dynMenuDef);
      MenuController.CloseAllMenus();
      dynMenu.Menu.OpenMenu();
    }

    private void OilIndustries_GarageMister_Interaction(int pid, string pname)
    {
      var localPlayers = Main.LocalPlayers.ToList();

      var dynMenuItems = new List<DynamicMenuItem>();

      if (DoesPlayerHaveJob())
      {
        dynMenuItems.Add(new DynamicMenuItem
        {
          TextLeft = $"Cisternu",
          TextDescription = "Nazdar, co potrebujes?",
          OnClick = () =>
          {
            var ped = new Ped(pid);
            if (GroupManager.CheckGroupInQuestDistance(Game.PlayerPed.Position))
            {
              SpawnCistern();
            }
            MenuController.CloseAllMenus();
          }
        });

        dynMenuItems.Add(new DynamicMenuItem
        {
          TextLeft = $"Tahac",
          TextDescription = "Nemas svuj tahac jo? Nejakej ti muzu na dnesek pujcit, ale bude te to stat 20$",
          OnClick = () =>
          {
            var ped = new Ped(pid);
            if (GroupManager.CheckGroupInQuestDistance(Game.PlayerPed.Position))
            {
              SpawnHauler();
            }
            MenuController.CloseAllMenus();
          }
        });

        dynMenuItems.Add(new DynamicMenuItem
        {
          TextLeft = $"Zavrit",
          OnClick = () => {
            MenuController.CloseAllMenus();
          }
        });
      }
      else
      {
        dynMenuItems.Add(new DynamicMenuItem
        {
          TextLeft = $"Ok",
          TextDescription = "Tebe neznam, vysmahni",
          OnClick = () => {
            MenuController.CloseAllMenus();
          }
        });
      }

      var dynMenuDef = new DynamicMenuDefinition
      {
        MainName = pname,
        Name = $"Oil Industries",
        Items = dynMenuItems
      };

      var dynMenu = new DynamicMenu(dynMenuDef);
      MenuController.CloseAllMenus();
      dynMenu.Menu.OpenMenu();
    }

    private async void SpawnCistern()
    {
      var spawnpoint = new Vector3(OilIndustriesSaticData.CisternSpawnpoint.X, OilIndustriesSaticData.CisternSpawnpoint.Y, OilIndustriesSaticData.CisternSpawnpoint.Z);

      await Delay(0);
      if (GetParkingSpotBlockingEntity(spawnpoint, OilIndustriesSaticData.CisternSpawnpoint.Heading) > 0)
      {
        Notify.Alert("Spawnpoint je blokovan");
        return;
      }

      var ret = await OfwFunctions.ServerAsyncCallbackToSyncWithText<int>("ofw_oilindustries:SpawnCistern", "Cisterna se spawnuje");

      if (ret == -1)
        return;

      int frameCounter = 0;
      while (!NetworkDoesNetworkIdExist(ret) || !NetworkDoesEntityExistWithNetworkId(ret))
      {
        await Delay(100);
        frameCounter++;
        if (frameCounter > 100)
        {
          Notify.Error("Nepodarilo se identifikovat vozidlo vytvorene serverem!");
          return;
        }
      }

      int vehID = NetworkGetEntityFromNetworkId(ret);
      //TaskWarpPedIntoVehicle(Game.PlayerPed.Handle, vehID, -1);
      await Delay(100);
    }

    private async void SpawnHauler()
    {
      var spawnpoint = new Vector3(OilIndustriesSaticData.TruckSpawnpoint.X, OilIndustriesSaticData.TruckSpawnpoint.Y, OilIndustriesSaticData.TruckSpawnpoint.Z);

      await Delay(0);
      if (GetParkingSpotBlockingEntity(spawnpoint, OilIndustriesSaticData.TruckSpawnpoint.Heading) > 0)
      {
        Notify.Alert("Spawnpoint je blokovan");
        return;
      }

      var ret = await OfwFunctions.ServerAsyncCallbackToSyncWithText<int>("ofw_oilindustries:SpawnHauler", "Tahac se spawnuje");

      if (ret == -1)
        return;

      int frameCounter = 0;
      while (!NetworkDoesNetworkIdExist(ret) || !NetworkDoesEntityExistWithNetworkId(ret))
      {
        await Delay(100);
        frameCounter++;
        if (frameCounter > 100)
        {
          Notify.Error("Nepodarilo se identifikovat vozidlo vytvorene serverem!");
          return;
        }
      }

      int vehID = NetworkGetEntityFromNetworkId(ret);
      TaskWarpPedIntoVehicle(Game.PlayerPed.Handle, vehID, -1);
      await Delay(100);
      SetVehicleProperties(vehID, new VehiclePropertiesBag { plate = "LOL OIL" });
    }

    private async Task HandleMarkerLocations()
    {
      if (!DoesPlayerHaveJob())
      {
        await Delay(2000);
        return;
      }

      bool sleep = true;
      float treshold = 50f;
      var playerPos = Game.PlayerPed.Position;
      float dist;
      Vector3 target;

      target = PosToVector3(OilIndustriesSaticData.CisternSpawnpoint);
      dist = Vector3.Distance(playerPos, target);
      if (dist < treshold)
      {
        DrawTextedMarker(dist, "Spawn cisterna", 27, target, 5f, 255, 255, 255, 40);
        sleep = false;
      }

      target = PosToVector3(OilIndustriesSaticData.TruckSpawnpoint);
      dist = Vector3.Distance(playerPos, target);
      if (dist < treshold)
      {
        DrawTextedMarker(dist, "Spawn truck", 27, target, 5f, 255, 255, 255, 40);
        sleep = false;
      }

      if (sleep)
        await Delay(500);
    }

    private async Task CisternsHandler()
    {
      if (!DoesPlayerHaveJob())
      {
        await Delay(10000);
        return;
      }
      bool sleep = true;

      if (FactionData == null || FactionData.Cisterns.Count <= 0)
      {
        cisternsBlips.Clear();
        return;
      }

      var playerPos = Game.PlayerPed.Position;

      foreach (var ci in FactionData.Cisterns)
      {
        if (NetworkDoesNetworkIdExist(ci.Key) && NetworkDoesEntityExistWithNetworkId(ci.Key))
        {
          int cisID = NetworkGetEntityFromNetworkId(ci.Key);
          var cisVeh = new Vehicle(cisID);
          var blip = GetBlipFromEntity(cisID);
          if (blip <= 0)
          {
            OfwFunctions.CreateVehicleBlip(cisID, "Cisterna", 479, 2, 0.8f);
            Debug.WriteLine("Blip created");
          }

          var cisPos = cisVeh.Position;
          var cisFwd = cisVeh.ForwardVector;
          var textPos = cisPos + new Vector3(cisFwd.X * -6, cisFwd.Y * -6, -2f);
          var dist = Vector3.Distance(textPos, playerPos);

          if (dist < 30)
          {
            DrawNoItemStorageVehicleInfo(textPos, dist, ci.Value.CurrentObjectName ?? "Prazdna", $"~n~{ci.Value.FilledCapacity} / {ci.Value.MaxCapacity}");
            sleep = false;
          }
        }
      }

      if (sleep)
        await Delay(1000);
    }
  }
}
