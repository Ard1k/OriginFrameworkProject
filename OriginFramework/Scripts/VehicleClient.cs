using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using OriginFramework.Menus;
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

      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);

      DecorRegister("carjack_tried", 2);

      Tick += OnTick;

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

          TriggerServerEvent("ofw_veh:AddPersistentVehicle", VehToNet(veh), plate, JsonConvert.SerializeObject(props));
        }
      }), false);


      InternalDependencyManager.Started(eScriptArea.VehicleClient);
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
    }

    static int lastEnteringVeh = 0;
    private async Task OnTick()
    {
      int ped = Game.PlayerPed.Handle;
      int enteringVeh = GetVehiclePedIsTryingToEnter(ped);
      //bool isInVeh = IsPedInAnyVehicle(ped, false);
      int curVeh = GetVehiclePedIsIn(ped, false);

      LockRandomCar(ped, enteringVeh);
      UnlockOnEnter(ped, enteringVeh);
      HandleEngineControl(ped, curVeh);

      lastEnteringVeh = enteringVeh;
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
          API.SetPedConfigFlag(PlayerPedId(), 429, true);
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
      return IsVehicleKeyAvailable(veh);
    }

    public bool IsVehicleKeyAvailable(int veh)
    {
      if (veh == 0)
        return false;

      string curVehLp = GetVehicleNumberPlateText(veh).ToLowerInvariant();
      return InventoryManager.PlayerInventoryCache?.Items.Any(it => ItemsDefinitions.Items[it.ItemId]?.UsableType == eUsableType.CarKey && curVehLp == it.RelatedTo?.ToLowerInvariant()) == true;
    }

    private void UnlockOnEnter(int ped, int enteringVeh)
    {
      if (enteringVeh > 0 && enteringVeh != lastEnteringVeh)
      {
        if (IsVehicleKeyAvailable(enteringVeh) && API.GetPedInVehicleSeat(enteringVeh, -1) == 0)
        {
          string model = API.GetDisplayNameFromVehicleModel((uint)GetEntityModel(enteringVeh)).ToLowerInvariant();
          if (GetVehicleDoorsLockedForPlayer(enteringVeh, ped))
          {
            UnlockVehicle(enteringVeh, false);
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
          //if (doorStatus == 7)
          //  LockVehicle(enteringVeh, false);

          Debug.WriteLine($"Veh door status: {doorStatus} {DecorExistOn(enteringVeh, "carjack_tried")}");

          int vehDriver = GetPedInVehicleSeat(enteringVeh, -1);
          

          Debug.WriteLine($"{vehDriver} {vehDriver > 0} {IsEntityAPed(vehDriver)} {IsPedAPlayer(vehDriver)}");
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
      if (!DecorExistOn(veh, "carjack_tried"))
      {
        int hash = GetEntityModel(veh);
        int vehClass = GetVehicleClass(veh);
        
        int lockChance = 20;
        int rand = random.Next(0, 101);

        bool isLocked = lockChance >= rand;

        DecorSetBool(veh, "carjack_tried", true);
        Debug.WriteLine($"IsLocked = {isLocked}, chance {rand}/{lockChance}");
        if (isLocked)
        {
          SetVehicleDoorsLocked(veh, 2);
        }
        else
        {
          //register
        }
      }
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
    private async void RespawnedCarRestoreProperties(int vehNetId, string originalPlate, string properties)
    {
      if (originalPlate == null)
        return;

      if (string.IsNullOrEmpty(properties))
      {
        //Debug.WriteLine("ofw_veh:RespawnedCarRestoreProperties: no properties sent");
        return;
      }

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

      var propertiesBag = JsonConvert.DeserializeObject<VehiclePropertiesBag>(properties);
      var veh = NetToVeh(vehNetId);
      originalPlate = originalPlate.Trim().ToLower();

      if (GetVehicleNumberPlateText(veh)?.Trim().ToLower() == originalPlate)
      {
        TriggerServerEvent("ofw_veh:AckPropertiesSynced", originalPlate);
        return; //uz ma nastaveny properties
      }

      if (propertiesBag == null)
        return;

      if (GetEntityCoords(veh, false).DistanceToSquared2D(Game.PlayerPed.Position) > 10000)
        return;

      Vehicles.SetVehicleProperties(veh, propertiesBag);
      TriggerServerEvent("ofw_veh:AckPropertiesSynced", originalPlate);
    }

    public void LockVehicle(int vehHandle, bool disableLightsFlash)
    {
      SetVehicleDoorsLockedForAllPlayers(vehHandle, true);
      NotifyVehicleLocked();
      Vector3 vehCoords = GetEntityCoords(vehHandle, false);
      //PlayLockSound(vehCoords, meta.veh, veh.Handle);

      if (!disableLightsFlash)
      {
        FlashTurnSignals(vehHandle, 2);
      }
    }

    public void UnlockVehicle(int vehHandle, bool disableLightsFlash)
    {
      SetVehicleDoorsLockedForAllPlayers(vehHandle, false);
      NotifyVehicleLocked();
      Vector3 vehCoords = GetEntityCoords(vehHandle, false);
      //PlayLockSound(vehCoords, meta.veh, veh.Handle);

      if (!disableLightsFlash)
      {
        FlashTurnSignals(vehHandle, 2);
      }
    }

    private async void FlashTurnSignals(int veh, int count, Action cb = null)
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

    public void NotifyVehicleLocked()
    {
      Notify.Info("Vozidlo bylo zamčeno");
    }

    public void NotifyVehicleUnlocked()
    {
      Notify.Info("Vozidlo bylo odemčeno");
    }
  }
}
