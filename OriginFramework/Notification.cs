using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
  public static class Notify
  {
    public static void Custom(string message, bool blink = true, bool saveToBrief = true)
    {
      SetNotificationTextEntry("CELL_EMAIL_BCON"); // 10x ~a~
      foreach (string s in CitizenFX.Core.UI.Screen.StringToArray(message))
      {
        AddTextComponentSubstringPlayerName(s);
      }
      DrawNotification(blink, saveToBrief);
    }
    public static void Alert(string message, bool blink = true, bool saveToBrief = true)
    {
      Custom("~y~~h~Alert~h~~s~: " + message, blink, saveToBrief);
    }
    public static void Error(string message, bool blink = true, bool saveToBrief = true)
    {
      Custom("~r~~h~Error~h~~s~: " + message, blink, saveToBrief);
      Debug.Write("[OFW] [ERROR] " + message + "\n");
    }
    public static void Info(string message, bool blink = true, bool saveToBrief = true)
    {
      Custom("~b~~h~Info~h~~s~: " + message, blink, saveToBrief);
    }
    public static void Success(string message, bool blink = true, bool saveToBrief = true)
    {
      Custom("~g~~h~Success~h~~s~: " + message, blink, saveToBrief);
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
