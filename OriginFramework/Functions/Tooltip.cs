using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
  public class Tooltip
  {
    public static async void DrawTooltip(float x, float y, string title, string[] descriptions)
    {
      TextUtils.DrawTextOnScreen(title, x, y);
    }
  }
}
