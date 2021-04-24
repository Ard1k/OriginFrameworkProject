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
    //private PersistentVehicleDatabaseBag data = new PersistentVehicleDatabaseBag() { Vehicles = new List<PersistentVehicleBag>() };
    //private static List<Vehicle> queue = new List<Vehicle>();
    //private bool isFirstTick = true;

    public PersistentVehiclesServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
    }

    private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      //var serializedDB = GetResourceKvpString(KvpManager.getKvpString(KvpEnum.ServerPersistenVehicleDB));
      //if (serializedDB != null)
      //{
      //  Debug.WriteLine(serializedDB);
      //  data = JsonConvert.DeserializeObject<PersistentVehicleDatabaseBag>(serializedDB);
      //}
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

      //var res = await VSql.FetchScalarAsync("select `data` from `cfw_jsondata` where `key` = 'pvs'", null);
      //Debug.WriteLine($"Data: {res ?? "null"}");
      //if (res != null && res != DBNull.Value && res is string)
      //{
      //  Debug.WriteLine($"Filling data: {res ?? "null"}");
      //  data = JsonConvert.DeserializeObject<PersistentVehicleDatabaseBag>((string)res);
      //  if (data == null) data = new PersistentVehicleDatabaseBag();
      //  if (data.Vehicles == null) data.Vehicles = new List<PersistentVehicleBag>();
      //}

      //Tick -= RefreshVehiclePositions;
      //Tick += RefreshVehiclePositions;
      //Tick -= PeriodicSaveToKvp;
      //Tick += PeriodicSaveToKvp;
      //Tick -= QueueProcessor;
      //Tick += QueueProcessor;
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      //SetResourceKvp(KvpManager.getKvpString(KvpEnum.ServerPersistenVehicleDB), JsonConvert.SerializeObject(data));
    }

    //private async Task QueueProcessor()
    //{
    //  if (queueCooldown <= 0)
    //  {

    //    for (int i = queue.Count - 1; i >= 0; i--)
    //    {
    //      var veh = queue[i];
    //      Debug.WriteLine("Processing netId: " + veh.NetworkId);
    //      data.Vehicles.Add(new PersistentVehicleBag { NetID = veh.NetworkId, LastKnownPos = new VehiclePosBag(veh.Position.X, veh.Position.Y, veh.Position.Z, veh.Heading), ModelHash = veh.Model });
    //      queue.Remove(veh);
    //    }

    //    await Delay(1000);
    //  }
    //  else
    //  {
    //    queueCooldown -= 1;
    //    await Delay(1000);
    //  }
    //}

    //private async Task RefreshVehiclePositions()
    //{
    //  if (isFirstTick)
    //  {
    //    Debug.WriteLine(data?.ToString());
    //    //Restore vehicles before updating and cleaning them
    //    while (data == null)
    //      await Delay(0);

    //    if (data != null && data.Vehicles != null)
    //    {
    //      foreach (var iveh in data.Vehicles)
    //      {
    //        var cveh = Vehicle.FromNetworkId(iveh.NetID);

    //        if (cveh != null || cveh.Model == iveh.ModelHash)
    //        {
    //          Debug.WriteLine($"PersistentVehicles: Restoring - Vehicle known, skipping [NetID: {iveh.NetID}, Model: {iveh.ModelHash}]");
    //          continue;
    //        }

    //        var veh = SpawnPersistentVehicle(iveh.ModelHash, new Vector3(iveh.LastKnownPos.X, iveh.LastKnownPos.Y, iveh.LastKnownPos.Z), iveh.LastKnownPos.Heading);
    //        iveh.LocalID = await veh;
    //        Debug.WriteLine($"PersistentVehicles: Restoring - Spawning vehicle [OldNetID: {iveh.NetID}, NewNetID: {iveh.NetID}, Model: {iveh.ModelHash}]");
    //      }
    //    }
    //    Debug.WriteLine("Done restoring");
    //    isFirstTick = false;
    //  }
    //  else
    //  {
    //    for (int i = data.Vehicles.Count - 1; i >= 0; i--)
    //    {
    //      var iveh = data.Vehicles[0];
    //      Vehicle cveh = null;

    //      if (iveh.LocalID > 0)
    //      {
    //        var guess = new Vehicle(iveh.LocalID);
    //        if (guess != null && guess.NetworkId > 0)
    //        {
    //          iveh.NetID = guess.NetworkId;
    //          iveh.LocalID = 0;
    //        }
    //        cveh = guess;
    //      }
    //      else
    //      {
    //        cveh = (Vehicle)Vehicle.FromNetworkId(iveh.NetID);
    //        if (cveh == null || cveh.Model != iveh.ModelHash)
    //        {
    //          Debug.WriteLine($"PersistentVehicles: Removing inconsistent vehicle from sync [NetID: {iveh.NetID}, Model: {iveh.ModelHash}]");
    //          data.Vehicles.Remove(iveh);
    //          continue;
    //        }
    //      }

    //      iveh.LastKnownPos.X = cveh.Position.X;
    //      iveh.LastKnownPos.Y = cveh.Position.Y;
    //      iveh.LastKnownPos.Z = cveh.Position.Z;
    //      iveh.LastKnownPos.Heading = cveh.Heading;
    //    }
    //  }

    //  await Delay(1000);
    //}

    //private async Task PeriodicSaveToKvp()
    //{
    //  await Delay(10000);
    //  string serialized = JsonConvert.SerializeObject(data);

    //  var param = new Dictionary<string, object> { };
    //  param.Add("@value", serialized);

    //  await VSql.ExecuteAsync("INSERT INTO cfw_jsondata (`key`, `data`) VALUES ('pvs', @value) ON DUPLICATE KEY UPDATE `key` = 'pvs', `data` = @value", param);
    //  Debug.WriteLine("PersistenVehicles: CurrentData: " + JsonConvert.SerializeObject(data));
    //}

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
