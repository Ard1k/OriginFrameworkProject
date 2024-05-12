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
using System.IO;

namespace OriginFrameworkServer
{
  public class VehicleServer : BaseScript
  {
    private static List<PersistentVehicleBag> persistentVehicles = new List<PersistentVehicleBag>();
    private static Dictionary<int, Tuple<int, string>> requestedTunings = new Dictionary<int, Tuple<int, string>>();
    private static List<int> failedSpawns = new List<int>();
    private JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
      Error = (obj, args) =>
      {
        var context = args.ErrorContext;

        context.Handled = true;
      }
    };

    private Random rand = new Random();
    private static List<VehColor> cachedColors = null;

    public VehicleServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
      rand.Next();
      rand.Next();
      rand.Next();
      cachedColors = VehColor.Defined.Where(c => c.IsUnused == false).ToList();
    }

    private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.PersistentVehiclesServer))
        return;

      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);

      DefinedVehicles.KnownVehiclesByHash = new Dictionary<int, VehicleInformation>();
      foreach (var veh in DefinedVehicles.KnownVehicles)
      {
        var hashKey = GetHashKey(veh.Key);
        if (DefinedVehicles.KnownVehiclesByHash.ContainsKey(hashKey))
        {
          Debug.WriteLine($"VehicleClient: Duplicate vehicle hash {veh.Key} ({hashKey})");
          continue;
        }
        DefinedVehicles.KnownVehiclesByHash.Add(hashKey, veh.Value);
      }

      #region register commands
      RegisterCommand("car", new Action<int, List<object>, string>(async (source, args, raw) =>
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
        int modelHash = GetHashKey(args[0].ToString());

        var vehID = SpawnPersistentVehicle(modelHash, new Vector3(pos.X, pos.Y, pos.Z), ped.Heading);

        string plate = null;
        while (plate == null)
        {
          string genPlate = $"SPW{rand.Next(10000, 100000)}"; //TODO lepsi reseni
          bool plateExists = await VehicleServer.DoesPlateExist(genPlate, false);
          if (!plateExists)
            plate = genPlate;
        }

        await Delay(0);

        while (!DoesEntityExist(vehID))
            await Delay(0);

        var veh = new Vehicle(vehID);
        
        int color1 = cachedColors[rand.Next(cachedColors.Count)].ColorIndex;
        int color2 = cachedColors[rand.Next(cachedColors.Count)].ColorIndex;
        int colorp = cachedColors[rand.Next(cachedColors.Count)].ColorIndex;
        int colorw = cachedColors[rand.Next(cachedColors.Count)].ColorIndex;

        var persistBag = new PersistentVehicleBag { NetID = veh.NetworkId, Plate = plate, LastKnownPos = new PosBag(veh.Position.X, veh.Position.Y, veh.Position.Z, veh.Heading), ModelHash = veh.Model, KeepUnlocked = true, BrokenLock = true, Properties = JsonConvert.SerializeObject(new VehiclePropertiesBag { plate = plate, model = modelHash, color1 = color1, color2 = color2, pearlescentColor = colorp, wheelColor = colorw }) };
        persistentVehicles.Add(persistBag);
        TriggerClientEvent("ofw_veh:RespawnedCarRestoreProperties", persistBag.NetID, persistBag.Plate, persistBag.KeepUnlocked, persistBag.BrokenLock, persistBag.Properties, persistBag.Damage);
        sourcePlayer.TriggerEvent("ofw_veh:EnterSpawnedVehicle", veh.NetworkId);
      }), false);

      RegisterCommand("getcar", new Action<int, List<object>, string>(async (source, args, raw) =>
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

      //new PersistentVehicleBag { NetID = veh.NetworkId, LastKnownPos = new PosBag(veh.Position.X, veh.Position.Y, veh.Position.Z, veh.Heading), ModelHash = veh.Model }
      RegisterCommand("resptest", new Action<int, List<object>, string>((source, args, raw) =>
      {
        var sourcePlayer = Players.Where(p => p.Handle == source.ToString()).FirstOrDefault();

        //X = 225.9405f, Y = -773.949f, Z = 29.6246f, Heading = 250.7624f
        float x = 225;
        float y = -773;
        float z = 30;
        float heading = 250;

        if (sourcePlayer != null)
        {
          var ped = Ped.FromPlayerHandle(source.ToString());
          var pos = ped.Position;
          x = pos.X;
          y = pos.Y;
          z = pos.Z;
          heading = ped.Heading;
        }

        persistentVehicles.Add(new PersistentVehicleBag { NetID = 445, LastKnownPos = new PosBag(x, y, z, heading), ModelHash = 1663218586 });

      }), false);

      RegisterCommand("delnet", new Action<int, List<object>, string>((source, args, raw) =>
      {
        try
        {
          if (args == null || args.Count != 1)
            return;

          int ent = NetworkGetEntityFromNetworkId(Convert.ToInt32(args[0]));
          DeleteEntity(ent);

        }
        catch { }
      }), false);
      #endregion

      while (SettingsManager.Settings == null)
        await Delay(0);

      while (!VSql.IsReadyToUse)
        await Delay(0);

      Tick += VehicleManagerTick;
      Tick += CleanupTick;

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
    private async void InstallRequestedTuning([FromSource] Player source, int vehNetId, int model, string installProperties, NetworkCallbackDelegate callback)
    {
      if (source == null || vehNetId == 0)
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

      if (!requestedTunings.ContainsKey(vehNetId) || requestedTunings[vehNetId].Item1 != model)
      {
        _ = callback("");
        return;
      }

      var reqTuning = JsonConvert.DeserializeObject<VehiclePropertiesBag>(requestedTunings[vehNetId].Item2);

      if (reqTuning == null)
      {
        requestedTunings.Remove(vehNetId);
        _ = callback("");
        return;
      }

      int veh = NetworkGetEntityFromNetworkId(vehNetId);
      string plate = GetVehicleNumberPlateText(veh).ToLower().Trim();
      
      var param = new Dictionary<string, object>();
      param.Add("@plate", plate);
      var result = await VSql.FetchAllAsync("SELECT `id`, `properties` FROM `vehicle` WHERE `plate` = @plate", param);

      VehiclePropertiesBag savedProperties = null;
      if (result != null && result.Count > 0)
      {
        var sProps = (string)result[0]["properties"];

        if (sProps != null)
        {
          savedProperties = JsonConvert.DeserializeObject<VehiclePropertiesBag>(sProps, jsonSettings);
        }
      }

      await Delay(0); //navrat do main threadu;
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
      requestedTunings[vehNetId] = new Tuple<int, string>(model, newReqTuning);

      if (savedProperties != null)
      {
        savedProperties.Add(installTuning);

        var param2 = new Dictionary<string, object>();
        param2.Add("@plate", plate);
        param2.Add("@properties", JsonConvert.SerializeObject(savedProperties));
        var result2 = await VSql.ExecuteAsync("UPDATE `vehicle` SET `properties` = @properties WHERE `plate` = @plate", param2);

        await Delay(0);
      }

      try
      {
        var persisVeh = persistentVehicles.Where(p => p.NetID == vehNetId).FirstOrDefault();
        if (persisVeh != null && persisVeh.Properties != null)
        {
          var persistProperties = JsonConvert.DeserializeObject<VehiclePropertiesBag>(persisVeh.Properties, jsonSettings);
          if (persistProperties != null)
          {
            persistProperties.Add(installTuning);
            persisVeh.Properties = JsonConvert.SerializeObject(persistProperties, Formatting.None, jsonSettings);
          }
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"ofw_veh:InstallRequestedTuning: persisten properties update exception: {ex.Message}");
      }

      int veh2 = NetworkGetEntityFromNetworkId(vehNetId);
      int entityOwner = NetworkGetEntityOwner(veh2);
      {
        var entityOwnerPlayer = Players.Where(p => p.Handle == entityOwner.ToString()).FirstOrDefault();
        
        if (entityOwnerPlayer == null)
        {
          Debug.WriteLine("ofw_veh:InstallRequestedTuning: entityOwnerNotFound, trying sourcePlayer");
          entityOwnerPlayer = Players.Where(p => p.Handle == source.Handle).FirstOrDefault();
        }

        if (entityOwnerPlayer == null)
        {
          Debug.WriteLine("ofw_veh:InstallRequestedTuning: sourcePlayer not found, not installing tuning");
          entityOwnerPlayer = Players.Where(p => p.Handle == source.Handle).FirstOrDefault();
        }

        if (entityOwnerPlayer != null)
          entityOwnerPlayer.TriggerEvent("ofw_veh:ApplyPropertiesOnVehicle", vehNetId, installProperties);
      }

      await Delay(300);
      _ = callback("true");
    }

    #endregion

    #region garaz
    [EventHandler("ofw_garage:TakeOutVehicle")]
    private async void TakeOutVehicle([FromSource] Player source, string plate, string garage, Vector3 pos, float heading, NetworkCallbackDelegate callback)
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

      var character = CharacterCaretakerServer.GetPlayerLoggedCharacter(sourcePlayer);
      if (character == null)
      {
        sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neplatná postava");
        _ = callback(-1);
        return;
      }

      plate = plate.ToLower().Trim();

      var param = new Dictionary<string, object>();
      param.Add("@plate", plate);
      param.Add("@place", garage);
      param.Add("@charId", character.Id);
      param.Add("@orgId", character.OrganizationId);
      var result = await VSql.FetchAllAsync("SELECT `id`, `properties`, `damage` FROM `vehicle` WHERE `plate` = @plate and (`owner_char` = @charId OR (@orgId is not null AND `owner_organization` = @orgId)) and `place` = @place" , param);

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

      var sDamage = (string)result[0]["damage"];

      VehiclePropertiesBag vehData = JsonConvert.DeserializeObject<VehiclePropertiesBag>(sProps, jsonSettings);

      var modelHash = (int)vehData.model;

      var keysResult = await InventoryServer.TryGiveCarKeys(character, modelHash, plate);

      await Delay(0);

      if (keysResult != null)
      {
        sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", keysResult);
        _ = callback(-1);
        return;
      }

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
      persistentVehicles.Add(new PersistentVehicleBag { NetID = veh.NetworkId, Plate = plate, LastKnownPos = new PosBag(veh.Position.X, veh.Position.Y, veh.Position.Z, veh.Heading), ModelHash = veh.Model, GarageId = garageId, Damage = sDamage });

      _ = callback(veh.NetworkId);
    }

    [EventHandler("ofw_garage:TakeOutFirstVehicle")]
    private async void TakeOutFirstVehicle([FromSource] Player source, Vector3 pos, float heading, NetworkCallbackDelegate callback)
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

      var character = CharacterCaretakerServer.GetPlayerLoggedCharacter(sourcePlayer);
      if (character == null)
      {
        sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neplatná postava");
        _ = callback(-1);
        return;
      }

      var param = new Dictionary<string, object>();
      param.Add("@charId", character.Id);
      param.Add("@orgId", character.OrganizationId);
      var result = await VSql.FetchAllAsync("SELECT `id`, `properties`, `plate`, `damage` FROM `vehicle` WHERE `owner_char` = @charId", param);

      if (result == null || result.Count <= 0)
      {
        Debug.WriteLine("ofw_garage:TakeOutFirstVehicle: Car not found ");
        source.TriggerEvent("ofw:ValidationErrorNotification", "Auto není v seznamu vlastněných");
        _ = callback(-1);
        return;
      }
      int garageId = (int)result[0]["id"];
      if (IsOwnedVehicleOut(garageId))
      {
        Debug.WriteLine("ofw_garage:TakeOutFirstVehicle: Car is not stored");
        source.TriggerEvent("ofw:ValidationErrorNotification", "Auto není v garáži");
        _ = callback(-1);
        return;
      }

      var sProps = (string)result[0]["properties"];

      if (sProps == null)
      {
        Debug.WriteLine("ofw_garage:TakeOutFirstVehicle: Invalid car properties ");
        source.TriggerEvent("ofw:ValidationErrorNotification", "Auto není správně uloženo v DB!");
        _ = callback(-1);
        return;
      }

      VehiclePropertiesBag vehData = JsonConvert.DeserializeObject<VehiclePropertiesBag>(sProps, jsonSettings);

      var sDamage = (string)result[0]["damage"];
      var modelHash = (int)vehData.model;

      var keysResult = await InventoryServer.TryGiveCarKeys(character, modelHash, vehData.plate);

      await Delay(0);

      if (keysResult != null)
      {
        sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", keysResult);
        _ = callback(-1);
        return;
      }

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
      var persistBag = new PersistentVehicleBag { NetID = veh.NetworkId, Plate = (string)result[0]["plate"], LastKnownPos = new PosBag(veh.Position.X, veh.Position.Y, veh.Position.Z, veh.Heading), ModelHash = veh.Model, GarageId = garageId, Properties = sProps, Damage = sDamage };
      persistentVehicles.Add(persistBag);
      TriggerClientEvent("ofw_veh:RespawnedCarRestoreProperties", persistBag.NetID, persistBag.Plate, persistBag.KeepUnlocked, persistBag.BrokenLock, persistBag.Properties, persistBag.Damage);
      persistBag.IsInPropertiesSync = true;
      persistBag.LastPropertiesSync = GetGameTimer();
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
      var result = await VSql.FetchAllAsync(
        "    SELECT v.* " +
        "      FROM `vehicle` v " +
        " LEFT JOIN `character` c ON c.`id` = @charId " +
        " LEFT JOIN `organization` o ON o.`id` = c.`organization_id` " +
        "     WHERE v.`place` = @place " +
        "       AND (" +
        "             v.`owner_char` = @charId " +
        "             OR ( " +
        "                  c.`organization_id` is not null AND " +
        "                  c.`organization_id` = v.`owner_organization` AND " +
        "                  ( " +
        "                    o.`owner` = c.`id` " +
        "                    OR EXISTS(select 1 from `organization_manager` om where om.`organization_id` = c.`organization_id` and om.`character_id` = c.`id`) " +
        "                    OR EXISTS(select 1 from `organization_vehiclerights` vr where vr.`vehicle_id` = v.`id` and vr.`character_id` = c.`id`) " +
        "                  ) " +
        "                ) " +
        "           ); "
        , param);

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
    private async void ReturnVehicleToGarage([FromSource] Player source, string plate, string garage, string properties, string damage)
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

      VehicleDamageBag dmg = null;
      if (damage != null)
        dmg = JsonConvert.DeserializeObject<VehicleDamageBag>(damage);

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

      var persistVeh = persistentVehicles.FirstOrDefault(x => x.Plate?.ToLower().Trim() == plate);

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
      param2.Add("@dmg", dmg != null ? JsonConvert.SerializeObject(dmg) : null);
      var result2 = await VSql.ExecuteAsync("UPDATE `vehicle` SET `properties` = @properties, `damage` = @dmg WHERE `plate` = @plate", param2);

      if (persistVeh != null)
      {
        persistentVehicles.Remove(persistVeh);
      }

      InventoryServer.RemoveCarKeys(plate);

      sourcePlayer.TriggerEvent("ofw_garage:VehicleReturned", plate);
    }

    [EventHandler("ofw_garage:GetOrganizationVehicles")]
    private async void GetOrganizationVehicles([FromSource] Player source, NetworkCallbackDelegate callback)
    {
      if (source == null)
      {
        _ = callback(String.Empty);
        return;
      }

      var sourcePlayer = Players.Where(p => p.Handle == source.Handle).FirstOrDefault();
      if (sourcePlayer == null)
      {
        Debug.WriteLine($"ofw_garage:GetOrganizationVehicles: Nenalezen hrac {source.Handle}");
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
      param.Add("@charId", charId);
      var result = await VSql.FetchAllAsync(
        "    SELECT v.* " +
        "      FROM `vehicle` v " +
        " LEFT JOIN `character` c ON c.`id` = @charId " +
        " LEFT JOIN `organization` o ON o.`id` = c.`organization_id` " +
        "     WHERE v.`owner_organization` = c.`organization_id` " +
        "       AND c.`organization_id` is not null " +
        "       AND " +
        "           (" +
        "                o.`owner` = c.`id`" +
        "             OR EXISTS(select 1 from `organization_manager` om where om.`organization_id` = c.`organization_id` and om.`character_id` = c.`id`) " +
        "             OR EXISTS(select 1 from `organization_vehiclerights` vr where vr.`vehicle_id` = v.`id` and vr.`character_id` = c.`id`) " +
        "           ) "
        , param) ;

      if (result == null || result.Count <= 0)
      {
        Debug.WriteLine($"ofw_garage:GetOrganizationVehicles: Zadne auto pro char:{charId}");
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

    [EventHandler("ofw_garage:GetCharacterVehicles")]
    private async void GetCharacterVehicles([FromSource] Player source, NetworkCallbackDelegate callback)
    {
      if (source == null)
      {
        _ = callback(String.Empty);
        return;
      }

      var sourcePlayer = Players.Where(p => p.Handle == source.Handle).FirstOrDefault();
      if (sourcePlayer == null)
      {
        Debug.WriteLine($"ofw_garage:GetCharacterVehicles: Nenalezen hrac {source.Handle}");
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
      param.Add("@charId", charId);
      var result = await VSql.FetchAllAsync(
        "    SELECT v.* " +
        "      FROM `vehicle` v " +
        "     WHERE v.`owner_char` = @charId "
        , param);

      if (result == null || result.Count <= 0)
      {
        Debug.WriteLine($"ofw_garage:GetCharacterVehicles: Zadne auto pro char:{charId}");
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
    #endregion


    //Legacy, nez se to nekde pouzije, tak zkontrolovat kod
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

    #region persistence eventy
    [EventHandler("ofw_veh:AckPropertiesSynced")]
    private async void AckPropertiesSynced([FromSource] Player source, string plate)
    {
      if (plate == null)
        return;

      plate = plate.ToLower().Trim();

      var syncVeh = persistentVehicles.Where(x => x.Plate?.ToLower().Trim() == plate).FirstOrDefault();

      if (syncVeh != null)
      {
        syncVeh.IsInPropertiesSync = false;
        Debug.WriteLine("ofw_veh:AckPropertiesSynced: Ack received for synced properties plate: " + plate);
      }
    }

    [EventHandler("ofw_veh:AddPersistentVehicle")]
    private async void AddPersistentVehicle([FromSource] Player source, int netId, string plate, bool keepUnlocked, bool brokenLock, string properties, string damage)
    {
      try
      {
        if (plate == null)
          return;

        plate = plate.ToLower().Trim();
        var vehId = NetworkGetEntityFromNetworkId(netId);
        var veh = Vehicle.FromHandle(vehId);

        var existing = persistentVehicles.Where(x => x.NetID == netId).FirstOrDefault();

        if (existing != null)
        {
          if (existing.Plate == null) existing.Plate = plate;
          if (existing.Properties == null) existing.Properties = properties;
          if (existing.Damage == null) existing.Damage = damage;
        }
        else
          persistentVehicles.Add(new PersistentVehicleBag { NetID = netId, Plate = plate, LastKnownPos = new PosBag(veh.Position.X, veh.Position.Y, veh.Position.Z, veh.Heading), KeepUnlocked = keepUnlocked, BrokenLock = brokenLock, ModelHash = veh.Model, Properties = properties, Damage = damage });
      }
      catch { }
    }

    [EventHandler("ofw_veh:UpdatePersistentVehicle")]
    private async void UpdatePersistentVehicle([FromSource] Player source, int netId, string damage)
    {
      try
      {
        var persistVeh = persistentVehicles.FirstOrDefault(b => b.NetID == netId);
        if (persistVeh == null)
          return;

        persistVeh.Damage = damage;
      }
      catch { }
    }
    #endregion

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      for (int i = persistentVehicles.Count - 1; i >= 0; i--)
      {
        try
        {
          if (persistentVehicles[i].NetID <= 0)
            continue;

          var vehId = NetworkGetEntityFromNetworkId(persistentVehicles[i].NetID);
          DeleteEntity(vehId);
        }
        catch { }
      }
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

    private async Task CleanupTick()
    {
      for (int i = failedSpawns.Count - 1; i >= 0; i--)
      {
        var ent = failedSpawns[i];

        if (DoesEntityExist(ent))
        {
          DeleteEntity(ent);
          await Delay(0);
          failedSpawns.Remove(ent);
          Debug.WriteLine("Removed failed type A");
        }
      }

      //var vehicles = GetAllVehicles();

      //if (vehicles == null)
      //  return;

      //foreach (int veh in vehicles)
      //{
      //  Console.WriteLine($"owner: {NetworkGetFirstEntityOwner(veh)}, loc:{veh} net: {NetworkGetNetworkIdFromEntity(veh)}");

      //  if (NetworkGetFirstEntityOwner(veh) != -1)
      //    continue;

      //  int netId = NetworkGetNetworkIdFromEntity(veh);
      //  if (!IsVehicleKnown(netId))
      //  {
      //    DeleteEntity(veh);
      //    Debug.WriteLine($"OFW_VEH: Removed unknown vehicle {netId}");
      //  }
      //}

      await Delay(100);
    }

    private async Task VehicleManagerTick()
    {
      if (Players.Count() <= 0)
        return; //nikdo na serveru neni, nema cenu kontrolovat

      for (int i = persistentVehicles.Count - 1; i >= 0; i--)
      {
        var iveh = persistentVehicles[i];
        if (iveh.IsServerRespawning)
          continue;

        if (NetworkGetEntityFromNetworkId(iveh.NetID) <= 0 || GetEntityModel(NetworkGetEntityFromNetworkId(iveh.NetID)) != iveh.ModelHash)
        {
          if (iveh.ModelHash == 0 || iveh.LastKnownPos.IsEmpty())
          {
            persistentVehicles.Remove(iveh);
            Debug.WriteLine($"PersistentVehicles: Removing no longer existing vehicle from sync [NetID: {iveh.NetID}, Model: {iveh.ModelHash}]");
            continue;
          }

          iveh.IsServerRespawning = true;
          int respVehID = -1;
          respVehID = SpawnPersistentVehicle(iveh.ModelHash, new Vector3(iveh.LastKnownPos.X, iveh.LastKnownPos.Y, iveh.LastKnownPos.Z), iveh.LastKnownPos.Heading);

          await Delay(200);

          int frameCounter = 0;
          while (!DoesEntityExist(respVehID))
          {
            await Delay(1000);
            frameCounter++;
            if (frameCounter >= 5)
            {
              Debug.WriteLine("PersistentVehicles: Vehicle server respawn timeout!");
              failedSpawns.Add(respVehID);
              //Debug.WriteLine($"{JsonConvert.SerializeObject(iveh, Formatting.Indented)}");
              iveh.IsServerRespawning = false;
              break;
            }
          }

          if (!DoesEntityExist(respVehID))
            continue;

          var veh = new Vehicle(respVehID);
          iveh.NetID = veh.NetworkId;
          iveh.IsServerRespawning = false;

          if (iveh.VehicleVendorSlot != null)
          {
            VehicleVendorServer.SlotNetIdUpdated(iveh.VehicleVendorSlot.Value, iveh.NetID);
          }

          string syncProperties = null;
          if (iveh.GarageId != null)
          {
            var param = new Dictionary<string, object>();
            param.Add("@id", iveh.GarageId);
            var result = await VSql.FetchAllAsync("SELECT `properties` FROM `vehicle` WHERE `id` = @id", param);

            if (result != null && result.Count > 0)
            {
              syncProperties = (string)result[0]["properties"];
            }

            if (syncProperties != null)
              iveh.Properties = syncProperties;
            await Delay(0);
          }

          if (iveh.Properties != null && iveh.Plate != null)
          {
            TriggerClientEvent("ofw_veh:RespawnedCarRestoreProperties", iveh.NetID, iveh.Plate, iveh.KeepUnlocked, iveh.BrokenLock, iveh.Properties, iveh.Damage);
            iveh.IsInPropertiesSync = true;
            iveh.LastPropertiesSync = GetGameTimer();
          }

          continue;
        }

        if (iveh.IsInPropertiesSync && iveh.LastPropertiesSync != null)
        {
          if (GetGameTimer() - iveh.LastPropertiesSync.Value > 10000) //10s
          {
            TriggerClientEvent("ofw_veh:RespawnedCarRestoreProperties", iveh.NetID, iveh.Plate, iveh.KeepUnlocked, iveh.BrokenLock, iveh.Properties, iveh.Damage);
            iveh.LastPropertiesSync = GetGameTimer();
          }
        }

        var vehID = NetworkGetEntityFromNetworkId(iveh.NetID);
        var vehEnt = new Vehicle(vehID);

        var lastPos = iveh.LastKnownPos;
        lastPos.X = vehEnt.Position.X;
        lastPos.Y = vehEnt.Position.Y;
        lastPos.Z = vehEnt.Position.Z;
        lastPos.Heading = vehEnt.Heading;
        iveh.LastKnownPos = lastPos;

        if (GetEntityRoutingBucket(vehID) != 0)
        {
          SetEntityRoutingBucket(vehID, 0);

          string syncProperties = null;
          if (iveh.GarageId != null)
          {
            var param = new Dictionary<string, object>();
            param.Add("@id", iveh.GarageId);
            var result = await VSql.FetchAllAsync("SELECT `properties` FROM `vehicle` WHERE `id` = @id", param);

            if (result != null && result.Count > 0)
            {
              syncProperties = (string)result[0]["properties"];
            }

            if (syncProperties != null)
              iveh.Properties = syncProperties;
            await Delay(0);
          }

          if (iveh.Properties != null && iveh.Plate != null)
          {
            TriggerClientEvent("ofw_veh:RespawnedCarRestoreProperties", iveh.NetID, iveh.Plate, iveh.KeepUnlocked, iveh.BrokenLock, iveh.Properties, iveh.Damage);
            iveh.IsInPropertiesSync = true;
            iveh.LastPropertiesSync = GetGameTimer();
          }
        }
      }

      await Delay(1000);
    }

    public static void VendorSlotVehRemoveAndDepsawn(int vendorSlot)
    {
      var found = persistentVehicles.Where(p => p.VehicleVendorSlot == vendorSlot).FirstOrDefault();

      if (found == null)
        return;

      persistentVehicles.Remove(found);

      if (found.NetID <= 0)
        return;

      var vehId = NetworkGetEntityFromNetworkId(found.NetID);
      DeleteEntity(vehId);
    }

    public static async void PopulateVendorSlot(int vendorSlot, PosBag pos, string plate, int modelHash, string properties)
    {
      VendorSlotVehRemoveAndDepsawn(vendorSlot); //pro jistotu zkusime smazat

      await Delay(0);

      persistentVehicles.Add(new PersistentVehicleBag { VehicleVendorSlot = vendorSlot, LastKnownPos = pos, Plate = plate, ModelHash = modelHash, Properties = properties, KeepUnlocked = true });
    }

    public static async Task<bool> MigrateVendorSlotVehToGarageVeh(int slotId, string newPlate, int? ownerChar, int? ownerOrg)
    {
      newPlate = newPlate.ToLower().Trim();
      var persistVeh = persistentVehicles.Where(p => p.VehicleVendorSlot == slotId).FirstOrDefault();
      if (persistVeh == null)
      {
        Debug.WriteLine("MigrateVendorSlotVehToGarageVeh: VendorVehicle not found!");
        return false;
      }

      string propsString = persistVeh.Properties;
      VehiclePropertiesBag properties = null;
      if (propsString != null)
        properties = JsonConvert.DeserializeObject<VehiclePropertiesBag>(propsString);
      
      if (properties == null)
        properties = new VehiclePropertiesBag { model = persistVeh.ModelHash, plate = newPlate };
      else
        properties.plate = newPlate;

      string newProperties = JsonConvert.SerializeObject(properties);

      var param = new Dictionary<string, object>();
      param.Add("@ownerChar", ownerChar);
      param.Add("@ownerOrg", ownerOrg);
      param.Add("@properties", newProperties);
      param.Add("@modelHash", persistVeh.ModelHash);
      param.Add("@plate", newPlate);

      var res = await VSql.ExecuteAsync("insert into `vehicle` (`model`, `plate`, `place`, `properties`, `owner_char`, `owner_organization`) values (@modelHash, @plate, 'main', @properties, @ownerChar, @ownerOrg)", param);
      if (res != 1)
      {
        Debug.WriteLine("MigrateVendorSlotVehToGarageVeh: Error saving VendorVehicle");
        return false;
      }

      var garageIdRes = await VSql.FetchScalarAsync("select `id` from `vehicle` where `plate` = @plate ", param);
      if (garageIdRes == null || garageIdRes == DBNull.Value)
      {
        Debug.WriteLine("MigrateVendorSlotVehToGarageVeh: New garage id not found, persistent vehicles can't be updated");
        persistentVehicles.Remove(persistVeh);
        return true;
      }

      persistVeh.GarageId = Convert.ToInt32(garageIdRes);
      persistVeh.VehicleVendorSlot = null;
      persistVeh.Properties = newProperties;
      persistVeh.Plate = newPlate;

      await Delay(0);
      TriggerClientEvent("ofw_veh:RespawnedCarRestoreProperties", persistVeh.NetID, persistVeh.Plate, persistVeh.KeepUnlocked, persistVeh.BrokenLock, persistVeh.Properties, persistVeh.Damage);
      persistVeh.IsInPropertiesSync = true;
      persistVeh.LastPropertiesSync = GetGameTimer();

      return true;
    }

    public static async Task<bool> DoesPlateExist(string plate, bool searchDB)
    {
      if (searchDB)
      {
        var param = new Dictionary<string, object>();
        param.Add("@plate", plate);
        var result = await VSql.FetchAllAsync("SELECT `id` FROM `vehicle` WHERE `plate` = @plate", param);
        if (result != null && result.Count > 0)
          return true;
      }
      if (persistentVehicles.Any(p => p.Plate == plate))
        return true;

      return false;
    }

    /// <summary>
    /// Spawne auto a vrati vehId. Ale bacha, v tuhle chvili uz ma entita handle, ale pravdepodobne jeste realne neexistuje. Pri pouziti pockat nez DoesEntityExist bude true
    /// </summary>
    /// <param name="hash"></param>
    /// <param name="pos"></param>
    /// <param name="heading"></param>
    /// <returns></returns>
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
