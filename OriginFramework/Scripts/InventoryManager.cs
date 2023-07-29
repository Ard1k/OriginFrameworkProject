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
    public static InventoryBag PlayerInventoryCache { get; set; }
    public static InventoryBag ForkliftInventoryCache { get; set; }
    private static int ScaleForm { get; set; } = -1;
    public bool IsWaiting { get; private set; } = false;
    public bool IsWaitingInput { get; set; } = false;
    public static bool IsInventoryOpen { get; private set; } = false;
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
    InventoryBounds leftInvBounds = null;
    InventoryBounds leftInv2Bounds = null;
    InventoryBounds leftInv3Bounds = null;
    InventoryBounds rightInvBounds = null;
    RectBounds backgroundBounds = null;
    CursorData cursorData = new CursorData();
    DragAndDropData dragData = new DragAndDropData();
    InventoryTooltip tooltipData = new InventoryTooltip();
    InventoryBag LeftInv = null;
    InventoryBag RightInv = null;
    List<Vector3> groundMarkers = null;
    bool groundMarkersLocked = false;
    bool isLeftInvPlayerType = false;
    float groundMarkersDistanceDraw = 10f;

    int rInvScrollOffset = 0;
    int rInvScrollMaxOffset = 0;
    float rScrollBarHeight = 1.0f;

    int lInvScrollOffset = 0;
    int lInvScrollMaxOffset = 0;
    float lScrollBarHeight = 1.0f;

    int lastScrollAction = 0;
    int lastLastLeftClick = 0;

    public InventoryManager()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.InventoryManager, eScriptArea.CharacterCaretaker))
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
        CloseTooltip();
        return;
      }

      if (Game.IsControlJustPressed(0, Control.ReplayStartStopRecordingSecondary)) //F2
      {
        IsInventoryOpen = !IsInventoryOpen;

        if (IsInventoryOpen)
        {
          IsWaiting = true;

          WeaponManager.UnequipWeapon();

          int vehFront = Vehicles.GetVehicleInFront(null);
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
              var model = GetEntityModel(vehFront);

              TriggerServerEvent("ofw_inventory:ReloadInventory", $"trunk_{lp}_{vehClass}_{model}");
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
            if (Vehicles.IsPlayerDrivingForklift())
              TriggerServerEvent("ofw_inventory:ReloadInventory", $"fork_{lp}");
            else
              TriggerServerEvent("ofw_inventory:ReloadInventory", $"glovebox_{lp}");
          }
          else
          {
            //otevrit svet
            TriggerServerEvent("ofw_inventory:ReloadInventory", $"world_{(int)Math.Floor(Game.PlayerPed.Position.X/2)}_{(int)Math.Floor(Game.PlayerPed.Position.Y/2)}");
          }
        }
        else
          CloseTooltip();
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

      if (RightInv != null && RightInv.IsDisableMoveControls())
      {
        Game.DisableControlThisFrame(0, Control.MoveLeft);
        Game.DisableControlThisFrame(0, Control.MoveLeftOnly);
        Game.DisableControlThisFrame(0, Control.MoveRight);
        Game.DisableControlThisFrame(0, Control.MoveRightOnly);
        Game.DisableControlThisFrame(0, Control.MoveUp);
        Game.DisableControlThisFrame(0, Control.MoveUpOnly);
        Game.DisableControlThisFrame(0, Control.MoveDown);
        Game.DisableControlThisFrame(0, Control.MoveDownOnly);
        Game.DisableControlThisFrame(0, Control.MoveUpDown);
        Game.DisableControlThisFrame(0, Control.MoveLeftRight);
      }

      if (Game.IsDisabledControlJustPressed(0, Control.FrontendRright) || Game.IsDisabledControlJustPressed(0, Control.FrontendPauseAlternate))
      {
        IsInventoryOpen = false;
        CloseTooltip();
        OfwFunctions.BlockEsc(500);
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

      Render();

      //TODO tooltip - tohle je neskutecna picovina delat kazdy frame. Kdyz je to ted pres NUI, meli by se eventy posilat jen kdyz se to zmeni... ale ted mrdat :D
      if (IsDrawingTooltip)
      {
        tooltipData.SetAndComputeData((float)(cursorData.xRelative + cellWidth / 3), (float)cursorData.yRelative, cursorData.HoverItem);
        tooltipData.Render();
      }
      else
        CloseTooltip();
    }

    #region event handlers
    [EventHandler("ofw_inventory:InventoryLoaded")]
    private void InventoryLoaded(string leftInventory, string rightInventory, bool isOnlyCacheUpdate)
    {
      if (isOnlyCacheUpdate)
      {
        var invCache = JsonConvert.DeserializeObject<InventoryBag>(leftInventory);

        if (invCache.Place == $"char_{CharacterCaretaker.LoggedCharacter?.Id ?? -1}")
        {
          PlayerInventoryCache = invCache;
          SkinManager.UpdateSkinFromInv(PlayerInventoryCache.Items);
        }
        else if (invCache.Place.StartsWith("fork_"))
        {
          ForkliftInventoryCache = invCache;
        }

        return;
      }

      LeftInv = JsonConvert.DeserializeObject<InventoryBag>(leftInventory);
      RightInv = JsonConvert.DeserializeObject<InventoryBag>(rightInventory);

      if (LeftInv == null)
      {
        TheBugger.DebugLog("INVENTORY - InventoryUpdated: invalid inventory data");
        IsInventoryOpen = false;
        IsWaiting = false;
        CloseTooltip();
        return;
      }

      if (LeftInv.Place == $"char_{CharacterCaretaker.LoggedCharacter?.Id ?? -1}")
      {
        PlayerInventoryCache = JsonConvert.DeserializeObject<InventoryBag>(leftInventory);
        SkinManager.UpdateSkinFromInv(PlayerInventoryCache.Items);
      }
      else if (LeftInv.Place.StartsWith("fork_"))
      {
        ForkliftInventoryCache = JsonConvert.DeserializeObject<InventoryBag>(leftInventory);
      }

      isLeftInvPlayerType = LeftInv.Place.StartsWith("char") ? true : false;
      
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
          TriggerServerEvent("ofw_inventory:ReloadInventory", RightInv?.Place ?? null, LeftInv?.Place);
        }
      }
      else if (place1 == $"char_{CharacterCaretaker.LoggedCharacter?.Id ?? -1}" || place2 == $"char_{CharacterCaretaker.LoggedCharacter?.Id ?? -1}")
      {
        //nepamatuju si kdy se to presne vola, ale kdyz se zmeni muj inv, tak si ho reloadnu
        TriggerServerEvent("ofw_inventory:ReloadPlayerCacheInventory");
      }
      else if (Vehicles.IsPlayerDrivingForklift())
      {
        var forkliftPlate = GetVehicleNumberPlateText(Game.PlayerPed.CurrentVehicle.Handle);
        if (place1 == $"fork_{forkliftPlate}" || place2 == $"fork_{forkliftPlate}")
          TriggerServerEvent("ofw_inventory:ReloadForkliftCacheInventory", forkliftPlate);
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

    [EventHandler("ofw_inventory:WeaponUsed")]
    private async void WeaponUsed(int itemId, int ammoCount)
    {
      var definition = ItemsDefinitions.Items[itemId];
      WeaponManager.EquipWeapon(itemId, ammoCount);
      
      IsInventoryOpen = false;
      CloseTooltip();
    }
    #endregion

    #region public functions
    public static void OpenForkliftInventory(int vehFront, int fork)
    {
      IsInventoryOpen = !IsInventoryOpen;
      var lp = GetVehicleNumberPlateText(vehFront);
      var vehClass = GetVehicleClass(vehFront);
      var model = GetEntityModel(vehFront);

      var lpFork = GetVehicleNumberPlateText(fork);

      TriggerServerEvent("ofw_inventory:ReloadInventory", $"trunk_{lp}_{vehClass}_{model}", $"fork_{lpFork}");
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

      leftInvBounds = new InventoryBounds
      {
        X1 = (invCenterWidth - invWidth) - (cellWidth * (gridCenterSpace / 2)),
        Y1 = 0.5d - (invHeight / 2),
        X2 = invCenterWidth - (cellWidth * (gridCenterSpace / 2)),
        Y2 = 0.5d + (invHeight / 2)
      };
      leftInvBounds.RecomputeExpandedBounds(5,5);

      leftInv2Bounds = new InventoryBounds
      {
        X1 = leftInvBounds.X1 - (cellWidth * 2) - cellWidth * gridCenterSpace,
        Y1 = 0.5d - (invHeight / 2),
        X2 = leftInvBounds.X1 - cellWidth * gridCenterSpace,
        Y2 = 0.5d + (invHeight / 2)
      };
      leftInv2Bounds.RecomputeExpandedBounds(2, 5);

      leftInv3Bounds = new InventoryBounds
      {
        X1 = leftInvBounds.X2 - (cellWidth),
        Y1 = leftInv2Bounds.Y2 + (0.5f / TextUtils.TxtHConst) + cellHeight * (gridCenterSpace / 6), //(0.5f / TextUtils.TxtHConst) je na zaklade textu
        X2 = leftInvBounds.X2,
        Y2 = leftInv2Bounds.Y2 + (0.5f / TextUtils.TxtHConst) + cellHeight + cellHeight * (gridCenterSpace / 6)
      };
      leftInv3Bounds.RecomputeExpandedBounds(1, 1);

      rightInvBounds = new InventoryBounds
      {
        X1 = invCenterWidth + cellWidth * (gridCenterSpace / 2),
        Y1 = 0.5d - (invHeight / 2),
        X2 = invCenterWidth + invWidth + cellWidth * (gridCenterSpace / 2),
        Y2 = 0.5d + (invHeight / 2)
      };
      rightInvBounds.RecomputeExpandedBounds(5, 5);

      backgroundBounds = new RectBounds
      {
        X1 = leftInvBounds.ExpX1 - (invBorderOverHeight / aspectFactor),
        Y1 = (leftInvBounds.ExpY1 - invBorderOverHeight),
        X2 = rightInvBounds.ExpX2 + (invBorderOverHeight / aspectFactor),
        Y2 = rightInvBounds.ExpY2 + invBorderOverHeight
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
      else if (isLeftInvPlayerType && DrawUtils.IsInBounds(leftInv2Bounds, xRelative, yRelative) && DrawUtils.TryGetBoundsGridPosition(leftInv2Bounds, xRelative, yRelative, cellWidth, cellHeight, out xGrid, out yGrid))
      {
        cursorData.InvData = LeftInv;
        cursorData.HoverItem = LeftInv?.Items?.Where(it => it.X == -1 && it.Y == yGrid * 2 + xGrid).FirstOrDefault();
        cursorData.InvType = "left2";
        cursorData.YGrid = yGrid * 2 + xGrid;
        cursorData.XGrid = -1;
        return;
      }
      else if (isLeftInvPlayerType && DrawUtils.IsInBounds(leftInv3Bounds, xRelative, yRelative)) //Zatim je to jen jedna bunka, nepotrebuju pocitat pozici v gridu, vim ji
      {
        cursorData.InvData = LeftInv;
        cursorData.HoverItem = LeftInv?.Items?.Where(it => it.X == -1 && it.Y == 100).FirstOrDefault();
        cursorData.InvType = "left3";
        cursorData.YGrid = 100;
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
        
        int clickedNow = GetGameTimer();
        if (clickedNow - lastLastLeftClick < 500)
        {
          lastLastLeftClick = 0;
          UseItem(cursorData.HoverItem, cursorData.InvData);
          dragData.SrcItem.IsDragged = false;
          dragData.Clear();
          return;
        }
        else
        {
          lastLastLeftClick = clickedNow;
        }

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
            string input = await TextUtils.GetUserInput($"Kolik chcete přesunout z {ItemsDefinitions.Items[dragData.SrcItem.ItemId].FormatAmount(dragData.SrcItem.Count)}?", null, 7);
            int splitCount;
            decimal money;
            if (ItemsDefinitions.Items[dragData.SrcItem.ItemId].IsMoney == true && Decimal.TryParse(input, out money))
            {
              splitCount = (int)(money * 100);
              TriggerServerEvent("ofw_inventory:Operation_Split", dragData.SrcItem.Id, dragData.SrcItem.Place, dragData.TargetInv.Place, dragData.targetX, dragData.targetY, splitCount);
            }
            else
            if (ItemsDefinitions.Items[dragData.SrcItem.ItemId].IsMoney == false && Int32.TryParse(input, out splitCount))
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

      if (IsWaiting)
        DrawRect2(backgroundBounds, 255, 0, 0, 100);
      //DrawRect2(leftInvBounds, 100, 100, 100, 180);
      DrawUtils.DrawSprite2("inventory_textures", "background5x5", leftInvBounds.ExpX1, leftInvBounds.ExpY1, leftInvBounds.ExpX2, leftInvBounds.ExpY2, 0f, 50, 50, 50, 180);
      //DrawRect2(rightInvBounds, 100, 100, 100, 180);
      DrawUtils.DrawSprite2("inventory_textures", "background5x5", rightInvBounds.ExpX1, rightInvBounds.ExpY1, rightInvBounds.ExpX2, rightInvBounds.ExpY2, 0f, 50, 50, 50, 180);
      if (isLeftInvPlayerType)
      {
        //DrawRect2(leftInv2Bounds, 100, 100, 100, 180);
        DrawUtils.DrawSprite2("inventory_textures", "background2x5", leftInv2Bounds.ExpX1, leftInv2Bounds.ExpY1, leftInv2Bounds.ExpX2, leftInv2Bounds.ExpY2, 0f, 50, 50, 50, 180);
        //DrawRect2(leftInv3Bounds, 100, 100, 100, 180);
        DrawUtils.DrawSprite2("inventory_textures", "background1x1", leftInv3Bounds.ExpX1, leftInv3Bounds.ExpY1, leftInv3Bounds.ExpX2, leftInv3Bounds.ExpY2, 0f, 50, 50, 50, 180);
      }

      //left scrollbar
      DrawUtils.DrawSprite2("inventory_textures", "scrollbar1",
                leftInvBounds.ExpX1 - cellWidth * scrollbarWidthToCellWidth,
                leftInvBounds.ExpY1 + (leftInvBounds.Y2 - leftInvBounds.Y1) * (lInvScrollMaxOffset > 0 ? lInvScrollOffset * ((1f - lScrollBarHeight) / lInvScrollMaxOffset) : 0),
                leftInvBounds.ExpX1,
                leftInvBounds.ExpY2 - (leftInvBounds.Y2 - leftInvBounds.Y1) * (lInvScrollMaxOffset > 0 ? (lInvScrollMaxOffset - lInvScrollOffset) * ((1f - lScrollBarHeight) / lInvScrollMaxOffset) : 0),
                0f, 255, 255, 255, 200);

      //right scrollbar
      DrawUtils.DrawSprite2("inventory_textures", "scrollbar1",
                rightInvBounds.ExpX2,
                rightInvBounds.ExpY1 + (rightInvBounds.Y2 - rightInvBounds.Y1) * (rInvScrollMaxOffset > 0 ? rInvScrollOffset * ((1f - rScrollBarHeight) / rInvScrollMaxOffset) : 0),
                rightInvBounds.ExpX2 + cellWidth * scrollbarWidthToCellWidth,
                rightInvBounds.ExpY2 - (rightInvBounds.Y2 - rightInvBounds.Y1) * (rInvScrollMaxOffset > 0 ? (rInvScrollMaxOffset - rInvScrollOffset) * ((1f - rScrollBarHeight) / rInvScrollMaxOffset) : 0),
                0f, 255, 255, 255, 200);

      TextUtils.DrawTextOnScreen(InventoryBag.GetPlaceName(LeftInv?.Place), (float)leftInvBounds.ExpX1, (float)leftInvBounds.ExpY1 - (0.5f / TextUtils.TxtHConst), 0.5f, Alignment.Left);
      TextUtils.DrawTextOnScreen(InventoryBag.GetPlaceName(RightInv?.Place), (float)rightInvBounds.ExpX2, (float)rightInvBounds.ExpY1 - 0.5f / TextUtils.TxtHConst, 0.5f, Alignment.Right);
      if (isLeftInvPlayerType)
        TextUtils.DrawTextOnScreen("Ruce", ((float)leftInv3Bounds.ExpX1 + (float)leftInv3Bounds.ExpX2) / 2, (float)leftInv3Bounds.ExpY1 - (0.5f / TextUtils.TxtHConst), 0.5f, Alignment.Center);

      if (LeftInv != null)
      {
        for (int y = 0; y + LeftInv.ScrollOffset < LeftInv.RowCount && y < 5; y++)
        {
          for (int x = 0; x < LeftInv.ColumnCount; x++)
          {
            var it = LeftInv.Items.Where(a => a.Y == y + LeftInv.ScrollOffset && a.X == x).FirstOrDefault();
            eItemCarryType carryType = eItemCarryType.Inventory;
            if (y + LeftInv.ScrollOffset < LeftInv.RowCountForklift)
              carryType = eItemCarryType.Forklift;
            else if (y + LeftInv.ScrollOffset < LeftInv.RowCountHands)
              carryType = eItemCarryType.Hands;
            RenderItem(x, y, leftInvBounds, it, false, carryType);
          }
        }

        if (isLeftInvPlayerType)
        {
          for (int y = 0; y < 10; y++)
          {
            var it = LeftInv.Items.Where(a => a.Y == y && a.X == -1).FirstOrDefault();
            RenderItem(y % 2, y / 2, leftInv2Bounds, it, true, eItemCarryType.Inventory);
          }

          var itCarry = LeftInv.Items.Where(a => a.Y == 100 && a.X == -1).FirstOrDefault();
          RenderItem(0, 0, leftInv3Bounds, itCarry, false, eItemCarryType.Hands);
        }
      }

      if (RightInv != null)
      {
        for (int y = 0; y + RightInv.ScrollOffset < RightInv.RowCount && y < 5; y++)
        {
          for (int x = 0; x < RightInv.ColumnCount; x++)
          {
            var it = RightInv.Items.Where(a => a.Y == y + RightInv.ScrollOffset && a.X == x).FirstOrDefault();
            eItemCarryType carryType = eItemCarryType.Inventory;
            if (y + RightInv.ScrollOffset < RightInv.RowCountForklift)
              carryType = eItemCarryType.Forklift;
            else if (y + RightInv.ScrollOffset < RightInv.RowCountHands)
              carryType = eItemCarryType.Hands;
            RenderItem(x, y, rightInvBounds, it, false, carryType);
          }
        }
      }

      RenderDragged(); //Kreslim na sebe, takze to s cim hybu budu malovat uplne nahoru
    }

    private void RenderItem(int x, int y, RectBounds bounds, InventoryItemBag it, bool isExtraSlots, eItemCarryType slotType)
    {
      var slotColors = new int[3];
      switch (slotType)
      {
        case eItemCarryType.Hands:
          slotColors[0] = 0;
          slotColors[1] = 0;
          slotColors[2] = 50;
          break;
        case eItemCarryType.Forklift:
          slotColors[0] = 50;
          slotColors[1] = 0;
          slotColors[2] = 0;
          break;
        default:
          slotColors[0] = 0;
          slotColors[1] = 0;
          slotColors[2] = 0;
          break;
      }

      if (isExtraSlots && it == null)
        DrawUtils.DrawSprite2("inventory_textures", $"empty_slot_{y * 2 + x}", bounds.X1 + x * cellWidth, bounds.Y1 + y * cellHeight, bounds.X1 + (x + 1) * cellWidth, bounds.Y1 + (y + 1) * cellHeight, 0f, slotColors[0], slotColors[1], slotColors[2], 80);
      else
        DrawUtils.DrawSprite2("inventory_textures", "empty_slot", bounds.X1 + x * cellWidth, bounds.Y1 + y * cellHeight, bounds.X1 + (x + 1) * cellWidth, bounds.Y1 + (y + 1) * cellHeight, 0f, slotColors[0], slotColors[1], slotColors[2], 80);

      if (it == null )
      {
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
      y1 = bounds.Y1 + (y + 1) * cellHeight - itemCountYOffset - cellHeight / 8;
      x2 = bounds.X1 + (x + 1) * cellWidth - cellWidth / 10;
      y2 = bounds.Y1 + (y + 1) * cellHeight - cellHeight / 8;

      SetTextFont(4);
      SetTextScale(itemCountScale, itemCountScale);
      SetTextColour(255, 255, 255, 255);
      SetTextEntry("STRING");
      AddTextComponentString(FontsManager.FiraSansString + ItemsDefinitions.Items[it.ItemId].FormatAmount(it.Count));
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
      AddTextComponentString(FontsManager.FiraSansString + ItemsDefinitions.Items[it.ItemId].FormatAmount(it.Count));
      SetTextWrap((float)(cursorData.xRelative - cellWidth / 2f + cellWidth / 10), (float)(cursorData.xRelative + cellWidth / 2f - cellWidth / 10));
      SetTextJustification((int)CitizenFX.Core.UI.Alignment.Right);
      DrawText((float)(cursorData.xRelative - cellWidth / 2f), (float)(cursorData.yRelative + cellHeight / 2f - itemCountYOffset - cellHeight / 10));
    }

    private async void UseItem(InventoryItemBag it, InventoryBag inv)
    {
      if (it == null || inv == null)
        return;

      var definition = ItemsDefinitions.Items[it.ItemId];
      if (definition == null || definition.UsableType == eUsableType.None)
      {
        Notify.Alert("Použít jak?");
        return;
      }

      if (definition.UsableType == eUsableType.Weapon)
        TriggerServerEvent("ofw_inventory:UseItem", it.Id, it.Place, it.ItemId);
      else if (definition.UsableType == eUsableType.IdentityCard)
      {
        IsInventoryOpen = false;
        await Delay(0);
        CloseTooltip();
        TriggerServerEvent("ofw_identity:ShowCard", it.Id);
      }
    }

    private void CloseTooltip()
    {
      var message = new
      {
        type = "hideInventoryTooltip",
      };
      SendNuiMessage(JsonConvert.SerializeObject(message));
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

            if (XGrid == -1 && YGrid == 100) //ruce
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

      private string _header = null;
      private List<object> _rows = null;
      private float _x = 0;
      private float _y = 0;
      private string _bgcolor = null;

      public void SetAndComputeData(float x, float y, InventoryItemBag item)
      {
        if (item == null)
          return;

        var itDef = ItemsDefinitions.Items[item.ItemId];
        _header = $"{itDef.Name ?? "Nepojmenovaný předmět"}";
        _rows = new List<object>();

        if (itDef?.Color?.Label != null)
        {
          _rows.Add(new { s1 = "Barva", s2 = itDef.Color.Label });
        }

        if (itDef.MaleSkin != null || itDef.FemaleSkin != null)
        {
          if (itDef.MaleSkin != null && itDef.FemaleSkin != null)
            _rows.Add(new { s1 = "Střih", s2 = "Univerzální" });
          else if (itDef.MaleSkin != null)
            _rows.Add(new { s1 = "Střih", s2 = "Pánský" });
          else if (itDef.FemaleSkin != null)
            _rows.Add(new { s1 = "Střih", s2 = "Dámský" });
        }

        if (item.Metadata != null)
        {
          if (item.Metadata.Any(m => m.StartsWith("_charname:")))
            _rows.Add(new { s1 = "Jméno", s2 = item.Metadata.First(m => m.StartsWith("_charname:")).Substring(10) });
          if (item.Metadata.Any(m => m.StartsWith("_born:")))
            _rows.Add(new { s1 = "Datum narození", s2 = item.Metadata.First(m => m.StartsWith("_born:")).Substring(6) });
          if (item.Metadata.Any(m => m.StartsWith("_created:")))
            _rows.Add(new { s1 = "Vystaveno", s2 = item.Metadata.First(m => m.StartsWith("_created:")).Substring(9) });
          if (item.Metadata.Any(m => m.StartsWith("_valid:")))
            _rows.Add(new { s1 = "Platnost do", s2 = item.Metadata.First(m => m.StartsWith("_valid:")).Substring(7) });
          if (item.Metadata.Any(m => m.StartsWith("_sn:")))
            _rows.Add(new { s1 = "S/N", s2 = item.Metadata.First(m => m.StartsWith("_sn:")).Substring(4) });
        }

        switch (itDef.CarryType)
        {
          default:
          case eItemCarryType.Inventory: _bgcolor = "black"; break;
          case eItemCarryType.Hands: 
            _bgcolor = "blue";
            _rows.Add(new { s1 = "Těžký", s2 = "Tohle si do kapsy rozhodně nenacpeš, budeš potřebovat svoje silný pracky." });
            break;
          case eItemCarryType.Forklift: 
            _bgcolor = "red";
            _rows.Add(new { s1 = "Extra těžký", s2 = "Na ruce zapomeň, ty s timhle rozhodně nehnou." });
            break;
        }

        _x = x;
        _y = y - vertical_offset;
      }

      public async void Render()
      {
        var message = new
        {
          type = "showInventoryTooltip",
          x = _x,
          y = _y,
          header = _header,
          rows = _rows,
          bgcolor = _bgcolor
        };
        SendNuiMessage(JsonConvert.SerializeObject(message));
      }
    }
    #endregion
  }
}
