using CitizenFX.Core;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OriginFramework.Scripts.BlipClient;

namespace OriginFramework.ClientDataBags
{
  public struct TuningShopBag
  {
    public string ShopID { get; set; }
    public List<Vector3> ShopPolygon { get; set; }
    public BlipBag Blip { get; set; }
  }
}
