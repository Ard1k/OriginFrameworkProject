using CitizenFX.Core;
using Newtonsoft.Json;
using OriginFramework.Menus;
using OriginFrameworkData;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.UI.Screen;
using static OriginFramework.OfwFunctions;
using Debug = CitizenFX.Core.Debug;

namespace OriginFramework
{
  public class GarageClient : BaseScript
  {
    private static List<GarageBag> garages = new List<GarageBag>()
    {
      new GarageBag { 
        Place = "main",
        Name = "Hlavní garáž",
        GarageLocation = new PosBag { X = 225.0306f, Y = -786.6935f, Z = 30.0226f, Heading = 0.0f },
        Parkings = new List<GarageBag.ParkingSpot>()
        {
          new GarageBag.ParkingSpot { Center = new PosBag { X = 233.2461f, Y = -774.0175f, Z = 29.5809f, Heading = 249.7061f }, Dimensions = GarageBag.DefaultSpot, Type = GarageBag.eParkingSpotType.TakeOut },
          new GarageBag.ParkingSpot { Center = new PosBag { X = 231.3454f, Y = -779.0848f, Z = 29.5763f, Heading = 249.2472f }, Dimensions = GarageBag.DefaultSpot, Type = GarageBag.eParkingSpotType.TakeOut },
          new GarageBag.ParkingSpot { Center = new PosBag { X = 228.6336f, Y = -786.6664f, Z = 29.5463f, Heading = 249.3046f }, Dimensions = GarageBag.DefaultSpot, Type = GarageBag.eParkingSpotType.TakeOut },
          new GarageBag.ParkingSpot { Center = new PosBag { X = 226.8026f, Y = -791.754f, Z = 29.4826f, Heading = 248.9561f }, Dimensions = GarageBag.DefaultSpot, Type = GarageBag.eParkingSpotType.TakeOut },

          new GarageBag.ParkingSpot { Center = new PosBag { X = 225.9405f, Y = -773.949f, Z = 29.6246f, Heading = 250.7624f }, Dimensions = GarageBag.DefaultSpot, Type = GarageBag.eParkingSpotType.Return },
          new GarageBag.ParkingSpot { Center = new PosBag { X = 224.388f, Y = -779.0581f, Z = 29.6358f, Heading = 248.8815f }, Dimensions = GarageBag.DefaultSpot, Type = GarageBag.eParkingSpotType.Return },
          new GarageBag.ParkingSpot { Center = new PosBag { X = 221.586f, Y = -786.6331f, Z = 29.6287f, Heading = 249.0752f }, Dimensions = GarageBag.DefaultSpot, Type = GarageBag.eParkingSpotType.Return },
        }
      }
    };
    private static ColorBag TakeOutColor = new ColorBag { R = 0, G = 255, B = 0, A = 160 };
    private static ColorBag ReturnColor = new ColorBag { R = 255, G = 0, B = 0, A = 160 };
    private static bool isInMarker = false;
    private const string menuName = "GarageMenu";

    public GarageClient()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      Tick += OnTick;

      if (TheBugger.DebugMode && false)
      {
        foreach (var g in garages)
        {
          if (g.Parkings == null || g.Parkings.Count <= 0)
            continue;
          foreach (var p in g.Parkings)
          {
            PolyzoneManager.AddPolyZone(p.Center, p.Dimensions);
          }
        }
      }
    }

    private async Task OnTick()
    {
      var isInVeh = Game.PlayerPed.IsInVehicle();
      var coordsEntity = isInVeh ? Game.PlayerPed.CurrentVehicle.Handle : Game.PlayerPed.Handle;

      var currentPos = GetEntityCoords(coordsEntity, true);
      bool markerHit = false;

      foreach (var g in garages)
      {
        if (g.Parkings == null || g.Parkings.Count <= 0 || PosBagToVector3(g.GarageLocation).DistanceToSquared2D(currentPos) > 2500f)
          continue;

        foreach (var p in g.Parkings)
        {
          if (p.Type == GarageBag.eParkingSpotType.Return &&
              (!isInVeh || (isInVeh && GetPedInVehicleSeat(coordsEntity, -1) != Game.PlayerPed.Handle)))
            continue;

          if (p.Type == GarageBag.eParkingSpotType.TakeOut && isInVeh)
            continue;

          DrawBoxMarker(p.Center, p.Dimensions, p.Type == GarageBag.eParkingSpotType.TakeOut ? TakeOutColor : ReturnColor);

          if (markerHit)
            continue;

          if (IsPointInBoxMarker(currentPos, p.Center, p.Dimensions))
          {
            if (!isInMarker)
            {
              isInMarker = true;
              OpenGarageMenu(g, p);
            }

            markerHit = true;
          }
        }

        if (!markerHit)
        {
          isInMarker = false;
          NativeMenuManager.CloseAndUnlockMenu(menuName);
        }
      }
    }

    public static void OpenGarageMenu(GarageBag g, GarageBag.ParkingSpot p)
    {
      switch (p.Type) 
      {
        case GarageBag.eParkingSpotType.TakeOut:
          OpenTakeOutMenu(g, p);
          break;
        case GarageBag.eParkingSpotType.Return:
          OpenReturnMenu(g, p);
          break; ;
      }
    }

    private static async void OpenTakeOutMenu(GarageBag g, GarageBag.ParkingSpot p)
    {
      var data = await Callbacks.ServerAsyncCallbackToSync<string>("ofw_garage:GetVehiclesInGarage", g.Place);
      List<GarageVehicleBag> vehicles = null;
      if (!string.IsNullOrEmpty(data))
        vehicles = JsonConvert.DeserializeObject<List<GarageVehicleBag>>(data);

      NativeMenuManager.OpenNewMenu(menuName, () => {

        var menu = new NativeMenu
        {
          MenuTitle = $"Garáž: {g.Name}",
          Items = new List<NativeMenuItem>()
        };

        if (vehicles != null)
        {
          foreach (var veh in vehicles)
          {
            var veh2 = veh;
            var item = new NativeMenuItem
            {
              Name = $"{GetDisplayNameFromVehicleModel((uint)veh2.Properties.model)} [{veh2.Plate}]",
              NameRight = veh2.IsOut ? "V provozu" : null
            };

            if (!veh2.IsOut)
            {
              item.OnSelected = (menuItem) =>
              {
                if (OfwFunctions.GetBoxMarkerBlockingVehicle(p.Center, p.Dimensions) != 0)
                  Notify.Error("Spawn je blokován");
                else
                  TakeOutVehicle(veh2, g, p);
              };
            }

            menu.Items.Add(item);
          }
        }

        return menu;
      });
    }

    private static async void TakeOutVehicle(GarageVehicleBag veh, GarageBag g, GarageBag.ParkingSpot p)
    {
      var vehNetId = await Callbacks.ServerAsyncCallbackToSync<int>("ofw_garage:TakeOutVehicle", veh.Plate, veh.Place, OfwFunctions.PosBagToVector3(p.Center), p.Center.Heading);
      if (vehNetId == -1 || vehNetId == 0 || !NetworkDoesEntityExistWithNetworkId(vehNetId))
        return;

      var vehId = NetToVeh(vehNetId);
      Vehicles.SetVehicleProperties(vehId, veh.Properties);
      SetPedIntoVehicle(Game.PlayerPed.Handle, vehId, -1);
    }

    private static void OpenReturnMenu(GarageBag g, GarageBag.ParkingSpot p)
    {
      var plate = GetVehicleNumberPlateText(Game.PlayerPed.CurrentVehicle.Handle);
      var properties = Vehicles.GetVehicleProperties(Game.PlayerPed.CurrentVehicle.Handle);
      var propertiesString = JsonConvert.SerializeObject(properties);

      NativeMenuManager.OpenNewMenu(menuName, () => {
        var menu = new NativeMenu
        {
          MenuTitle = "Zaparkovat vozidlo?",
          Items = new List<NativeMenuItem>
          {
            new NativeMenuItem
            {
              Name = "Ano",
              IsClose = true,
              OnSelected = (item) =>
              {
                TriggerServerEvent("ofw_garage:ReturnVehicle", plate, g.Place, propertiesString);
              }
            },
            new NativeMenuItem
            {
              Name = "Ne",
              IsClose = true,
            }
          }
        };

        return menu;
      });
    }

    [EventHandler("ofw_garage:VehicleReturned")]
    private async void VehicleReturned(string plate)
    {
      if (string.IsNullOrEmpty(plate))
      {
        Debug.WriteLine("ofw_garage:VehicleReturned: invalid vehicle plate");
        return;
      }

      if (!Game.PlayerPed.IsInVehicle())
        return;

      var veh = Game.PlayerPed.CurrentVehicle;

      if (GetVehicleNumberPlateText(veh.Handle).ToLower().Trim() != plate)
        return;

      if (GetPedInVehicleSeat(veh.Handle, -1) != Game.PlayerPed.Handle)
        return;

      TaskLeaveVehicle(Game.PlayerPed.Handle, veh.Handle, 0);

      int netVehId = VehToNet(veh.Handle);
      await (Delay(2000));

      int vehId = NetToVeh(netVehId);

      DeleteEntity(ref vehId);

      Notify.Success("Vozidlo zaparkováno");
    }
  }
}
