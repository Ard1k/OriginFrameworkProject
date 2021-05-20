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
  }
}
