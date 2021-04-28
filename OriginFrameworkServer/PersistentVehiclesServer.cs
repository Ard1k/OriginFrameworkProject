﻿using CitizenFX.Core;
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
    private PersistentVehicleDatabaseBag data = null;
    private bool isFirstTick = true;

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

        var vehID = SpawnPersistentVehicle(GetHashKey(args[0].ToString()), new Vector3(pos.X, pos.Y, pos.Z), ped.Heading);

        Task.Run(async () => {
          while (!DoesEntityExist(vehID))
            await Delay(0);

          var veh = new Vehicle(vehID);
          data.Vehicles.Add(new PersistentVehicleBag { NetID = veh.NetworkId, LastKnownPos = new VehiclePosBag(veh.Position.X, veh.Position.Y, veh.Position.Z, veh.Heading), ModelHash = veh.Model });
        });
      }), false);

      while (SettingsManager.Settings == null)
        await Delay(0);

      while (!VSql.IsReadyToUse)
        await Delay(0);

      //var res = await VSql.FetchScalarAsync("select `data` from `cfw_jsondata` where `key` = 'pvs'", null);
      //if (res != null && res != DBNull.Value && res is string)
      //{
      //  Debug.WriteLine($"Filling data: {res ?? "null"}");
      //  var newData = JsonConvert.DeserializeObject<PersistentVehicleDatabaseBag>((string)res);
      //  if (newData != null)
      //    data = newData;
      //  else
      //  {
          data = new PersistentVehicleDatabaseBag { Vehicles = new List<PersistentVehicleBag>() };
      //  }
      //}

      Tick += VehicleManagerTick;
      //Tick += PeriodicSave;
    }

    [EventHandler("ofw_veh:SpawnServerVehicle")]
    private async void SpawnServerVehicle([FromSource] Player source, string model, Vector3 pos, float heading, NetworkCallbackDelegate callback)
    {
      Debug.WriteLine("SRC: " + (source.ToString() ?? "no source"));
      //TODO pokud playerId > 0, tak najit jeho data, jinak z Player

      var vehID = SpawnPersistentVehicle(GetHashKey(model), pos, heading);

      int frameCounter = 0;
      while (!DoesEntityExist(vehID))
      {
        await Delay(100);
        frameCounter++;
        if (frameCounter > 200)
        {
          Debug.WriteLine("OFW_VEH: Vehicle server spawn timeout!");
          _ = callback(-1);
          return;
        }
      }

      var veh = new Vehicle(vehID);
      data.Vehicles.Add(new PersistentVehicleBag { NetID = veh.NetworkId, LastKnownPos = new VehiclePosBag(veh.Position.X, veh.Position.Y, veh.Position.Z, veh.Heading), ModelHash = veh.Model });

      _ = callback(veh.NetworkId);
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
    }

    private async Task VehicleManagerTick()
    {
      if (isFirstTick)
      {
        //TODO: Pokud chceme udelat respawn po server restartu, tak musime zajistit, ze pobliz spawnovaneho auta je nejaky hrac

        //Debug.WriteLine("WAIT DATA");
        //while (data == null || data.Vehicles == null)
        //  await Delay(0);
        //Debug.WriteLine("DATA READY");

        //while (Players.Count() <= 0)
        //{
        //  Debug.WriteLine("WAITING FOR PLAYER");
        //  await Delay(1000);
        //}

        //if (data != null && data.Vehicles != null)
        //{
        //  foreach (var iveh in data.Vehicles)
        //  {
        //    var existingVeh = NetworkGetEntityFromNetworkId(iveh.NetID);
        //    if (existingVeh > 0)
        //    {
        //      var existingVehEnt = new Vehicle(existingVeh);
        //      if (existingVehEnt.NetworkId == iveh.NetID && existingVehEnt.Model == iveh.ModelHash)
        //      {
        //        Debug.WriteLine($"PersistentVehicles: Restoring - Vehicle known, skipping [NetID: {iveh.NetID}, Model: {iveh.ModelHash}]");
        //        continue;
        //      }
        //    }

        //    int veh = 0;
        //    while (true)
        //    {
        //      veh = SpawnPersistentVehicle(iveh.ModelHash, new Vector3(iveh.LastKnownPos.X, iveh.LastKnownPos.Y, iveh.LastKnownPos.Z), iveh.LastKnownPos.Heading);

        //      int counter = 0;
        //      while (!DoesEntityExist(veh))
        //      {
        //        await Delay(100);
        //        counter++;

        //        if (counter >= 100 && !DoesEntityExist(veh))
        //        {
        //          Debug.WriteLine("Vehicle restore timeout, retrying spawn");
        //          break;
        //        }
        //      }

        //      if (DoesEntityExist(veh))
        //        break;
        //    }

        //    var vehNewEnt = new Vehicle(veh);
        //    Debug.WriteLine($"PersistentVehicles: Restoring - Spawning vehicle [OldNetID: {iveh.NetID}, NewNetID: {vehNewEnt.NetworkId}, Model: {iveh.ModelHash}]");

        //    iveh.NetID = vehNewEnt.NetworkId;
        //    iveh.IsServerRestored = true;
        //  }
        //}
        //Debug.WriteLine("Done restoring");
        isFirstTick = false;
      }
      else
      {
        for (int i = data.Vehicles.Count - 1; i >= 0; i--)
        {
          var iveh = data.Vehicles[0];

          if (NetworkGetEntityFromNetworkId(iveh.NetID) <= 0)
          {
            Debug.WriteLine($"PersistentVehicles: Removing no longer existing vehicle from sync [NetID: {iveh.NetID}, Model: {iveh.ModelHash}]");
            data.Vehicles.Remove(iveh);
            continue;
          }
          var vehID = NetworkGetEntityFromNetworkId(iveh.NetID);
          var vehEnt = new Vehicle(vehID);

          iveh.LastKnownPos.X = vehEnt.Position.X;
          iveh.LastKnownPos.Y = vehEnt.Position.Y;
          iveh.LastKnownPos.Z = vehEnt.Position.Z;
          iveh.LastKnownPos.Heading = vehEnt.Heading;
        }
      }

      await Delay(10000);
    }

    private async Task PeriodicSave()
    {
      await Delay(10000);

      if (data == null)
        return;

      string serialized = JsonConvert.SerializeObject(data);

      var param = new Dictionary<string, object> { };
      param.Add("@value", serialized);

      await VSql.ExecuteAsync("INSERT INTO cfw_jsondata (`key`, `data`) VALUES ('pvs', @value) ON DUPLICATE KEY UPDATE `key` = 'pvs', `data` = @value", param);
      Debug.WriteLine("PersistenVehicles: CurrentData: " + JsonConvert.SerializeObject(data));
    }

    public int SpawnPersistentVehicle(int hash, Vector3 pos, float heading)
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
      
      return CreateVehicle((uint)hash, pos.X, pos.Y, pos.Z, heading, true, false);
    }
  }
}
