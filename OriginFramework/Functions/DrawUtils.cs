using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.UI.Screen;

namespace OriginFramework
{
  public class DrawUtils
  {
    public static string[] StringToArray(string inputString)
    {
      return CitizenFX.Core.UI.Screen.StringToArray(inputString);
    }

    public static void DrawRect2(RectBounds bounds, int r, int g, int b, int a)
    {
      DrawRect2(bounds.X1, bounds.Y1, bounds.X2, bounds.Y2, r, g, b, a);
    }
    public static void DrawRect2(double x1, double y1, double x2, double y2, int r, int g, int b, int a)
    {
      float width = (float)(x2 - x1);
      float height = (float)(y2 - y1);

      DrawRect((float)(x1 + width / 2), (float)(y1 + height / 2), width, height, r, g, b, a);
    }

    public class RectBounds
    {
      public double X1 { get; set; }
      public double Y1 { get; set; }
      public double X2 { get; set; }
      public double Y2 { get; set; }
    }
  }
}
