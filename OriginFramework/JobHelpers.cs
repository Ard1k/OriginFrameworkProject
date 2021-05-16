using CitizenFX.Core;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.UI.Screen;

namespace OriginFramework
{
	public static class JobHelpers
	{
    public static bool isMessageFreemodeShown = false;

    public static async void MessageFreemodeJobFinished(string textSmall)
    {
      await MessageFreemode("~g~ukol dokoncen", textSmall, 255, 255, 255, 255);
    }
    public static async void MessageFreemodeJobStarted(string textSmall)
    {
      await MessageFreemode("novy ukol", textSmall, 255, 255, 255, 255);
    }
    public static async void MessageFreemodeJobFailed(string textSmall)
    {
      await MessageFreemode("~r~ukol ukoncen", textSmall, 255, 255, 255, 255);
    }

    public static async Task<bool> MessageFreemode(string textBig, string textSmall, int r, int g, int b, int a)
		{
      while (isMessageFreemodeShown)
        await BaseScript.Delay(0);

      isMessageFreemodeShown = true;

      var scaleform = RequestScaleformMovie("MP_BIG_MESSAGE_FREEMODE");

      while (!HasScaleformMovieLoaded(scaleform))
        await BaseScript.Delay(0);

      BeginScaleformMovieMethod(scaleform, "SHOW_SHARD_WASTED_MP_MESSAGE");
      PushScaleformMovieMethodParameterString(textBig);
      PushScaleformMovieMethodParameterString(textSmall);
      EndScaleformMovieMethod();

      float time = 0;

      while (time <= 5000)
      {
        time += (GetFrameTime() * 1000);
        DrawScaleformMovieFullscreen(scaleform, r, g, b, a, 0);
        await BaseScript.Delay(0);
      }

      SetScaleformMovieAsNoLongerNeeded(ref scaleform);

      isMessageFreemodeShown = false;
      return true;
    }
	}
}
