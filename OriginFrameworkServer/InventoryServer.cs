using CitizenFX.Core;
using OriginFrameworkData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;
using OriginFrameworkData.DataBags;
using CitizenFX.Core.Native;
using System.Xml.Linq;
using System.Diagnostics;
using Debug = CitizenFX.Core.Debug;

namespace OriginFrameworkServer
{
  public class InventoryServer : BaseScript
  {
    private LockObj syncLock = new LockObj("InventoryServer");
    private List<Vector3> groundMarkersCache = new List<Vector3>();
    public InventoryServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
    }

    private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.InventoryServer))
        return;

      RegisterCommand("giveitem", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        var sourcePlayer = Players.Where(p => p.Handle == source.ToString()).FirstOrDefault();
        if (sourcePlayer == null)
          return;

        if (!CharacterCaretakerServer.HasPlayerAdminLevel(sourcePlayer, 10))
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Nedostatecne opravneni");
          return;
        }

        if (args == null || args.Count != 3)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Natplatne pocet parametru");
          return;
        }

        var player = Players.Where(p => p.Handle == args[0].ToString()).FirstOrDefault();
        if (player == null)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Nepodarilo se najit hrace");
          return;
        }

        int charId = CharacterCaretakerServer.GetPlayerLoggedCharacterId(player);
        if (charId <= 0)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Hrac neni lognuty");
          return;
        }
        var item_id = Convert.ToInt32(args[1]);
        var count = Convert.ToInt32(args[2]);

        using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
        {
          string result = await GiveItem($"char_{charId}", item_id, count);
          if (result == null)
            sourcePlayer.TriggerEvent("ofw:SuccessNotification", "Má to tam");
          else
            sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", result);
        }
      }), false);

      RegisterCommand("removeitem", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        var sourcePlayer = Players.Where(p => p.Handle == source.ToString()).FirstOrDefault();
        if (sourcePlayer == null)
          return;

        if (!CharacterCaretakerServer.HasPlayerAdminLevel(sourcePlayer, 10))
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Nedostatecne opravneni");
          return;
        }

        if (args == null || args.Count != 3)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Natplatne pocet parametru");
          return;
        }

        var player = Players.Where(p => p.Handle == args[0].ToString()).FirstOrDefault();
        if (player == null)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Nepodarilo se najit hrace");
          return;
        }

        int charId = CharacterCaretakerServer.GetPlayerLoggedCharacterId(player);
        if (charId <= 0)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Hrac neni lognuty");
          return;
        }
        var item_id = Convert.ToInt32(args[1]);
        var count = Convert.ToInt32(args[2]);

        using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
        {
          string result = await RemoveItem($"char_{charId}", item_id, count);
          if (result == null)
            sourcePlayer.TriggerEvent("ofw:SuccessNotification", "Item vymazany");
          else
            sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", result);
        }
      }), false);

      RegisterCommand("resetinventory", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        var sourcePlayer = Players.Where(p => p.Handle == source.ToString()).FirstOrDefault();
        if (sourcePlayer == null)
          return;

        if (!CharacterCaretakerServer.HasPlayerAdminLevel(sourcePlayer, 10))
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Nedostatecne opravneni");
          return;
        }

        if (args == null || args.Count != 1)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Natplatne pocet parametru");
          return;
        }

        var player = Players.Where(p => p.Handle == args[0].ToString()).FirstOrDefault();
        if (player == null)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Nepodarilo se najit hrace");
          return;
        }

        int charId = CharacterCaretakerServer.GetPlayerLoggedCharacterId(player);
        if (charId <= 0)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Hrac neni lognuty");
          return;
        }

        using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
        {
          string result = await ResetInventory($"char_{charId}");
          if (result == null)
            sourcePlayer.TriggerEvent("ofw:SuccessNotification", "Inventar resetovany");
          else
            sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", result);
        }
      }), false);

      ReloadGroundMarkers();

      InternalDependencyManager.Started(eScriptArea.InventoryServer);
    }

    #region private
    private string PlayerOIDToPlace(OIDBag oid)
    {
      int charid = CharacterCaretakerServer.GetPlayerLoggedCharacterId(oid.OID);

      if (charid <= 0)
        return null;

      return $"char_{charid}";
    }

    private async void ReloadGroundMarkers()
    {
      var result = await VSql.FetchAllAsync("select distinct(`place`) from `inventory_item` where `place` like 'world_%'", null);
      groundMarkersCache.Clear();
      if (result == null || result.Count <= 0)
        return;

      foreach (var row in result)
      {
        var splits = ((string)row["place"]).Split('_');
        int x, y;
        if (splits.Length != 3 || !Int32.TryParse(splits[1], out x) || !Int32.TryParse(splits[2], out y))
        {
          Debug.WriteLine($" !!! Inventory ground markers cant parse {row["place"]}");
          continue;
        }
        else
        {
          groundMarkersCache.Add(new Vector3((float)(x * 2) + 1, (float)(y * 2) + 1, 0f));
        }
      }

      TriggerClientEvent("ofw_inventory:GroundMarkersUpdated", JsonConvert.SerializeObject(groundMarkersCache));
    }
    #endregion

    #region private fetch
    private async Task<InventoryBag> FetchInventory(string place)
    {
      if (string.IsNullOrEmpty(place))
        return null;

      InventoryBag inv = new InventoryBag(place);

      var param = new Dictionary<string, object>();
      param.Add("@place", place);
      var result = await VSql.FetchAllAsync("select * from `inventory_item` where `place` = @place", param);

      if (result == null || result.Count <= 0)
        return inv;

      foreach (var row in result)
      {
        inv.Items.Add(InventoryItemBag.ParseFromSql(row));
      }

      return inv;
    }

    private async Task<InventoryItemBag> FetchItem(int id, string place)
    {
      if (id <= 0 || place == null)
        return null;

      var param = new Dictionary<string, object>();
      param.Add("@id", id);
      param.Add("@place", place);
      var result = await VSql.FetchAllAsync("select * from `inventory_item` where `id` = @id and `place` = @place", param);

      if (result == null || result.Count <= 0)
        return null;

      return InventoryItemBag.ParseFromSql(result[0]);
    }

    private async Task<InventoryItemBag> FetchItem(string place, int x, int y)
    {
      if (string.IsNullOrEmpty(place))
        return null;

      var param = new Dictionary<string, object>();
      param.Add("@place", place);
      param.Add("@x", x);
      param.Add("@y", y);
      var result = await VSql.FetchAllAsync("select * from `inventory_item` where `place` = @place and `x` = @x and `y` = @y", param);

      if (result == null || result.Count <= 0)
        return null;

      return InventoryItemBag.ParseFromSql(result[0]);
    }
    #endregion

    #region inventory operations
    private async Task<string> ResetInventory(string place)
    {
      if (string.IsNullOrEmpty(place))
        return "Neznámý předmět nebo inventář";

      var inv = await FetchInventory(place);

      if (inv == null)
        return "Nezdařilo se načtení dat inventáře";

      var param = new Dictionary<string, object>();
      param.Add("@place", place);
      await VSql.ExecuteAsync("delete from `inventory_item` where `place` = @place", param);
      BaseScript.TriggerClientEvent("ofw_inventory:InventoryUpdated", place, null);
      if (place.StartsWith("world_"))
        ReloadGroundMarkers();
      return null;
    }

    private async Task<string> GiveItem(string place, int item_id, int count)
    {
      if (item_id <= 0 || string.IsNullOrEmpty(place))
        return "Neznámý předmět nebo inventář";

      if (count <= 0)
        return $"Neplatné množství: {count}";

      var itemDef = ItemsDefinitions.Items[item_id];
      if (itemDef == null)
        return $"Neplatné id předmětu: {item_id}";

      var inv = await FetchInventory(place);

      if (inv == null)
        return "Nezdařilo se načtení dat inventáře";

      var sql = new StringBuilder();

      int x, y;
      while (count > 0)
      {
        InventoryItemBag partialStack;

        if (!inv.GetNextPossibleSlot(out x, out y, item_id, out partialStack))
          return "V inventáři není místo";

        if (partialStack != null)
        {
          int amt = itemDef.StackSize - partialStack.Count;
          if (count - amt < 0)
            amt = count;
          count -= amt;

          partialStack.Count += amt;

          sql.AppendLine($" update `inventory_item` set `count` = '{partialStack.Count}' where `id` = '{partialStack.Id}'; ");
        }
        else
        {
          inv.Items.Add(new InventoryItemBag { X = x, Y = y });
          int amt = itemDef.StackSize;
          if (count - amt < 0)
            amt = count;
          count -= amt;

          sql.AppendLine($" insert into `inventory_item` (`place`, `item_id`, `x`, `y`, `count`) VALUES ('{place}', '{item_id}', '{x}', '{y}', '{amt}'); ");
        }
      }

      //Debug.WriteLine("INVENTORY - GiveItem sql: " + sql.ToString());
      await VSql.ExecuteAsync(sql.ToString(), null);
      BaseScript.TriggerClientEvent("ofw_inventory:InventoryUpdated", place, null);
      if (place.StartsWith("world_"))
        ReloadGroundMarkers();
      return null;
    }

    private async Task<string> RemoveItem(string place, int item_id, int count)
    {
      if (item_id <= 0 || string.IsNullOrEmpty(place))
        return "Neznámý předmět nebo inventář";

      if (count <= 0)
        return $"Neplatné množství: {count}";

      var itemDef = ItemsDefinitions.Items[item_id];
      if (itemDef == null)
        return $"Neplatné id předmětu: {item_id}";

      var inv = await FetchInventory(place);

      if (inv == null)
        return "Nezdařilo se načtení dat inventáře";

      if (!inv.HasEnoughItems(item_id, count))
        return "Nedostatečný počet předmětu k odstranění";

      var sql = new StringBuilder();

      while (count > 0)
      {
        InventoryItemBag foundItem = inv.GetNextItemOfType(item_id);

        if (foundItem == null)
          return "Nedostatečný počet předmětu k odstranění";

        if (foundItem.Count > count)
        {
          foundItem.Count = foundItem.Count - count;
          count = 0;

          sql.AppendLine($" update `inventory_item` set `count` = '{foundItem.Count}' where `id` = '{foundItem.Id}'; ");
        }
        else
        {
          sql.AppendLine($" delete from `inventory_item` where `id` = '{foundItem.Id}'; ");

          count -= foundItem.Count;
          inv.Items.Remove(foundItem);
        }
      }

      await VSql.ExecuteAsync(sql.ToString(), null);
      BaseScript.TriggerClientEvent("ofw_inventory:InventoryUpdated", place, null);
      if (place.StartsWith("world_"))
        ReloadGroundMarkers();
      return null;
    }

    private async Task<string> SplitItem(int id, string sourcePlace, string targetPlace, int target_x, int target_y, int count)
    {
      if (id <= 0 || string.IsNullOrEmpty(sourcePlace) || string.IsNullOrEmpty(targetPlace))
        return "Neznámý předmět nebo inventář";

      var srcItem = await FetchItem(id, sourcePlace);
      var targetItem = await FetchItem(targetPlace, target_x, target_y);

      if (srcItem == null)
        return "Rozdělovaný předmět nenalezen";

      if (srcItem.Place == targetPlace && srcItem.X == target_x && srcItem.Y == target_y)
        return "Nelze rozdělit sám do sebe";

      if (targetItem != null)
        return "Nelze rozdělit do plného slotu";

      if (count <= 0)
        return "Neplatný počet";

      if (count > srcItem.Count)
        count = srcItem.Count;

      if (srcItem.Count == count)
      {
        var param = new Dictionary<string, object>();
        param.Add("@id_source", id);
        param.Add("@place", targetPlace);
        param.Add("@target_x", target_x);
        param.Add("@target_y", target_y);
        await VSql.ExecuteAsync($" update `inventory_item` set `x` = @target_x, `y` = @target_y, `place` = @place where `id` = @id_source; ", param);
        BaseScript.TriggerClientEvent("ofw_inventory:InventoryUpdated", srcItem.Place, targetPlace);
        return null;
      }
      else
      {
        var param = new Dictionary<string, object>();
        param.Add("@id_source", id);
        param.Add("@place", targetPlace);
        param.Add("@target_x", target_x);
        param.Add("@target_y", target_y);
        await VSql.ExecuteAsync($" update `inventory_item` set `count` = '{srcItem.Count - count}' where `id` = @id_source; " +
                                $" insert into `inventory_item` (`place`, `item_id`, `x`, `y`, `count`) VALUES (@place, '{srcItem.ItemId}', @target_x, @target_y, '{count}');", param);
        BaseScript.TriggerClientEvent("ofw_inventory:InventoryUpdated", srcItem.Place, targetPlace);

        if (sourcePlace.StartsWith("world_") || targetPlace.StartsWith("world_"))
          ReloadGroundMarkers();
        return null;
      }
    }
      
    private async Task<string> MoveOrMergeItem(int id, string sourcePlace, string targetPlace, int target_x, int target_y)
    {
      if (id <= 0 || string.IsNullOrEmpty(sourcePlace) || string.IsNullOrEmpty(targetPlace))
        return "Neznámý předmět nebo inventář";

      var srcItem = await FetchItem(id, sourcePlace);
      var targetItem = await FetchItem(targetPlace, target_x, target_y);

      if (srcItem == null)
        return "Přesouvaný předmět nenalezen";

      if (srcItem.Place == targetPlace && srcItem.X == target_x && srcItem.Y == target_y)
        return "Nelze přesunout sám na sebe";

      if (target_x == -1 && (ItemsDefinitions.Items[srcItem.ItemId].SpecialSlotType == null || target_y != (int)ItemsDefinitions.Items[srcItem.ItemId].SpecialSlotType))
        return "Sem to nepatří";

      if (targetItem == null) //move
      {
        var param = new Dictionary<string, object>();
        param.Add("@id_source", id);
        param.Add("@place", targetPlace);
        param.Add("@target_x", target_x);
        param.Add("@target_y", target_y);
        await VSql.ExecuteAsync($" update `inventory_item` set `x` = @target_x, `y` = @target_y, `place` = @place where `id` = @id_source; ", param);
        BaseScript.TriggerClientEvent("ofw_inventory:InventoryUpdated", srcItem.Place, targetPlace);
        if (sourcePlace.StartsWith("world_") || targetPlace.StartsWith("world_"))
          ReloadGroundMarkers();
        return null;
      }
      else //merge
      {
        if (srcItem.Id == targetItem.Id)
          return "Nelze přesunout sám na sebe";

        var sql = new StringBuilder();

        if (srcItem.ItemId != targetItem.ItemId || srcItem.Metadata != targetItem.Metadata)
          return "Předmět nelze sloučit";

        int stackSize = ItemsDefinitions.Items[targetItem.ItemId].StackSize;

        if (targetItem.Count >= stackSize)
          return "Cílový předmět už je plný stack";

        if (srcItem.Count + targetItem.Count <= stackSize)
          sql.AppendLine($" delete from `inventory_item` where `id` = '{id}'; ");
        else
          sql.AppendLine($" update `inventory_item` set `count` = {srcItem.Count - (stackSize - targetItem.Count)} where `id` = '{id}'; ");

        int newCount = srcItem.Count + targetItem.Count;
        if (newCount > stackSize)
          newCount = stackSize;
        sql.AppendLine($" update `inventory_item` set `count` = {newCount} where `id` = '{targetItem.Id}'; ");

        await VSql.ExecuteAsync(sql.ToString(), null);
        BaseScript.TriggerClientEvent("ofw_inventory:InventoryUpdated", srcItem.Place, targetItem.Place);
        if (sourcePlace.StartsWith("world_") || targetPlace.StartsWith("world_"))
          ReloadGroundMarkers();
        return null;
      }
    }
    #endregion

    [EventHandler("ofw_inventory:ReloadInventory")]
    private async void ReloadInventory([FromSource] Player source, string rightInvPlace)
    {
      if (source == null)
        return;

      var oid = OIDServer.GetOriginServerID(source);
      if (oid == null)
      {
        source.Drop("Nepodařilo se získat identifikátory uživatele!");
        return;
      }

      using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
      {
        var inv = await FetchInventory(PlayerOIDToPlace(oid));
        InventoryBag rightInv = null;
        if (!string.IsNullOrEmpty(rightInvPlace))
          rightInv = await FetchInventory(rightInvPlace);
        source.TriggerEvent("ofw_inventory:InventoryLoaded", JsonConvert.SerializeObject(inv), JsonConvert.SerializeObject(rightInv));
      }
    }

    [EventHandler("ofw_inventory:Operation_MoveOrMerge")]
    private async void Operation_MoveOrMerge([FromSource] Player source, int id, string source_place, string target_place, int target_x, int target_y)
    {
      if (source == null)
        return;

      var oid = OIDServer.GetOriginServerID(source);
      if (oid == null)
      {
        source.Drop("Nepodařilo se získat identifikátory uživatele!");
        return;
      }

      using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
      {
        string result = await MoveOrMergeItem(id, source_place, target_place, target_x, target_y);
        if (result != null)
          source.TriggerEvent("ofw_inventory:InventoryNotUpdated", result);
      }
    }

    [EventHandler("ofw_inventory:Operation_Split")]
    private async void Operation_Split([FromSource] Player source, int id, string source_place, string target_place, int target_x, int target_y, int count)
    {
      if (source == null)
        return;

      var oid = OIDServer.GetOriginServerID(source);
      if (oid == null)
      {
        source.Drop("Nepodařilo se získat identifikátory uživatele!");
        return;
      }

      using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
      {
        string result = await SplitItem(id, source_place, target_place, target_x, target_y, count);
        if (result != null)
          source.TriggerEvent("ofw_inventory:InventoryNotUpdated", result);
      }
    }

    [EventHandler("ofw_inventory:GroundMarkersReqUpdate")]
    private async void GroundMarkersReqUpdate([FromSource] Player source)
    {
      source.TriggerEvent("ofw_inventory:GroundMarkersUpdated", JsonConvert.SerializeObject(groundMarkersCache));
    }
  }
}
