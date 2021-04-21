using CitizenFX.Core;
using OriginFrameworkData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;

namespace OriginFrameworkServer
{
    public class MainServer : BaseScript
    {

    public MainServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
    }

    private void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        Delay(0);
    }


    [EventHandler("ofw:GetPlayerCompanyData")]
    private void GetPlayerCompanyBag([FromSource] Player source, int playerId, NetworkCallbackDelegate callback)
    {
      //TODO pokud playerId > 0, tak najit jeho data, jinak z Player

      var bag = new PlayerCompanyBag
      {
        CompanyName = "TestCompany",
        CompanyCode = "TC",
        IsCompanyManager = true,
        IsCompanyOwner = true
      };

      _ = callback(JsonConvert.SerializeObject(bag));
    }

    [EventHandler("ofw:GetPlayerPrincipals")]
    private void GetPlayerPrincipals([FromSource] Player source, int playerId, NetworkCallbackDelegate callback)
    {
      

      _ = callback();
    }

    [EventHandler("ofw:TestDB")]
    private async void TestDB([FromSource] Player source, NetworkCallbackDelegate callback)
    {
      var result = await VSql.FetchAllAsync("select * from users", null);

      _ = callback(JsonConvert.SerializeObject(result));
    }
  }
}
