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
	public class DrifterAndBooster : BaseScript
	{
    public static bool IsDrifterEnabled { get; set; } = GetResourceKvpInt(KvpManager.getKvpString(KvpEnum.DrifterEnabled)) > 0;
    private static bool isBoosterEnabled = false;

    private float speed_limit = 1000f;
    private float kmh_multiplier = 3.6f;
    private Control actionKey = Control.Sprint;

    public DrifterAndBooster()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        await Delay(0);

      Tick += OnTick;
    }


    private async Task OnTick()
		{
      await (Delay(250));

      if (IsDrifterEnabled || isBoosterEnabled)
      {
        if (IsPedInAnyVehicle(Game.PlayerPed.Handle, false))
        {
          var vehicle = GetVehiclePedIsIn(Game.PlayerPed.Handle, false);
          if (GetPedInVehicleSeat(vehicle, -1) == Game.PlayerPed.Handle)
          {
            var vehicleSpeed = GetEntitySpeed(vehicle);
            if ((IsControlPressed(0, (int)actionKey) || IsDisabledControlPressed(0, (int)actionKey)) && (vehicleSpeed * kmh_multiplier) <= speed_limit)
            {
              if (IsDrifterEnabled)
              {
                SetVehicleReduceGrip(vehicle, true);
                SetVehicleReduceTraction(vehicle, true);
              }
              if (isBoosterEnabled)
                SetVehicleForwardSpeed(vehicle, vehicleSpeed * 1.3f);
            }
            else
            {
              if (IsDrifterEnabled)
              {
                SetVehicleReduceGrip(vehicle, false);
                SetVehicleReduceTraction(vehicle, false);
                SetVehicleCheatPowerIncrease(vehicle, 1f);
              }
            }
          }
        }
      }
		}

    public static void EnableDrifter()
    {
      IsDrifterEnabled = true;
      Notify.Info("Drift aktivovan!");
    }
    public static void DisableDrifter()
    {
      IsDrifterEnabled = false;
      Notify.Info("Drift deaktivovan!");
    }
    public static void EnableBooster()
    {
      isBoosterEnabled = true;
      Notify.Info("Booster aktivovan!");
    }
    public static void DisableBooster()
    {
      isBoosterEnabled = false;
      Notify.Info("Booster deaktivovan!");
    }
  }
}
