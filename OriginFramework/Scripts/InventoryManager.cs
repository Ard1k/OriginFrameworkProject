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
    CursorData cursorData = new CursorData();
    DragAndDropData dragData = new DragAndDropData();

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
          Color = new int[] { 255,0,0,255 },
          Texture = "crate"
        },
        new InventoryItemBag
        {
          X = 4,
          Y = 4,
          Name = "Fun",
          Color = new int[] { 255,0,0,255 },
          Texture = "crate"
        },
        new InventoryItemBag
        {
          X = 1,
          Y = 0,
          Name = "Fun",
          Color = new int[] { 0,255,0,255 },
          Texture = "crate"
        },
        new InventoryItemBag
        {
          X = 2,
          Y = 0,
          Name = "Fun",
          Color = new int[] { 0,0,255,255 },
          Texture = "crate"
        }
      }
    };

    InventoryBag rightInv = new InventoryBag
    {
      Name = "Test target",
      RowCount = 3,
      Items = new List<InventoryItemBag> {
        new InventoryItemBag
        {
          X = 1,
          Y = 1,
          Name = "Fun",
          Color = new int[] { 255,0,0,255 },
          Texture = "crate"
        },
        new InventoryItemBag
        {
          X = 1,
          Y = 2,
          Name = "Fun",
          Color = new int[] { 255,0,0,255 },
          Texture = "crate"
        },
        new InventoryItemBag
        {
          X = 2,
          Y = 1,
          Name = "Fun",
          Color = new int[] { 0,255,0,255 },
          Texture = "crate"
        },
        new InventoryItemBag
        {
          X = 2,
          Y = 2,
          Name = "Fun",
          Color = new int[] { 0,0,255,255 },
          Texture = "crate"
        },       
        new InventoryItemBag
        {
          X = 0,
          Y = 0,
          Name = "Fun",
          Color = new int[] { 255,255,255,255 },
          Texture = "item_component"
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

    private async Task OnTick()
    {
      if (Game.IsControlJustPressed(0, Control.ReplayStartStopRecordingSecondary)) //F2
      {
        IsInventoryOpen = !IsInventoryOpen;
      }

      if (!IsInventoryOpen)
        return;

      Game.DisableControlThisFrame(0, Control.LookLeftRight);
      Game.DisableControlThisFrame(0, Control.LookUpDown);
      Game.DisableControlThisFrame(0, Control.Attack);
      Game.DisableControlThisFrame(0, Control.Attack2);
      Game.DisableControlThisFrame(0, Control.MeleeAttack1);
      Game.DisableControlThisFrame(0, Control.MeleeAttack2);
      Game.DisableControlThisFrame(0, Control.Aim);
      Game.DisableControlThisFrame(0, Control.VehicleMouseControlOverride);

      int screen_w = 1, screen_h = 1;
      GetActiveScreenResolution(ref screen_w, ref screen_h);

      if (screen_w != screen_width || screen_h != screen_height)
      {
        screen_width = screen_w;
        screen_height = screen_h;

        RefreshBounds();
      }

      int cursor_x = 1, cursor_y = 1;
      GetNuiCursorPosition(ref cursor_x, ref cursor_y);

      ComputeCursorData(cursor_x, cursor_y);

      HandleDragAndDrop();
      HandleCursorSprites();

      ShowCursorThisFrame();
      Render();

      if (dragData.SrcItem == null)
        Tooltip.DrawTooltip((float)cursorData.xRelative + 0.05f, (float)cursorData.yRelative, $"INV:{cursorData.InvType} X:{cursorData.XGrid} Y:{cursorData.YGrid}", null);
    }

    #region private functions
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

    private void ComputeCursorData(int x, int y)
    {
      double xRelative = (double)x / (double)screen_width;
      double yRelative = (double)y / (double)screen_height;

      TheBugger.DebugLog($"X:{x}({xRelative}) Y:{y}({yRelative})");

      if (cursorData == null)
        cursorData = new CursorData();

      cursorData.xRelative = xRelative;
      cursorData.yRelative = yRelative;

      int xGrid, yGrid;
      if (DrawUtils.IsInBounds(leftInvBounds, xRelative, yRelative) && DrawUtils.TryGetBoundsGridPosition(leftInvBounds, xRelative, yRelative, cellWidth, cellHeight, out xGrid, out yGrid))
      {
        cursorData.InvData = leftInv;
        cursorData.HoverItem = leftInv.Items?.Where(it => it.X == xGrid && it.Y == yGrid).FirstOrDefault();
        cursorData.InvType = "left";
        cursorData.XGrid = xGrid;
        cursorData.YGrid = yGrid;
        return;
      }
      else if (DrawUtils.IsInBounds(rightInvBounds, xRelative, yRelative) && DrawUtils.TryGetBoundsGridPosition(rightInvBounds, xRelative, yRelative, cellWidth, cellHeight, out xGrid, out yGrid))
      {
        cursorData.InvData = rightInv;
        cursorData.HoverItem = rightInv.Items?.Where(it => it.X == xGrid && it.Y == yGrid).FirstOrDefault();
        cursorData.InvType = "right";
        cursorData.XGrid = xGrid;
        cursorData.YGrid = yGrid;
        return;
      }

      cursorData.InvData = null;
      cursorData.HoverItem = null;
      cursorData.InvType = "NONE";
      cursorData.XGrid = -1;
      cursorData.YGrid = -1;
    }

    private void HandleDragAndDrop()
    {
      if (Game.IsDisabledControlJustPressed(0, Control.Attack) && dragData.SrcItem == null && cursorData.HoverItem != null)
      {
        dragData.SrcItem = cursorData.HoverItem;
        dragData.SrcInv = cursorData.InvData;
        dragData.SrcItem.IsDragged = true;
      }

      if (Game.IsDisabledControlJustReleased(0, Control.Attack) && dragData.SrcItem != null)
      {
        if (cursorData.InvData != null && cursorData.HoverItem == null && cursorData.IsHoverOnEmptySlot)
        {
          dragData.TargetInv = cursorData.InvData;
          dragData.targetX = cursorData.XGrid;
          dragData.targetY = cursorData.YGrid;

          //samotny presun
          dragData.SrcInv.Items.Remove(dragData.SrcItem);
          dragData.SrcItem.IsDragged = false;
          dragData.SrcItem.X = dragData.targetX;
          dragData.SrcItem.Y = dragData.targetY;
          dragData.TargetInv.Items.Add(dragData.SrcItem);

          dragData.Clear();
        }
        else
        {
          dragData.SrcItem.IsDragged = false;
          dragData.Clear();
        }
      }


    }

    private void HandleCursorSprites()
    {
      if (dragData.SrcItem == null)
      {
        if (cursorData.InvData != null && cursorData.HoverItem != null)
          SetCursorSprite(3);
        else
          SetCursorSprite(0);
      }
      else if (cursorData.InvData != null && cursorData.HoverItem == null && cursorData.YGrid < cursorData.InvData.RowCount)
      {
        SetCursorSprite(9);
      }
      else
      {
        SetCursorSprite(4);
      }
    }

    private void Render()
    {
      if (!HasStreamedTextureDictLoaded("inventory_textures"))
        RequestStreamedTextureDict("inventory_textures", true);

      DrawRect2(backgroundBounds, 0, 0, 0, 30);
      DrawRect2(leftInvBounds, 0, 0, 0, 100);
      DrawRect2(rightInvBounds, 0, 0, 0, 100);

      if (leftInv != null)
      {
        for (int y = 0; y < leftInv.RowCount; y++)
        {
          for (int x = 0; x < 5; x++)
          {
            var it = leftInv.Items.Where(a => a.Y == y && a.X == x).FirstOrDefault();
            RenderItem(x, y, leftInvBounds, it);
          }
        }
      }

      if (rightInv != null)
      {
        for (int y = 0; y < rightInv.RowCount; y++)
        {
          for (int x = 0; x < 5; x++)
          {
            var it = rightInv.Items.Where(a => a.Y == y && a.X == x).FirstOrDefault();
            RenderItem(x, y, rightInvBounds, it);
          }
        }
      }

      RenderDragged(); //Kreslim na sebe, takze to s cim hybu budu malovat uplne nahoru
    }

    private void RenderItem(int x, int y, RectBounds bounds, InventoryItemBag it)
    {
      if (it == null)
        it = new InventoryItemBag { Name = "Empty", X = x, Y = y, Color = new int[] { 255, 255, 255, 30 }, Texture = "empty_slot" };

      if (!it.IsDragged)
        DrawUtils.DrawSprite2("inventory_textures", it.Texture, bounds.X1 + x * cellWidth, bounds.Y1 + y * cellHeight, bounds.X1 + (x + 1) * cellWidth, bounds.Y1 + (y + 1) * cellHeight, 0f, it.Color[0], it.Color[1], it.Color[2], it.Color[3]);
    }

    private void RenderDragged()
    {
      if (dragData.SrcItem == null)
        return;

      var it = dragData.SrcItem;

      DrawSprite("inventory_textures", it.Texture, (float)cursorData.xRelative, (float)cursorData.yRelative, (float)cellWidth, (float)cellHeight, 0f, it.Color[0], it.Color[1], it.Color[2], it.Color[3]);
    }
    #endregion

    #region private classes
    private class CursorData
    {
      public InventoryBag InvData { get; set; }
      public InventoryItemBag HoverItem { get; set; }
      public string InvType { get; set; }
      public int XGrid { get; set; }
      public int YGrid { get; set; }
      public double xRelative { get; set; }
      public double yRelative { get; set; }

      public bool IsHoverOnEmptySlot { get {
          if (InvData != null && HoverItem == null)
          {
            return YGrid < InvData.RowCount;
          }

          return false;
        } }
    }

    private class DragAndDropData
    {
      public InventoryBag SrcInv { get; set; }
      public InventoryItemBag SrcItem { get; set; }
      public InventoryBag TargetInv { get; set; }
      public int targetX { get; set; }
      public int targetY { get; set; }

      public void Clear()
      {
        SrcInv = null;
        SrcItem = null;
        TargetInv = null;
      }
    }
    #endregion
  }
}
