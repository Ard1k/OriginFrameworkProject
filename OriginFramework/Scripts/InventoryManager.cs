using CitizenFX.Core;
using CitizenFX.Core.UI;
using Newtonsoft.Json;
using OriginFramework.Menus;
using OriginFrameworkData;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
  public class InventoryManager : BaseScript
  {
    public bool IsInventoryOpen { get; private set; } = false;
    private double invSizeOverHeight = 0.5d;
    private double invBorderOverHeight = 0.01d;
    private int gridXCount = 5;
    private int gridYCount = 5;
    private double gridCenterSpace = 1d;
    private int screen_width = 1;
    private int screen_height = 1;
    Bounds leftInvBounds = null;
    Bounds rightInvBounds = null;
    Bounds backgroundBounds = null;

    public InventoryManager()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.InventoryManager))
        return;

      Tick += OnTick;

      InternalDependencyManager.Started(eScriptArea.InventoryManager);
    }

    private void RefreshBounds()
    {
      double aspectFactor = (double)screen_width / (double)screen_height;

      double invHeight = invSizeOverHeight;
      double invWidth = invSizeOverHeight / aspectFactor;
      double cellWidth = invWidth / gridXCount;
      double cellHeight = invHeight / gridYCount;

      leftInvBounds = new Bounds
      {
        X1 = (0.5d - invWidth) - (cellWidth * (gridCenterSpace / 2)),
        Y1 = 0.5d - (invHeight / 2),
        X2 = 0.5d - (cellWidth * (gridCenterSpace / 2)),
        Y2 = 0.5d + (invHeight / 2)
      };

      rightInvBounds = new Bounds
      {
        X1 = 0.5d + cellWidth * (gridCenterSpace / 2),
        Y1 = 0.5d - (invHeight / 2),
        X2 = 0.5d + invWidth + cellWidth * (gridCenterSpace / 2),
        Y2 = 0.5d + (invHeight / 2)
      };

      backgroundBounds = new Bounds
      {
        X1 = leftInvBounds.X1 - (invBorderOverHeight / aspectFactor),
        Y1 = leftInvBounds.Y1 - invBorderOverHeight,
        X2 = rightInvBounds.X2 + (invBorderOverHeight / aspectFactor),
        Y2 = rightInvBounds.Y2 + invBorderOverHeight
      };
    }

    private async Task OnTick()
    {
      if (Game.IsControlJustPressed(0, Control.ReplayStartStopRecordingSecondary)) //F2
      {
        IsInventoryOpen = !IsInventoryOpen;
      }

      if (!IsInventoryOpen)
        return;

      int screen_w = 1, screen_h = 1;
      GetActiveScreenResolution(ref screen_w, ref screen_h);

      if (screen_w != screen_width || screen_h != screen_height)
      {
        screen_width = screen_w;
        screen_height = screen_h;

        RefreshBounds();
      }

      int x = 1, y = 1;
      GetNuiCursorPosition(ref x, ref y);

      

      Game.DisableControlThisFrame(0, Control.LookLeftRight);
      Game.DisableControlThisFrame(0, Control.LookUpDown);
      Game.DisableControlThisFrame(0, Control.Attack);
      Game.DisableControlThisFrame(0, Control.Attack2);
      Game.DisableControlThisFrame(0, Control.MeleeAttack1);
      Game.DisableControlThisFrame(0, Control.MeleeAttack2);
      Game.DisableControlThisFrame(0, Control.Aim);
      Game.DisableControlThisFrame(0, Control.VehicleMouseControlOverride);

      if (Game.IsDisabledControlPressed(0, Control.Attack))
      {
        SetCursorSprite(4);
      }
      else
        SetCursorSprite(0);

      ShowCursorThisFrame();
      Render();
    }

    private void Render()
    {
      DrawRect2(backgroundBounds, 0, 0, 0, 30);
      DrawRect2(leftInvBounds, 0, 0, 0, 100);
      DrawRect2(rightInvBounds, 0, 0, 0, 100);
    }

    private void DrawRect2(Bounds bounds, int r, int g, int b, int a)
    {
      DrawRect2(bounds.X1, bounds.Y1, bounds.X2, bounds.Y2, r, g, b, a);
    }
    private void DrawRect2(double x1, double y1, double x2, double y2, int r, int g, int b, int a)
    {
      float width = (float)(x2 - x1);
      float height = (float)(y2 - y1);

      DrawRect((float)(x1 + width / 2), (float)(y1 + height / 2), width, height, r, g, b, a);
    }

    private class Bounds
    {
      public double X1 { get; set; }
      public double Y1 { get; set; }
      public double X2 { get; set; }
      public double Y2 { get; set; }
    }
  }
}
