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
using System.Dynamic;

namespace OriginFrameworkServer
{
  public class FactionDataManager : BaseScript
  {
    private static Dictionary<eFaction, object> FactionCache = new Dictionary<eFaction, object>();
    private static LockObj syncLock = new LockObj();
    private static List<eFaction> ChangedFactionData = new List<eFaction>();

    public FactionDataManager()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
    }

    private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        await Delay(0);

      Tick += PeriodicFactionSave;
    }
    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      await SaveFactionData();
    }

    [EventHandler("ofw_factions:GetFactionData")]
    private async void GetFactionDataCallback([FromSource] Player source, int faction, NetworkCallbackDelegate callback)
    {
      var factionData = await GetFactionData((eFaction)faction);

      _ = callback(factionData == null ? null : JsonConvert.SerializeObject(factionData));
    }

    public static async Task<object> GetFactionData(eFaction faction)
    {
      using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
      {
        if (FactionCache.ContainsKey(faction))
          return FactionCache[faction];

        var definition = FactionDefinitionBag.getDefinition(faction);
        if (definition == null)
          //ze serveru se nam to nestane. A pokud se to vola z klienta a nekdo sem posle sracku, tak to padne na null reference az na klientovi a na to prcam.
          return null;

        while (!VSql.IsReadyToUse)
          await Delay(0);

        var param = new Dictionary<string, object> { };
        param.Add("@key", definition.DBDataKey);

        var res = await VSql.FetchScalarAsync("select `data` from `cfw_jsondata` where `key` = @key", param);
        if (res == null || res == DBNull.Value || !(res is string))
        {
          var instance = Activator.CreateInstance(definition.FactionDataType);

          FactionCache.Add(faction, instance);
          return instance;
        }
        
        var factionData = JsonConvert.DeserializeObject((string)res, definition.FactionDataType);
        FactionCache.Add(faction, factionData);
        return factionData;
      }
    }

    public static async Task UpdateFactionData(eFaction faction, object data)
    {
      using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
      {
        if (FactionCache.ContainsKey(faction))
        {
          //V podstate si sem predavam tu stejnou referenci. Ale uz to volam kvuli ulozeni a distribuci na klienta, tak fuck it
          FactionCache[faction] = data;
        }
        else
        {
          FactionCache.Add(faction, data);
        }

        if (!ChangedFactionData.Contains(faction))
          ChangedFactionData.Add(faction);

        TriggerClientEvent("ofw_factions:FactionDataUpdated", (int)faction, JsonConvert.SerializeObject(data));
      }
    }

    private async Task PeriodicFactionSave()
    {
      await Delay(60000); //jednou za minutu snad ok
      await SaveFactionData();
    }

    private async Task SaveFactionData()
    {
      using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
      {
        foreach (var f in ChangedFactionData)
        {
          if (!FactionCache.ContainsKey(f))
            continue;

          var data = FactionCache[f];
          if (data == null)
            continue;

          var definition = FactionDefinitionBag.getDefinition(f);
          if (definition == null)
            continue;

          string serialized = JsonConvert.SerializeObject(data);

          var param = new Dictionary<string, object> { };
          param.Add("@value", serialized);
          param.Add("@key", definition.DBDataKey);

          await VSql.ExecuteAsync("INSERT INTO cfw_jsondata (`key`, `data`) VALUES (@key, @value) ON DUPLICATE KEY UPDATE `key` = @key, `data` = @value", param);
        }

        ChangedFactionData.Clear();
      }
    }
  }
}
