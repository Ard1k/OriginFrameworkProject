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
  public class PersistentVehiclesServer : BaseScript
  {
    public PersistentVehiclesServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
    }

    private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      RegisterCommand("supercar", new Action<int, List<object>, string>((source, args, raw) =>
      {
        if (args == null || args.Count <= 0)
          return;

        var ped = Ped.FromPlayerHandle(source.ToString());
        var pos = ped.Position;
        
        var veh = SpawnPersistentVehicle(GetHashKey(args[0].ToString()), new Vector3(pos.X, pos.Y, pos.Z), ped.Heading);

        Task.Run(async () => {
          var netId = NetworkGetNetworkIdFromEntity(await veh);
          //Debug.WriteLine("NID:" + netId);
        });

      }), false);

      while (SettingsManager.Settings == null)
        await Delay(0);

      while (!VSql.IsReadyToUse)
        await Delay(0);
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
    }

    public async Task<int> SpawnPersistentVehicle(int hash, Vector3 pos, float heading)
    {
      // native "CREATE_VEHICLE"
      // hash "0xAF35D0D2583051B0"
      // jhash(0xDD75460A)

      // arguments {
      //  Hash "modelHash",
      //	float "x",
      //	float "y",
      //	float "z",
      //	float "heading",
      //	BOOL "isNetwork",
      //	BOOL "p6",  -- nastavovat na false
      //}
      // returns "Vehicle"
      
      var vehid = CreateVehicle((uint)hash, pos.X, pos.Y, pos.Z, heading, true, false);
      await Delay(20000);
      return vehid;
    }
  }
}
