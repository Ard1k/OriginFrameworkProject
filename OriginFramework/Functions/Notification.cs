using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using System.Dynamic;

namespace OriginFramework
{
  public static class Notify
  {
    public static void Custom(string message, bool blink = true, bool saveToBrief = true)
    {
      SetNotificationTextEntry("CELL_EMAIL_BCON"); // 10x ~a~
      foreach (string s in CitizenFX.Core.UI.Screen.StringToArray(FontsManager.FiraSansString + (message ?? String.Empty)))
      {
        AddTextComponentSubstringPlayerName(s);
      }
      DrawNotification(blink, saveToBrief);
    }

    public static void ShowNotification(string msg, List<string> opts = null)
    {
      if (opts == null || opts.Count <= 0)
      {
        msg = FontsManager.FiraSansString + msg;
        SetNotificationTextEntry("STRING");
        AddTextComponentSubstringPlayerName(msg);
      }
      else
      {
        SetNotificationTextEntry(msg);

        foreach (var str in opts)
        {
          AddTextComponentSubstringPlayerName(str);
        }
      }

      PlaySoundFrontend(-1, "CONFIRM_BEEP", "HUD_MINI_GAME_SOUNDSET", true);
      var notificationId = DrawNotification(true, true);
    }

    public static void ShowAdvancedNotification(string title, string subject, string msg, string icon, int iconType, List<string> opts = null)
    {
      title = title != null ? FontsManager.FiraSansString + title : "";
      subject = subject != null ? FontsManager.FiraSansString + subject : "";

      PlaySound(-1, "Text_Arrive_Tone", "Phone_SoundSet_Default", false, 0, false);

      if (opts != null && opts.Count > 0)
      {
        SetNotificationTextEntry("STRING");
        AddTextComponentSubstringPlayerName(msg);

        foreach (var str in opts)
        {
          AddTextComponentSubstringPlayerName(str);
        }
      }
      else
      {
        msg = FontsManager.FiraSansString + msg;
        SetNotificationTextEntry("STRING");
        AddTextComponentSubstringPlayerName(msg);
      }

      SetNotificationMessage(icon, icon, false, iconType, title, subject);
      DrawNotification(true, true);
    }

    public static void ShowInventoryNotification(string itemName, string amount)
    {
      var message = new
      {
        type = "inventoryNotification",
        amount = amount,
        text = itemName
      };
      SendNuiMessage(JsonConvert.SerializeObject(message));
      PlaySoundFrontend(-1, "CONFIRM_BEEP", "HUD_MINI_GAME_SOUNDSET", true);
    }

    public static void Alert(string message, bool blink = true, bool saveToBrief = true)
    {
      //Custom("~y~~h~Alert~h~~s~: " + (message ?? String.Empty), blink, saveToBrief);
      ShowNotification(/*"~y~~h~Alert~h~~s~: " + */(message ?? String.Empty));
    }
    public static void Error(string message, bool blink = true, bool saveToBrief = true)
    {
      //Custom("~r~~h~Error~h~~s~: " + (message ?? String.Empty), blink, saveToBrief);
      ShowNotification(/*"~r~~h~Error~h~~s~: " + */(message ?? String.Empty));
      Debug.Write("[OFW] [ERROR] " + (message ?? String.Empty) + "\n");
    }
    public static void Info(string message, bool blink = true, bool saveToBrief = true)
    {
      //Custom("~b~~h~Info~h~~s~: " + (message ?? String.Empty), blink, saveToBrief);
      ShowNotification(/*"~b~~h~Info~h~~s~: " + */(message ?? String.Empty));
    }
    public static void Success(string message, bool blink = true, bool saveToBrief = true)
    {
      //Custom("~g~~h~Success~h~~s~: " + (message ?? String.Empty), blink, saveToBrief);
      ShowNotification(/*"~g~~h~Success~h~~s~: " + */(message ?? String.Empty));
    }
  }

  public static class Subtitle
  {
    public static void Custom(string message, int duration = 2500, bool drawImmediately = true)
    {
      BeginTextCommandPrint("CELL_EMAIL_BCON"); // 10x ~a~
      foreach (string s in CitizenFX.Core.UI.Screen.StringToArray(message))
      {
        AddTextComponentSubstringPlayerName(s);
      }
      EndTextCommandPrint(duration, drawImmediately);
    }

    public static void Alert(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
    {
      Custom((prefix != null ? "~y~" + prefix + " ~s~" : "~y~") + message, duration, drawImmediately);
    }

    public static void Error(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
    {
      Custom((prefix != null ? "~r~" + prefix + " ~s~" : "~r~") + message, duration, drawImmediately);
    }

    public static void Info(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
    {
      Custom((prefix != null ? "~b~" + prefix + " ~s~" : "~b~") + message, duration, drawImmediately);
    }

    public static void Success(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
    {
      Custom((prefix != null ? "~g~" + prefix + " ~s~" : "~g~") + message, duration, drawImmediately);
    }
  }
}
