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
    private async void GetVehicles([FromSource] Player source, NetworkCallbackDelegate callback)
    {
      if (source == null)
        return;

      var player = ESX.GetPlayerFromId(Int32.Parse(source.Handle));

      var param = new Dictionary<string, object>();
      param.Add("@id", player.identifier);
      var result = await VSql.FetchAllAsync("SELECT `plate`, `vehicle`, `type`, `job`, `stored`, `garage` FROM `owned_vehicles` WHERE `owner` = @id", param);

      _ = callback(result != null ? JsonConvert.SerializeObject(result) : null);
    }

    [EventHandler("ofw_esxgarage:IsVehicleOwned")]
    private async void IsVehicleOwned([FromSource] Player source, string plate, NetworkCallbackDelegate callback)
    {
      if (source == null)
        return;

      var player = ESX.GetPlayerFromId(Int32.Parse(source.Handle));

      var param = new Dictionary<string, object>();
      param.Add("@plate", plate);
      var result = await VSql.FetchScalarAsync("SELECT 1 FROM `owned_vehicles` WHERE `plate` = @plate", param);

      _ = callback((result != null && result != DBNull.Value) ? true : false);
    }

    [EventHandler("ofw_esxgarage:SaveVehicle")]
    private async void SaveVehicle([FromSource] Player source, string garageId, dynamic vehicleProps)
    {
      var player = ESX.GetPlayerFromId(Int32.Parse(source.Handle));
      string plate = (string)vehicleProps.plate;

      var param1 = new Dictionary<string, object>();
      param1.Add("@plate", plate);

      var result1 = await VSql.FetchScalarAsync("SELECT `vehicle` FROM `owned_vehicles` WHERE `plate` = @plate", param1);
      if (result1 != null && result1 is string)
      {
        dynamic storedVeh = JsonConvert.DeserializeObject<ExpandoObject>((string)result1);
        if (storedVeh?.model?.ToString() != vehicleProps?.model?.ToString())
        {
          Debug.WriteLine("Model changed, not saving vehicle! Following player is probably cheater: " + player.identifier);
          return;
        }
      }

      var param2 = new Dictionary<string, object>();
      param2.Add("@plate", plate);
      param2.Add("@vehicle", JsonConvert.SerializeObject(vehicleProps));
      await VSql.ExecuteAsync("UPDATE `owned_vehicles` SET `stored` = 1, `vehicle` = @vehicle WHERE `plate` = @plate", param2);
    }

    #endregion
  }
}
