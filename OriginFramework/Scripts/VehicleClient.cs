using CitizenFX.Core;
using Newtonsoft.Json;
using OriginFramework.Menus;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static OriginFramework.OfwFunctions;


namespace OriginFramework
{
  public class VehicleClient : BaseScript
  {
    public VehicleClient()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }


    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.VehicleClient))
        return;

      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);

      
      RegisterCommand("dv", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        if (CharacterCaretaker.LoggedCharacter?.AdminLevel > 0 == false)
        {
          Notify.Error("Nedostatečné oprávnění!");
        }

        if (Game.PlayerPed.IsInVehicle())
        {
          int veh = Game.PlayerPed.CurrentVehicle.Handle;
          DeleteEntity(ref veh);
          return;
        }

        int vehFront = Vehicles.GetVehicleInFront(null);
        if (vehFront > 0)
        {
          DeleteEntity(ref vehFront);
          return;
        }
      }), false);

      RegisterCommand("vehmakepersist", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        if (CharacterCaretaker.LoggedCharacter?.AdminLevel > 0 == false)
        {
          Notify.Error("Nedostatečné oprávnění!");
        }

        if (Game.PlayerPed.IsInVehicle())
        {
          int veh = Game.PlayerPed.CurrentVehicle.Handle;
          if (!NetworkGetEntityIsNetworked(veh))
            return;

          string plate = GetVehicleNumberPlateText(veh).ToLower().Trim();
          var props = Vehicles.GetVehicleProperties(veh);

          TriggerServerEvent("ofw_veh:AddPersistentVehicle", VehToNet(veh), plate, JsonConvert.SerializeObject(props));
        }
      }), false);


      InternalDependencyManager.Started(eScriptArea.VehicleClient);
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
    }

    [EventHandler("ofw_veh:ApplyPropertiesOnVehicle")]
    private void ApplyPropertiesOnVehicle(int vehNetId, string properties)
    {
      if (string.IsNullOrEmpty(properties))
      {
        Debug.WriteLine("ofw_veh:ApplyPropertiesOnVehicle: no properties sent");
        return;
      }

      if (vehNetId == 0 || !NetworkDoesEntityExistWithNetworkId(vehNetId))
      {
        Debug.WriteLine("ofw_veh:ApplyPropertiesOnVehicle: invalid vehicle network id");
        return;
      }

      var propertiesBag = JsonConvert.DeserializeObject<VehiclePropertiesBag>(properties);
      var veh = NetToVeh(vehNetId);

      if (propertiesBag == null)
        return;

      Vehicles.SetVehicleProperties(veh, propertiesBag);
    }

    //"ofw_veh:RespawnedCarRestoreProperties"
    [EventHandler("ofw_veh:RespawnedCarRestoreProperties")]
    private async void RespawnedCarRestoreProperties(int vehNetId, string originalPlate, string properties)
    {
      if (originalPlate == null)
        return;

      if (string.IsNullOrEmpty(properties))
      {
        //Debug.WriteLine("ofw_veh:RespawnedCarRestoreProperties: no properties sent");
        return;
      }

      if (vehNetId == 0 || !NetworkDoesEntityExistWithNetworkId(vehNetId))
      {
        //Debug.WriteLine("ofw_veh:RespawnedCarRestoreProperties: invalid vehicle network id");
        return;
      }

      if (Game.Player.Handle != NetworkGetEntityOwner(NetToVeh(vehNetId)))
      {
        //Debug.WriteLine("ofw_veh:RespawnedCarRestoreProperties: Iam not entity owner");
        return;
      }

      var propertiesBag = JsonConvert.DeserializeObject<VehiclePropertiesBag>(properties);
      var veh = NetToVeh(vehNetId);
      originalPlate = originalPlate.Trim().ToLower();

      if (GetVehicleNumberPlateText(veh)?.Trim().ToLower() == originalPlate)
      {
        TriggerServerEvent("ofw_veh:AckPropertiesSynced", originalPlate);
        return; //uz ma nastaveny properties
      }

      if (propertiesBag == null)
        return;

      if (GetEntityCoords(veh, false).DistanceToSquared2D(Game.PlayerPed.Position) > 10000)
        return;

      Vehicles.SetVehicleProperties(veh, propertiesBag);
      TriggerServerEvent("ofw_veh:AckPropertiesSynced", originalPlate);
    }
  }
}
