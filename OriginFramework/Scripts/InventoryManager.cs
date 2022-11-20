using CitizenFX.Core;
using CitizenFX.Core.UI;
using Newtonsoft.Json;
using OriginFramework.Helpers;
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
    private static int ScaleForm { get; set; } = -1;
    public bool IsWaiting { get; private set; } = false;
    public bool IsWaitingInput { get; set; } = false;
    public bool IsInventoryOpen { get; private set; } = false;
    public bool IsDrawingTooltip { get { return dragData?.SrcItem == null && cursorData?.HoverItem != null; } }
    private double invSizeOverHeight = 0.5d;
    private double invBorderOverHeight = 0.00d;
    private float itemCountScale = 0.4f;
    private float itemCountYOffset { get { return itemCountScale / 28f; } }
    private int gridXCount = 5;
    private int gridYCount = 5;
    private double gridCenterSpace = 0.5d; //modifier pro cell width
    private double invCenterWidth = 0.55d;
    private int screen_width = 1;
    private int screen_height = 1;
    double cellWidth = 0d;
    double cellHeight = 0d;
    RectBounds leftInvBounds = null;
    RectBounds leftInv2Bounds = null;
    RectBounds rightInvBounds = null;
    RectBounds backgroundBounds = null;
    CursorData cursorData = new CursorData();
    DragAndDropData dragData = new DragAndDropData();
    InventoryTooltip tooltipData = new InventoryTooltip();

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
      if (CharacterCaretaker.LoggedCharacter == null)
      {
        IsInventoryOpen = false;
        return;
      }

      if (Game.IsControlJustPressed(0, Control.ReplayStartStopRecordingSecondary)) //F2
      {
        IsInventoryOpen = !IsInventoryOpen;

        if (IsInventoryOpen)
        {
          IsWaiting = true;
          TriggerServerEvent("ofw_inventory:ReloadInventory", null);
        }
      }

      if (!IsInventoryOpen)
        return;

      if (IsWaitingInput)
      {
        Render(); //jen vykreslim inv ale nic kolem neupdatuju
        return;
      }

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

      if (IsWaitingInput) //Cekam na input, nechci nic dal kreslit
        return;

      HandleCursorSprites();

      ShowCursorThisFrame();
      RenderInstructionalButtons();

      if (IsDrawingTooltip)
        tooltipData.SetAndComputeData((float)(cursorData.xRelative + cellWidth/3), (float)cursorData.yRelative, cursorData.HoverItem);

      Render();

      if (IsDrawingTooltip)
        tooltipData.Render();
    }

    #region event handlers
    [EventHandler("ofw_inventory:InventoryLoaded")]
    private void InventoryLoaded(string leftInventory, string rightInventory)
    {
      if (string.IsNullOrEmpty(leftInventory))
      {
        TheBugger.DebugLog("INVENTORY - InventoryUpdated: invalid inventory data");
        IsInventoryOpen = false;
        IsWaiting = false;
        return;
      }

      var leftInv = JsonConvert.DeserializeObject<InventoryBag>(leftInventory);
      LeftInv = leftInv;
      SkinManager.UpdateSkinFromInv(leftInv.Items);

      if (!string.IsNullOrEmpty(rightInventory))
      {
        var rightInv = JsonConvert.DeserializeObject<InventoryBag>(leftInventory);
        LeftInv = rightInv;
      }

      IsWaiting = false;
    }

    [EventHandler("ofw_inventory:InventoryNotUpdated")]
    private void InventoryNotUpdated(string reason)
    {
      Notify.Error(reason ?? "INV: Akce se nezdařila");
      if (LeftInv?.Items != null)
      {
        foreach (var invItem in LeftInv.Items.Where(it => it.IsWaitingActionResult).ToList())
        {
          invItem.IsWaitingActionResult = false;
        }
      }

      if (RightInv?.Items != null)
      {
        foreach (var invItem in RightInv.Items.Where(it => it.IsWaitingActionResult).ToList())
        {
          invItem.IsWaitingActionResult = false;
        }
      }
    }

    [EventHandler("ofw_inventory:InventoryUpdated")]
    private void InventoryUpdated(string place1, string place2)
    {
      if (IsInventoryOpen)
      {
        if ((place1 != null && (LeftInv?.Place == place1 || RightInv?.Place == place1)) ||
            (place2 != null && (LeftInv?.Place == place2 || RightInv?.Place == place2)))
        {
          IsWaiting = true;
          TriggerServerEvent("ofw_inventory:ReloadInventory", null);
        }
      }
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
        X1 = (invCenterWidth - invWidth) - (cellWidth * (gridCenterSpace / 2)),
        Y1 = 0.5d - (invHeight / 2),
        X2 = invCenterWidth - (cellWidth * (gridCenterSpace / 2)),
        Y2 = 0.5d + (invHeight / 2)
      };

      leftInv2Bounds = new RectBounds
      {
        X1 = leftInvBounds.X1 - (cellWidth * 2) - cellWidth * gridCenterSpace,
        Y1 = 0.5d - (invHeight / 2),
        X2 = leftInvBounds.X1 - cellWidth * gridCenterSpace,
        Y2 = 0.5d + (invHeight / 2)
      };

      rightInvBounds = new RectBounds
      {
        X1 = invCenterWidth + cellWidth * (gridCenterSpace / 2),
        Y1 = 0.5d - (invHeight / 2),
        X2 = invCenterWidth + invWidth + cellWidth * (gridCenterSpace / 2),
        Y2 = 0.5d + (invHeight / 2)
      };

      backgroundBounds = new RectBounds
      {
        X1 = leftInvBounds.X1 - (invBorderOverHeight / aspectFactor),
        Y1 = (leftInvBounds.Y1 - invBorderOverHeight),
        X2 = rightInvBounds.X2 + (invBorderOverHeight / aspectFactor),
        Y2 = rightInvBounds.Y2 + invBorderOverHeight
      };
    }

    private void ComputeCursorData(int x, int y)
    {
      double xRelative = (double)x / (double)screen_width;
      double yRelative = (double)y / (double)screen_height;

      //TheBugger.DebugLog($"X:{x}({xRelative}) Y:{y}({yRelative})");

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
      else if (DrawUtils.IsInBounds(leftInv2Bounds, xRelative, yRelative) && DrawUtils.TryGetBoundsGridPosition(leftInv2Bounds, xRelative, yRelative, cellWidth, cellHeight, out xGrid, out yGrid))
      {
        cursorData.InvData = LeftInv;
        cursorData.HoverItem = LeftInv?.Items?.Where(it => it.X == -1 && it.Y == yGrid * 2 + xGrid).FirstOrDefault();
        cursorData.InvType = "left2";
        cursorData.YGrid = yGrid * 2 + xGrid;
        cursorData.XGrid = -1;
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

    private bool IsShiftKeyPressed()
    {
      return (Game.IsControlPressed(0, Control.Sprint) || Game.IsDisabledControlPressed(0, Control.Sprint));
    }

    private async void RenderInstructionalButtons()
    {
      ScaleForm = RequestScaleformMovie("INSTRUCTIONAL_BUTTONS");
      while (!HasScaleformMovieLoaded(ScaleForm))
      {
        return;
      }

      BeginScaleformMovieMethod(ScaleForm, "CLEAR_ALL");
      EndScaleformMovieMethod();

      BeginScaleformMovieMethod(ScaleForm, "SET_DATA_SLOT");
      ScaleformMovieMethodAddParamInt(0);
      PushScaleformMovieMethodParameterString("~INPUT_SPRINT~");
      PushScaleformMovieMethodParameterString($"{FontsManager.FiraSansString}Rozdeleni stacku");
      EndScaleformMovieMethod();

      BeginScaleformMovieMethod(ScaleForm, "DRAW_INSTRUCTIONAL_BUTTONS");
      ScaleformMovieMethodAddParamInt(0);
      EndScaleformMovieMethod();

      DrawScaleformMovieFullscreen(ScaleForm, 255, 255, 255, 255, 0);
    }

    private async void HandleDragAndDrop()
    {
      if (IsWaiting)
      {
        if (dragData?.SrcItem?.IsDragged == true)
          dragData.SrcItem.IsDragged = false;
        dragData.Clear();

        return;
      }

      if (Game.IsDisabledControlJustPressed(0, Control.Attack) && dragData.SrcItem == null && cursorData.HoverItem != null && cursorData.HoverItem.IsWaitingActionResult == false)
      {
        dragData.SrcItem = cursorData.HoverItem;
        dragData.SrcInv = cursorData.InvData;
        dragData.SrcItem.IsDragged = true;
      }

      if (Game.IsDisabledControlJustReleased(0, Control.Attack) && dragData.SrcItem != null)
      {
        if (cursorData.InvData != null && 
            (cursorData.HoverItem == null || (cursorData.HoverItem.ItemId == dragData.SrcItem.ItemId && IsShiftKeyPressed() == false)) && 
            cursorData.IsHoverOnValidSlot && 
            dragData.SrcItem?.Id != cursorData.HoverItem?.Id)
        {
          dragData.TargetInv = cursorData.InvData;
          dragData.targetX = cursorData.XGrid;
          dragData.targetY = cursorData.YGrid;

          if (IsShiftKeyPressed())
          {
            dragData.SrcItem.IsWaitingActionResult = true;
            dragData.SrcItem.IsDragged = false;
            IsWaitingInput = true;
            SetMouseCursorVisibleInMenus(false);
            string input = await TextUtils.GetUserInput($"Kolik chcete přesunout z {dragData.SrcItem.Count}?", null, 7);
            int splitCount;
            if (Int32.TryParse(input, out splitCount))
            {
              TriggerServerEvent("ofw_inventory:Operation_Split", dragData.SrcItem.Id, dragData.TargetInv.Place, dragData.targetX, dragData.targetY, splitCount);
            }
            else
            {
              dragData.SrcItem.IsWaitingActionResult = false;
              Notify.Error("Neplatné číslo");
            }
            IsWaitingInput = false;
            SetMouseCursorVisibleInMenus(true);

            dragData.Clear();
          }
          else
          {
            TriggerServerEvent("ofw_inventory:Operation_MoveOrMerge", dragData.SrcItem.Id, dragData.TargetInv.Place, dragData.targetX, dragData.targetY);

            dragData.SrcItem.IsWaitingActionResult = true;
            dragData.SrcItem.IsDragged = false;
            dragData.Clear();
          }
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
      else if (cursorData.InvData != null && 
               (cursorData.HoverItem == null || (cursorData.HoverItem.ItemId == dragData.SrcItem.ItemId && IsShiftKeyPressed() == false)) &&
               cursorData.IsHoverOnValidSlot && 
               dragData.SrcItem?.Id != cursorData.HoverItem?.Id)
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

      DrawRect2(backgroundBounds, IsWaiting ? 255 : 0, 0, 0, 60);
      DrawRect2(leftInvBounds, 0, 0, 0, 120);
      DrawRect2(rightInvBounds, 0, 0, 0, 120);
      DrawRect2(leftInv2Bounds, 0, 0, 0, 180);

      TextUtils.DrawTextOnScreen("Tvůj inventář", (float)leftInvBounds.X1, (float)leftInvBounds.Y1 - 0.5f / TextUtils.TxtHConst, 0.5f, Alignment.Left);
      TextUtils.DrawTextOnScreen(InventoryBag.GetPlaceName(RightInv?.Place), (float)rightInvBounds.X2, (float)rightInvBounds.Y1 - 0.5f / TextUtils.TxtHConst, 0.5f, Alignment.Right);

      if (LeftInv != null)
      {
        for (int y = 0; y < LeftInv.RowCount; y++)
        {
          for (int x = 0; x < 5; x++)
          {
            var it = LeftInv.Items.Where(a => a.Y == y && a.X == x).FirstOrDefault();
            RenderItem(x, y, leftInvBounds, it, false);
          }
        }

        for (int y = 0; y < 10; y++)
        {
          var it = LeftInv.Items.Where(a => a.Y == y && a.X == -1).FirstOrDefault();
          RenderItem(y % 2, y / 2, leftInv2Bounds, it, true);
        }
      }

      if (RightInv != null)
      {
        for (int y = 0; y < RightInv.RowCount; y++)
        {
          for (int x = 0; x < 5; x++)
          {
            var it = RightInv.Items.Where(a => a.Y == y && a.X == x).FirstOrDefault();
            RenderItem(x, y, rightInvBounds, it, false);
          }
        }
      }

      //TextUtils.DrawTextOnScreen

      RenderDragged(); //Kreslim na sebe, takze to s cim hybu budu malovat uplne nahoru
    }

    private void RenderItem(int x, int y, RectBounds bounds, InventoryItemBag it, bool isExtraSlots)
    {
      if (it == null)
      {
        DrawUtils.DrawSprite2("inventory_textures", isExtraSlots ? $"empty_slot_{y * 2 + x}" : "empty_slot", bounds.X1 + x * cellWidth, bounds.Y1 + y * cellHeight, bounds.X1 + (x + 1) * cellWidth, bounds.Y1 + (y + 1) * cellHeight, 0f, 255, 255, 255, 100);
        return;
      }

      if (!it.IsDragged && !it.IsWaitingActionResult)
      {
        DrawUtils.DrawSprite2("inventory_textures", ItemsDefinitions.Items[it.ItemId].Texture, bounds.X1 + x * cellWidth, bounds.Y1 + y * cellHeight, bounds.X1 + (x + 1) * cellWidth, bounds.Y1 + (y + 1) * cellHeight, 0f, ItemsDefinitions.Items[it.ItemId]?.Color?.R ?? 255, ItemsDefinitions.Items[it.ItemId]?.Color?.G ?? 255, ItemsDefinitions.Items[it.ItemId]?.Color?.B ?? 255, 255);
        if (it.Count > 1)
          RenderItemCount(x, y, it, bounds);
      }
    }

    private void RenderDragged()
    {
      if (dragData.SrcItem == null || IsWaitingInput == true)
        return;

      var it = dragData.SrcItem;

      DrawSprite("inventory_textures", ItemsDefinitions.Items[it.ItemId].Texture, (float)cursorData.xRelative, (float)cursorData.yRelative, (float)cellWidth, (float)cellHeight, 0f, ItemsDefinitions.Items[it.ItemId]?.Color?.R ?? 255, ItemsDefinitions.Items[it.ItemId]?.Color?.G ?? 255, ItemsDefinitions.Items[it.ItemId]?.Color?.B ?? 255, 255);
      if (it.Count > 1)
        RenderDraggedItemCount(it);
    }

    private void RenderItemCount(int x, int y, InventoryItemBag it, RectBounds bounds)
    {
      double x1, y1, x2, y2;
      x1 = bounds.X1 + x * cellWidth + cellWidth / 10;
      y1 = bounds.Y1 + (y + 1) * cellHeight - itemCountYOffset - cellHeight / 10;
      x2 = bounds.X1 + (x + 1) * cellWidth - cellWidth / 10;
      y2 = bounds.Y1 + (y + 1) * cellHeight - cellHeight / 10;

      if (IsDrawingTooltip && tooltipData.Bounds.IntersectsWith(x1, y1, x2, y2))
        return;

      SetTextFont(4);
      SetTextScale(itemCountScale, itemCountScale);
      SetTextColour(255, 255, 255, 255);
      SetTextEntry("STRING");
      AddTextComponentString(FontsManager.FiraSansString + it.Count);
      SetTextWrap((float)x1, (float)x2);
      SetTextJustification((int)CitizenFX.Core.UI.Alignment.Right);
      DrawText((float)x1, (float)y1);
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

      public bool IsHoverOnValidSlot { get {
          if (InvData != null)
          {
            if (XGrid < 0 && YGrid >= 0 && YGrid < 10)
              return true;

            if (InvData.RowCount < 0 && YGrid >= 0)
              return true;

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

    private class InventoryTooltip
    {
      private static float textScale = 0.3f;
      private static float tooltipWidth = 0.3f / Screen.AspectRatio;
      private static float vertical_offset = (tooltipWidth / 30) * Screen.AspectRatio;
      public RectBounds Bounds { get; private set; } = new RectBounds();
      private string _text = null;
      private string _text2 = null;

      public void SetAndComputeData(float x, float y, InventoryItemBag item)
      {
        if (item == null)
          return;

        var itDef = ItemsDefinitions.Items[item.ItemId];
        _text2 = String.Empty;
        int linesCount = 1;

        _text = $"~h~{itDef.Name ?? "Nepojmenovaný předmět"}~h~";

        if (itDef.Color != null)
        {
          _text2 += $"Barva: {itDef.Color.Label}~n~";
          linesCount++;
        }

        if (itDef.MaleSkin != null || itDef.FemaleSkin != null)
        {
          if (itDef.MaleSkin != null && itDef.FemaleSkin != null)
            _text2 += "Střih: Univerzální~n~";
          else if (itDef.MaleSkin != null)
            _text2 += "Střih: Pánský~n~";
          else if (itDef.FemaleSkin != null)
            _text2 += "Střih: Dámský~n~";

          linesCount++;
        }

        Bounds.X1 = x;
        Bounds.Y1 = y - vertical_offset;
        Bounds.X2 = x + tooltipWidth;
        Bounds.Y2 = y + linesCount * (textScale / TextUtils.TxtHConst) + vertical_offset;
      }

      public async void Render()
      {
        DrawUtils.DrawRect2(Bounds, 50, 50, 50, 255);
        TextUtils.DrawTextOnScreen(_text, (float)Bounds.X1 + tooltipWidth/2, (float)Bounds.Y1 + vertical_offset, textScale, Alignment.Center);
        if (!string.IsNullOrEmpty(_text2))
          TextUtils.DrawTextOnScreen(_text2, (float)Bounds.X1 + tooltipWidth/30, (float)Bounds.Y1 + vertical_offset + textScale / TextUtils.TxtHConst, textScale);
      }
    }
    #endregion
  }
}
