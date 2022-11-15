﻿using CitizenFX.Core;
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

    public static void DrawSprite2(string textureDict, string texture, double x1, double y1, double x2, double y2, float rotation, int r, int g, int b, int a)
    {
      float width = (float)(x2 - x1);
      float height = (float)(y2 - y1);

      DrawSprite(textureDict, texture, (float)(x1 + width / 2), (float)(y1 + height / 2), width, height, rotation, r, g, b, a);
    }

    public static bool IsInBounds(RectBounds bounds, double xRel, double yRel)
    {
      if (bounds == null)
      {
        TheBugger.DebugLog("IsInBounds - null bounds passed");
        return false;
      }

      if (xRel > bounds.X1 && xRel < bounds.X2 && yRel > bounds.Y1 && yRel < bounds.Y2)
        return true;

      return false;
    }

    public static bool TryGetBoundsGridPosition(RectBounds bounds, double xRel, double yRel, double xCellWidth, double yCellHeigth, out int x, out int y)
    {
      x = -1;
      y = -1;

      if (bounds == null)
      {
        TheBugger.DebugLog("TryGetBoundsGridPosition - null bounds passed");
        return false;
      }

      if (xCellWidth <= 0 || yCellHeigth <= 0)
      {
        TheBugger.DebugLog("TryGetBoundsGridPosition - invalid cell dimensions passed");
        return false;
      }

      x = (int)Math.Floor(((xRel - bounds.X1) / xCellWidth));
      y = (int)Math.Floor(((yRel - bounds.Y1) / yCellHeigth));

      return true;
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