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
  public class LoginServer : BaseScript
  {
    public LoginServer()
    {
    }

    [EventHandler("ofw_login:GetCharacters")]
    private async void GetCharacters([FromSource] Player source, NetworkCallbackDelegate callback)
    {
      if (source == null)
        return;

      var oid = OIDServer.GetOriginServerID(source);
      if (oid == null)
      {
        source.Drop("Nepodařilo se získat identifikátory uživatele!");
        return;
      }

      var param = new Dictionary<string, object>();
      param.Add("@identifier", oid.PrimaryIdentifier);
      var result = await VSql.FetchScalarAsync("select 1 from `user` where `identifier` = @identifier", param);

      if (Convert.ToInt32(result) != 1)
      {
        //registrace
        var registerParam = new Dictionary<string, object>();
        registerParam.Add("@identifier", oid.PrimaryIdentifier);
        registerParam.Add("@name", source.Name);
        registerParam.Add("@steam", oid.Steam);
        registerParam.Add("@license", oid.License);
        registerParam.Add("@discord", oid.Discord);
        registerParam.Add("@ip", oid.IP);
        registerParam.Add("@admin_level", 0);
        await VSql.ExecuteAsync("insert into `user` (`identifier`, `name`, `steam`, `license`, `discord`, `ip`, `admin_level`) VALUES (@identifier, @name, @steam, @license, @discord, @ip, @admin_level);", registerParam);
      }
      else
      {
        //update identifikatoru
        var updateParam = new Dictionary<string, object>();
        updateParam.Add("@identifier", oid.PrimaryIdentifier);
        updateParam.Add("@name", source.Name);
        updateParam.Add("@steam", oid.Steam);
        updateParam.Add("@license", oid.License);
        updateParam.Add("@discord", oid.Discord);
        updateParam.Add("@ip", oid.IP);
        await VSql.ExecuteAsync("update `user` set `name` = @name, `steam` = @steam, `license` = @license, `discord` = @discord, `ip` = @ip where `identifier` = @identifier", updateParam);
      }

      var result2 = await VSql.FetchAllAsync("select c.*, u.`admin_level` from `user` u, `character` c where u.`identifier` = c.`user_identifier` and c.`user_identifier` = @identifier", param);

      List<CharacterBag> characters = new List<CharacterBag>();

      if (result2 == null || result2.Count <= 0)
        _ = callback("nochar");

      foreach (var row in result2)
      {
        characters.Add(new CharacterBag { 
          Id = Convert.ToInt32(row["id"]),
          UserIdentifier = Convert.ToString(row["user_identifier"]),
          Name = Convert.ToString(row["name"]),
          AdminLevel = Convert.ToInt32(row["admin_level"])
        });
      }

      _ = callback(JsonConvert.SerializeObject(characters));
    }

    [EventHandler("ofw_login:CreateCharacter")]
    private async void CreateCharacter([FromSource] Player source, string newCharacterDataString, NetworkCallbackDelegate callback)
    {
      if (source == null)
        return;

      var oid = OIDServer.GetOriginServerID(source);
      if (oid == null)
      {
        source.Drop("Nepodařilo se získat identifikátory uživatele!");
        return;
      }

      CharacterBag charData = null;
      if (newCharacterDataString != null)
      {
        charData = JsonConvert.DeserializeObject<CharacterBag>(newCharacterDataString);
      }

      if (charData == null)
      {
        _ = callback(false, "Chyba přenosu dat!");
        return;
      }

      if (charData.Name == null)
      {
        _ = callback(false, "Neplatné jméno");
        return;
      }

      var param = new Dictionary<string, object>();
      param.Add("@user_identifier", oid.PrimaryIdentifier);
      param.Add("@name", charData.Name);
      await VSql.ExecuteAsync("insert into `character` (`user_identifier`, `name`) VALUES (@user_identifier, @name)", param);

      _ = callback(true, "ok");
    }

    [EventHandler("ofw_login:LoginCharacter")]
    private async void LoginCharacter([FromSource] Player source, int idChar, NetworkCallbackDelegate callback)
    {
      if (source == null)
        return;

      var oid = OIDServer.GetOriginServerID(source);
      if (oid == null)
      {
        source.Drop("Nepodařilo se získat identifikátory uživatele!");
        return;
      }

      var param = new Dictionary<string, object>();
      param.Add("@identifier", oid.PrimaryIdentifier);
      param.Add("@idChar", idChar);
      int rowsUpdated = await VSql.ExecuteAsync("update `user` as u inner join `character` as c on u.`identifier` = c.`user_identifier` set u.`active_character` = @idChar where u.`identifier` = @identifier and c.`id` = @idChar", param);

      if (rowsUpdated != 1)
      {
        _ = callback(false, "Přihlášení se nezdařilo");
        return;
      }

      _ = callback(true, "ok");
    }
  }
}
