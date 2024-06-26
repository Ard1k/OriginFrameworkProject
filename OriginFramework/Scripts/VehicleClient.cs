﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using OriginFramework.Menus;
using OriginFramework.Scripts;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static OriginFramework.OfwFunctions;


namespace OriginFramework
{
  public class VehicleClient : BaseScript
  {
    Random random = new Random();
    private static bool _canStartCurrentCarChecked = false;

    public static string MenuName { get { return "vehicle_menu"; } }
    public static int MenuFocusedVehicle { get; set; }

    public VehicleClient()
    {
      random.Next();
      random.Next();
      random.Next();
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }


    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.VehicleClient))
        return;

      DefinedVehicles.KnownVehiclesByHash = new Dictionary<int, VehicleInformation>();
      foreach (var veh in DefinedVehicles.KnownVehicles)
      {
        var hashKey = GetHashKey(veh.Key);
        if (DefinedVehicles.KnownVehiclesByHash.ContainsKey(hashKey))
        {
          Debug.WriteLine($"VehicleClient: Duplicate vehicle hash {veh.Key} ({hashKey})");
          continue;
        }
        DefinedVehicles.KnownVehiclesByHash.Add(hashKey, veh.Value);
      }

      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);

      DecorRegister(Decorators.CarjackTried, 2);
      DecorRegister(Decorators.BrokenLock, 2);

      Tick += OnTick;
      Tick += OnSlowTick;

      RegisterCommand("dv", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        if (CharacterCaretaker.LoggedCharacter?.AdminLevel > 0 == false)
        {
          Notify.Error("Nedostatečné oprávnění!");
        }

        if (Game.PlayerPed.IsInVehicle())
        {
          int veh = Game.PlayerPed.CurrentVehicle.Handle;
          DeleteEntity(ref veh);
          return;
        }

        int vehFront = Vehicles.GetVehicleInFront(null);
        if (vehFront > 0)
        {
          DeleteEntity(ref vehFront);
          return;
        }
      }), false);

      RegisterCommand("vehmakepersist", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        if (CharacterCaretaker.LoggedCharacter?.AdminLevel > 0 == false)
        {
          Notify.Error("Nedostatečné oprávnění!");
        }

        if (Game.PlayerPed.IsInVehicle())
        {
          int veh = Game.PlayerPed.CurrentVehicle.Handle;
          if (!NetworkGetEntityIsNetworked(veh))
            return;

          string plate = GetVehicleNumberPlateText(veh).ToLower().Trim();
          var props = Vehicles.GetVehicleProperties(veh);
          var damage = Vehicles.GetVehicleDamage(veh);

          TriggerServerEvent("ofw_veh:AddPersistentVehicle", VehToNet(veh), plate, true, true, JsonConvert.SerializeObject(props), JsonConvert.SerializeObject(damage));
        }
      }), false);


      InternalDependencyManager.Started(eScriptArea.VehicleClient);
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
    }

    private async Task OnSlowTick()
    {
      await Delay(30000);
      int curVeh = GetVehiclePedIsIn(Game.PlayerPed.Handle, false);

      if (curVeh == 0)
        return;

      if (DoesEntityExist(curVeh) && NetworkGetEntityIsNetworked(curVeh))
      {
        int netId = VehToNet(curVeh);
        if (netId != 0)
        {
          TriggerServerEvent("ofw_veh:UpdatePersistentVehicle", netId, JsonConvert.SerializeObject(Vehicles.GetVehicleDamage(curVeh)));
        }
      }
    }

    static int lastEnteringVeh = 0;
    static int lastDrivenVeh = 0;

    private async Task OnTick()
    {
      int ped = Game.PlayerPed.Handle;
      int enteringVeh = GetVehiclePedIsTryingToEnter(ped);
      int curVeh = GetVehiclePedIsIn(ped, false);

      LockRandomCar(ped, enteringVeh);
      UnlockOnEnter(ped, enteringVeh);
      HandleEngineControl(ped, curVeh);
      AutolockCar(curVeh);
      HandleLockKeypress(curVeh);
      PreventVehicleLeave(curVeh);
      HandleVehicleMenu(false);
      DrawVehicleMenuLinePointer();

      if (curVeh != 0 && GetPedInVehicleSeat(curVeh, -1) == ped)
        lastDrivenVeh = curVeh;

      lastEnteringVeh = enteringVeh;
    }

    public void DrawVehicleMenuLinePointer()
    {
      if (Game.PlayerPed.IsInVehicle() || MenuFocusedVehicle == 0 || (!NativeMenuManager.IsMenuOpen(MenuName) && !NativeMenuManager.IsMenuOpen(TuningInstallClient.MenuName)))
        return;

      Vector3 vehPos = GetEntityCoords(MenuFocusedVehicle, false);
      float screenX = 0, screenY = 0, lineThickness = 0.0003f;
      if (World3dToScreen2d(vehPos.X, vehPos.Y, vehPos.Z, ref screenX, ref screenY))
      {
        DrawLine_2d(screenX, screenY, NativeMenuManager.LineAnchorX + lineThickness/2, NativeMenuManager.LineAnchorY, lineThickness, 255, 255, 255, 150);
      }
    }

    public static void HandleVehicleMenu(bool forceOpen)
    {
      if (!Game.IsControlJustPressed(0, Control.PhoneCameraFocusLock) && !forceOpen) //L
        return;

      if (NativeMenuManager.IsMenuOpen(MenuName))
      {
        NativeMenuManager.CloseAndUnlockMenu(MenuName);
        return;
      }

      if (Game.PlayerPed.IsInVehicle())
      {
        int veh = Game.PlayerPed.CurrentVehicle.Handle;
        MenuFocusedVehicle = veh;
      }
      else
      {
        int vehFront = Vehicles.GetVehicleInFront(null);
        MenuFocusedVehicle = vehFront;
      }

      if (MenuFocusedVehicle != 0)
      {
        NativeMenuManager.OpenNewMenu(MenuName, getVehicleMenu);
      }
    }

    private static NativeMenu getVehicleMenu()
    {
      if (MenuFocusedVehicle == 0)
        return null;

      var menu = new NativeMenu
      {
        MenuTitle = $"Menu vozidla {GetDisplayNameFromVehicleModel((uint)GetEntityModel(MenuFocusedVehicle))} [{GetVehicleNumberPlateText(MenuFocusedVehicle)}]",
        Items = new List<NativeMenuItem>
          {
            new NativeMenuItem
            {
              Name = "Zavřít",
              IsClose = true,
            }
          }
      };

      if (TuningShopClient.CurrentShop != null && MenuFocusedVehicle != 0)
      {
        menu.Items.Insert(0, new NativeMenuItem
        {
          Name = "Instalovat tuning",
          OnSelected = (item) =>
          {
            TuningInstallClient.OpenTuningInstall(MenuFocusedVehicle);
          }
        });
      }

      if (TuningShopClient.CurrentShop != null && Game.PlayerPed.IsInVehicle())
      {
        menu.Items.Insert(0, new NativeMenuItem
        {
          Name = "Katalog tuningu",
          OnSelected = (item) =>
          {
            TuningCatalogClient.OpenCatalog();
          }
        });
      }

      return menu;
    }

    public void PreventVehicleLeave(int veh)
    {
      if (veh != 0)
      {
        bool lockedForPlayer = GetVehicleDoorsLockedForPlayer(veh, Game.PlayerPed.Handle);
        if (lockedForPlayer)
        {
          DisableControlAction(0, 75, true); //INPUT_VEH_EXIT
        }
        else
        {
          EnableControlAction(0, 75, true); //INPUT_VEH_EXIT
        }
      }
    }

    public void HandleLockKeypress(int veh)
    {
      if (IsControlJustPressed(0, 314)) //NUM+
      {
        if (veh == 0)
        {
          //mimo auto
          int nearestVeh = 0;
          float nearestDist = float.MaxValue;
          var vehicles = World.GetAllVehicles();

          if (vehicles == null || vehicles.Length <= 0)
            return;

          foreach (var v in vehicles)
          {
            var dist = v.Position.DistanceToSquared(Game.PlayerPed.Position);

            if (dist < nearestDist)
            {
              nearestDist = dist;
              nearestVeh = v.Handle;
            }
          }

          if (nearestVeh != 0)
          {
            var plate = GetVehicleNumberPlateText(nearestVeh)?.ToLower();
            var keyItem = InventoryManager.PlayerInventoryCache?.Items?.Where(it => it.RelatedTo?.ToLower() == plate).FirstOrDefault();

            if (keyItem == null)
              return;

            var keyDefinition = ItemsDefinitions.Items[keyItem.ItemId];

            if (!keyDefinition.IsKeyRemote && !keyDefinition.IsKeyAuto)
              return;

            bool lockedForPlayer = GetVehicleDoorsLockedForPlayer(nearestVeh, Game.PlayerPed.Handle);

            if (lockedForPlayer)
            {
              UnlockVehicle(nearestVeh, !keyDefinition.IsKeyAuto, false);
            }
            else
            {
              LockVehicle(nearestVeh, !keyDefinition.IsKeyAuto, false);
            }
          }
        }
        else
        {
          //jsem v aute
          if (GetPedInVehicleSeat(veh, -1) != Game.PlayerPed.Handle)
            return;

          bool lockedForPlayer = GetVehicleDoorsLockedForPlayer(veh, Game.PlayerPed.Handle);
          if (lockedForPlayer)
          {
            UnlockVehicle(veh, true, true);
          }
          else
          {
            LockVehicle(veh, true, true);
          }
        }
      }
    }

    public void AutolockCar(int veh)
    {
      if (lastDrivenVeh == 0 || veh != 0 || !DoesEntityExist(lastDrivenVeh))
      {
        lastDrivenVeh = 0;
        return;
      }

      var pos = GetEntityCoords(lastDrivenVeh, false);
      if (Game.PlayerPed.Position.DistanceToSquared(pos) > 15f)
      {
        if (IsVehicleKeyAvailable(lastDrivenVeh, true))
          LockVehicle(lastDrivenVeh, false, true);
        
        lastDrivenVeh = 0;
      }
    }

    public void HandleEngineControl(int ped, int veh)
    {
      if (veh == 0)
      {
        SetPedConfigFlag(ped, 429, false);
        _canStartCurrentCarChecked = false;
      }
      else if (veh > 0 && !_canStartCurrentCarChecked)
      {
        if (!CanStartCurrentCar(veh))
        {
          SetPedConfigFlag(PlayerPedId(), 429, true);
        }
        else
        {
          SetPedConfigFlag(ped, 429, false);
        }

        _canStartCurrentCarChecked = true;
      }
    }

    public static void CheckCarKeysAgain()
    {
      _canStartCurrentCarChecked = false;
    }

    private bool CanStartCurrentCar(int veh)
    {
      if (DecorExistOn(veh, Decorators.BrokenLock) && DecorGetBool(veh, Decorators.BrokenLock))
        return true;

      return IsVehicleKeyAvailable(veh, false);
    }

    public bool IsVehicleKeyAvailable(int veh, bool keylessOnly)
    {
      if (veh == 0)
        return false;

      string curVehLp = GetVehicleNumberPlateText(veh).ToLowerInvariant();
      int model = GetEntityModel(veh);
      return InventoryManager.PlayerInventoryCache?.Items.Any(it => ItemsDefinitions.Items[it.ItemId]?.UsableType == eUsableType.CarKey && curVehLp == it.RelatedTo?.ToLowerInvariant() && (keylessOnly == false || ItemsDefinitions.Items[it.ItemId]?.IsKeyAuto == true)) == true;
    }

    private void UnlockOnEnter(int ped, int enteringVeh)
    {
      if (enteringVeh > 0 && enteringVeh != lastEnteringVeh)
      {
        if ((IsVehicleKeyAvailable(enteringVeh, true) || (DecorExistOn(enteringVeh, Decorators.BrokenLock) && DecorGetBool(enteringVeh, Decorators.BrokenLock))) && API.GetPedInVehicleSeat(enteringVeh, -1) == 0)
        {
          string model = API.GetDisplayNameFromVehicleModel((uint)GetEntityModel(enteringVeh)).ToLowerInvariant();
          if (GetVehicleDoorsLockedForPlayer(enteringVeh, ped))
          {
            UnlockVehicle(enteringVeh, false, true);
          }
        }
      }
    }

    private void LockRandomCar(int ped, int enteringVeh)
    {
      bool isInVehicle = IsPedInAnyVehicle(ped, false); //ani nenaseda

      if (!isInVehicle)
      {
        if (enteringVeh > 0 && enteringVeh != lastEnteringVeh)
        {
          var doorStatus = GetVehicleDoorLockStatus(enteringVeh);

          int vehDriver = GetPedInVehicleSeat(enteringVeh, -1);

          if (vehDriver > 0 && IsEntityAPed(vehDriver) && !IsPedAPlayer(vehDriver)) // stealing vehicle from NPC
          {
            HandleCarjackLockLogic(enteringVeh);
          }
        }
        //else if (API.IsPedInAnyVehicle(ped, false)) //pro vykradeni auta kdyz v nem sedim
        //{
        //  isInVehicle = true;
        //  enteringVehicle = 0;
        //  currentVehicle = API.GetVehiclePedIsIn(ped, false);
        //  EnteredVehicle(currentVehicle);
        //}
      }
      else
      {
        //if (!IsPedInAnyVehicle(ped, false))
        //{
        //  isInVehicle = false;
        //  currentVehicle = 0;
        //  enteringVehicle = 0;
        //  isEnteringVehicleWithNPC = false;
        //}
      }
    }

    private void HandleCarjackLockLogic(int veh)
    {
      if (!DecorExistOn(veh, Decorators.CarjackTried))
      {
        int hash = GetEntityModel(veh);
        int vehClass = GetVehicleClass(veh);
        
        int lockChance = 20;
        int rand = random.Next(0, 101);

        bool isLocked = lockChance >= rand;

        DecorSetBool(veh, Decorators.CarjackTried, true);
        Debug.WriteLine($"IsLocked = {isLocked}, chance {rand}/{lockChance}");
        if (isLocked)
        {
          SetVehicleDoorsLocked(veh, 2);
        }
        else
        {
          //register know and give key
          DecorSetBool(veh, Decorators.BrokenLock, true);

          if (!NetworkGetEntityIsNetworked(veh))
            return;

          string plate = GetVehicleNumberPlateText(veh).ToLower().Trim();
          var props = Vehicles.GetVehicleProperties(veh);
          var damage = Vehicles.GetVehicleDamage(veh);

          TriggerServerEvent("ofw_veh:AddPersistentVehicle", VehToNet(veh), plate, true, true, JsonConvert.SerializeObject(props), JsonConvert.SerializeObject(damage));
        }
      }
    }

    public static void CarKeyUsed(string plate, ItemDefinition keyDefinition)
    {
      bool isInVehicle = IsPedInAnyVehicle(Game.PlayerPed.Handle, false);

      if (isInVehicle == true)
        return;

      float lockDist = 30;

      var pp = Game.PlayerPed.Position;
      var vehicles = World.GetAllVehicles().Where(e => e.Position.DistanceToSquared(pp) < lockDist && GetVehicleNumberPlateText(e.Handle)?.ToLower() == plate.ToLower()).ToList();

      if (vehicles.Count <= 0)
        return;

      foreach (var v in vehicles)
      {
        int lockStatus = GetVehicleDoorLockStatus(v.Handle);
        bool lockedForPlayer = GetVehicleDoorsLockedForPlayer(v.Handle, Game.PlayerPed.Handle);
        if (lockStatus == 2 || lockedForPlayer)
        {
          UnlockVehicle(v.Handle, !keyDefinition.IsKeyAuto, false);
        }
        else
        {
          LockVehicle(v.Handle, !keyDefinition.IsKeyAuto, false);
        }
      }
    }

    public bool IsKeyControlExcluded(int veh)
    {
      //TODO + zapojit

      return false;
    }

    [EventHandler("ofw_veh:ApplyPropertiesOnVehicle")]
    private void ApplyPropertiesOnVehicle(int vehNetId, string properties)
    {
      if (string.IsNullOrEmpty(properties))
      {
        Debug.WriteLine("ofw_veh:ApplyPropertiesOnVehicle: no properties sent");
        return;
      }

      if (vehNetId == 0 || !NetworkDoesEntityExistWithNetworkId(vehNetId))
      {
        Debug.WriteLine("ofw_veh:ApplyPropertiesOnVehicle: invalid vehicle network id");
        return;
      }

      var propertiesBag = JsonConvert.DeserializeObject<VehiclePropertiesBag>(properties);
      var veh = NetToVeh(vehNetId);

      if (propertiesBag == null)
        return;

      Vehicles.SetVehicleProperties(veh, propertiesBag);
    }

    //"ofw_veh:RespawnedCarRestoreProperties"
    [EventHandler("ofw_veh:RespawnedCarRestoreProperties")]
    private async void RespawnedCarRestoreProperties(int vehNetId, string originalPlate, bool keepUnlocked, bool brokenLock, string properties, string damage)
    {
      if (vehNetId == 0 || !NetworkDoesEntityExistWithNetworkId(vehNetId))
      {
        //Debug.WriteLine("ofw_veh:RespawnedCarRestoreProperties: invalid vehicle network id");
        return;
      }
      if (Game.Player.Handle != NetworkGetEntityOwner(NetToVeh(vehNetId)))
      {
        //Debug.WriteLine("ofw_veh:RespawnedCarRestoreProperties: Iam not entity owner");
        return;
      }

      var veh = NetToVeh(vehNetId);
      if (brokenLock && !DecorExistOn(veh, Decorators.BrokenLock))
      {
        DecorSetBool(veh, Decorators.BrokenLock, true);
      }

      if (originalPlate == null)
        return;

      if (string.IsNullOrEmpty(properties))
      {
        Debug.WriteLine("ofw_veh:RespawnedCarRestoreProperties: no properties sent");
        return;
      }

      //Debug.WriteLine(damage);
      var propertiesBag = JsonConvert.DeserializeObject<VehiclePropertiesBag>(properties);
      var dmgBag = damage != null ? JsonConvert.DeserializeObject<VehicleDamageBag>(damage) : null;
      
      originalPlate = originalPlate.Trim().ToLower();

      if (GetVehicleNumberPlateText(veh)?.Trim().ToLower() == originalPlate)
      {
        TriggerServerEvent("ofw_veh:AckPropertiesSynced", originalPlate);
        return; //uz ma nastaveny properties
      }

      if (propertiesBag == null)
        return;

      var dist = GetEntityCoords(veh, false).DistanceToSquared2D(Game.PlayerPed.Position);
      if (dist > 10000f)
        return;

      Vehicles.SetVehicleProperties(veh, propertiesBag);
      Vehicles.SetVehicleDamage(veh, dmgBag);
      if (!keepUnlocked)
        LockVehicle(veh, true, true);
      TriggerServerEvent("ofw_veh:AckPropertiesSynced", originalPlate);
    }

    [EventHandler("ofw_veh:EnterSpawnedVehicle")]
    private async void EnterSpawnedVehicle(int vehNetId)
    {
      if (vehNetId == 0 || !NetworkDoesEntityExistWithNetworkId(vehNetId))
      {
        return;
      }

      var veh = NetToVeh(vehNetId);

      SetPedIntoVehicle(Game.PlayerPed.Handle, veh, -1);
    }

    public static void LockVehicle(int vehHandle, bool disableLightsFlash, bool suppressNotification)
    {
      SetVehicleDoorsLockedForAllPlayers(vehHandle, true);
      if (!suppressNotification)
        NotifyVehicleLocked();
      Vector3 vehCoords = GetEntityCoords(vehHandle, false);
      TriggerServerEvent("InteractSound_SV:PlayWithinDistanceAtCoords", vehCoords, 100, "lock", 0.4);

      if (!disableLightsFlash)
      {
        FlashTurnSignals(vehHandle, 2);
      }
    }

    public static void UnlockVehicle(int vehHandle, bool disableLightsFlash, bool suppressNotification)
    {
      SetVehicleDoorsLockedForAllPlayers(vehHandle, false);
      if (!suppressNotification)
        NotifyVehicleUnlocked();
      Vector3 vehCoords = GetEntityCoords(vehHandle, false);
      TriggerServerEvent("InteractSound_SV:PlayWithinDistanceAtCoords", vehCoords, 100, "unlock2", 0.4);

      if (!disableLightsFlash)
      {
        FlashTurnSignals(vehHandle, 2);
      }
    }

    private static async void FlashTurnSignals(int veh, int count, Action cb = null)
    {
      int duration = 300;

      for (int i = 1; i <= count; i++)
      {
        API.SetVehicleLights(veh, 2);
        await Delay(duration);
        API.SetVehicleLights(veh, 0);
        if (i < count)
        {
          await Delay(duration);
        }
      }
      cb?.Invoke();
    }

    public static void NotifyVehicleLocked()
    {
      Notify.Info("Vozidlo bylo zamčeno");
    }

    public static void NotifyVehicleUnlocked()
    {
      Notify.Info("Vozidlo bylo odemčeno");
    }
  }
}
