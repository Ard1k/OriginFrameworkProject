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
using static OriginFrameworkServer.OfwServerFunctions;
using System.IO;
using Newtonsoft.Json.Linq;

namespace OriginFrameworkServer
{
  public class IdentityServer : BaseScript
  {
    private Dictionary<int, IdentityShowCardBag> CardsCache = new Dictionary<int, IdentityShowCardBag>();
    private Dictionary<int, int> PlayerShowingCard = new Dictionary<int, int>();

    public IdentityServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
    }

    #region event handlers

    private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.IdentityServer))
        return;

      InternalDependencyManager.Started(eScriptArea.IdentityServer);
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
    }

    [EventHandler("ofw_identity:ShowCard")]
    private async void ShowCard([FromSource] Player source, int id)
    {
      if (source == null || id <= 0)
        return;

      int sourceIntHandle = Int32.Parse(source.Handle);

      if (CardsCache.ContainsKey(id))
      {
        if (PlayerShowingCard.ContainsKey(sourceIntHandle))
          PlayerShowingCard.Remove(sourceIntHandle);

        PlayerShowingCard.Add(sourceIntHandle, id);

        TriggerClientEvent("ofw_identity:ShowingCard", sourceIntHandle, JsonConvert.SerializeObject(CardsCache[id]));
        return;
      }

      var param = new Dictionary<string, object>();
      param.Add("@id", id);
      var resCard = await VSql.FetchAllAsync("select * from `inventory_item` where `id` = @id", param);
      if (resCard == null || resCard.Count <= 0)
      {
        source.TriggerEvent("ofw:ValidationErrorNotification", "Neznámá karta");
        return;
      }

      int charId = 0;
      uint model = 0;
      Dictionary<string, int> cardSkin = null;
      int itemId = Convert.ToInt32(resCard[0]["item_id"]);

      string[] metadata = null;
      if (resCard[0].ContainsKey("metadata") && resCard[0]["metadata"] != null && resCard[0]["metadata"] != DBNull.Value)
      {
        metadata = Convert.ToString(resCard[0]["metadata"])?.Split('|')?.Where(m => m != null)?.ToArray();
      }

      if (metadata == null)
      {
        if (resCard == null || resCard.Count <= 0)
        {
          source.TriggerEvent("ofw:ValidationErrorNotification", "Chybí data karty");
          return;
        }
      }

      var charStr = metadata.Where(m => m != null && m.StartsWith("_charid:")).FirstOrDefault();
      var modelStr = metadata.Where(m => m != null && m.StartsWith("_model:")).FirstOrDefault();
      var skinStr = metadata.Where(m => m != null && m.StartsWith("_skin:")).FirstOrDefault();

      Int32.TryParse(charStr.Substring(8), out charId);
      uint.TryParse(modelStr.Substring(7), out model);
      cardSkin = JsonConvert.DeserializeObject<Dictionary<string, int>>(skinStr.Substring(6));

      if (charId <= 0 || model == 0 || cardSkin == null)
      {
        source.TriggerEvent("ofw:ValidationErrorNotification", "Chybí data karty");
        return;
      }

      var cardData = new IdentityShowCardBag
      {
        CardId = id,
        ItemId = itemId,
        Model = model,
        CardSkin = cardSkin,
        CharName = metadata.FirstOrDefault(m => m.StartsWith("_charname:"))?.Substring(10),
        Born = metadata.FirstOrDefault(m => m.StartsWith("_born:"))?.Substring(6),
        ValidTo = metadata.FirstOrDefault(m => m.StartsWith("_valid:"))?.Substring(7),
        Created = metadata.FirstOrDefault(m => m.StartsWith("_created:"))?.Substring(9),
        Sn = metadata.FirstOrDefault(m => m.StartsWith("_sn:"))?.Substring(4),
    };

      if (!CardsCache.ContainsKey(id))
        CardsCache.Add(id, cardData);

      if (PlayerShowingCard.ContainsKey(sourceIntHandle))
        PlayerShowingCard.Remove(sourceIntHandle);

      PlayerShowingCard.Add(sourceIntHandle, id);

      //var param2 = new Dictionary<string, object>();
      //param.Add("@idChar", charId);
      //var charResult = await VSql.FetchAllAsync("select `name` from `character` where `id` = @idChar", param);
      //if (charResult == null || charResult.Count <= 0)
      //{
      //  source.TriggerEvent("ofw:ValidationErrorNotification", "Nelze data postavy");
      //  return;
      //}

      //if (charResult[0].ContainsKey("name") && charResult[0]["name"] != null && charResult[0]["name"] != DBNull.Value)
      //{
      //  cardData.CharName = (string)charResult[0]["name"];
      //}

      TriggerClientEvent("ofw_identity:ShowingCard", sourceIntHandle, JsonConvert.SerializeObject(cardData));
    }

    [EventHandler("ofw_identity:HideCard")]
    private async void HideCard([FromSource] Player source)
    {
      if (source == null)
        return;

      int sourceIntHandle = Int32.Parse(source.Handle);

      if (PlayerShowingCard.ContainsKey(sourceIntHandle))
        PlayerShowingCard.Remove(sourceIntHandle);

      TriggerClientEvent("ofw_identity:HidingCard", sourceIntHandle);
    }


    #endregion

    #region private


    #endregion
  }
}