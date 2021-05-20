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
using static OriginFrameworkServer.OfwServerFunctions;

namespace OriginFrameworkServer
{
  class OilIndustriesFactionServer : BaseScript
  {
    private OilIndustriesFactionDataBag Data = null;
    private LockObj syncLock = new LockObj();

    public OilIndustriesFactionServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
    }

    private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        await Delay(0);

      PersistentVehiclesServer.PersistentVehRemoved += PersistentVehiclesServer_PersistentVehRemoved;

      Data = (OilIndustriesFactionDataBag)(await FactionDataManager.GetFactionData(eFaction.OilIndustries));
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
    }

    [EventHandler("ofw_oilindustries:Test")]
    private async void Test([FromSource] Player source)
    {
      using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
      {
        Data.ProcessedOil += 1;
        await FactionDataManager.UpdateFactionData(eFaction.OilIndustries, Data);
      }
    }

    [EventHandler("ofw_oilindustries:SpawnCistern")]
    private async void SpawnCistern([FromSource] Player source, NetworkCallbackDelegate callback)
    {
      var ret = await ServerAsyncCallbackToSync<int>("ofw_veh:SpawnServerVehicle", "tanker", new Vector3(OilIndustriesSaticData.CisternSpawnpoint.X, OilIndustriesSaticData.CisternSpawnpoint.Y, OilIndustriesSaticData.CisternSpawnpoint.Z), OilIndustriesSaticData.CisternSpawnpoint.Heading);

      using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
      {
        Data.Cisterns.Add(ret);
        _ = callback(ret);
      }
    }

    private async void PersistentVehiclesServer_PersistentVehRemoved(object sender, PersistentVehicleRemovedArgs e)
    {
      if (e == null || e.NetID <= 0 || Data == null)
        return;

      if (Data.Cisterns.Any(c => c == e.NetID))
      {
        Data.Cisterns.Remove(e.NetID);
        await FactionDataManager.UpdateFactionData(eFaction.OilIndustries, Data);
      }
    }
  }
}
