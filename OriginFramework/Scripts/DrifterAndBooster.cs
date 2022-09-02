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
    public static bool IsBoosterEnabled = false;

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

      if (!await InternalDependencyManager.CanStart(eScriptArea.DrifterAndBooster))
        return;

      Tick += OnTick;

      InternalDependencyManager.Started(eScriptArea.DrifterAndBooster);
    }

    private async Task OnTick()
		{
      await (Delay(250));

      if (IsDrifterEnabled || IsBoosterEnabled)
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
              if (IsBoosterEnabled)
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
      IsBoosterEnabled = true;
      Notify.Info("Booster aktivovan!");
    }
    public static void DisableBooster()
    {
      IsBoosterEnabled = false;
      Notify.Info("Booster deaktivovan!");
    }
  }
}
