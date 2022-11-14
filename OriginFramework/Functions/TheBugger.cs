using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
  public class TheBugger
  {
    public static bool DebugMode { get; set; } = true;

    public static void DebugLog(string data)
    {
      if (DebugMode) Debug.WriteLine(@data);
    }
  }
}
