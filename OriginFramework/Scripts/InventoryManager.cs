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
using static OriginFramework.DrawUtils;

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
    double cellWidth = 0d;
    double cellHeight = 0d;
    RectBounds leftInvBounds = null;
    RectBounds rightInvBounds = null;
    RectBounds backgroundBounds = null;

    #region Mockup
    InventoryBag leftInv = new InventoryBag
    {
      Name = "Test player",
      RowCount = 5,
      Items = new List<InventoryItemBag> {
        new InventoryItemBag
        {
          X = 0,
          Y = 0,
          Name = "Fun",
          Color = new int[] { 255,0,0,255 }
        },
        new InventoryItemBag
        {
          X = 4,
          Y = 4,
          Name = "Fun",
          Color = new int[] { 255,0,0,255 }
        }
      }
    };
    #endregion

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
      cellWidth = invWidth / gridXCount;
      cellHeight = invHeight / gridYCount;

      leftInvBounds = new RectBounds
      {
        X1 = (0.5d - invWidth) - (cellWidth * (gridCenterSpace / 2)),
        Y1 = 0.5d - (invHeight / 2),
        X2 = 0.5d - (cellWidth * (gridCenterSpace / 2)),
        Y2 = 0.5d + (invHeight / 2)
      };

      rightInvBounds = new RectBounds
      {
        X1 = 0.5d + cellWidth * (gridCenterSpace / 2),
        Y1 = 0.5d - (invHeight / 2),
        X2 = 0.5d + invWidth + cellWidth * (gridCenterSpace / 2),
        Y2 = 0.5d + (invHeight / 2)
      };

      backgroundBounds = new RectBounds
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

      int cursor_x = 1, cursor_y = 1;
      GetNuiCursorPosition(ref cursor_x, ref cursor_x);

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

      if (leftInv != null)
      {
        for (int y = 0; y < 5; y++)
        {
          for (int x = 0; x < leftInv.RowCount; x++)
          {
            var it = leftInv.Items.Where(a => a.Y == y && a.X == x).FirstOrDefault();
            RenderItem(x, y, it);
          }
        }
      }
    }

    private void RenderItem(int x, int y, InventoryItemBag it)
    {
      if (it == null)
        it = new InventoryItemBag { Name = "Empty", X = x, Y = y, Color = new int[] { 255,255,255,100 } };

      DrawUtils.DrawRect2(leftInvBounds.X1 + x * cellWidth + cellWidth * 0.98d, leftInvBounds.Y1 + y * cellHeight + cellHeight * 0.98d, leftInvBounds.X1 + (x + 1) * cellWidth - cellWidth * 0.98d, leftInvBounds.Y1 + (y + 1) * cellHeight - cellHeight * 0.98d, it.Color[0], it.Color[1], it.Color[2], it.Color[3]);
    }
  }
}
