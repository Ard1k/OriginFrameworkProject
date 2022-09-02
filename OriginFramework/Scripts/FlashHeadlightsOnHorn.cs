using CitizenFX.Core;
using OriginFrameworkData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
	public class FlashHeadlightsOnHorn : BaseScript
	{
    public static bool IsEnabled { get; set; } = GetResourceKvpInt(KvpManager.getKvpString(KvpEnum.FlashOnHorn)) > 0;

    public FlashHeadlightsOnHorn()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.FlashHeadlightsAndHorn))
        return;

      Tick += OnTick;

      InternalDependencyManager.Started(eScriptArea.FlashHeadlightsAndHorn);
    }


    private async Task OnTick()
		{
      if (!IsEnabled)
      {
        await Delay(1000);
        return;
      }

      if (IsPedInAnyVehicle(Game.PlayerPed.Handle, false))
      {
        var vehicle = GetVehiclePedIsIn(Game.PlayerPed.Handle, false);
        if (GetPedInVehicleSeat(vehicle, -1) == Game.PlayerPed.Handle)
        {
          if (IsDisabledControlJustPressed(0, (int)Control.VehicleHorn))
          {
            SetVehicleLights(vehicle, 2);
            SetVehicleLightMultiplier(vehicle, 12f);
          }
          else if (IsDisabledControlJustReleased(0, (int)Control.VehicleHorn))
          {
            SetVehicleLights(vehicle, 0);
            SetVehicleLightMultiplier(vehicle, 1.0f);
          }
        }
      }
		}

    public static void EnableFlashOnHorn()
    {
      IsEnabled = true;
      Notify.Info("Dalkovky s klaksonem aktivovany!");
    }
    public static void DisableFlashOnHorn()
    {
      IsEnabled = false;
      Notify.Info("Dalkovky s klaksonem deaktivovany!");
    }
  }
}
