using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
  public class Blips
  {
    public static int CreateBlip(Vector3 coords, string name, int sprite, int color, float scale)
    {
      var blip = AddBlipForCoord(coords.X, coords.Y, coords.Z);
      SetBlipSprite(blip, sprite);
      SetBlipDisplay(blip, 4);
      SetBlipColour(blip, color);
      SetBlipAsShortRange(blip, false);
      SetBlipScale(blip, scale);
      BeginTextCommandSetBlipName("STRING");
      AddTextComponentString(name);
      EndTextCommandSetBlipName(blip);

      return blip;
    }

    public static int CreateVehicleBlip(int entity, string name, int sprite, int color, float scale)
    {
      var old = GetBlipFromEntity(entity);
      if (old > 0)
        RemoveBlip(ref old);

      var blip = AddBlipForEntity(entity);
      SetBlipSprite(blip, sprite);
      SetBlipDisplay(blip, 4);
      SetBlipColour(blip, color);
      SetBlipAsShortRange(blip, false);
      SetBlipScale(blip, scale);
      BeginTextCommandSetBlipName("STRING");
      AddTextComponentString(name);
      EndTextCommandSetBlipName(blip);

      return blip;
    }

    public static int CreateRadiusBlip(Vector3 coords, float radius, int color)
    {
      var blip = AddBlipForRadius(coords.X, coords.Y, coords.Z, radius);
      SetBlipDisplay(blip, 4);
      SetBlipColour(blip, color);
      SetBlipAsShortRange(blip, false);

      return blip;
    }
  }
}
