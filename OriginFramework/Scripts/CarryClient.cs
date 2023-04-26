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
    public static Dictionary<int, CarriableClientWorldItem> CarriableCache = new Dictionary<int, CarriableClientWorldItem>();
    public static CarriableClientWorldItem PickableCarryItem = null;
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

      var playerPos = Game.PlayerPed.Position;
      bool anyInRange = false;
      float closestDistSq = 99999f;
      bool pickableSet = false;

      foreach (var it in CarriableCache) 
      {
        if (it.Value.entityId != null && !DoesEntityExist(it.Value.entityId.Value))
          it.Value.entityId = null;

        float distSq = it.Value.Position.DistanceToSquared2D(playerPos);
        bool isInRange = distSq <= _renderDistSq;
        if (isInRange)
          anyInRange = true;

        if (!isInRange && it.Value.entityId != null)
        {
          int entityId = it.Value.entityId.Value;
          DeleteObject(ref entityId);
          it.Value.entityId = null;
        }
        else if (isInRange && it.Value.entityId == null)
        {
          var itemDef = ItemsDefinitions.Items[it.Value.ItemId];
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
          if (PickableCarryItem == null || distSq < closestDistSq)
          {
            PickableCarryItem = it.Value;
            pickableSet = true;
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

      if (carryEntityNet != 0)
      {
        DisplayHelpTextThisFrame("OFW_CARRYEX_PUT", true);

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

        if (IsControlJustPressed(0, 38) || IsDisabledControlJustPressed(0, 38))
        {
          PutDownItem();
        }
      }

      if (PickableCarryItem != null)
      {
        DisplayHelpTextThisFrame("OFW_CARRYEX_PICK", true);

        if (IsControlJustPressed(0, 38) || IsDisabledControlJustPressed(0, 38))
        {
          PickUpItem();
        }
      }
    }

    private async Task OnSlowTick()
    {
      await Delay(200);
      var handsIt = InventoryManager.PlayerInventoryCache?.Items?.Where(it => it.X == -1 && it.Y == 100).FirstOrDefault();
      if (handsIt != null && carryEntityNet == 0)
      {
        await SetItemCarry(ItemsDefinitions.Items[handsIt.ItemId]);
      }
      else if (handsIt == null && carryEntityNet != 0)
      {
        if (!NetworkDoesNetworkIdExist(carryEntityNet) || !NetworkDoesEntityExistWithNetworkId(carryEntityNet))
        {
          carryEntityNet = 0;
          return;
        }
        var carryEntity = NetworkGetEntityFromNetworkId(carryEntityNet);

        DeleteObject(ref carryEntity);
        carryEntityNet = 0;
        ClearPedTasksImmediately(Game.PlayerPed.Handle);
      }
      else if (handsIt != null && carryEntityNet != 0)
      {
        if (!IsEntityPlayingAnim(Game.PlayerPed.Handle, ItemsDefinitions.Items[handsIt.ItemId].CarryInfo.CarryAnim, "idle", 3))
          TaskPlayAnim(Game.PlayerPed.Handle, ItemsDefinitions.Items[handsIt.ItemId].CarryInfo.CarryAnim, "idle", 4.0f, 1.0f, -1, 49, 0f, false, false, false);
      }
    }

    private async Task SetItemCarry(ItemDefinition itDef)
    {
      if (itDef.CarryType != eItemCarryType.Hands || itDef.CarryInfo == null)
        return;

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

    private async Task LoadAnimDict(string anim)
    {
      RequestAnimDict(anim);
      while (!HasAnimDictLoaded(anim))
      {
        await Delay(50);
      }
    }

    public void PutDownItem()
    {
      var carryEntity = NetToObj(carryEntityNet);

      Vector3 itemCoords = GetEntityCoords(carryEntity, false);
      Vector3 itemRot = GetEntityRotation(carryEntity, 0);
      
      float groundZ = itemCoords.Z;
      bool isGround = GetGroundZFor_3dCoord(itemCoords.X, itemCoords.Y, itemCoords.Z, ref groundZ, false);
      var handsIt = InventoryManager.PlayerInventoryCache?.Items?.Where(it => it.X == -1 && it.Y == 100).FirstOrDefault();

      if (isGround && handsIt != null)
      {
        TriggerServerEvent("ofw_carry:PutDownItem", handsIt.Id, JsonConvert.SerializeObject(new PosBag(itemCoords.X, itemCoords.Y, groundZ + 0.01f, itemRot.Z)));
      }
    }

    public void PickUpItem()
    {
      int? invItemId = PickableCarryItem?.InvItemId;
      var handsIt = InventoryManager.PlayerInventoryCache?.Items?.Where(it => it.X == -1 && it.Y == 100).FirstOrDefault();

      if (invItemId != null && handsIt == null)
      {
        TriggerServerEvent("ofw_carry:PickUpItem", invItemId.Value);
      }
      else
      {
        Notify.Alert("Máš plné ruce");
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
