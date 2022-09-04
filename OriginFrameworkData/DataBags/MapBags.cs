using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
  public class MapBag
  {
    public int Id { get; set; }
    public int Name { get; set; }
  }

  public class MapPropBag
  {
    public int Id { get; set; }
    public int MapId { get; set; }
    public int PropHash { get; set; }

    public float EntityPosX { get; set; }
    public float EntityPosY { get; set; }
    public float EntityPosZ { get; set; }
    public float EntityYaw { get; set; }
    public float EntityPitch { get; set; }
    public float EntityRoll { get; set; }
  }
}
