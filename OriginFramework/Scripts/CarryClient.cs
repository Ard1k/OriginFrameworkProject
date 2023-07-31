using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using OriginFramework.Helpers;
using OriginFramework.Menus;
using OriginFrameworkData.DataBags;
using static CitizenFX.Core.Native.API;
using static OriginFramework.OfwFunctions;

namespace OriginFramework.Scripts
{
  public class CarryClient : BaseScript
  {
    public class CarriableClientWorldItem : CarriableWoldItemBag
    {
      public Vector3 Position { get; set; }
      public int? entityId { get; set; }
    }

    public static int carryEntityNet = 0;
    public static int forkliftEntityNet = 0;
    public static Dictionary<int, CarriableClientWorldItem> CarriableCache = new Dictionary<int, CarriableClientWorldItem>();
    public static CarriableClientWorldItem PickableCarryItem = null;
    public static Vector3 forkliftRange = new Vector3(0.5f, 0.5f, 2f);
    private static bool _collectionChanging = false;
    private const float _renderDistSq = 2500f;
    private const float _pickupDistSq = 1.2f;

    public CarryClient()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.CarryClient, eScriptArea.InventoryManager))
        return;

      AddTextEntry("OFW_CARRYEX_PICK", $"~INPUT_PICKUP~ {FontsManager.FiraSansString}Zvednout předmět");
      AddTextEntry("OFW_CARRYEX_PUT", $"~INPUT_PICKUP~ {FontsManager.FiraSansString}Položit");
      AddTextEntry("OFW_CARRYEX_CARTRUNKFORK", $"~INPUT_PICKUP~ {FontsManager.FiraSansString}Interakce s kufrem auta");

      Tick += OnSlowTick;
      Tick += OnTick;
      Tick += ManageWorldItems;

      TriggerServerEvent("ofw_carry:RequestCacheSync");

      InternalDependencyManager.Started(eScriptArea.CarryClient);
    }

    private async Task ManageWorldItems()
    {
      if (CarriableCache == null || CarriableCache.Count <= 0 || _collectionChanging)
      {
        PickableCarryItem = null;
        await Delay(200);
        return;
      }

      bool isDrivingAnyVeh = Game.PlayerPed.CurrentVehicle != null;
      bool isDrivinkForklift = Vehicles.IsPlayerDrivingForklift();
      var pickCurrentPos = isDrivinkForklift ? GetForkliftLoadingPos(Game.PlayerPed.CurrentVehicle.Position, Game.PlayerPed.CurrentVehicle.ForwardVector) : Game.PlayerPed.Position;
      bool anyInRange = false;
      float closestDistSq = 99999f;
      bool pickableSet = false;

      foreach (var it in CarriableCache) 
      {
        if (it.Value.entityId != null && !DoesEntityExist(it.Value.entityId.Value))
          it.Value.entityId = null;

        float distSq = it.Value.Position.DistanceToSquared2D(pickCurrentPos);
        bool isInRange = distSq <= _renderDistSq;
        if (isInRange)
          anyInRange = true;

        var itemDef = ItemsDefinitions.Items[it.Value.ItemId];

        if (!isInRange && it.Value.entityId != null)
        {
          int entityId = it.Value.entityId.Value;
          DeleteObject(ref entityId);
          it.Value.entityId = null;
        }
        else if (isInRange && it.Value.entityId == null)
        {
          int entityId = CreateObject(GetHashKey(itemDef.CarryInfo.PropName),
                                      it.Value.Position.X,
                                      it.Value.Position.Y,
                                      it.Value.Position.Z,
                                      false, false, false);
          SetEntityCollision(entityId, false, false);
          SetEntityRotation(entityId, 0f, 0f, it.Value.PosBag.Heading, 1, false);

          it.Value.entityId = entityId;
        }

        if (distSq < _pickupDistSq)
        {
          if ((itemDef.CarryType == eItemCarryType.Forklift && isDrivinkForklift) || (itemDef.CarryType == eItemCarryType.Hands && !isDrivingAnyVeh))
          {
            if (PickableCarryItem == null || distSq < closestDistSq)
            {
              PickableCarryItem = it.Value;
              pickableSet = true;
            }
          }
        }

        if (_collectionChanging)
          return;
      }

      if (!pickableSet)
        PickableCarryItem = null;

      if (anyInRange == false)
        await Delay(200);
    }

    private async Task OnTick()
    {
      if (carryEntityNet != 0 && Game.PlayerPed.CurrentVehicle != null && GetPedInVehicleSeat(Game.PlayerPed.CurrentVehicle.Handle, -1) == Game.PlayerPed.Handle)
      {
        TaskLeaveVehicle(Game.PlayerPed.Handle, Game.PlayerPed.CurrentVehicle.Handle, 0);
      }

      bool isDrivinkForklift = Vehicles.IsPlayerDrivingForklift();

      if (isDrivinkForklift)
      {
        var fork = Game.PlayerPed.CurrentVehicle;
        //var targetPos = GetForkliftLoadingPos(fork.Position, fork.ForwardVector);
        //DrawBoxMarker(targetPos, forkliftRange, fork.Heading, 255, 255, 0, 100);

        int vehFront = Vehicles.GetVehicleInFront(fork.Handle);
        bool isCarLocked = GetVehicleDoorsLockedForPlayer(vehFront, Game.PlayerPed.Handle);
        bool isNearTrunk = Vehicles.IsEntityCloseToTrunk(fork.Handle, vehFront);

        if (vehFront > 0 && !isCarLocked && !InventoryManager.IsInventoryOpen)
        {
          if (isNearTrunk)
          {
            DisplayHelpTextThisFrame("OFW_CARRYEX_CARTRUNKFORK", true);

            if (IsControlJustPressed(0, 38) || IsDisabledControlJustPressed(0, 38))
            {
              InventoryManager.OpenForkliftInventory(vehFront, fork.Handle);
            }

            return; //Jsem u kufru, chci prebit moznost polozit/zvedat
          }
        }
      }

      if (carryEntityNet != 0 || forkliftEntityNet != 0)
      {
        DisplayHelpTextThisFrame("OFW_CARRYEX_PUT", true);

        if (!isDrivinkForklift)
        {
          DisableControlAction(0, 68, true); //disable sprint
          DisableControlAction(0, 70, true); //disable sprint
          DisableControlAction(0, 91, true); //disable sprint
          DisableControlAction(0, 114, true); // disable sprint
          DisableControlAction(0, 347, true); //disable sprint

          DisableControlAction(0, 21, true); //disable sprint
          DisableControlAction(0, 24, true); //disable attack
          DisableControlAction(0, 25, true); //disable aim
          DisableControlAction(0, 47, true); //disable weapon
          DisableControlAction(0, 58, true); //disable weapon
          DisableControlAction(0, 263, true); //disable melee
          DisableControlAction(0, 264, true); //disable melee
          DisableControlAction(0, 257, true); //disable melee
          DisableControlAction(0, 140, true); //disable melee
          DisableControlAction(0, 141, true); //disable melee
          DisableControlAction(0, 142, true); //disable melee
          DisableControlAction(0, 143, true); //disable melee

          DisableControlAction(0, 22, true); //disable jump
        }

        if (IsControlJustPressed(0, 38) || IsDisabledControlJustPressed(0, 38))
        {
          PutDownItem(isDrivinkForklift);
        }
      }

      if (PickableCarryItem != null)
      {
        DisplayHelpTextThisFrame("OFW_CARRYEX_PICK", true);

        if (IsControlJustPressed(0, 38) || IsDisabledControlJustPressed(0, 38))
        {
          PickUpItem(isDrivinkForklift);
        }
      }
    }

    private async Task OnSlowTick()
    {
      await Delay(200);
      var handsIt = InventoryManager.PlayerInventoryCache?.Items?.Where(it => it.X == -1 && it.Y == 100).FirstOrDefault();
      
      if (handsIt != null && carryEntityNet != 0 && (!NetworkDoesNetworkIdExist(carryEntityNet) || !NetworkDoesEntityExistWithNetworkId(carryEntityNet)))
      {
        carryEntityNet = 0;
      }
      else if (handsIt != null && carryEntityNet == 0)
      {
        await SetItemCarry(ItemsDefinitions.Items[handsIt.ItemId], false);
      }
      else if (handsIt == null && carryEntityNet != 0)
      {
        if (!NetworkDoesNetworkIdExist(carryEntityNet) || !NetworkDoesEntityExistWithNetworkId(carryEntityNet))
        {
          carryEntityNet = 0;
        }
        else
        {
          var carryEntity = NetworkGetEntityFromNetworkId(carryEntityNet);

          DeleteObject(ref carryEntity);
          carryEntityNet = 0;
          ClearPedTasksImmediately(Game.PlayerPed.Handle);
        }
      }
      else if (handsIt != null && carryEntityNet != 0)
      {
        if (!IsEntityPlayingAnim(Game.PlayerPed.Handle, ItemsDefinitions.Items[handsIt.ItemId].CarryInfo.CarryAnim, "idle", 3))
          TaskPlayAnim(Game.PlayerPed.Handle, ItemsDefinitions.Items[handsIt.ItemId].CarryInfo.CarryAnim, "idle", 4.0f, 1.0f, -1, 49, 0f, false, false, false);
      }

      bool isDrivinkForklift = Vehicles.IsPlayerDrivingForklift();
      if (isDrivinkForklift && InventoryManager.ForkliftInventoryCache == null)
      {
        //sem v jesterce, ale nemam cache
        TriggerServerEvent("ofw_inventory:ReloadForkliftCacheInventory", GetVehicleNumberPlateText(Game.PlayerPed.CurrentVehicle.Handle));
        return;
      }
      else if (!isDrivinkForklift && InventoryManager.ForkliftInventoryCache != null)
      {
        if (forkliftEntityNet != 0)
          PutDownItem(true); //zkusime polozit, je tezky sledovat entitu kdyz v tom nesedim tak, aby se to nedojebalo
        //nesedim v jesterce a je cache, tak to smaznu
        InventoryManager.ForkliftInventoryCache = null;
        return;
      }
      
      //ridim jesterku a i vim, jestli tam neco je
      var forkIt = InventoryManager.ForkliftInventoryCache?.Items?.Where(it => it.X == 0 && it.Y == 0).FirstOrDefault();

      if (forkIt != null && forkliftEntityNet != 0 && (!NetworkDoesNetworkIdExist(forkliftEntityNet) || !NetworkDoesEntityExistWithNetworkId(forkliftEntityNet)))
      {
        forkliftEntityNet = 0;
      }
      else if (forkIt != null && forkliftEntityNet == 0)
      {
        await SetItemCarry(ItemsDefinitions.Items[forkIt.ItemId], true);
      }
      else if (forkIt == null && forkliftEntityNet != 0)
      {
        if (!NetworkDoesNetworkIdExist(forkliftEntityNet) || !NetworkDoesEntityExistWithNetworkId(forkliftEntityNet))
        {
          forkliftEntityNet = 0;
        }
        else
        {
          var forkEntity = NetworkGetEntityFromNetworkId(forkliftEntityNet);

          DeleteObject(ref forkEntity);
          forkliftEntityNet = 0;
        }
      }
      
    }

    private async Task SetItemCarry(ItemDefinition itDef, bool isForklift)
    {
      if (itDef.CarryInfo == null)
        return;

      if (itDef.CarryType == eItemCarryType.Hands && !isForklift)
      {
        await LoadAnimDict(itDef.CarryInfo.CarryAnim);
        TaskPlayAnim(Game.PlayerPed.Handle, itDef.CarryInfo.CarryAnim, "idle", 4.0f, 1.0f, -1, 49, 0f, false, false, false);
        int carryEntity = CreateObject(GetHashKey(itDef.CarryInfo.PropName), 0f, 0f, 0f, true, false, false);
        SetEntityCollision(carryEntity, false, false);

        AttachEntityToEntity(carryEntity, Game.PlayerPed.Handle, GetPedBoneIndex(Game.PlayerPed.Handle, itDef.CarryInfo.PedBoneId),
          itDef.CarryInfo.XPos, itDef.CarryInfo.YPos, itDef.CarryInfo.ZPos,
          itDef.CarryInfo.XRot, itDef.CarryInfo.YRot, itDef.CarryInfo.ZRot,
          true, true, false, true, 1, true);

        carryEntityNet = ObjToNet(carryEntity);
      }

      if (itDef.CarryType == eItemCarryType.Forklift && isForklift)
      {
        int forkEntity = CreateObject(GetHashKey(itDef.CarryInfo.PropName), 0f, 0f, 0f, true, false, false);
        SetEntityCollision(forkEntity, false, false);
        AttachEntityToEntity(forkEntity, Game.PlayerPed.CurrentVehicle.Handle, GetEntityBoneIndexByName(Game.PlayerPed.CurrentVehicle.Handle, itDef.CarryInfo.EntityBoneName),
          itDef.CarryInfo.XPos, itDef.CarryInfo.YPos, itDef.CarryInfo.ZPos,
          itDef.CarryInfo.XRot, itDef.CarryInfo.YRot, itDef.CarryInfo.ZRot,
          true, true, false, true, 1, true);
        forkliftEntityNet = ObjToNet(forkEntity);
      }
    }

    public Vector3 GetForkliftLoadingPos(Vector3 forkliftPos, Vector3 fwdVect)
    {
      return forkliftPos + fwdVect * 2f + new Vector3(0f, 0f, -0.6f);
    }

    private async Task LoadAnimDict(string anim)
    {
      RequestAnimDict(anim);
      while (!HasAnimDictLoaded(anim))
      {
        await Delay(50);
      }
    }

    public void PutDownItem(bool isForklift)
    {
      if (isForklift)
      {
        var forkEntity = NetToObj(forkliftEntityNet);

        Vector3 itemCoords = GetEntityCoords(forkEntity, false);
        Vector3 itemRot = GetEntityRotation(forkEntity, 0);

        float groundZ = itemCoords.Z;
        bool isGround = GetGroundZFor_3dCoord(itemCoords.X, itemCoords.Y, itemCoords.Z, ref groundZ, false);
        if (!isGround)
        {
          //paleta se casto muze bugovat, tak ji zkusime prizvednou nez ji slamnem na zem
          groundZ = groundZ + 1f;
          isGround = GetGroundZFor_3dCoord(itemCoords.X, itemCoords.Y, itemCoords.Z + 1f, ref groundZ, false);
        }
        var forkIt = InventoryManager.ForkliftInventoryCache?.Items?.Where(it => it.X == 0 && it.Y == 0).FirstOrDefault();

        if (isGround && forkIt != null)
        {
          TriggerServerEvent("ofw_carry:PutDownItem", forkIt.Id, JsonConvert.SerializeObject(new PosBag(itemCoords.X, itemCoords.Y, groundZ + 0.01f, itemRot.Z)), true, InventoryManager.ForkliftInventoryCache.Place);
        }

        if (!isGround)
        {
          Notify.Alert("Nerovná zem, nelze položit");
        }
      }
      else
      {
        var carryEntity = NetToObj(carryEntityNet);

        Vector3 itemCoords = GetEntityCoords(carryEntity, false);
        Vector3 itemRot = GetEntityRotation(carryEntity, 0);

        float groundZ = itemCoords.Z;
        bool isGround = GetGroundZFor_3dCoord(itemCoords.X, itemCoords.Y, itemCoords.Z, ref groundZ, false);
        var handsIt = InventoryManager.PlayerInventoryCache?.Items?.Where(it => it.X == -1 && it.Y == 100).FirstOrDefault();

        if (isGround && handsIt != null)
        {
          TriggerServerEvent("ofw_carry:PutDownItem", handsIt.Id, JsonConvert.SerializeObject(new PosBag(itemCoords.X, itemCoords.Y, groundZ + 0.01f, itemRot.Z)), false);
        }

        if (!isGround)
        {
          Notify.Alert("Nerovná zem, nelze položit");
        }
      }
    }

    public void PickUpItem(bool isForklift)
    {
      int? invItemId = PickableCarryItem?.InvItemId;

      if (isForklift)
      {
        var forkIt = InventoryManager.ForkliftInventoryCache?.Items?.Where(it => it.X == 0 && it.Y == 0).FirstOrDefault();

        if (invItemId != null && forkIt == null)
        {
          TriggerServerEvent("ofw_carry:PickUpItem", invItemId.Value, true, InventoryManager.ForkliftInventoryCache.Place);
        }
        else
        {
          Notify.Alert("Máš plné ruce");
        }
      }
      else
      {
        var handsIt = InventoryManager.PlayerInventoryCache?.Items?.Where(it => it.X == -1 && it.Y == 100).FirstOrDefault();

        if (invItemId != null && handsIt == null)
        {
          TriggerServerEvent("ofw_carry:PickUpItem", invItemId.Value, false);
        }
        else
        {
          Notify.Alert("Máš plné ruce");
        }
      }
    }

    [EventHandler("ofw_carry:WorldItemAdd")]
    private async void WorldItemAdd(string carriableString)
    {
      var it = JsonConvert.DeserializeObject<CarriableClientWorldItem>(carriableString);
      _collectionChanging = true;
      await Delay(0);

      if (CarriableCache.ContainsKey(it.InvItemId))
      {
        if (CarriableCache[it.InvItemId].entityId != null)
        {
          int entityId = CarriableCache[it.InvItemId].entityId.Value;
          DeleteObject(ref entityId);
        }
        CarriableCache.Remove(it.InvItemId);
      }

      it.Position = OfwFunctions.PosBagToVector3(it.PosBag);
      CarriableCache.Add(it.InvItemId, it);

      _collectionChanging = false;
    }

    [EventHandler("ofw_carry:WorldItemRemove")]
    private async void WorldItemRemove(int invItemId)
    {
      _collectionChanging = true;
      await Delay(0);

      if (CarriableCache.ContainsKey(invItemId))
      {
        if (CarriableCache[invItemId].entityId != null)
        {
          int entityId = CarriableCache[invItemId].entityId.Value;
          DeleteObject(ref entityId);
        }
        CarriableCache.Remove(invItemId);
      }

      _collectionChanging = false;
    }

    [EventHandler("ofw_carry:SyncCache")]
    private async void SyncCache(string carriableListString)
    {
      var items = JsonConvert.DeserializeObject<List<CarriableClientWorldItem>>(carriableListString);
      _collectionChanging = true;
      await Delay(0);

      foreach (var it in items)
      {
        if (CarriableCache.ContainsKey(it.InvItemId))
        {
          if (CarriableCache[it.InvItemId].entityId != null)
          {
            int entityId = CarriableCache[it.InvItemId].entityId.Value;
            DeleteObject(ref entityId);
          }
          CarriableCache.Remove(it.InvItemId);
        }

        it.Position = OfwFunctions.PosBagToVector3(it.PosBag);
        CarriableCache.Add(it.InvItemId, it);
      }

      _collectionChanging = false;
    }
  }
}
