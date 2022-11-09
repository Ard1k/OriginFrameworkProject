using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFramework.Helpers
{
  internal class MapHelper
  {
    public MapBag Map;
    public List<int> spawnedNetIds = new List<int>();

    private MapHelper()
    { 
    }

    public static MapHelper InitializeNewMap(string name)
    {
      var helper = new MapHelper();
      helper.Map = new MapBag();
      helper.Map.Name = name;
      helper.Map.Props = new List<MapPropBag>();

      return helper;
    }

    public void AddProp(int modelHash, float posX, float posY, float posZ, float pitch, float roll, float yaw)
    {
      Map.Props.Add(new MapPropBag { 
        PropHash = modelHash,
        EntityPosX = posX,
        EntityPosY = posY,
        EntityPosZ = posZ,
        EntityPitch = pitch,
        EntityRoll = roll,
        EntityYaw = yaw
      });
    }
  }
}
