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
  public class CharacterCaretakerServer : BaseScript
  {
    //oid + chardata
    public static Dictionary<int, CharacterBag> LoggedPlayers = new Dictionary<int, CharacterBag>();

    public CharacterCaretakerServer()
    {
    }

    [EventHandler("ofw_char_caretaker:UpdateCharacterServer")]
    private async void UpdateCharacter([FromSource] Player source, string charData)
    {
      if (source == null)
        return;

      var oid = OIDServer.GetOriginServerID(source);
      if (oid == null)
      {
        source.Drop("Nepodařilo se získat identifikátory uživatele!");
        return;
      }

      CharacterBag charFromClient = null;

      if (charData != null)
        charFromClient = JsonConvert.DeserializeObject<CharacterBag>(charData);

      if (!LoggedPlayers.ContainsKey(oid.OID) || charFromClient == null || LoggedPlayers[oid.OID].Id != charFromClient.Id)
      {
        source.TriggerEvent("ofw_char_caretaker:ForceRelogin");
        return;
      }

      var param = new Dictionary<string, object>();
      param.Add("@idChar", LoggedPlayers[oid.OID].Id);

      StringBuilder sql = new StringBuilder();

      if (charFromClient.LastKnownPos != null)
      {
        param.Add("@pos", JsonConvert.SerializeObject(charFromClient.LastKnownPos));
        sql.AppendLine("update `character` set `pos` = @pos where `id` = @idChar;");
        LoggedPlayers[oid.OID].LastKnownPos = charFromClient.LastKnownPos;
      }

      string sqlString = sql.ToString();
      if (!string.IsNullOrEmpty(sqlString))
        VSql.ExecuteAsync(sqlString, param);
    }


    public static async Task<bool> LoginPlayer(OIDBag oid, int idCharacter)
    {
      var param = new Dictionary<string, object>();
      param.Add("@identifier", oid.PrimaryIdentifier);
      param.Add("@id_char", idCharacter);
      var result = await VSql.FetchAllAsync("select c.*, u.`admin_level` from `user` u, `character` c where u.`identifier` = c.`user_identifier` and c.`user_identifier` = @identifier and c.`id` = @id_char", param);

      if (result == null || result.Count != 1)
        return false;

      if (!LoggedPlayers.ContainsKey(oid.OID))
        LoggedPlayers.Add(oid.OID, null);

      var character = CharacterBag.ParseFromSql(result[0]);
      if (character.IsNew)
      {
        await VehicleVendorServer.GiveFirstCar(character.Id);
        var startingItems = new Dictionary<int, int>();
        var random = new Random();
        var torso = ItemsDefinitions.Items.Where(x => x?.SpecialSlotType != null && x.SpecialSlotType == eSpecialSlotType.Torso).OrderBy(x => random.Next()).FirstOrDefault();
        if (torso != null)
          startingItems.Add(torso.ItemId, 1);
        var legs = ItemsDefinitions.Items.Where(x => x?.SpecialSlotType != null && x.SpecialSlotType == eSpecialSlotType.Legs).OrderBy(x => random.Next()).FirstOrDefault();
        if (legs != null)
          startingItems.Add(legs.ItemId, 1);
        var boots = ItemsDefinitions.Items.Where(x => x?.SpecialSlotType != null && x.SpecialSlotType == eSpecialSlotType.Boots).OrderBy(x => random.Next()).FirstOrDefault();
        if (boots != null)
          startingItems.Add(boots.ItemId, 1);
        await InventoryServer.GiveCharStartingItems(character.Id, startingItems);
        await VSql.ExecuteAsync("update `character` set `is_new` = 0 where `id` = @charId", new Dictionary<string, object>() { { "@charId", character.Id } });
      }
      LoggedPlayers[oid.OID] = character;
      return true;
    }

    public static async void UpdateOrganization(OIDBag oid, Player player, int? organizationId, PlayerList playerList)
    {
      if (!LoggedPlayers.ContainsKey(oid.OID) || LoggedPlayers[oid.OID] == null)
        return;

      int? changedOrganization = organizationId ?? LoggedPlayers[oid.OID].OrganizationId;

      LoggedPlayers[oid.OID].OrganizationId = organizationId;

      if (player != null)
      {
        player.TriggerEvent("ofw_char_caretaker:UpdateOrganization", organizationId);
      }

      if (changedOrganization != null)
        OrganizationServer.OrganizationUpdated(changedOrganization.Value, playerList);
    }

    public static int GetPlayerLoggedCharacterId(int oid)
    {
      if (!LoggedPlayers.ContainsKey(oid))
        return -1;

      return LoggedPlayers[oid].Id;
    }

    public static int GetPlayerLoggedCharacterId(Player player)
    {
      var oid = OIDServer.GetOriginServerID(player);
      if (oid == null)
      {
        return -1;
      }

      if (!LoggedPlayers.ContainsKey(oid.OID))
        return -1;

      return LoggedPlayers[oid.OID].Id;
    }

    public static CharacterBag GetPlayerLoggedCharacter(Player player)
    {
      var oid = OIDServer.GetOriginServerID(player);
      if (oid == null)
      {
        return null;
      }

      if (!LoggedPlayers.ContainsKey(oid.OID))
        return null;

      return LoggedPlayers[oid.OID];
    }

    public static int GetCharLoggedServerId(int charId)
    {
      if (LoggedPlayers == null)
        return -1;
      var oid = LoggedPlayers.FirstOrDefault(x => x.Value?.Id == charId);
      if (EqualityComparer<KeyValuePair<int, CharacterBag>>.Default.Equals(oid, default(KeyValuePair<int, CharacterBag>)) || oid.Key <= 0) return -1;

      return OIDServer.GetLastKnownServerID(oid.Key);
    }

    public static bool HasPlayerAdminLevel(Player source, int level)
    {
      var oid = OIDServer.GetOriginServerID(source);
      if (oid == null)
      {
        source.Drop("Nepodařilo se získat identifikátory uživatele!");
        return false;
      }

      if (!LoggedPlayers.ContainsKey(oid.OID))
        return false;

      return LoggedPlayers[oid.OID].AdminLevel >= level;
    }

    [EventHandler("ofw_char:GetPlayerCharacter")]
    private async void GetPlayerCharacter([FromSource] Player source, int playerHandle, NetworkCallbackDelegate callback)
    {
      var player = Players.Where(p => p.Handle == playerHandle.ToString()).FirstOrDefault();
      if (player == null)
      {
        _ = callback(String.Empty);
        return;
      }

      var character = GetPlayerLoggedCharacter(player);
      if (character == null)
      {
        _ = callback(String.Empty);
        return;
      }
      await Delay(2000);

      _ = callback(JsonConvert.SerializeObject(character));
    }
  }
}
