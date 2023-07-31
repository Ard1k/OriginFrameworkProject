using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using Newtonsoft.Json;
using OriginFramework.Helpers;
using OriginFramework.Menus;
using OriginFrameworkData.DataBags;
using static CitizenFX.Core.Native.API;

namespace OriginFramework.Scripts
{
  public class WeatherTimeClient : BaseScript
  {
    public string CurrentWeather { get; set; } = "EXTRASUNNY";
    public string LastWeather { get; set; } = null;

    public WeatherTimeClient()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
      NetworkOverrideClockMillisecondsPerGameMinute(10000);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      Tick += OnTick;
      Tick += OnTick2;
    }

    private async Task OnTick2()
    {
      //Debug.WriteLine($"H:{GetClockHours()} M:{GetClockMinutes()}");
      await Delay(1000);
      
    }

    private async Task OnTick()
    {
      if (LastWeather != CurrentWeather)
      {
        LastWeather = CurrentWeather;
        SetWeatherTypeOverTime(CurrentWeather, 15.0f);
        await Delay(15000);
      }

      ClearOverrideWeather();
      ClearWeatherTypePersist();
      SetWeatherTypePersist(LastWeather);
      SetWeatherTypeNow(LastWeather);
      SetWeatherTypeNowPersist(LastWeather);

      if (LastWeather == "XMAS")
      {

        SetForceVehicleTrails(true);
        SetForcePedFootstepsTracks(true);
      }
      else
      {
        SetForceVehicleTrails(false);
        SetForcePedFootstepsTracks(false);
      }
    }

    [EventHandler("ofw:SyncWeatherTime")]
    private void SyncWeatherTime(string weather, int hours, int minutes)
    {
      CurrentWeather = weather;
      NetworkOverrideClockMillisecondsPerGameMinute(10000);
      NetworkOverrideClockTime(hours, minutes, 0);
    }

  }
}
