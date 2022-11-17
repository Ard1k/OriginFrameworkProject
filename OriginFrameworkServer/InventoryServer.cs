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

namespace OriginFrameworkServer
{
  public class InventoryServer : BaseScript
  {
    public InventoryServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
    }

    private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      RegisterCommand("giveitem", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        var sourcePlayer = Players.Where(p => p.Handle == source.ToString()).FirstOrDefault();
        if (sourcePlayer == null)
          return;

        if (!CharacterCaretakerServer.HasPlayerAdminLevel(sourcePlayer, 10));

        if (args == null || args.Count != 3)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Natplatne parametry");
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

        if (await GiveItem($"char_{charId}", item_id, count) == true)
          sourcePlayer.TriggerEvent("ofw:SuccessNotification", "Ma to tam");
        else
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neco se pokazilo");
      }), false);
    }

    #region private
    private string PlayerOIDToPlace(OIDBag oid)
    {
      int charid = CharacterCaretakerServer.GetPlayerLoggedCharacterId(oid.OID);

      if (charid <= 0)
        return null;

      return $"char_{charid}";
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
    #endregion

    #region inventory operations
    private async Task<bool> GiveItem(string place, int item_id, int count)
    {
      if (item_id <= 0 || string.IsNullOrEmpty(place))
        return false;

      if (count <= 0)
        return false;

      var itemDef = ItemsDefinitions.Items[item_id];
      if (itemDef == null)
        return false;

      var inv = await FetchInventory(place);

      if (inv == null)
        return false;

      var sql = new StringBuilder();

      int x, y;
      while (count > 0)
      {
        if (!inv.GetNextEmptySlot(out x, out y))
          return false;

        inv.Items.Add(new InventoryItemBag { X = x, Y = y });
        int amt = itemDef.StackSize;
        if (count - amt < 0)
          amt = count;
        count -= amt;

        sql.AppendLine($" insert into `inventory_item` (`place`, `item_id`, `x`, `y`, `count`) VALUES ('{place}', '{item_id}', '{x}', '{y}', '{amt}'); ");
      }

      //Debug.WriteLine("INVENTORY - GiveItem sql: " + sql.ToString());
      await VSql.ExecuteAsync(sql.ToString(), null);
      return true;
    }
    #endregion

    [EventHandler("ofw_inventory:GetMyCharacterInventory")]
    private async void GetMyCharacterInventory([FromSource] Player source)
    {
      if (source == null)
        return;

      var oid = OIDServer.GetOriginServerID(source);
      if (oid == null)
      {
        source.Drop("Nepodařilo se získat identifikátory uživatele!");
        return;
      }

      var inv = await FetchInventory(PlayerOIDToPlace(oid));

      source.TriggerEvent("ofw_inventory:InventoryUpdated", JsonConvert.SerializeObject(inv));
    }
  }
}
