using CitizenFX.Core;
using Newtonsoft.Json;
using OriginFramework.Menus;
using OriginFrameworkData;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
	public class TuningInstallClient : BaseScript
	{
    public static string MenuName { get { return "tuning_install_menu"; } }
    public static int CurrentVehicleNet = 0;
    public static int CurrentVehicleClass = 0;
    public static int CurrentVehicleModel = 0;
    //public static VehiclePropertiesBag OriginalProperties = null;
    public static VehiclePropertiesBag RequestedProperties = null;

    public TuningInstallClient()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      Tick += OnTick;
    }

    private async Task OnTick()
		{
      if (CurrentVehicleNet == 0)
      {
        if (NativeMenuManager.IsMenuOpen(MenuName))
          NativeMenuManager.CloseAndUnlockMenu(MenuName);

        await Delay(250);
        return;
      }

      int vehCheck = 0;
      if (NetworkDoesEntityExistWithNetworkId(CurrentVehicleNet))
        vehCheck = NetToVeh(CurrentVehicleNet);

      if (vehCheck == 0 || GetEntityModel(vehCheck) != CurrentVehicleModel || !NativeMenuManager.IsMenuOpen(MenuName))
      {
        //if (RequestedProperties != null)
        //{
        //  TriggerServerEvent("ofw_veh:UpdateRequestedTuning", VehToNet(CurrentVehicle), GetEntityModel(CurrentVehicle), JsonConvert.SerializeObject(RequestedProperties));
        //  Debug.WriteLine(JsonConvert.SerializeObject(RequestedProperties));
        //  RequestedProperties = null;
        //}
        CurrentVehicleNet = 0;
        CurrentVehicleModel = 0;
        CurrentVehicleClass = 0;
        //OriginalProperties = null;
      }
		}

    public static async void OpenTuningInstall(int? vehicle = null)
    {
      if (vehicle == null)
        vehicle = GetVehiclePedIsIn(Game.PlayerPed.Handle, false);

      if (vehicle == null || vehicle == 0)
      {
        Notify.Alert("Pro instalaci tuningu musíš být ve vozidle");
        return;
      }
      CurrentVehicleNet = VehToNet(vehicle.Value);
      CurrentVehicleModel = GetEntityModel(vehicle.Value);
      CurrentVehicleClass = GetVehicleClass(vehicle.Value);
      SetVehicleModKit(vehicle.Value, 0);
      //OriginalProperties = Vehicles.GetVehicleProperties(CurrentVehicle);

      string reqData = await Callbacks.ServerAsyncCallbackToSync<string>("ofw_veh:GetRequestedTuning", CurrentVehicleNet, CurrentVehicleModel);
      if (!string.IsNullOrEmpty(reqData) && reqData != "nodata")
      {
        RequestedProperties = JsonConvert.DeserializeObject<VehiclePropertiesBag>(reqData);
        //Vehicles.SetVehicleProperties(CurrentVehicle, RequestedProperties);
      }
      else
        RequestedProperties = new VehiclePropertiesBag();
      NativeMenuManager.OpenNewMenu(MenuName, getTuningInstallMenu);
    }

    private static NativeMenu getTuningInstallMenu()
    {
      if (CurrentVehicleNet == 0)
      {
        return null;
      }

      var menu = new NativeMenu
      {
        MenuTitle = "Nainstalovat tuning",
        Items = propertiesToTuningList(),
      };

      return menu;
    }

    private static List<NativeMenuItem> propertiesToTuningList()
    {
      var items = new List<NativeMenuItem>();
      if (RequestedProperties == null)
      return items;

      if (RequestedProperties.color1 != null)
      {
        items.Add(new NativeMenuItem
        {
          Name = "Primární lak",
          NameRight = null,
          OnSelected = (item) => {
            InstallTuning(new VehiclePropertiesBag { color1 = RequestedProperties.color1 }, 
              () => {
                var veh = NetToVeh(CurrentVehicleNet);

                int colorPrimary = 0, colorSecondary = 0;
                GetVehicleColours(veh, ref colorPrimary, ref colorSecondary);
                SetVehicleColours(veh, RequestedProperties.color1.Value, colorSecondary);
                RequestedProperties.color1 = null;
              });
          },
        });
      }

      if (RequestedProperties.color2 != null)
      {
        items.Add(new NativeMenuItem
        {
          Name = "Sekundární lak",
          NameRight = null,
          OnSelected = (item) => {
            InstallTuning(new VehiclePropertiesBag { color2 = RequestedProperties.color2 },
              () => {
                var veh = NetToVeh(CurrentVehicleNet);

                int colorPrimary = 0, colorSecondary = 0;
                GetVehicleColours(veh, ref colorPrimary, ref colorSecondary);
                SetVehicleColours(veh, colorPrimary, RequestedProperties.color2.Value);
                RequestedProperties.color2 = null;
              });
          },
        });
      }

      return items;
    }

    private static async void InstallTuning(VehiclePropertiesBag requestedTuning, Action modifyCurrentCar)
    {
      //bool tuningResult = await Callbacks.ServerAsyncCallbackToSync<bool>("ofw_veh:PurchaseTuning", CurrentVehicleNet, CurrentVehicleModel, JsonConvert.SerializeObject(requestedTuning));
      if (CurrentVehicleNet == 0)
        return;

      bool tuningResult = true;
      if (tuningResult == true && modifyCurrentCar != null)
      {
        if (CurrentVehicleNet != 0 && NetworkDoesEntityExistWithNetworkId(CurrentVehicleNet))
          modifyCurrentCar();

        NativeMenuManager.OpenNewMenu(MenuName, getTuningInstallMenu);
      }
    }
  }
}
