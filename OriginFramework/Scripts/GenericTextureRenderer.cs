using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;


namespace OriginFramework
{
  public class GenericTextureRenderer : BaseScript
  {
    public GenericTextureRenderer()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
    }

    const string url = "https://google.com";
      //"https://w.soundcloud.com/player/?url=https%3A//api.soundcloud.com/tracks/467725488&color=%23ff5500&auto_play=true&hide_related=false&show_comments=true&show_user=true&show_reposts=false&show_teaser=true&visual=true";
    const float scale = 0.1f;
    string sfName = "ofw_renderer_1";

    const int width = 1280;
    const int height = 720;

    int sfHandle = 0;
    bool txdHasBeenSet = false;
    long duiObj = -1;

    long txd = 0;
    string dui = null;
    long tx = 0;

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      //while (!HasScaleformMovieLoaded(sfHandle))
      //{
      //  sfHandle = RequestScaleformMovie(sfName);
      //  await Delay(0);
      //}

      ////runtimeTxd = "meows";

      //txd = CreateRuntimeTxd("meows");
      //duiObj = CreateDui(url, width, height);
      //dui = GetDuiHandle(duiObj);
      //tx = CreateRuntimeTextureFromDuiHandle(txd, "woof", dui);

      //Tick += OnTick;
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
      
      //DestroyDui(duiObj);
      //SetScaleformMovieAsNoLongerNeeded(ref sfHandle);
    }

    private async Task OnTick()
		{
      var ped = Game.PlayerPed.Handle;
      var pos = Game.PlayerPed.Position;

      if (sfHandle != 0 && !txdHasBeenSet)
      {
        PushScaleformMovieFunction((int)sfHandle, "SET_TEXTURE");

        PushScaleformMovieMethodParameterString("meows"); // txd
        PushScaleformMovieMethodParameterString("woof"); // txn

        PushScaleformMovieFunctionParameterInt(0); // x
        PushScaleformMovieFunctionParameterInt(0); // y
        PushScaleformMovieFunctionParameterInt(width);
        PushScaleformMovieFunctionParameterInt(height);

        PopScaleformMovieFunctionVoid();

        txdHasBeenSet = true;
      }

      Debug.WriteLine($"SfHandle: {sfHandle} HasLoaded: {HasScaleformMovieLoaded(sfHandle)}");

      if (sfHandle != 0 && HasScaleformMovieLoaded(sfHandle))
      {
        Debug.WriteLine("test");

        DrawScaleformMovie_3dNonAdditive(
          (int)sfHandle,
          pos.X - 1f, pos.Y, pos.Z + 2f,
          0, 0, 0,
          2f, 2f, 2f,
          scale * 1f, scale * (9f / 16f), 1f,
          2);
      }
    }
  }
}
