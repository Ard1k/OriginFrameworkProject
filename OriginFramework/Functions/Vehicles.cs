using CitizenFX.Core;
using CitizenFX.Core.Native;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
  internal class Vehicles
  {
    public static int GetVehicleInFront(int? entity)
    {
      Vector3 entityWorld = GetOffsetFromEntityInWorldCoords(entity ?? PlayerPedId(), 0.0f, 4.0f, 0.0f);
      Vector3 entityCoords = GetEntityCoords(entity ?? PlayerPedId(), false);
      int rayHandle = CastRayPointToPoint(entityCoords.X, entityCoords.Y, entityCoords.Z, entityWorld.X, entityWorld.Y, entityWorld.Z, 2, entity ?? Game.PlayerPed.Handle, 0);
      int entityHitResult = 0;
      bool wasHit = false;
      Vector3 endCoordsResult = new Vector3(), surfaceResult = new Vector3();
      int result = GetRaycastResult(rayHandle, ref wasHit, ref endCoordsResult, ref surfaceResult, ref entityHitResult);
      //DrawLine(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, entityWorld.X, entityWorld.Y, entityWorld.Z, wasHit ? 255 : 0, wasHit ? 0 : 255, 0, 255);
      return entityHitResult;
    }

    public static bool IsPedCloseToTrunk(int veh)
    {
      return IsEntityCloseToTrunk(null, veh);
    }

    public static bool IsEntityCloseToTrunk(int? entity, int veh)
    {
      var coords = GetEntityCoords(veh, false);
      Vector3 min = new Vector3(), max = new Vector3();
      GetModelDimensions((uint)GetEntityModel(veh), ref min, ref max);
      float heading = GetEntityHeading(veh) - 90.0f;

      var trunkPos = coords + new Vector3((float)Math.Cos((Math.PI / 180) * heading), (float)Math.Sin((Math.PI / 180) * heading), 0.0f) * Math.Abs(min.Y);

      if (Vector3.Distance(entity != null ? GetEntityCoords((int)entity, false) : Game.PlayerPed.Position, trunkPos) <= (entity != null ? 4f : 1.2f))
        return true;
      else
        return false;
    }

    public static async Task<bool> IsVehicleWithPlateOutOfGarageSpawned(string plate, dynamic esx)
    {
      bool ret = false;
      bool completed = false;
      Func<bool, bool> CallbackFunction = (data) => { ret = data; completed = true; return true; };
      BaseScript.TriggerServerEvent("ofw_veh:IsVehicleWithPlateOutOfGarage", plate, CallbackFunction);
      while (!completed)
      {
        await Main.Delay(0);
      }

      if (ret)
        return ret;

      var vehs = World.GetAllVehicles();
      bool isAnyFound = false;

      if (vehs != null)
      {
        foreach (var veh in vehs)
        {
          var cplate = GetVehicleNumberPlateText(veh.Handle).Trim();
          if (cplate != null && cplate == plate)
            isAnyFound = true;
        }
      }

      return isAnyFound;
    }

    public static VehiclePropertiesBag GetVehicleProperties(int vehicleId)
    {
      if (!DoesEntityExist(vehicleId))
        return null;

      int colorPrimary = 0, colorSecondary = 0;
      GetVehicleColours(vehicleId, ref colorPrimary, ref colorSecondary);
      int pearlescentColor = 0, wheelColor = 0;
      GetVehicleExtraColours(vehicleId, ref pearlescentColor, ref wheelColor);
      var extras = new Dictionary<string, bool>();
      for (int i = 0; i < 20; i++)
      {
        if (DoesExtraExist(vehicleId, i))
        {
          extras.Add(i.ToString(), IsVehicleExtraTurnedOn(vehicleId, i));
        }
      }
      var neonEnabled = new List<bool>();
      for (int i = 0; i < 4; i++)
      {
        neonEnabled.Add(IsVehicleNeonLightEnabled(vehicleId, i));
      }

      int r = 0, g = 0, b = 0;
      GetVehicleNeonLightsColour(vehicleId, ref r, ref g, ref b);
      var neonColor = new List<int> { r, g, b };

      int r2 = 0, g2 = 0, b2 = 0;
      GetVehicleTyreSmokeColor(vehicleId, ref r2, ref g2, ref b2);
      var tyreSmokeColor = new List<int> { r2, g2, b2 };

      var bag = new VehiclePropertiesBag();

      bag.model = GetEntityModel(vehicleId);
      bag.plate = GetVehicleNumberPlateText(vehicleId).Trim();
      bag.plateIndex = GetVehicleNumberPlateTextIndex(vehicleId);

      bag.bodyHealth = (float)Math.Round(GetVehicleBodyHealth(vehicleId), 1);
      bag.engineHealth = (float)Math.Round(GetVehicleEngineHealth(vehicleId), 1);
      bag.fuelLevel = (float)Math.Round(GetVehicleFuelLevel(vehicleId), 1);
      bag.dirtLevel = (float)Math.Round(GetVehicleDirtLevel(vehicleId), 1);

      bag.color1 = colorPrimary;
      bag.color2 = colorSecondary;
      bag.pearlescentColor = pearlescentColor;
      bag.wheelColor = wheelColor;

      bag.wheels = GetVehicleWheelType(vehicleId);
      bag.customTires = GetVehicleModVariation(vehicleId, 23);
      bag.windowTint = GetVehicleWindowTint(vehicleId);
      bag.xenonColor = GetVehicleXenonLightsColour(vehicleId);
      bag.neonEnabled = neonEnabled;
      bag.neonColor = neonColor;
      bag.tyreSmokeColor = tyreSmokeColor;
      bag.extras = extras;

      for (int i = 0; i < bag.tunings.Length; i++)
      {
        if (VehTuningTypeDefinition.Defined[i].IsDisabled)
          continue;

        if (VehTuningTypeDefinition.Defined[i].IsToggle)
          bag.tunings[i] = IsToggleModOn(vehicleId, i);
        else
          bag.tunings[i] = GetVehicleMod(vehicleId, i);
      }

      return bag;
    }

    public static void SetVehicleProperties(int vehicleId, VehiclePropertiesBag props)
    {
      SetVehicleModKit(vehicleId, 0);

      int colorPrimary = 0, colorSecondary = 0;
      GetVehicleColours(vehicleId, ref colorPrimary, ref colorSecondary);
      int pearlescentColor = 0, wheelColor = 0;
      GetVehicleExtraColours(vehicleId, ref pearlescentColor, ref wheelColor);

      if (props.plate != null) SetVehicleNumberPlateText(vehicleId, props.plate);
      if (props.plateIndex != null) SetVehicleNumberPlateTextIndex(vehicleId, props.plateIndex.Value);
      if (props.bodyHealth != null) SetVehicleBodyHealth(vehicleId, props.bodyHealth.Value);
      if (props.engineHealth != null) SetVehicleEngineHealth(vehicleId, props.engineHealth.Value);
      if (props.fuelLevel != null) SetVehicleFuelLevel(vehicleId, props.fuelLevel.Value);
      if (props.dirtLevel != null) SetVehicleDirtLevel(vehicleId, props.dirtLevel.Value);
      if (props.color1 != null) SetVehicleColours(vehicleId, props.color1.Value, colorSecondary);
      if (props.color2 != null) SetVehicleColours(vehicleId, props.color1 ?? colorPrimary, props.color2.Value);
      if (props.pearlescentColor != null) SetVehicleExtraColours(vehicleId, props.pearlescentColor.Value, wheelColor);
      if (props.wheelColor != null) SetVehicleExtraColours(vehicleId, props.pearlescentColor ?? pearlescentColor, props.wheelColor.Value);
      if (props.wheels != null) SetVehicleWheelType(vehicleId, props.wheels.Value);
      if (props.windowTint != null) SetVehicleWindowTint(vehicleId, props.windowTint.Value);

      if (props.neonEnabled != null)
      {
        SetVehicleNeonLightEnabled(vehicleId, 0, props.neonEnabled[0]);
        SetVehicleNeonLightEnabled(vehicleId, 1, props.neonEnabled[1]);
        SetVehicleNeonLightEnabled(vehicleId, 2, props.neonEnabled[2]);
        SetVehicleNeonLightEnabled(vehicleId, 3, props.neonEnabled[3]);
      }

      if (props.extras != null)
      {
        for (int i = 0; i < 20; i++)
        {
          if (props.extras.ContainsKey(i.ToString()))
          {
            SetVehicleExtra(vehicleId, i, !props.extras[i.ToString()]);
          }
        }
      }

      if (props.neonColor != null) SetVehicleNeonLightsColour(vehicleId, props.neonColor[0], props.neonColor[1], props.neonColor[2]);
      if (props.xenonColor != null) SetVehicleXenonLightsColour(vehicleId, props.xenonColor.Value);

      for (int i = 0; i < props.tunings.Length; i++)
      {
        if (props.tunings[i] == null || VehTuningTypeDefinition.Defined[i].IsDisabled)
          continue;


        if (VehTuningTypeDefinition.Defined[i].IsToggle)
          ToggleVehicleMod(vehicleId, i, Convert.ToBoolean(props.tunings[i]));
        else
        {
          SetVehicleMod(vehicleId, i, Convert.ToInt32(props.tunings[i]), props.customTires ?? false); //Convert protoze je to pole objektu a pri deserializaci to muze skoncit jako byte treba...
        }
      }

      if (props.tyreSmokeColor != null) SetVehicleTyreSmokeColor(vehicleId, props.tyreSmokeColor[0], props.tyreSmokeColor[1], props.tyreSmokeColor[2]);
    }

    public static bool IsPlayerDrivingForklift()
    {
      return Game.PlayerPed.CurrentVehicle != null && Game.PlayerPed.CurrentVehicle.Model.Hash == 1491375716 && GetPedInVehicleSeat(Game.PlayerPed.CurrentVehicle.Handle, -1) == Game.PlayerPed.Handle;
    }
  }
}
