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

      LoggedPlayers[oid.OID] = CharacterBag.ParseFromSql(result[0]);
      return true;
    }
  }
}
