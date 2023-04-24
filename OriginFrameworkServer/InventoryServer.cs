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
using System.Security.Cryptography;

namespace OriginFrameworkServer
{
  public class InventoryServer : BaseScript
  {
    private static LockObj syncLock = new LockObj("InventoryServer");
    private List<Vector3> groundMarkersCache = new List<Vector3>();

    //Tohle neni idealni, ale resi to spoustu problemu :D
    private static InventoryServer _scriptInstance;
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

        int item_id = 0;
        if (!Int32.TryParse(args[1].ToString(), out item_id))
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neplatné id předmětu");
          return;
        }
        int count = 0;
        if (!Int32.TryParse(args[2].ToString(), out count))
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neplatný počet");
          return;
        }

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

        int item_id = 0;
        if (!Int32.TryParse(args[1].ToString(), out item_id))
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neplatné id předmětu");
          return;
        }
        int count = 0;
        if (!Int32.TryParse(args[2].ToString(), out count))
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neplatný počet");
          return;
        }

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

      RegisterCommand("givebankcash", new Action<int, List<object>, string>(async (source, args, raw) =>
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

        var character = CharacterCaretakerServer.GetPlayerLoggedCharacter(player);
        if (character == null)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Hrac neni lognuty");
          return;
        }

        bool isOrg = false;
        if (args[1].ToString().ToLower().Trim() == "char")
          isOrg = false;
        else if (args[1].ToString().ToLower().Trim() == "org")
          isOrg = true;
        else
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neplatny ucet");
          return;
        }

        if (isOrg && character.OrganizationId == null)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Hráč nemá organizaci");
          return;
        }

        int count = 0;
        if (!Int32.TryParse(args[2].ToString(), out count))
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neplatný počet");
          return;
        }

        using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
        {
          var param = new Dictionary<string, object>();
          param.Add("@charId", character.Id);
          param.Add("@orgId", character.OrganizationId);
          param.Add("@amount", count);

          if (isOrg)
            await VSql.ExecuteAsync("update `organization` set `bank_money` = `bank_money` + @amount where `id` = @orgId", param);
          else
            await VSql.ExecuteAsync("update `character` set `bank_money` = `bank_money` + @amount where `id` = @charId", param);

          await Delay(0);
          sourcePlayer.TriggerEvent("ofw:SuccessNotification", "Má to tam");
        }
      }), false);

      ReloadGroundMarkers();

      _scriptInstance = this;
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

    private async Task<int> FetchItemTypeCountInInventory(int itemId, string place)
    {
      if (string.IsNullOrEmpty(place))
        return 0;

      var param = new Dictionary<string, object>();
      param.Add("@place", place);
      param.Add("@itemid", itemId);
      var result = await VSql.FetchScalarAsync("select sum(`count`) count from `inventory_item` where `place` = @place and `item_id` = @itemid", param);

      if (result == null || result is DBNull)
        return 0;

      return Convert.ToInt32(result);
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
      if (place.StartsWith("char_"))
      {
        int serverId = CharacterCaretakerServer.GetCharLoggedServerId(Int32.Parse(place.Substring(5)));
        if (serverId > 0)
        {
          var player = Players.Where(p => p.Handle == serverId.ToString()).FirstOrDefault();
          if (player != null)
          {
            player.TriggerEvent("ofw:InventoryNotification", "Inventář", "-VŠE");
          }
        }
      }
      return null;
    }

    private async Task<string> GiveItem(string place, int item_id, int count)
    {
      int resultCount = count;

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
      if (place.StartsWith("char_"))
      {
        int serverId = CharacterCaretakerServer.GetCharLoggedServerId(Int32.Parse(place.Substring(5)));
        if (serverId > 0)
        {
          var player = Players.Where(p => p.Handle == serverId.ToString()).FirstOrDefault();
          if (player != null)
          {
            var def = ItemsDefinitions.Items[item_id];
            if (def != null)
            {
              player.TriggerEvent("ofw:InventoryNotification", def.Name, def.FormatAmount(resultCount, true));
            }
          }
        }
      }
      return null;
    }

    private async Task<string> RemoveItem(string place, int item_id, int count)
    {
      int resultCount = count * -1;

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
      if (place.StartsWith("char_"))
      {
        int serverId = CharacterCaretakerServer.GetCharLoggedServerId(Int32.Parse(place.Substring(5)));
        if (serverId > 0)
        {
          var player = Players.Where(p => p.Handle == serverId.ToString()).FirstOrDefault();
          if (player != null)
          {
            var def = ItemsDefinitions.Items[item_id];
            if (def != null)
            {
              player.TriggerEvent("ofw:InventoryNotification", def.Name, def.FormatAmount(resultCount, true));
            }
          }
        }
      }
      return null;
    }

    private async Task<string> RemoveItems(string place, Dictionary<int, int> items)
    {
      if (items == null || items.Count == 0 || string.IsNullOrEmpty(place) || items.Any(it => it.Key <= 0 || it.Value <= 0))
        return "Neznámý inventář, předmět nebo počet";

      var removeItems = new Dictionary<int, int>(items);

      foreach (var item in items)
      {
        if (ItemsDefinitions.Items[item.Key] == null)
          return $"Neplatné id předmětu: {item.Key}";
      }

      var inv = await FetchInventory(place);

      if (inv == null)
        return "Nezdařilo se načtení dat inventáře";
      var sql = new StringBuilder();

      for (int i = 0; i < items.Count; i++)
      {
        int key = items.Keys.ElementAt(i);

        if (!inv.HasEnoughItems(key, items[key]))
          return "Nedostatečný počet předmětu k odstranění";

        while (items[key] > 0)
        {
          InventoryItemBag foundItem = inv.GetNextItemOfType(key);

          if (foundItem == null)
            return "Nedostatečný počet předmětu k odstranění";
          if (foundItem.Count > items[key])
          {
            foundItem.Count = foundItem.Count - items[key];
            items[key] = 0;

            sql.AppendLine($" update `inventory_item` set `count` = '{foundItem.Count}' where `id` = '{foundItem.Id}'; ");
          }
          else
          {
            sql.AppendLine($" delete from `inventory_item` where `id` = '{foundItem.Id}'; ");

            items[key] -= foundItem.Count;
            inv.Items.Remove(foundItem);
          }
        }
      }
      await VSql.ExecuteAsync(sql.ToString(), null);

      BaseScript.TriggerClientEvent("ofw_inventory:InventoryUpdated", place, null);
      if (place.StartsWith("world_"))
        ReloadGroundMarkers();
      if (place.StartsWith("char_"))
      {
        int serverId = CharacterCaretakerServer.GetCharLoggedServerId(Int32.Parse(place.Substring(5)));
        if (serverId > 0)
        {
          var player = Players.Where(p => p.Handle == serverId.ToString()).FirstOrDefault();
          if (player != null)
          {
            foreach (var removeItem in removeItems)
            {
              var def = ItemsDefinitions.Items[removeItem.Key];
              if (def != null)
              {
                player.TriggerEvent("ofw:InventoryNotification", def.Name, def.FormatAmount(removeItem.Value * -1, true));
              }
            }
          }
        }
      }
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

      if (target_x == -1 && target_y < 100 && (ItemsDefinitions.Items[srcItem.ItemId].SpecialSlotType == null || target_y != (int)ItemsDefinitions.Items[srcItem.ItemId].SpecialSlotType))
        return "Sem to nepatří";

      if (ItemsDefinitions.Items[srcItem.ItemId].CarryType != eItemCarryType.Inventory)
      {
        if (target_x == -1 && target_y == 100 && ItemsDefinitions.Items[srcItem.ItemId].CarryType == eItemCarryType.Forklift)
          return "Sem se předmět nevejde";

        if (target_x >= 0 && target_y >= InventoryBag.PlaceToRowCount(targetPlace, ItemsDefinitions.Items[srcItem.ItemId].CarryType))
          return "Sem se předmět nevejde";
      }

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

      if (target_x == -1 && target_y < 100 && (ItemsDefinitions.Items[srcItem.ItemId].SpecialSlotType == null || target_y != (int)ItemsDefinitions.Items[srcItem.ItemId].SpecialSlotType))
        return "Sem to nepatří";

      if (ItemsDefinitions.Items[srcItem.ItemId].CarryType != eItemCarryType.Inventory)
      {
        if (target_x == -1 && target_y == 100 && ItemsDefinitions.Items[srcItem.ItemId].CarryType == eItemCarryType.Forklift)
          return "Sem se předmět nevejde";

        if (target_x >= 0 && target_y >= InventoryBag.PlaceToRowCount(targetPlace, ItemsDefinitions.Items[srcItem.ItemId].CarryType))
          return "Sem se předmět nevejde";
      }

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

    [EventHandler("ofw_inventory:SyncAmmo")]
    private async void SyncAmmo([FromSource] Player source, int itemId, int diff)
    {
      var oid = OIDServer.GetOriginServerID(source);
      if (oid == null)
      {
        source.Drop("Nepodařilo se získat identifikátory uživatele!");
        return;
      }

      if (ItemsDefinitions.Items.Length < itemId - 1 || ItemsDefinitions.Items[itemId] == null)
      {
        Debug.WriteLine($"INV - UseItem - unknown item id {itemId}");
        return;
      }

      var definition = ItemsDefinitions.Items[itemId];
      var playerInv = PlayerOIDToPlace(oid);

      using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
      {
        int ammoCount = await FetchItemTypeCountInInventory(definition.AmmoItemId, playerInv);
        if (ammoCount < diff)
        {
          Debug.WriteLine($"Ammo sync - player {oid.PrimaryIdentifier} tried to sync more ammo used than he had {diff}/{ammoCount}");
          diff = ammoCount;
        }

        await RemoveItem(playerInv, definition.AmmoItemId, diff);
      }
    }

    [EventHandler("ofw_inventory:UseItem")]
    private async void UseItem([FromSource] Player source, int id, string place, int itemId)
    {
      var oid = OIDServer.GetOriginServerID(source);
      if (oid == null)
      {
        source.Drop("Nepodařilo se získat identifikátory uživatele!");
        return;
      }

      if (ItemsDefinitions.Items.Length < itemId - 1 || ItemsDefinitions.Items[itemId] == null)
      {
        Debug.WriteLine($"INV - UseItem - unknown item id {itemId}");
        return;
      }

      var definition = ItemsDefinitions.Items[itemId];

      if (definition.UsableType == eUsableType.None)
      {
        source.TriggerEvent("ofw:ValidationErrorNotification", "Předmět neumíš použít");
        return;
      }

      switch (definition.UsableType)
      {
        case eUsableType.Weapon:
          if (place != PlayerOIDToPlace(oid))
            source.TriggerEvent("ofw:ValidationErrorNotification", "Zbraň musíš mít u sebe");
          var serverItem = await FetchItem(id, place);
          if (serverItem == null || serverItem.ItemId != itemId)
            return;

          int ammoCount = await FetchItemTypeCountInInventory(definition.AmmoItemId, place);
          source.TriggerEvent("ofw_inventory:WeaponUsed", itemId, ammoCount);
          break;
      }
      

    }

    [EventHandler("ofw_inventory:RemoveInventoryItemsCB")]
    private async void RemoveInventoryItemsCB([FromSource] Player source, string playerHandle, string itemsJson, CallbackDelegate callback)
    {
      if (source != null)
      {
        _ = callback("Nelze volat z klienta");
        return;
      }

      Dictionary<int, int> items = JsonConvert.DeserializeObject<Dictionary<int, int>>(itemsJson);

      var player = Players.Where(p => p.Handle == playerHandle).FirstOrDefault();
      if (player == null)
      {
        _ = callback("Nepodarilo se najit hrace");
        return;
      }
      int charId = CharacterCaretakerServer.GetPlayerLoggedCharacterId(player);
      if (charId <= 0)
      {
        _ = callback("Hrac neni lognuty");
        return;
      }
      using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
      {
        string result = await RemoveItems($"char_{charId}", items);
        await Delay(0); //MRDKA. Nedavat pryc. Potrebujem zpatky do main threadu
        if (result == null)
        {
          _ = callback(String.Empty);
          return;
        }
        else
        {
          _ = callback(result);
          return;
        }
      }
    }

    [EventHandler("ofw_inventory:GetCharBankBalance")]
    private async void GetCharBankBalance([FromSource] Player source, NetworkCallbackDelegate callback)
    {
      if (source == null)
      {
        _ = callback(false, 0);
        return;
      }

      var character = CharacterCaretakerServer.GetPlayerLoggedCharacter(source);
      if (character == null)
      {
        _ = callback(false, 0);
        source.Drop("Nepodařilo se získat identifikátory uživatele!");
        return;
      }

      var param = new Dictionary<string, object>();
      param.Add("@charId", character.Id);
      var result = await VSql.FetchScalarAsync("select `bank_money` from `character` where id = @charId", param);

      if (result == null || result == DBNull.Value)
        _ = callback(false, 0);
      else
        _ = callback(true, Convert.ToInt32(result));
    }

    [EventHandler("ofw_inventory:GetOrganizationBankBalance")]
    private async void GetOrganizationBankBalance([FromSource] Player source, NetworkCallbackDelegate callback)
    {
      if (source == null)
      {
        _ = callback(false, 0);
        return;
      }

      var character = CharacterCaretakerServer.GetPlayerLoggedCharacter(source);
      if (character == null)
      {
        _ = callback(false, 0);
        source.Drop("Nepodařilo se získat identifikátory uživatele!");
        return;
      }

      if (character.OrganizationId == null || !OrganizationServer.Organizations.Any(org => org.Id == character.OrganizationId && (org.Owner == character.Id || org.Managers.Any(c => c.CharId == character.Id))))
      {
        _ = callback(false, 0);
        return;
      }

      var param = new Dictionary<string, object>();
      param.Add("@orgId", character.OrganizationId);
      var result = await VSql.FetchScalarAsync("select `bank_money` from `organization` where `id` = @orgId", param);

      if (result == null || result == DBNull.Value)
        _ = callback(false, 0);
      else
        _ = callback(true, Convert.ToInt32(result));
    }

    [EventHandler("ofw_inventory:WithdrawCharBankBalance")]
    private async void WithdrawCharBankBalance([FromSource] Player source, int amount)
    {
      if (source == null)
      {
        return;
      }

      var character = CharacterCaretakerServer.GetPlayerLoggedCharacter(source);
      if (character == null)
      {
        source.Drop("Nepodařilo se získat identifikátory uživatele!");
        return;
      }

      if (amount <= 0)
      {
        source.TriggerEvent("ofw:ValidationErrorNotification", "Neplatná částka");
        return;
      }

      using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
      {
        var param = new Dictionary<string, object>();
        param.Add("@charId", character.Id);
        var bankResult = await VSql.FetchScalarAsync("select `bank_money` from `character` where id = @charId", param);
        int bankMoney = Convert.ToInt32(bankResult);
        if (bankMoney < amount)
        {
          source.TriggerEvent("ofw:ValidationErrorNotification", "Na účtě není tolik peněz");
          return;
        }

        string result = await GiveItem($"char_{character.Id}", 17, amount);
        if (result != null)
        {
          source.TriggerEvent("ofw:ValidationErrorNotification", result);
          return;
        }

        param.Add("@amount", amount);
        await VSql.ExecuteAsync("update `character` set `bank_money` = `bank_money` - @amount where id = @charId", param);
      }
    }

    [EventHandler("ofw_inventory:WithdrawOrganizationBankBalance")]
    private async void WithdrawOrganizationBankBalance([FromSource] Player source, int amount)
    {
      if (source == null)
      {
        return;
      }

      var character = CharacterCaretakerServer.GetPlayerLoggedCharacter(source);
      if (character == null)
      {
        source.Drop("Nepodařilo se získat identifikátory uživatele!");
        return;
      }

      if (amount <= 0)
      {
        source.TriggerEvent("ofw:ValidationErrorNotification", "Neplatná částka");
        return;
      }

      if (character.OrganizationId == null || !OrganizationServer.Organizations.Any(org => org.Id == character.OrganizationId && (org.Owner == character.Id || org.Managers.Any(c => c.CharId == character.Id))))
      {
        source.TriggerEvent("ofw:ValidationErrorNotification", "Nemáš organizaci nebo nejsi manažer");
        return;
      }

      using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
      {
        var param = new Dictionary<string, object>();
        param.Add("@orgId", character.OrganizationId);
        var bankResult = await VSql.FetchScalarAsync("select `bank_money` from `organization` where id = @orgId", param);
        int bankMoney = Convert.ToInt32(bankResult);
        if (bankMoney < amount)
        {
          source.TriggerEvent("ofw:ValidationErrorNotification", "Na účtě není tolik peněz");
          return;
        }

        string result = await GiveItem($"char_{character.Id}", 17, amount);
        if (result != null)
        {
          source.TriggerEvent("ofw:ValidationErrorNotification", result);
          return;
        }

        param.Add("@amount", amount);
        await VSql.ExecuteAsync("update `organization` set `bank_money` = `bank_money` - @amount where id = @orgId", param);
      }
    }

    #region public
    public static async void PayBankOrganization(Player player, int organizationId, int amount, Func<Player, Task<bool>> OnSuccess, Action<Player, string> OnError)
    {
      if (amount <= 0)
      {
        OnError(player, "Neplatná částka");
        return;
      }
      using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
      {
        var param = new Dictionary<string, object>();
        param.Add("@orgId", organizationId);
        var bankResult = await VSql.FetchScalarAsync("select `bank_money` from `organization` where id = @orgId", param);
        int bankMoney = Convert.ToInt32(bankResult);
        if (bankMoney < amount)
        {
          await Delay(0);
          OnError(player, "Na účtě není tolik peněz");
          return;
        }
        param.Add("@amount", amount);
        await VSql.ExecuteAsync("update `organization` set `bank_money` = `bank_money` - @amount where id = @orgId", param);
        await Delay(0);
        
        var wasSuccess = await OnSuccess(player);

        if (!wasSuccess)
        {
          await VSql.ExecuteAsync("update `organization` set `bank_money` = `bank_money` + @amount where id = @orgId", param);
        }
      }
    }
    public static async void PayBankCharacter(Player player, int characterId, int amount, Func<Player, Task<bool>> OnSuccess, Action<Player, string> OnError)
    {
      if (amount <= 0)
      {
        OnError(player, "Neplatná částka");
        return;
      }
      using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
      {
        var param = new Dictionary<string, object>();
        param.Add("@charId", characterId);
        var bankResult = await VSql.FetchScalarAsync("select `bank_money` from `character` where id = @charId", param);
        int bankMoney = Convert.ToInt32(bankResult);
        if (bankMoney < amount)
        {
          await Delay(0);
          OnError(player, "Na účtě není tolik peněz");
          return;
        }
        param.Add("@amount", amount);
        await VSql.ExecuteAsync("update `character` set `bank_money` = `bank_money` - @amount where id = @charId", param);
        await Delay(0);

        var wasSuccess = await OnSuccess(player);

        if (!wasSuccess)
        {
          await VSql.ExecuteAsync("update `character` set `bank_money` = `bank_money` + @amount where id = @charId", param);
        }
      }
    }

    public static async void PayItem(Player player, string fromPlace, int itemId, int amount, Func<Player, Task<bool>> OnSuccess, Action<Player, string> OnError)
    {
      if (amount <= 0)
      {
        OnError(player, "Neplatný počet");
        return;
      }
      if (_scriptInstance == null)
      {
        OnError(player, "Server instance error");
        return;
      }

      using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
      {
        var resRemove = await _scriptInstance.RemoveItem(fromPlace, itemId, amount);
        if (resRemove != null)
        { 
          await Delay(0);
          OnError(player, resRemove);
          return;
        }

        var wasSuccess = await OnSuccess(player);

        if (!wasSuccess)
        {
          await Delay(0);
          await _scriptInstance.GiveItem(fromPlace, itemId, amount);
        }
      }
    }
    #endregion
  }
}
