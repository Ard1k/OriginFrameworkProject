using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using Newtonsoft.Json;
using OriginFramework.ClientDataBags;
using OriginFramework.Helpers;
using OriginFramework.Menus;
using OriginFrameworkData.DataBags;
using static CitizenFX.Core.Native.API;

namespace OriginFramework.Scripts
{
  public class TuningShopClient : BaseScript
  {
    public static List<TuningShopBag> Shops = new List<TuningShopBag>
    {
      new TuningShopBag
      {
        ShopID = "LSC",
        ShopPolygon = new List<Vector3>
        {
          new Vector3(-1147.0f, -1988.9f, 13.2f),
          new Vector3(-1132.0f, -2005f, 13.2f),
          new Vector3(-1158.0f, -2029.9f, 13.2f),
          new Vector3(-1172.0f, -2014.9f, 13.2f)
        },
        Blip = new BlipBag
        {
          PosVector3 = new Vector3(-1154.3f, -2007.2f, 13.2f),
          BlipId = 446,
          Color = 81,
          Label = "LSC",
          Scale = 1,
          UniqueId = "LSC_1"
        }
      },
    };

    public static Nullable<TuningShopBag> CurrentShop = null;

    public TuningShopClient()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.TuningShopClient, eScriptArea.BlipClient))
        return;

      foreach (var s in Shops)
      {
        BlipClient.AddBlip(s.Blip);
      }

      Tick += OnTick;
      Tick += OnSlowTick;
      AddTextEntry("TUNING_SHOP_INSTANDCAT", $"~INPUT_CELLPHONE_CAMERA_GRID~ {FontsManager.FiraSansString}Katalog"); //183

      InternalDependencyManager.Started(eScriptArea.TuningShopClient);
    }

    private async Task OnTick()
    {
      foreach (var shop in Shops)
      {
        OfwFunctions.DrawPolygon(shop.ShopPolygon);

        TextUtils.DrawTextOnScreen($"CURRENTSHOP:{CurrentShop?.Blip?.Label ?? "---"}", 0.5f, 0.8f, 0.3f, CitizenFX.Core.UI.Alignment.Center);
      }

      if (CurrentShop != null)
      {
        if (Game.PlayerPed.CurrentVehicle != null && GetPedInVehicleSeat(Game.PlayerPed.CurrentVehicle.Handle, -1) == Game.PlayerPed.Handle && NativeMenuManager.IsHidden)
        {
          DisplayHelpTextThisFrame("TUNING_SHOP_INSTANDCAT", true);

          //if (Game.IsControlPressed(0, Control.Sprint) && Game.IsControlJustPressed(0, Control.PhoneCameraGrid)) //SHIFT + G
          //{
          //  TuningInstallClient.OpenTuningInstall(Game.PlayerPed.CurrentVehicle.Handle);
          //}
          if (Game.IsControlJustPressed(0, Control.PhoneCameraGrid)) //G
          {
            VehicleClient.HandleVehicleMenu(true);
            //TuningCatalogClient.OpenCatalog();
          }
        }
      }
    }

    private async Task OnSlowTick()
    {
      bool isInAnyShop = false;

      foreach (var shop in Shops)
      {
        if (Game.PlayerPed.Position.DistanceToSquared(shop.Blip.PosVector3) < 2000f)
        {
          bool isInside = OfwFunctions.IsInsidePolygon(shop.ShopPolygon, Game.PlayerPed.Position);

          if (isInside)
          {
            isInAnyShop = true;
          }

          if (isInside && CurrentShop != null && CurrentShop?.ShopID != shop.ShopID)
          {
            LeftShop();
            EnteredShop(shop);
          }
          else if (isInside && CurrentShop == null)
          {
            EnteredShop(shop);
          }

          break;
        }
      }

      if (!isInAnyShop && CurrentShop != null)
      {
        LeftShop();
      }

      await Delay(1000);
    }

    private void EnteredShop(TuningShopBag enteredShop)
    {
      CurrentShop = enteredShop;
    }

    private void LeftShop()
    {
      CurrentShop = null;
      NativeMenuManager.CloseAndUnlockMenu(TuningCatalogClient.MenuName);
    }

    //[EventHandler("ofw_tax:UpdateShops")]
    //private void UpdateShops(string shopsData)
    //{
    //  
    //}
  }
}
