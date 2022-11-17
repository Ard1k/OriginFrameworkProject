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
    public bool IsWaiting { get; private set; } = false;
    public bool IsInventoryOpen { get; private set; } = false;
    private double invSizeOverHeight = 0.5d;
    private double invBorderOverHeight = 0.01d;
    private float itemCountScale = 0.4f;
    private float itemCountYOffset { get { return itemCountScale / 28f; } }
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
    InventoryBag LeftInv = null;
    InventoryBag RightInv = null;
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

        if (IsInventoryOpen)
        {
          IsWaiting = true;
          TriggerServerEvent("ofw_inventory:GetMyCharacterInventory");
        }
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

      Game.DisableControlThisFrame(0, Control.FrontendRright);
      Game.DisableControlThisFrame(0, Control.FrontendPauseAlternate);

      if (Game.IsDisabledControlJustPressed(0, Control.FrontendRright) || Game.IsDisabledControlJustPressed(0, Control.FrontendPauseAlternate))
      {
        IsInventoryOpen = false;
        return;
      }

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

    #region event handlers
    [EventHandler("ofw_inventory:InventoryUpdated")]
    private void InventoryUpdated(string inventory)
    {
      if (string.IsNullOrEmpty(inventory))
      {
        TheBugger.DebugLog("INVENTORY - InventoryUpdated: invalid inventory data");
        IsInventoryOpen = false;
        IsWaiting = false;
        return;
      }

      var inv = JsonConvert.DeserializeObject<InventoryBag>(inventory);
      LeftInv = inv;
      IsWaiting = false;
    }
    #endregion

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
        cursorData.InvData = LeftInv;
        cursorData.HoverItem = LeftInv?.Items?.Where(it => it.X == xGrid && it.Y == yGrid).FirstOrDefault();
        cursorData.InvType = "left";
        cursorData.XGrid = xGrid;
        cursorData.YGrid = yGrid;
        return;
      }
      else if (DrawUtils.IsInBounds(rightInvBounds, xRelative, yRelative) && DrawUtils.TryGetBoundsGridPosition(rightInvBounds, xRelative, yRelative, cellWidth, cellHeight, out xGrid, out yGrid))
      {
        cursorData.InvData = RightInv;
        cursorData.HoverItem = RightInv?.Items?.Where(it => it.X == xGrid && it.Y == yGrid).FirstOrDefault();
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

      DrawRect2(backgroundBounds, IsWaiting ? 255 : 0, 0, 0, 30);
      DrawRect2(leftInvBounds, 0, 0, 0, 100);
      DrawRect2(rightInvBounds, 0, 0, 0, 100);

      if (LeftInv != null)
      {
        for (int y = 0; y < LeftInv.RowCount; y++)
        {
          for (int x = 0; x < 5; x++)
          {
            var it = LeftInv.Items.Where(a => a.Y == y && a.X == x).FirstOrDefault();
            RenderItem(x, y, leftInvBounds, it);
          }
        }
      }

      if (RightInv != null)
      {
        for (int y = 0; y < RightInv.RowCount; y++)
        {
          for (int x = 0; x < 5; x++)
          {
            var it = RightInv.Items.Where(a => a.Y == y && a.X == x).FirstOrDefault();
            RenderItem(x, y, rightInvBounds, it);
          }
        }
      }

      RenderDragged(); //Kreslim na sebe, takze to s cim hybu budu malovat uplne nahoru
    }

    private void RenderItem(int x, int y, RectBounds bounds, InventoryItemBag it)
    {
      if (it == null)
      {
        DrawUtils.DrawSprite2("inventory_textures", "empty_slot", bounds.X1 + x * cellWidth, bounds.Y1 + y * cellHeight, bounds.X1 + (x + 1) * cellWidth, bounds.Y1 + (y + 1) * cellHeight, 0f, 255, 255, 255, 30);
        return;
      }

      if (!it.IsDragged)
      {
        DrawUtils.DrawSprite2("inventory_textures", ItemsDefinitions.Items[it.ItemId].Texture, bounds.X1 + x * cellWidth, bounds.Y1 + y * cellHeight, bounds.X1 + (x + 1) * cellWidth, bounds.Y1 + (y + 1) * cellHeight, 0f, 255, 255, 255, 255);
        RenderItemCount(x, y, it, bounds);
      }
    }

    private void RenderDragged()
    {
      if (dragData.SrcItem == null)
        return;

      var it = dragData.SrcItem;

      DrawSprite("inventory_textures", ItemsDefinitions.Items[it.ItemId].Texture, (float)cursorData.xRelative, (float)cursorData.yRelative, (float)cellWidth, (float)cellHeight, 0f, 255, 255, 255, 255);
      RenderDraggedItemCount(it);
    }

    private void RenderItemCount(int x, int y, InventoryItemBag it, RectBounds bounds)
    {
      SetTextFont(4);
      SetTextScale(itemCountScale, itemCountScale);
      SetTextColour(255, 255, 255, 255);
      SetTextEntry("STRING");
      AddTextComponentString(FontsManager.FiraSansString + it.Count);
      SetTextWrap((float)(bounds.X1 + x * cellWidth + cellWidth/10), (float)(bounds.X1 + (x + 1) * cellWidth - cellWidth/10));
      SetTextJustification((int)CitizenFX.Core.UI.Alignment.Right);
      DrawText((float)(bounds.X1 + x * cellWidth), (float)(bounds.Y1 + (y + 1) * cellHeight - itemCountYOffset - cellHeight / 10));
    }

    private void RenderDraggedItemCount(InventoryItemBag it)
    {
      SetTextFont(4);
      SetTextScale(itemCountScale, itemCountScale);
      SetTextColour(255, 255, 255, 255);
      SetTextEntry("STRING");
      AddTextComponentString(FontsManager.FiraSansString + it.Count);
      SetTextWrap((float)(cursorData.xRelative - cellWidth / 2f + cellWidth / 10), (float)(cursorData.xRelative + cellWidth / 2f - cellWidth / 10));
      SetTextJustification((int)CitizenFX.Core.UI.Alignment.Right);
      DrawText((float)(cursorData.xRelative - cellWidth / 2f), (float)(cursorData.yRelative + cellHeight / 2f - itemCountYOffset - cellHeight / 10));
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
