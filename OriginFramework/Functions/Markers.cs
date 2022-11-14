using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
  public class Markers
  {
    public static async void DrawTextedMarker(float distance, string text, int type, Vector3 pos, float size, int r, int g, int b, int a)
    {
      if (distance < 5f)
        distance = 5f;

      const float perspectiveScale = 3f;
      float _x = 0, _y = 0;
      World3dToScreen2d(pos.X, pos.Y, pos.Z + 1f, ref _x, ref _y);
      var p = GetGameplayCamCoords();
      //var fov = (1 / GetGameplayCamFov()) * 75;
      var scale = ((1 / distance) * perspectiveScale) /* * fov*/;

      SetTextScale(1, scale);
      SetTextFont(0);
      SetTextProportional(true);
      SetTextColour(255, 255, 255, 255);
      SetTextOutline();
      SetTextEntry("STRING");
      SetTextCentre(true);
      AddTextComponentString(text);
      DrawText(_x, _y);

      DrawMarker(type, pos.X, pos.Y, pos.Z, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0f, size, size, size, r, g, b, a, false, false, 2, false, null, null, false);
    }
  }
}
