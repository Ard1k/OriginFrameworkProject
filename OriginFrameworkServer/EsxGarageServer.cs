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
  public class EsxGarageServer : BaseScript
  {
    private dynamic ESX = null;
    private int recoverVehiclePrice = 1000;
    private JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
      Error = (obj, args) =>
      {
        var context = args.ErrorContext;

        context.Handled = true;
      }
    };

    public EsxGarageServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
    }

		#region event handlers

		private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        await Delay(0);

      recoverVehiclePrice = SettingsManager.Settings.GarageRecoverPrice;

      while (ESX == null)
      {
        TriggerEvent("esx:getSharedObject", new object[] { new Action<dynamic>(esx => { ESX = esx; }) });
        await Delay(0);
      }
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
      
    }

    [EventHandler("ofw_esxgarage:GetVehicles")]
    private async void GetVehicles([FromSource] Player source, string type, string garage, string playerJob, NetworkCallbackDelegate callback)
    {
      if (source == null)
        return;

      var player = ESX.GetPlayerFromId(Int32.Parse(source.Handle));

      if (garage == null || !garage.StartsWith("Imp"))
      {
        var param = new Dictionary<string, object>();
        param.Add("@id", player.identifier);
        param.Add("@type", type ?? "car");
        param.Add("@society", (playerJob != null) ? $"society:{playerJob}" : null);
        var result = await VSql.FetchAllAsync("SELECT `plate`, `vehicle`, `type`, `job`, `stored`, `garage` FROM `owned_vehicles` WHERE (`owner` = @id OR `owner` = @society) AND `type` = @type", param);

        _ = callback(result != null ? JsonConvert.SerializeObject(result) : null);
      }
      else
      {
        var param = new Dictionary<string, object>();
        param.Add("@garage", garage);
        param.Add("@type", type ?? "car");
        var result = await VSql.FetchAllAsync("SELECT `plate`, `vehicle`, `type`, `job`, `stored`, `garage` FROM `owned_vehicles` WHERE `garage` = @garage AND `type` = @type", param);

        _ = callback(result != null ? JsonConvert.SerializeObject(result) : null);
      }
    }

    [EventHandler("ofw_esxgarage:CanParkVehicle")]
    private async void CanParkVehicle([FromSource] Player source, string plate, string garageType, NetworkCallbackDelegate callback)
    {
      if (source == null)
        return;

      var player = ESX.GetPlayerFromId(Int32.Parse(source.Handle));

      var param = new Dictionary<string, object>();
      param.Add("@plate", plate);
      param.Add("@type", garageType ?? "car");
      var result = await VSql.FetchScalarAsync("SELECT 1 FROM `owned_vehicles` WHERE `plate` = @plate AND `type` = @type", param);

      _ = callback((result != null && result != DBNull.Value) ? true : false);
    }

    [EventHandler("ofw_esxgarage:RecoverVehicle")]
    private async void RecoverVehicle([FromSource] Player source, string plate, string garageId)
    {
      if (source == null)
        return;

      bool useBank = false;
      var xPlayer = ESX.GetPlayerFromId(Int32.Parse(source.Handle));
      if (xPlayer.getAccount("money").money < recoverVehiclePrice)
      {
        if (xPlayer.getAccount("bank").money < recoverVehiclePrice)
        {
          source.TriggerEvent("ofw:ValidationErrorNotification", "Nemas dost penez!");
          return;
        }
        else
          useBank = true;
      }

      var param = new Dictionary<string, object>();
      param.Add("@plate", plate);
      param.Add("@garage", garageId);
      await VSql.ExecuteAsync("UPDATE `owned_vehicles` SET `stored` = 1, `garage` = @garage WHERE `plate` = @plate", param);

      //Potrebujem do main threadu, proto delay
      await Delay(0);

      if (useBank)
        xPlayer.removeAccountMoney("bank", recoverVehiclePrice);
      else
        xPlayer.removeAccountMoney("money", recoverVehiclePrice);

      source.TriggerEvent("ofw:SuccessNotification", $"Vozidlo obnoveno do garaze: {garageId}");
    }

    [EventHandler("ofw_esxgarage:SaveVehicle")]
    private async void SaveVehicle([FromSource] Player source, string garageId, string vehiclePropsSerialized)
    {
      if (vehiclePropsSerialized == null)
        return;

      var vehicleProps = JsonConvert.DeserializeObject<VehiclePropertiesBag>(vehiclePropsSerialized);

      var player = ESX.GetPlayerFromId(Int32.Parse(source.Handle));
      string plate = (string)vehicleProps.plate;

      var param1 = new Dictionary<string, object>();
      param1.Add("@plate", plate);

      var result1 = await VSql.FetchScalarAsync("SELECT `vehicle` FROM `owned_vehicles` WHERE `plate` = @plate", param1);
      if (result1 != null && result1 is string)
      {
        var storedVeh = JsonConvert.DeserializeObject<VehiclePropertiesBag>((string)result1, jsonSettings);
        if (storedVeh?.model.ToString() != vehicleProps?.model.ToString())
        {
          Debug.WriteLine("Model changed, not saving vehicle! Following player is probably cheater: " + player.identifier);
          return;
        }
      }

      var param2 = new Dictionary<string, object>();
      param2.Add("@plate", plate);
      param2.Add("@garage", garageId);
      param2.Add("@vehicle", vehiclePropsSerialized);
      await VSql.ExecuteAsync("UPDATE `owned_vehicles` SET `stored` = 1, `vehicle` = @vehicle, `garage` = @garage WHERE `plate` = @plate", param2);
    }

    #endregion
  }
}
