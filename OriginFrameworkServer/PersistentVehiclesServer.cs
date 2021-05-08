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
  public class PersistentVehiclesServer : BaseScript
  {
    private static PersistentVehicleDatabaseBag data = null;
    private bool isFirstTick = true;
    private JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
      Error = (obj, args) =>
      {
        var context = args.ErrorContext;

        context.Handled = true;
      }
    };

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
    private async void SpawnServerVehicle([FromSource] Player source, string model, Vector3 pos, float heading, CallbackDelegate callback)
    {
      if (source != null)
      {
        Debug.WriteLine("SpawnServerVehicle can't be called from client! SRC: " + (source.ToString()));
        return;
      }

      var vehID = SpawnPersistentVehicle(GetHashKey(model), pos, heading);

      int frameCounter = 0;
      while (!DoesEntityExist(vehID))
      {
        await Delay(100);
        frameCounter++;
        if (frameCounter > 300)
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

    [EventHandler("ofw_veh:SpawnServerVehicleFromGarage")]
    private async void SpawnServerVehicleFromGarage([FromSource] Player source, string plate, Vector3 pos, float heading, NetworkCallbackDelegate callback)
    {
      if (source == null)
        return;

      var param = new Dictionary<string, object>();
      param.Add("@plate", plate);
      var result = await VSql.FetchAllAsync("SELECT `stored`, `vehicle` FROM `owned_vehicles` WHERE `plate` = @plate", param);

      if (result == null || result.Count <= 0)
      {
        Debug.WriteLine("OFW_SpawnServerVehicleFromGarage: Car with plate not found: " + plate);
        source.TriggerEvent("ofw:ValidationErrorNotification", "Auto neni v seznamu vlastnenych!");
        _ = callback(-1);
        return;
      }
      if ((sbyte)result[0]["stored"] <= 0)
      {
        Debug.WriteLine("OFW_SpawnServerVehicleFromGarage: Car is not stored: " + plate);
        source.TriggerEvent("ofw:ValidationErrorNotification", "Auto neni v garazi!");
        _ = callback(-1);
        return;
      }

      var sProps = (string)result[0]["vehicle"];

      if (sProps == null)
      {
        Debug.WriteLine("OFW_SpawnServerVehicleFromGarage: Invalid car properties: " + plate);
        source.TriggerEvent("ofw:ValidationErrorNotification", "Auto neni spravne ulozeno v DB!");
        _ = callback(-1);
        return;
      }

      VehiclePropertiesBag vehData = JsonConvert.DeserializeObject<VehiclePropertiesBag>(sProps, jsonSettings);

      var modelHash = (int)vehData.model;

      await Delay(0);

      int vehID = -1;
      try
      {
        vehID = SpawnPersistentVehicle(modelHash, pos, heading);
      }
      catch 
      {
        Debug.WriteLine("OFW_VEH: Vehicle from garage server spawn error!");
        source.TriggerEvent("ofw:ValidationErrorNotification", "Error pri spawnu auta, zkus to prosim znovu!");
        _ = callback(-1);
        return;
      }

      await Delay(200);

      int frameCounter = 0;
      while (!DoesEntityExist(vehID))
      {
        await Delay(100);
        frameCounter++;
        if (frameCounter >= 20)
        {

          Debug.WriteLine("OFW_VEH: Vehicle from garage server spawn timeout!");
          source.TriggerEvent("ofw:ValidationErrorNotification", "Timeout pro spawn auta, zkus to prosim znovu!");
          _ = callback(-1);
          return;
        }
      }

      var veh = new Vehicle(vehID);
      data.Vehicles.Add(new PersistentVehicleBag { NetID = veh.NetworkId, Plate = plate, LastKnownPos = new VehiclePosBag(veh.Position.X, veh.Position.Y, veh.Position.Z, veh.Heading), ModelHash = veh.Model });

      //kdyby ho do restartu serveru nevratil a uz bylo opraveny, tak at se nevrati poskozeni
      vehData.engineHealth = null;
      vehData.bodyHealth = null;
      vehData.windows = null;
      vehData.tyres = null;
      vehData.doors = null;

      var param2 = new Dictionary<string, object>();
      param2.Add("@plate", plate);
      param2.Add("@vehicle", JsonConvert.SerializeObject(vehData));
      await VSql.ExecuteAsync("UPDATE `owned_vehicles` SET `stored` = 0, `vehicle` = @vehicle WHERE `plate` = @plate", param2);

      _ = callback(veh.NetworkId);
    }

    [EventHandler("ofw_veh:IsVehicleWithPlateOutOfGarage")]
    private async void IsVehicleWithPlateOutOfGarage([FromSource] Player source, string plate, NetworkCallbackDelegate callback)
    {
      if (source == null)
        return;

      if (data == null || data.Vehicles == null)
      {
        _ = callback(false);
        return;
      }

      for (int i = 0; i < data.Vehicles.Count; i++)
      {
        if (data.Vehicles[i].Plate == plate)
        {
          _ = callback(true);
          return;
        }
      }

      _ = callback(false);
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
    }

    public static bool IsVehicleKnown(int netID)
    {
      if (data == null || data.Vehicles == null)
        return false;

      for (int i = 0; i < data.Vehicles.Count; i++)
      {
        if (data.Vehicles[i].NetID == netID)
          return true;
      }

      return false;
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

          if (NetworkGetEntityFromNetworkId(iveh.NetID) <= 0 || GetEntityModel(NetworkGetEntityFromNetworkId(iveh.NetID)) != iveh.ModelHash)
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
