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

namespace OriginFrameworkServer
{
    public class MainServer : BaseScript
    {

    public MainServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
    }

    //public static Vehicle lastSuperVeh = null;

    //private static async Task TickTask()
    //{
    //  Debug.WriteLine("superveh owner: " + lastSuperVeh?.Owner ?? "null");
    //  Debug.WriteLine("superveh ownerHandle: " + lastSuperVeh?.Owner?.Handle ?? "null");
    //  Debug.WriteLine("superveh netID: " + lastSuperVeh?.NetworkId ?? "null");
    //  Debug.WriteLine("superveh pos: " + lastSuperVeh?.Position ?? "null");
    //  Debug.WriteLine("superveh handle: " + lastSuperVeh?.Handle ?? "null");
    //  Debug.WriteLine("superveh state: " + lastSuperVeh?.State ?? "null");

    //  await Delay(5000);
    //}

    private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        await Delay(0);

      //RegisterCommand("supercar", new Action<int, List<object>, string>((source, args, raw) =>
      //{
      //  // native "CREATE_VEHICLE"
      //  // hash "0xAF35D0D2583051B0"
      //  // jhash(0xDD75460A)

      //  // arguments {
      //  //  Hash "modelHash",
      //  //	float "x",
      //  //	float "y",
      //  //	float "z",
      //  //	float "heading",
      //  //	BOOL "isNetwork",
      //  //	BOOL "p6",
      //  //}
      //  // returns "Vehicle"
      //  if (args == null || args.Count <= 0)
      //    return;

      //  var ped = Ped.FromPlayerHandle(source.ToString());

      //  var pos = ped.Position;
      //  int vehResult = CitizenFX.Core.Native.Function.Call<int>(Hash.CREATE_VEHICLE, GetHashKey(args[0].ToString()), pos.X, pos.Y, pos.Z, ped.Heading, true, false);
      //  Debug.WriteLine("vehResult: " + vehResult);
      //  lastSuperVeh = new Vehicle(vehResult);
      //  Debug.WriteLine("vehCreated: " + lastSuperVeh);
      //}), false);

      //Tick += TickTask;
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

    [EventHandler("ofw:TestDB")]
    private async void TestDB([FromSource] Player source, NetworkCallbackDelegate callback)
    {
      var result = await VSql.FetchAllAsync("select * from users", null);

      _ = callback(JsonConvert.SerializeObject(result));
    }
  }
}
