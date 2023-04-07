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

      if (vehCheck == 0 || GetEntityModel(vehCheck) != CurrentVehicleModel || (RequestedProperties != null && !NativeMenuManager.IsMenuOpen(MenuName)))
      {
        CurrentVehicleNet = 0;
        CurrentVehicleModel = 0;
        CurrentVehicleClass = 0;
        RequestedProperties = null;
      }

      await Delay(250);
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

      if (!HasStreamedTextureDictLoaded("inventory_textures"))
        RequestStreamedTextureDict("inventory_textures", true);

      CurrentVehicleNet = VehToNet(vehicle.Value);
      CurrentVehicleModel = GetEntityModel(vehicle.Value);
      CurrentVehicleClass = GetVehicleClass(vehicle.Value);
      SetVehicleModKit(vehicle.Value, 0);

      string reqData = await Callbacks.ServerAsyncCallbackToSync<string>("ofw_veh:GetRequestedTuning", CurrentVehicleNet, CurrentVehicleModel);
      if (!string.IsNullOrEmpty(reqData) && reqData != "nodata")
      {
        RequestedProperties = JsonConvert.DeserializeObject<VehiclePropertiesBag>(reqData);
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
          Name = VehTuningTypeDefinition.DefinedSpecial["color0"].Name,
          NameRight = $"{VehTuningTypeDefinition.DefinedSpecial["color0"].ComputeUpgradePrice(CurrentVehicleModel, RequestedProperties.color1.Value)}",
          IconRight = ItemsDefinitions.Items[VehTuningTypeDefinition.DefinedSpecial["color0"].PriceItemId].Texture,
          IconRightTextureDict = "inventory_textures",
          OnSelected = (item) => {
            InstallTuning(new VehiclePropertiesBag { color1 = RequestedProperties.color1 });
          },
        });
      }

      if (RequestedProperties.color2 != null)
      {
        items.Add(new NativeMenuItem
        {
          Name = VehTuningTypeDefinition.DefinedSpecial["color1"].Name,
          NameRight = $"{VehTuningTypeDefinition.DefinedSpecial["color1"].ComputeUpgradePrice(CurrentVehicleModel, RequestedProperties.color2.Value)}",
          IconRight = ItemsDefinitions.Items[VehTuningTypeDefinition.DefinedSpecial["color1"].PriceItemId].Texture,
          IconRightTextureDict = "inventory_textures",
          OnSelected = (item) => {
            InstallTuning(new VehiclePropertiesBag { color2 = RequestedProperties.color2 });
          },
        });
      }

      if (RequestedProperties.pearlescentColor != null)
      {
        items.Add(new NativeMenuItem
        {
          Name = VehTuningTypeDefinition.DefinedSpecial["color2"].Name,
          NameRight = $"{VehTuningTypeDefinition.DefinedSpecial["color2"].ComputeUpgradePrice(CurrentVehicleModel, RequestedProperties.pearlescentColor.Value)}",
          IconRight = ItemsDefinitions.Items[VehTuningTypeDefinition.DefinedSpecial["color2"].PriceItemId].Texture,
          IconRightTextureDict = "inventory_textures",
          OnSelected = (item) => {
            InstallTuning(new VehiclePropertiesBag { pearlescentColor = RequestedProperties.pearlescentColor });
          },
        });
      }

      if (RequestedProperties.wheelColor != null && RequestedProperties.wheels == null && RequestedProperties.tunings[23] == null)
      {
        items.Add(new NativeMenuItem
        {
          Name = VehTuningTypeDefinition.DefinedSpecial["colorw"].Name,
          NameRight = $"{VehTuningTypeDefinition.DefinedSpecial["colorw"].ComputeUpgradePrice(CurrentVehicleModel, RequestedProperties.wheelColor.Value)}",
          IconRight = ItemsDefinitions.Items[VehTuningTypeDefinition.DefinedSpecial["colorw"].PriceItemId].Texture,
          IconRightTextureDict = "inventory_textures",
          OnSelected = (item) => {
            InstallTuning(new VehiclePropertiesBag { wheelColor = RequestedProperties.wheelColor });
          },
        });
      }

      if (RequestedProperties.wheels != null || RequestedProperties.tunings[23] != null)
      {
        items.Add(new NativeMenuItem
        {
          Name = VehTuningTypeDefinition.Defined[23].Name,
          NameRight = $"{VehTuningTypeDefinition.Defined[23].ComputeUpgradePrice(CurrentVehicleModel, Convert.ToInt32(RequestedProperties.tunings[23]))}",
          IconRight = ItemsDefinitions.Items[VehTuningTypeDefinition.Defined[23].PriceItemId].Texture,
          IconRightTextureDict = "inventory_textures",
          OnSelected = (item) => {
            var vp = new VehiclePropertiesBag() { wheels = RequestedProperties.wheels };
            vp.tunings[23] = RequestedProperties.tunings[23];
            InstallTuning(vp);
          },
        });
      }

      if (RequestedProperties.customTires != null && RequestedProperties.wheels == null && RequestedProperties.tunings[23] == null)
      {
        items.Add(new NativeMenuItem
        {
          Name = VehTuningTypeDefinition.DefinedSpecial["customtires"].Name,
          NameRight = $"{VehTuningTypeDefinition.DefinedSpecial["customtires"].ComputeUpgradePrice(CurrentVehicleModel, RequestedProperties.customTires.Value ? 1 : 0)}",
          IconRight = ItemsDefinitions.Items[VehTuningTypeDefinition.DefinedSpecial["customtires"].PriceItemId].Texture,
          IconRightTextureDict = "inventory_textures",
          OnSelected = (item) => {
            InstallTuning(new VehiclePropertiesBag() { customTires = RequestedProperties.customTires });
          },
        });
      }

      for (int i = 0; i <= 49; i++)
      {
        if (RequestedProperties?.tunings[i] == null || i == 23)
          continue;

        VehTuningTypeDefinition tuning = null;
        if (VehTuningTypeDefinition.Defined.ContainsKey(i))
          tuning = VehTuningTypeDefinition.Defined[i];

        if (tuning == null)
          continue;

        var vehCurrent = NetToVeh(CurrentVehicleNet);

        string menu_name = GetModSlotName(vehCurrent, i);
        if (string.IsNullOrWhiteSpace(menu_name))
          menu_name = tuning?.Name ?? "Unknown";
        if (menu_name.Contains("_"))
        {
          var lbl = GetLabelText(menu_name);
          if (lbl != "NULL")
            menu_name = lbl;
        }

        int i2 = i;

        items.Add(new NativeMenuItem
        {
          Name = menu_name,
          NameRight = $"{tuning.ComputeUpgradePrice(CurrentVehicleModel, RequestedProperties?.tunings[i] is bool? ? (((bool)RequestedProperties?.tunings[i]) ? 1 : 0) : Convert.ToInt32(RequestedProperties?.tunings[i]))}",
          IconRight = ItemsDefinitions.Items[tuning.PriceItemId].Texture,
          IconRightTextureDict = "inventory_textures",
          OnSelected = (item) => {
            var vp = new VehiclePropertiesBag();
            vp.tunings[i2] = RequestedProperties?.tunings[i2];
            InstallTuning(vp);
          },
        });
      }

      return items;
    }

    private static async void InstallTuning(VehiclePropertiesBag requestedTuning)
    {
      if (CurrentVehicleNet == 0)
        return;

      string result = await Callbacks.ServerAsyncCallbackToSync<string>("ofw_veh:InstallRequestedTuning", CurrentVehicleNet, CurrentVehicleModel, JsonConvert.SerializeObject(requestedTuning));
      if (string.IsNullOrEmpty(result) || result != "true")
        return;

      RequestedProperties.Substract(requestedTuning);
      NativeMenuManager.OpenNewMenu(MenuName, getTuningInstallMenu);
    }
  }
}
