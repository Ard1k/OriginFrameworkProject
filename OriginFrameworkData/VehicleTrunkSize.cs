using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData
{
  public static class VehicleTrunkSize
  {
    public static int GetTrunkRowcount(int category, int model, eItemCarryType type)
    {
      switch (type)
      {
        case eItemCarryType.Forklift:
          return 1;
        case eItemCarryType.Hands: 
          return 2;
        default:
          return 3;
      }
    }
  }
}
