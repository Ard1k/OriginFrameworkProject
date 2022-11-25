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
    private double invSizeOverHeight = 0.45d;
    private double invBorderOverHeight = 0.00d;
    private float itemCountScale = 0.25f;
    private float iconDrawScale = 0.75f;
    private float scrollbarWidthToCellWidth = 0.2f;
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
    InventoryBag LeftInv = null;
    InventoryBag RightInv = null;
    List<Vector3> groundMarkers = null;
    bool groundMarkersLocked = false;
    float groundMarkersDistanceDraw = 10f;

    int rInvScrollOffset = 0;
    int rInvScrollMaxOffset = 0;
    float rScrollBarHeight = 1.0f;

    int lInvScrollOffset = 0;
    int lInvScrollMaxOffset = 0;
    float lScrollBarHeight = 1.0f;

    int lastScrollAction = 0;

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
      Tick += DrawGroundMarkers;

      TriggerServerEvent("ofw_inventory:GroundMarkersReqUpdate");

      InternalDependencyManager.Started(eScriptArea.InventoryManager);
    }

    private async Task DrawGroundMarkers()
    {
      if (groundMarkers == null || groundMarkers.Count <= 0)
      {
        await Delay(500);
        return;
      }

      foreach (var m in groundMarkers)
      {
        if (Math.Abs(Game.PlayerPed.Position.X - m.X) < groundMarkersDistanceDraw && Math.Abs(Game.PlayerPed.Position.Y - m.Y) < groundMarkersDistanceDraw)
        {
          Vector3 vecNormal = new Vector3();
          float gndZ = 0f;
          GetGroundZCoordWithOffsets(m.X, m.Y, Game.PlayerPed.Position.Z, ref gndZ, ref vecNormal);
          DrawMarker(43, m.X, m.Y, gndZ + 0.2f, 0.0f, 0.0f, 0.0f, 0.0f, 180.0f, 0.0f, 1.5f, 1.5f, 1.9f, 0, 140, 255, 60, false, false, 2, false, null, null, false);
        }
      }
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

          int vehFront = Vehicles.GetVehicleInFront();
          int pedVehicle = GetVehiclePedIsIn(Game.PlayerPed.Handle, false);
          bool isCarLocked = GetVehicleDoorsLockedForPlayer(vehFront, Game.PlayerPed.Handle);
          bool isNearTrunk = Vehicles.IsPedCloseToTrunk(vehFront);

          if (vehFront > 0 && !isCarLocked)
          {
            if (isNearTrunk)
            {
              //otevrit kufr
              var lp = GetVehicleNumberPlateText(vehFront);
              var vehClass = GetVehicleClass(vehFront);

              TriggerServerEvent("ofw_inventory:ReloadInventory", $"trunk_{lp}_{vehClass}");
            }
            else
            {
              TriggerServerEvent("ofw_inventory:ReloadInventory", null);
            }
          }
          else if (pedVehicle > 0)
          {
            //otevrit kaslik
            var lp = GetVehicleNumberPlateText(pedVehicle);
            TriggerServerEvent("ofw_inventory:ReloadInventory", $"glovebox_{lp}");
          }
          else
          {
            //otevrit svet
            TriggerServerEvent("ofw_inventory:ReloadInventory", $"world_{(int)Math.Floor(Game.PlayerPed.Position.X/2)}_{(int)Math.Floor(Game.PlayerPed.Position.Y/2)}");
          }
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

      Game.DisableControlThisFrame(0, Control.WeaponWheelUpDown);
      Game.DisableControlThisFrame(0, Control.WeaponWheelNext);
      Game.DisableControlThisFrame(0, Control.WeaponWheelPrev);
      Game.DisableControlThisFrame(0, Control.SelectNextWeapon);
      Game.DisableControlThisFrame(0, Control.SelectPrevWeapon);

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

      ComputeScrollOffsets();
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
      LeftInv = JsonConvert.DeserializeObject<InventoryBag>(leftInventory);
      RightInv = JsonConvert.DeserializeObject<InventoryBag>(rightInventory);

      if (LeftInv == null)
      {
        TheBugger.DebugLog("INVENTORY - InventoryUpdated: invalid inventory data");
        IsInventoryOpen = false;
        IsWaiting = false;
        return;
      }

      SkinManager.UpdateSkinFromInv(LeftInv.Items);
      
      RecomputeMaxScrollOffsets();
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
          TriggerServerEvent("ofw_inventory:ReloadInventory", RightInv.Place ?? null);
        }
      }
    }

    [EventHandler("ofw_inventory:GroundMarkersUpdated")]
    private void GroundMarkersUpdated(string data)
    {
      if (data == null)
      {
        groundMarkers = null;
        return;
      }

      groundMarkersLocked = true;
      groundMarkers = JsonConvert.DeserializeObject<List<Vector3>>(data);
      groundMarkersLocked = false;
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
        cursorData.HoverItem = LeftInv?.Items?.Where(it => it.X == xGrid && it.Y == yGrid + LeftInv.ScrollOffset).FirstOrDefault();
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
        cursorData.HoverItem = RightInv?.Items?.Where(it => it.X == xGrid && it.Y == yGrid + RightInv.ScrollOffset).FirstOrDefault();
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

    private void ComputeScrollOffsets()
    {
      var time = GetGameTimer();

      if (lInvScrollOffset > lInvScrollMaxOffset)
        lInvScrollOffset = lInvScrollMaxOffset;

      if (rInvScrollOffset > rInvScrollMaxOffset)
        rInvScrollOffset = rInvScrollMaxOffset;

      if (lastScrollAction - GetGameTimer() <= -100)
      {
        if (Game.IsDisabledControlJustPressed(0, Control.SelectNextWeapon))
        {
          if (cursorData.InvType == "left" && lInvScrollOffset < lInvScrollMaxOffset)
          {
            lInvScrollOffset++;
            lastScrollAction = time;
          }
          else if (cursorData.InvType == "right" && rInvScrollOffset < rInvScrollMaxOffset)
          {
            rInvScrollOffset++;
            lastScrollAction = time;
          }
        }
        else if (Game.IsDisabledControlJustPressed(0, Control.SelectPrevWeapon))
        {
          if (cursorData.InvType == "left" && lInvScrollOffset > 0)
          {
            lInvScrollOffset--;
            lastScrollAction = time;
          }
          else if (cursorData.InvType == "right" && rInvScrollOffset > 0)
          {
            rInvScrollOffset--;
            lastScrollAction = time;
          }
        }
      }

      if (LeftInv != null)
        LeftInv.ScrollOffset = lInvScrollOffset;
      if (RightInv != null)
        RightInv.ScrollOffset = rInvScrollOffset;
    }

    private void RecomputeMaxScrollOffsets()
    {
      lScrollBarHeight = 1f;
      rScrollBarHeight = 1f;

      if (LeftInv != null)
      {
        lInvScrollMaxOffset = LeftInv.RowCount - 5;
        int maxY_LItem = LeftInv.Items?.Where(it => it.X >= 0).OrderByDescending(it => it.Y).FirstOrDefault()?.Y ?? -1;
        if (lInvScrollMaxOffset < maxY_LItem - 5)
          lInvScrollMaxOffset = maxY_LItem;
        if (lInvScrollMaxOffset < 0)
          lInvScrollMaxOffset = 0;

        if (lInvScrollMaxOffset > 0)
          lScrollBarHeight = 5f / (float)(5f + lInvScrollMaxOffset);
      }

      if (RightInv != null)
      {
        rInvScrollMaxOffset = RightInv.RowCount - 5;
        int maxY_RItem = RightInv.Items?.Where(it => it.X >= 0).OrderByDescending(it => it.Y).FirstOrDefault()?.Y ?? -1;
        if (rInvScrollMaxOffset < maxY_RItem - 5)
          rInvScrollMaxOffset = maxY_RItem;
        if (rInvScrollMaxOffset < 0)
          rInvScrollMaxOffset = 0;

        if (rInvScrollMaxOffset > 0)
          rScrollBarHeight = 5f / (float)(5f + rInvScrollMaxOffset);
      }
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
          dragData.targetY = cursorData.YGrid + (cursorData.XGrid >= 0 ? cursorData.InvData.ScrollOffset : 0);

          if (IsShiftKeyPressed() && dragData.SrcItem.Count > 1)
          {
            dragData.SrcItem.IsWaitingActionResult = true;
            dragData.SrcItem.IsDragged = false;
            IsWaitingInput = true;
            SetMouseCursorVisibleInMenus(false);
            string input = await TextUtils.GetUserInput($"Kolik chcete přesunout z {dragData.SrcItem.Count}?", null, 7);
            int splitCount;
            if (Int32.TryParse(input, out splitCount))
            {
              TriggerServerEvent("ofw_inventory:Operation_Split", dragData.SrcItem.Id, dragData.SrcItem.Place , dragData.TargetInv.Place, dragData.targetX, dragData.targetY, splitCount);
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
            TriggerServerEvent("ofw_inventory:Operation_MoveOrMerge", dragData.SrcItem.Id, dragData.SrcItem.Place, dragData.TargetInv.Place, dragData.targetX, dragData.targetY);

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
      DrawRect2(leftInvBounds, 100, 100, 100, 180);
      DrawRect2(rightInvBounds, 100, 100, 100, 180);
      DrawRect2(leftInv2Bounds, 100, 100, 100, 180);

      //left scrollbar
      DrawRect2(leftInvBounds.X1 - cellWidth * scrollbarWidthToCellWidth,
                leftInvBounds.Y1 + (leftInvBounds.Y2 - leftInvBounds.Y1) * (lInvScrollMaxOffset > 0 ? lInvScrollOffset * ((1f - lScrollBarHeight) / lInvScrollMaxOffset) : 0),
                leftInvBounds.X1,
                leftInvBounds.Y2 - (leftInvBounds.Y2 - leftInvBounds.Y1) * (lInvScrollMaxOffset > 0 ? (lInvScrollMaxOffset - lInvScrollOffset) * ((1f - lScrollBarHeight) / lInvScrollMaxOffset) : 0),
                255, 255, 255, 200);

      //right scrollbar
      DrawRect2(rightInvBounds.X2,
                rightInvBounds.Y1 + (rightInvBounds.Y2 - rightInvBounds.Y1) * (rInvScrollMaxOffset > 0 ? rInvScrollOffset * ((1f - rScrollBarHeight) / rInvScrollMaxOffset) : 0),
                rightInvBounds.X2 + cellWidth * scrollbarWidthToCellWidth,
                rightInvBounds.Y2 - (rightInvBounds.Y2 - rightInvBounds.Y1) * (rInvScrollMaxOffset > 0 ? (rInvScrollMaxOffset - rInvScrollOffset) * ((1f - rScrollBarHeight) / rInvScrollMaxOffset) : 0),
                255, 255, 255, 200);

      TextUtils.DrawTextOnScreen("Tvůj inventář", (float)leftInvBounds.X1, (float)leftInvBounds.Y1 - 0.5f / TextUtils.TxtHConst, 0.5f, Alignment.Left);
      TextUtils.DrawTextOnScreen(InventoryBag.GetPlaceName(RightInv?.Place), (float)rightInvBounds.X2, (float)rightInvBounds.Y1 - 0.5f / TextUtils.TxtHConst, 0.5f, Alignment.Right);

      if (LeftInv != null)
      {
        for (int y = 0; y + LeftInv.ScrollOffset < LeftInv.RowCount && y < 5; y++)
        {
          for (int x = 0; x < 5; x++)
          {
            var it = LeftInv.Items.Where(a => a.Y == y + LeftInv.ScrollOffset && a.X == x).FirstOrDefault();
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
        for (int y = 0; y + RightInv.ScrollOffset < RightInv.RowCount && y < 5; y++)
        {
          for (int x = 0; x < 5; x++)
          {
            var it = RightInv.Items.Where(a => a.Y == y + RightInv.ScrollOffset && a.X == x).FirstOrDefault();
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
        DrawUtils.DrawSprite2("inventory_textures", 
          ItemsDefinitions.Items[it.ItemId].Texture, 
          bounds.X1 + x * cellWidth + (cellWidth * ((1f - iconDrawScale) / 2)), 
          bounds.Y1 + y * cellHeight + (cellHeight * ((1f - iconDrawScale) / 2)), 
          bounds.X1 + (x + 1) * cellWidth - (cellWidth * ((1f - iconDrawScale) / 2)), 
          bounds.Y1 + (y + 1) * cellHeight - (cellHeight * ((1f - iconDrawScale) / 2)), 
          0f, ItemsDefinitions.Items[it.ItemId]?.Color?.R ?? 255, ItemsDefinitions.Items[it.ItemId]?.Color?.G ?? 255, ItemsDefinitions.Items[it.ItemId]?.Color?.B ?? 255, 255);
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

            return YGrid + InvData.ScrollOffset < InvData.RowCount;
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

        if (itDef?.Color?.Label != null)
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
