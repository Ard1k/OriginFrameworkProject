using CitizenFX.Core;
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
    private static bool isDrifterEnabled = false;
    private static bool isBoosterEnabled = false;

    private float speed_limit = 1000f;
    private float kmh_multiplier = 3.6f;
    private Control actionKey = Control.Sprint;

    public DrifterAndBooster()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        Delay(0);

      Tick += OnTick;
    }


    private async Task OnTick()
		{
      await (Delay(250));

      if (isDrifterEnabled || isBoosterEnabled)
      {
        if (IsPedInAnyVehicle(Game.PlayerPed.Handle, false))
        {
          var vehicle = GetVehiclePedIsIn(Game.PlayerPed.Handle, false);
          if (GetPedInVehicleSeat(vehicle, -1) == Game.PlayerPed.Handle)
          {
            var vehicleSpeed = GetEntitySpeed(vehicle);
            if ((IsControlPressed(0, (int)actionKey) || IsDisabledControlPressed(0, (int)actionKey)) && (vehicleSpeed * kmh_multiplier) <= speed_limit)
            {
              if (isDrifterEnabled)
                SetVehicleReduceGrip(vehicle, true);
              else if (isBoosterEnabled)
                SetVehicleForwardSpeed(vehicle, vehicleSpeed * 1.2f);
            }
            else
            {
              if (isDrifterEnabled)
                SetVehicleReduceGrip(vehicle, false);
            }
          }
        }
      }
		}

    public static void EnableDrifter()
    {
      isDrifterEnabled = true;
      Notify.Info("Drift enabled!");
    }
    public static void DisableDrifter()
    {
      isDrifterEnabled = false;
      Notify.Info("Drift disabled!");
    }
    public static void EnableBooster()
    {
      isBoosterEnabled = true;
      Notify.Info("Booster enabled!");
    }
    public static void DisableBooster()
    {
      isBoosterEnabled = false;
      Notify.Info("Booster disabled!");
    }
  }
}
