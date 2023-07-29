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
  public class CarryServer : BaseScript
  {
    private static List<CarriableWoldItemBag> CarriableCache = new List<CarriableWoldItemBag>();

    public CarryServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
    }

		#region event handlers

		private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.CarryServer, eScriptArea.VSql, eScriptArea.InventoryServer))
        return;

      var results = await VSql.FetchAllAsync("select * from `inventory_item` where `place` = 'freeobject'; ", null);
      if (results != null && results.Count > 0)
      {
        foreach (var row in results)
        {
          var it = InventoryItemBag.ParseFromSql(row);

          if (it == null || it.Metadata == null)
            continue;

          var posMeta = it.Metadata.Where(m => m != null && m.StartsWith("_posbag:")).FirstOrDefault();
          if (posMeta == null)
            continue;

          var posBag = JsonConvert.DeserializeObject<PosBag>(posMeta.Substring(8));

          CarriableCache.Add(new CarriableWoldItemBag 
          {
            InvItemId = it.Id,
            ItemId = it.ItemId,
            PosBag = posBag
          });
        }
      }

      InternalDependencyManager.Started(eScriptArea.CarryServer);
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
      
    }

    [EventHandler("ofw_carry:PutDownItem")]
    private async void PutDownItem([FromSource] Player source, int invItemId, string posBagString, bool isForklift, string sourcePlace)
    {
      if (source == null || (isForklift && sourcePlace == null))
        return;

      var character = CharacterCaretakerServer.GetPlayerLoggedCharacter(source);
      if (character == null)
      {
        source.TriggerEvent("ofw:ValidationErrorNotification", "Hrac neni lognuty");
        return;
      }

      if (posBagString == null)
      {
        source.TriggerEvent("ofw:ValidationErrorNotification", "Neplatna pozice");
        return;
      }

      var posBag = JsonConvert.DeserializeObject<PosBag>(posBagString);

      if (isForklift)
      {
        InventoryServer.CarriableForkliftPutDown(source, invItemId, sourcePlace, posBag, (player, error) =>
        {
          if (error != null)
          {
            player.TriggerEvent("ofw:ValidationErrorNotification", error);
            return;
          }
        });
      }
      else
      {
        InventoryServer.CarriablePutDown(source, invItemId, $"char_{character.Id}", posBag, (player, error) =>
        {
          if (error != null)
          {
            player.TriggerEvent("ofw:ValidationErrorNotification", error);
            return;
          }
        });
      }
    }

    [EventHandler("ofw_carry:PickUpItem")]
    private async void PickUpItem([FromSource] Player source, int invItemId, bool isForklift, string place)
    {
      if (source == null || (isForklift && place == null))
        return;

      var character = CharacterCaretakerServer.GetPlayerLoggedCharacter(source);
      if (character == null)
      {
        source.TriggerEvent("ofw:ValidationErrorNotification", "Hrac neni lognuty");
        return;
      }

      if (isForklift)
      {
        InventoryServer.CarriableForkliftPickUp(source, invItemId, place, (player, error) =>
        {
          if (error != null)
          {
            player.TriggerEvent("ofw:ValidationErrorNotification", error);
            return;
          }
        });
      }
      else
      {
        InventoryServer.CarriablePickUp(source, invItemId, $"char_{character.Id}", (player, error) =>
        {
          if (error != null)
          {
            player.TriggerEvent("ofw:ValidationErrorNotification", error);
            return;
          }
        });
      }
    }

    [EventHandler("ofw_carry:RequestCacheSync")]
    private async void RequestCacheSync([FromSource] Player source)
    {
      if (source == null)
        return;

      source.TriggerEvent("ofw_carry:SyncCache", JsonConvert.SerializeObject(CarriableCache));
    }

    #endregion

    #region public
    public static void AddCarriableToCache(int invItemId, int itemId, PosBag pos)
    {
      var carriable = new CarriableWoldItemBag()
      {
        InvItemId = invItemId,
        ItemId = itemId,
        PosBag = pos
      };
      CarriableCache.Add(carriable);
      TriggerClientEvent("ofw_carry:WorldItemAdd", JsonConvert.SerializeObject(carriable));
    }

    public static void RemoveCarriableFromCache(int invItemId)
    {
      var carriable = CarriableCache.Where(it => it.InvItemId == invItemId).FirstOrDefault();

      if (carriable != null)
        CarriableCache.Remove(carriable);

      TriggerClientEvent("ofw_carry:WorldItemRemove", invItemId);
    }
    #endregion

    #region private


    #endregion
  }
}
