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
using System.Runtime.ConstrainedExecution;

namespace OriginFrameworkServer
{
  public class VehicleServer : BaseScript
  {
    private static List<PersistentVehicleBag> persistentVehicles = new List<PersistentVehicleBag>();
    private static Dictionary<int, Tuple<int, string>> requestedTunings = new Dictionary<int, Tuple<int, string>>();
    private JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
      Error = (obj, args) =>
      {
        var context = args.ErrorContext;

        context.Handled = true;
      }
    };

    public VehicleServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
    }

    private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.PersistentVehiclesServer))
        return;

      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);

      RegisterCommand("car", new Action<int, List<object>, string>((source, args, raw) =>
      {
        var sourcePlayer = Players.Where(p => p.Handle == source.ToString()).FirstOrDefault();
        if (sourcePlayer == null)
          return;

        if (!CharacterCaretakerServer.HasPlayerAdminLevel(sourcePlayer, 10))
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Nedostatecne opravneni");
          return;
        }

        if (args == null || args.Count <= 0)
          return;

        var ped = Ped.FromPlayerHandle(source.ToString());
        var pos = ped.Position;

        var vehID = SpawnPersistentVehicle(GetHashKey(args[0].ToString()), new Vector3(pos.X, pos.Y, pos.Z), ped.Heading);

        Task.Run(async () =>
        {
          while (!DoesEntityExist(vehID))
            await Delay(0);

          var veh = new Vehicle(vehID);
          persistentVehicles.Add(new PersistentVehicleBag { NetID = veh.NetworkId, LastKnownPos = new PosBag(veh.Position.X, veh.Position.Y, veh.Position.Z, veh.Heading), ModelHash = veh.Model });
        });
      }), false);

      RegisterCommand("givecar", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        var sourcePlayer = Players.Where(p => p.Handle == source.ToString()).FirstOrDefault();
        if (sourcePlayer == null)
          return;

        if (!CharacterCaretakerServer.HasPlayerAdminLevel(sourcePlayer, 10))
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Nedostatecne opravneni");
          return;
        }

        if (args == null || args.Count <= 1)
          return;

        int model = GetHashKey((string)args[0]);
        string plate = (string)args[1];

        string properties = JsonConvert.SerializeObject(VehiclePropertiesBag.InitializeNew(model, plate));

        var param = new Dictionary<string, object>();
        param.Add("@model", model);
        param.Add("@plate", plate.ToLower().Trim());
        param.Add("@place", "main");
        param.Add("@properties", properties);
        param.Add("@owner_char", CharacterCaretakerServer.GetPlayerLoggedCharacterId(sourcePlayer));
        var result = await VSql.ExecuteAsync("insert into `vehicle` (`model`, `plate`, `place`, `properties`, `owner_char`) values (@model, @plate, @place, @properties, @owner_char)", param);

        if (result == 1)
          sourcePlayer.TriggerEvent("ofw:SuccessNotification", "Auto je v garazi");
        else
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Chyba");
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
          //data = new PersistentVehicleDatabaseBag { Vehicles = new List<PersistentVehicleBag>() };
      //  }
      //}

      Tick += VehicleManagerTick;
      //Tick += PeriodicSave;

      InternalDependencyManager.Started(eScriptArea.PersistentVehiclesServer);
    }


    #region tuning
    [EventHandler("ofw_veh:UpdateRequestedTuning")]
    private async void UpdateRequestedTuning([FromSource] Player source, int veh, int model, string properties)
    {
      if (source == null || veh == 0)
      {
        return;
      }

      if (requestedTunings.ContainsKey(veh))
        requestedTunings[veh] = new Tuple<int, string>(model, properties);
      else
        requestedTunings.Add(veh, new Tuple<int, string>(model, properties));
    }

    [EventHandler("ofw_veh:GetRequestedTuning")]
    private async void GetRequestedTuning([FromSource] Player source, int veh, int model, NetworkCallbackDelegate callback)
    {
      if (source == null || veh == 0)
      {
        return;
      }

      if (requestedTunings.ContainsKey(veh) && requestedTunings[veh].Item1 == model)
        _ = callback(requestedTunings[veh].Item2);
      else
        _ = callback("");
    }

    [EventHandler("ofw_veh:InstallRequestedTuning")]
    private async void InstallRequestedTuning([FromSource] Player source, int veh, int model, string installProperties, NetworkCallbackDelegate callback)
    {
      if (source == null || veh == 0)
      {
        return;
      }

      if (installProperties == null)
      {
        _ = callback("");
        return;
      }

      var installTuning = JsonConvert.DeserializeObject<VehiclePropertiesBag>(installProperties);

      if (installTuning == null)
      {
        _ = callback("");
        return;
      }

      if (requestedTunings.ContainsKey(veh) && requestedTunings[veh].Item1 == model)
      {
        var reqTuning = JsonConvert.DeserializeObject<VehiclePropertiesBag>(requestedTunings[veh].Item2);

        if (reqTuning == null)
        {
          requestedTunings.Remove(veh);
          _ = callback("");
          return;
        }

        var prices = installTuning.ComputeUpgradePrice();
        if (prices != null && prices.Count > 0)
        {
          var ret = await OfwServerFunctions.ServerAsyncCallbackToSync<string>("ofw_inventory:RemoveInventoryItemsCB", source.Handle, JsonConvert.SerializeObject(prices));

          if (!string.IsNullOrEmpty(ret))
          {
            _ = callback("");
            source.TriggerEvent("ofw:ValidationErrorNotification", ret);
            return;
          }
        }

        reqTuning.Substract(installTuning);

        var newReqTuning = JsonConvert.SerializeObject(reqTuning, Formatting.None, jsonSettings);
        requestedTunings[veh] = new Tuple<int, string>(model, newReqTuning);

        _ = callback("true");
      }
      else
        _ = callback("");
    }

    #endregion

    #region garaz
    [EventHandler("ofw_garage:TakeOutVehicle")]
    private async void SpawnServerVehicleFromGarage([FromSource] Player source, string plate, string garage, Vector3 pos, float heading, NetworkCallbackDelegate callback)
    {
      if (source == null)
      {
        _ = callback(-1);
        return;
      }

      var sourcePlayer = Players.Where(p => p.Handle == source.Handle).FirstOrDefault();
      if (sourcePlayer == null)
      {
        _ = callback(-1);
        return;
      }

      int charId = CharacterCaretakerServer.GetPlayerLoggedCharacterId(sourcePlayer);
      if (charId <= 0)
      {
        sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neplatná postava");
        _ = callback(-1);
        return;
      }

      plate = plate.ToLower().Trim();

      var param = new Dictionary<string, object>();
      param.Add("@plate", plate);
      param.Add("@place", garage);
      param.Add("@charId", charId);
      var result = await VSql.FetchAllAsync("SELECT `id`, `properties` FROM `vehicle` WHERE `plate` = @plate and `owner_char` = @charId and `place` = @place" , param);

      if (result == null || result.Count <= 0)
      {
        Debug.WriteLine("ofw_garage:TakeOutVehicle: Car with plate not found: " + plate);
        source.TriggerEvent("ofw:ValidationErrorNotification", "Auto není v seznamu vlastněných");
        _ = callback(-1);
        return;
      }
      int garageId = (int)result[0]["id"];
      if (IsOwnedVehicleOut(garageId))
      {
        Debug.WriteLine("ofw_garage:TakeOutVehicle: Car is not stored: " + plate);
        source.TriggerEvent("ofw:ValidationErrorNotification", "Auto není v garáži");
        _ = callback(-1);
        return;
      }

      var sProps = (string)result[0]["properties"];

      if (sProps == null)
      {
        Debug.WriteLine("ofw_garage:TakeOutVehicle: Invalid car properties: " + plate);
        source.TriggerEvent("ofw:ValidationErrorNotification", "Auto není správně uloženo v DB!");
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
        source.TriggerEvent("ofw:ValidationErrorNotification", "Chyba při spawnu auta, zkus to prosím znovu!");
        _ = callback(-1);
        return;
      }

      await Delay(200);

      int frameCounter = 0;
      while (!DoesEntityExist(vehID))
      {
        await Delay(100);
        frameCounter++;
        if (frameCounter >= 50)
        {

          Debug.WriteLine("OFW_VEH: Vehicle from garage server spawn timeout!");
          source.TriggerEvent("ofw:ValidationErrorNotification", "Timeout pro spawn auta, zkus to prosím znovu!");
          _ = callback(-1);
          return;
        }
      }

      var veh = new Vehicle(vehID);
      persistentVehicles.Add(new PersistentVehicleBag { NetID = veh.NetworkId, Plate = plate, LastKnownPos = new PosBag(veh.Position.X, veh.Position.Y, veh.Position.Z, veh.Heading), ModelHash = veh.Model, GarageId = garageId });

      _ = callback(veh.NetworkId);
    }

    [EventHandler("ofw_garage:GetVehiclesInGarage")]
    private async void GetVehiclesInGarage([FromSource] Player source, string garage, NetworkCallbackDelegate callback)
    {
      if (source == null)
      {
        _ = callback(String.Empty);
        return;
      }

      var sourcePlayer = Players.Where(p => p.Handle == source.Handle).FirstOrDefault();
      if (sourcePlayer == null)
      {
        Debug.WriteLine($"ofw_garage:GetVehiclesInGarage: Nenalezen hrac {source.Handle}");
        _ = callback(String.Empty);
        return;
      }

      int charId = CharacterCaretakerServer.GetPlayerLoggedCharacterId(sourcePlayer);
      if (charId <= 0)
      {
        sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neplatná postava");
        _ = callback(String.Empty);
        return;
      }

      var param = new Dictionary<string, object>();
      param.Add("@place", garage);
      param.Add("@charId", charId);
      var result = await VSql.FetchAllAsync("SELECT `id`, `model`, `plate`, `place`, `owner_char`, `owner_organization`, `properties` FROM `vehicle` WHERE `owner_char` = @charId and `place` = @place", param);

      if (result == null || result.Count <= 0)
      {
        Debug.WriteLine($"ofw_garage:GetVehiclesInGarage: Zadne auto pro place:{garage} char:{charId}");
        _ = callback(String.Empty);
        return;
      }

      var garageVehicles = new List<GarageVehicleBag>();
      foreach (var row in result)
      {
        garageVehicles.Add(GarageVehicleBag.ParseFromSql(row));
      }

      foreach (var v in garageVehicles)
      {
        v.IsOut = IsOwnedVehicleOut(v.Id);
      }

      _ = callback(JsonConvert.SerializeObject(garageVehicles));
    }

    [EventHandler("ofw_garage:ReturnVehicle")]
    private async void ReturnVehicleToGarage([FromSource] Player source, string plate, string garage, string properties)
    {
      if (source == null)
      {
        return;
      }

      var sourcePlayer = Players.Where(p => p.Handle == source.Handle).FirstOrDefault();
      if (sourcePlayer == null)
      {
        return;
      }

      VehiclePropertiesBag props = null;
      if (properties != null)
        props = JsonConvert.DeserializeObject<VehiclePropertiesBag>(properties);

      if (props == null)
      {
        sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neplatná data auta");
        return;
      }

      int charId = CharacterCaretakerServer.GetPlayerLoggedCharacterId(sourcePlayer);
      if (charId <= 0)
      {
        sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neplatná postava");
        return;
      }

      plate = plate.ToLower().Trim();
      var param = new Dictionary<string, object>();
      param.Add("@plate", plate);
      var result = await VSql.FetchAllAsync("SELECT `id`, `properties` FROM `vehicle` WHERE `plate` = @plate", param);

      if (result == null || result.Count <= 0)
      {
        sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neznámé auto");
        return;
      }

      var persistVeh = persistentVehicles.FirstOrDefault(x => x.Plate == plate);

      var sProps = (string)result[0]["properties"];

      if (sProps == null)
      {
        Debug.WriteLine("ofw_garage:TakeOutVehicle: Invalid car properties: " + plate);
        source.TriggerEvent("ofw:ValidationErrorNotification", "Auto není správně uloženo v DB!");
        return;
      }

      VehiclePropertiesBag savedProperties = JsonConvert.DeserializeObject<VehiclePropertiesBag>(sProps, jsonSettings);
      savedProperties.FillNulls(props);

      var param2 = new Dictionary<string, object>();
      param2.Add("@plate", plate);
      param2.Add("@properties", JsonConvert.SerializeObject(savedProperties));
      var result2 = await VSql.ExecuteAsync("UPDATE `vehicle` SET `properties` = @properties WHERE `plate` = @plate", param2);

      if (persistVeh != null)
      {
        persistentVehicles.Remove(persistVeh);
      }

      sourcePlayer.TriggerEvent("ofw_garage:VehicleReturned", plate);
    }
    #endregion


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
      persistentVehicles.Add(new PersistentVehicleBag { NetID = veh.NetworkId, LastKnownPos = new PosBag(veh.Position.X, veh.Position.Y, veh.Position.Z, veh.Heading), ModelHash = veh.Model });

      _ = callback(veh.NetworkId);
    }

    

    [EventHandler("ofw_veh:IsVehicleWithPlateOutOfGarage")]
    private async void IsVehicleWithPlateOutOfGarage([FromSource] Player source, string plate, NetworkCallbackDelegate callback)
    {
      if (source == null)
        return;

      for (int i = 0; i < persistentVehicles.Count; i++)
      {
        if (persistentVehicles[i].Plate == plate)
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
      for (int i = 0; i < persistentVehicles.Count; i++)
      {
        if (persistentVehicles[i].NetID == netID)
          return true;
      }

      return false;
    }

    public static bool IsOwnedVehicleOut(int garageId)
    {
      if (persistentVehicles.Any(v => v.GarageId == garageId))
        return true;

      return false;
    }

    private async Task VehicleManagerTick()
    {
      for (int i = persistentVehicles.Count - 1; i >= 0; i--)
      {
        var iveh = persistentVehicles[i];

        if (NetworkGetEntityFromNetworkId(iveh.NetID) <= 0 || GetEntityModel(NetworkGetEntityFromNetworkId(iveh.NetID)) != iveh.ModelHash)
        {
          Debug.WriteLine($"PersistentVehicles: Removing no longer existing vehicle from sync [NetID: {iveh.NetID}, Model: {iveh.ModelHash}]");
          //PersistentVehRemoved?.Invoke(this, new PersistentVehicleRemovedArgs { NetID = iveh.NetID });
          persistentVehicles.Remove(iveh);
          continue;
        }

        var vehID = NetworkGetEntityFromNetworkId(iveh.NetID);
        var vehEnt = new Vehicle(vehID);

        iveh.LastKnownPos.X = vehEnt.Position.X;
        iveh.LastKnownPos.Y = vehEnt.Position.Y;
        iveh.LastKnownPos.Z = vehEnt.Position.Z;
        iveh.LastKnownPos.Heading = vehEnt.Heading;
      }

      await Delay(10000);
    }

    //private async Task PeriodicSave()
    //{
    //  await Delay(10000);

    //  if (data == null)
    //    return;

    //  string serialized = JsonConvert.SerializeObject(data);

    //  var param = new Dictionary<string, object> { };
    //  param.Add("@value", serialized);

    //  await VSql.ExecuteAsync("INSERT INTO cfw_jsondata (`key`, `data`) VALUES ('pvs', @value) ON DUPLICATE KEY UPDATE `key` = 'pvs', `data` = @value", param);
    //  Debug.WriteLine("PersistenVehicles: CurrentData: " + JsonConvert.SerializeObject(data));
    //}

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
