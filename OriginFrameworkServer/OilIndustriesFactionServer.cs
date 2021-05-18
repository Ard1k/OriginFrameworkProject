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
  }
}
