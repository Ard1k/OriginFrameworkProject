using CitizenFX.Core;
using OriginFrameworkData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;
using OriginFrameworkData.DataBags;
using CitizenFX.Core.Native;
using static OriginFrameworkServer.OfwServerFunctions;
using System.IO;
using Newtonsoft.Json.Linq;

namespace OriginFrameworkServer
{
  public class WeatherTimeServer : BaseScript
  {

    private static string[] weathers = { 
      "EXTRASUNNY", 
      "CLEAR",
      "NEUTRAL",
      "SMOG",
      "FOGGY",
      "OVERCAST",
      "CLOUDS",
      "CLEARING",
      "RAIN",
      "THUNDER",
      "SNOW",
      "BLIZZARD",
      "SNOWLIGHT",
      "XMAS",
      "HALLOWEEN" };

    private static int currentWeather = 0;
    private const int minRegularWeather = 0;
    private const int maxRegularWeather = 9;
    private const double sunnyChange = 0.4;
    private const double rainyChange = 0.2;
    private static TimeSpan timeOffset = new TimeSpan(0);

    private static Random rand = new Random();

    public WeatherTimeServer()
    {
      rand.Next();
      rand.Next();
      rand.Next();

      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
    }

		private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      Tick += OnMinuteTick;

      RegisterCommand("weather", new Action<int, List<object>, string>((source, args, raw) =>
      {
        var sourcePlayer = Players.Where(p => p.Handle == source.ToString()).FirstOrDefault();
        if (sourcePlayer == null)
          return;

        if (!CharacterCaretakerServer.HasPlayerAdminLevel(sourcePlayer, 10))
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Nedostatečné oprávnění");
          return;
        }

        if (args == null || args.Count != 1)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neplatný počet argumentů");
          return;
        }

        if (args[0] is string weatherString)
        {
          int weatherInt = 0;

          if (Int32.TryParse(weatherString, out weatherInt) && weatherInt < weathers.Length && weatherInt >= 0)
          {
            currentWeather = (int)weatherInt;
            sourcePlayer.TriggerEvent("ofw:SuccessNotification", "Počasí nastaveno");
            SyncWeatherTime();
            return;
          }

          string upperWeather = weatherString.ToUpper().Trim();
          //if (upperWeather == "AUTO")
          //{
          //  sourcePlayer.TriggerEvent("ofw:SuccessNotification", "Počasí se bude automaticky nastavovat");
          //  return;
          //}

          for (int i = 0; i < weathers.Length; i++)
          {
            if (weathers[i] == upperWeather)
            {
              currentWeather = i;
              sourcePlayer.TriggerEvent("ofw:SuccessNotification", "Počasí nastaveno");
              SyncWeatherTime();
              return;
            }
          }
        }
        sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neplatný typ počasí");
      }), false);

      RegisterCommand("setday", new Action<int, List<object>, string>((source, args, raw) =>
      {
        var sourcePlayer = Players.Where(p => p.Handle == source.ToString()).FirstOrDefault();
        if (sourcePlayer == null)
          return;

        if (!CharacterCaretakerServer.HasPlayerAdminLevel(sourcePlayer, 10))
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Nedostatečné oprávnění");
          return;
        }
        timeOffset = new TimeSpan(0);

        var dayminutes = GetDayMinutes();
        int newMinutesOffset = 0;
        if (dayminutes >= 600)
          newMinutesOffset = 1440 - dayminutes + 600; // zbytek dne + 10:00
        else if (dayminutes <= 600)
          newMinutesOffset = 600 - dayminutes; //posun do 10:00

        timeOffset = TimeSpan.FromMinutes(newMinutesOffset / 6);

        SyncWeatherTime();
        sourcePlayer.TriggerEvent("ofw:SuccessNotification", "Nastaven den");
      }), false);

      RegisterCommand("setnight", new Action<int, List<object>, string>((source, args, raw) =>
      {
        var sourcePlayer = Players.Where(p => p.Handle == source.ToString()).FirstOrDefault();
        if (sourcePlayer == null)
          return;

        if (!CharacterCaretakerServer.HasPlayerAdminLevel(sourcePlayer, 10))
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Nedostatečné oprávnění");
          return;
        }
        timeOffset = new TimeSpan(0);

        var dayminutes = GetDayMinutes();
        int newMinutesOffset = 1440 - dayminutes;

        timeOffset = TimeSpan.FromMinutes(newMinutesOffset / 6);

        SyncWeatherTime();
        sourcePlayer.TriggerEvent("ofw:SuccessNotification", "Nastavena noc");
      }), false);
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
    }

    private async Task OnMinuteTick()
    {
      if (currentWeather <= 9)
      {
        if (ChangeWeatherThisTime())
          SetNextWeather();
      }

      SyncWeatherTime();
      await Delay(60000);
    }

    public static void SyncWeatherTime()
    {
      var dayminutes = GetDayMinutes();
      var hours = dayminutes / 60; //je to int, bude to cele cislo
      var minutes = dayminutes % 60;
      //Debug.WriteLine($"dayminutes:{dayminutes} hours:{hours} minutes:{minutes}");
      TriggerClientEvent("ofw:SyncWeatherTime", weathers[currentWeather], hours, minutes);
    }

    public static int GetDayMinutes()
    {
      return (((DateTime.Now + timeOffset).Hour % 4) * 60 + (DateTime.Now + timeOffset).Minute) * 6;
    }

    public static void SetNextWeather()
    {
      currentWeather = rand.Next(0, 10);
      if (currentWeather == 1 || currentWeather == 6 || currentWeather == 0) //clear, clouds, extrasunny
      {
        if (rand.NextDouble() > 0.5f)
          currentWeather = 7; //clearing
        else
          currentWeather = 5; //overcast
      }
      else if (currentWeather == 7 || currentWeather == 5) //clearing, overcast
      {
        var randRes = rand.Next(1, 6);
        switch (randRes)
        {
          case 1:
            if (currentWeather == 7) //clearing
              currentWeather = 4; //foggy
            else
              currentWeather = 8; //rain
            break;
          case 2: currentWeather = 6; break; //clouds
          case 3: currentWeather = 1; break; //clear
          case 4: currentWeather = 0; break; //extrasunny
          case 5: currentWeather = 3; break; //smog
          default: currentWeather = 4; break; //foggy
        }
      }
      else if (currentWeather == 9 || currentWeather == 8) //thunder, rain
      {
        currentWeather = 7; //clearing
      }
      else if (currentWeather == 3 || currentWeather == 4) //smog, foggy
      {
        currentWeather = 1; //clear
      }
    }

    public static bool IsRainyWeather()
    {
      return currentWeather == 3 || currentWeather == 4 || currentWeather == 8 || currentWeather == 9;
    }

    public static bool ChangeWeatherThisTime()
    {
      if (currentWeather == 7)
        return true;

      if (IsRainyWeather())
        return rand.NextDouble() + rainyChange > 1f;

      return rand.NextDouble() + sunnyChange > 1f;
    }

    //ofw:RequestWeatherTimeSync
    [EventHandler("ofw:RequestWeatherTimeSync")]
    private async void RequestWeatherTimeSync([FromSource] Player source, string mapName)
    {
      if (source == null)
        return;

      var dayminutes = GetDayMinutes();
      var hours = dayminutes / 60; //je to int, bude to cele cislo
      var minutes = dayminutes % 60;

      source.TriggerEvent("ofw:SyncWeatherTime", weathers[currentWeather], hours, minutes);
    }
  }
}
