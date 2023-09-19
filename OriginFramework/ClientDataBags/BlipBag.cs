using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFramework.ClientDataBags
{
  public class BlipBag
  {
    public Vector3 PosVector3 { get; set; }
    public int Id { get; set; }
    public string UniqueId { get; set; }
    public string Label { get; set; }
    public int BlipId { get; set; }
    public int Color { get; set; }
    public float Scale { get; set; }
  }
}
