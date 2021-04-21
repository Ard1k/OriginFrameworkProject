using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
  public class TestJob : BaseScript
  {
    private Control actionKey = Control.Sprint;
    private Control debugKey = Control.SniperZoomIn;
    private Control debugKey2 = Control.VehicleSelectPrevWeapon;

    public TestJob()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        Delay(0);

      if (!SettingsManager.Settings.TestJobActive)
        return;

      Tick += OnTick;
      Tick += OnEachFrame;
    }

    protected static int currentLocalVehicle = -1;
    protected static int currentNetVehicle = -1;
    protected static int closestVehicle = -1;

    private async Task OnEachFrame()
    {
      Game.DisableControlThisFrame(0, debugKey);
      Game.DisableControlThisFrame(0, debugKey2);

      if (IsDisabledControlJustPressed(0, (int)debugKey) || IsDisabledControlJustPressed(0, (int) debugKey2))
      {
        Debug.WriteLine("localVeh: " + currentLocalVehicle);
        Debug.WriteLine("netVeh: " + currentNetVehicle);
        Debug.WriteLine("closestVehicle: " + closestVehicle);
      }

      
    }


    private async Task OnTick()
		{
      if (IsControlJustPressed(0, (int)actionKey))
      {
        if (IsPedInAnyVehicle(Game.PlayerPed.Handle, true))
        {
          currentLocalVehicle = GetVehiclePedIsIn(Game.PlayerPed.Handle, false);

          //VehToNet(currentLocalVehicle);

          currentNetVehicle = NetworkGetNetworkIdFromEntity(currentLocalVehicle);
        }

        var p = Game.PlayerPed.Position;
        var vehs = World.GetAllVehicles();
        float closest = 999999999999f;
        bool isAnyFound = false;

        if (vehs != null)
        {
          foreach (var veh in vehs)
          {
            var dist = Vector3.Distance(p, veh.Position);
            //Debug.WriteLine($"veh: {veh.Handle} distance: {dist}");
            if (dist < 50 && dist < closest)
            {
              closest = dist;
              closestVehicle = veh.Handle;
              isAnyFound = true;
            }
          }
        }

        if (!isAnyFound)
          closestVehicle = -1;

        if (closestVehicle > 0)
        {
          Subtitle.Custom("ClosestVeh: " + closestVehicle + "Distance: " + closest);
        }

      }

      if (closestVehicle > 0)
      {
        var veh = (Vehicle)Vehicle.FromHandle(closestVehicle);
        DrawMarker(0, veh.Position.X, veh.Position.Y, veh.Position.Z + 2f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0f, 2f, 2f, 2f, 255, 0, 255, 255, true, false, 2, false, null, null, false);

        //doorIndex: 0 = Front Left 1 = Front Right 2 = Back Left 3 = Back Right 4 = Hood 5 = Trunk 6 = Trunk2

        if (veh.Doors.HasDoor(VehicleDoorIndex.Trunk))
        {
          var door = veh.Doors[VehicleDoorIndex.Trunk];
          int color_red = 0;
          int color_green = 0;
          if (door.IsOpen || door.IsBroken)
          {
            color_red = 0;
            color_green = 255;
          }
          else
          {
            color_red = 255;
            color_green = 0;
          }

          var bootBone = veh.Bones["boot"];
          if (bootBone != null)
          {
            DrawMarker(0, bootBone.Position.X, bootBone.Position.Y, bootBone.Position.Z, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0f, 0.2f, 0.2f, 0.2f, color_red, color_green, 0, 255, true, false, 2, false, null, null, false);
          }
          var tailLBone = veh.Bones["taillight_l"];
          if (tailLBone != null)
          {
            DrawMarker(0, tailLBone.Position.X, tailLBone.Position.Y, tailLBone.Position.Z, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0f, 0.2f, 0.2f, 0.2f, 255, 0, 0, 255, true, false, 2, false, null, null, false);
          }
          var tailRBone = veh.Bones["taillight_r"];
          if (tailRBone != null)
          {
            DrawMarker(0, tailRBone.Position.X, tailRBone.Position.Y, tailRBone.Position.Z, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0f, 0.2f, 0.2f, 0.2f, 255, 0, 0, 255, true, false, 2, false, null, null, false);
          }
        }
        if (veh.Doors.HasDoor(VehicleDoorIndex.Hood))
        {
          var dPos = GetEntryPositionOfDoor(veh.Handle, 4);
          DrawMarker(0, dPos.X, dPos.Y, dPos.Z + 0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0f, 0.5f, 0.5f, 0.5f, 0, 0, 255, 255, true, false, 2, false, null, null, false);
        }
        if (veh.Doors.HasDoor(VehicleDoorIndex.FrontLeftDoor))
        {
          var dPos = GetEntryPositionOfDoor(veh.Handle, 0);
          DrawMarker(0, dPos.X, dPos.Y, dPos.Z + 0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0f, 0.5f, 0.5f, 0.5f, 0, 0, 255, 255, true, false, 2, false, null, null, false);
        }
        if (veh.Doors.HasDoor(VehicleDoorIndex.FrontRightDoor))
        {
          var dPos = GetEntryPositionOfDoor(veh.Handle, 1);
          DrawMarker(0, dPos.X, dPos.Y, dPos.Z + 0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0f, 0.5f, 0.5f, 0.5f, 0, 0, 255, 255, true, false, 2, false, null, null, false);
        }
        if (veh.Doors.HasDoor(VehicleDoorIndex.BackLeftDoor))
        {
          var dPos = GetEntryPositionOfDoor(veh.Handle, 2);
          DrawMarker(0, dPos.X, dPos.Y, dPos.Z + 0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0f, 0.5f, 0.5f, 0.5f, 0, 0, 255, 255, true, false, 2, false, null, null, false);
        }
        if (veh.Doors.HasDoor(VehicleDoorIndex.BackRightDoor))
        {
          var dPos = GetEntryPositionOfDoor(veh.Handle, 3);
          DrawMarker(0, dPos.X, dPos.Y, dPos.Z + 0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0f, 0.5f, 0.5f, 0.5f, 0, 0, 255, 255, true, false, 2, false, null, null, false);
        }
      }

      //await (Delay(0));
      //if (isDrifterEnabled || isBoosterEnabled)
      //{
      //  if (IsPedInAnyVehicle(Game.PlayerPed.Handle, false))
      //  {
      //    var vehicle = GetVehiclePedIsIn(Game.PlayerPed.Handle, false);
      //    if (GetPedInVehicleSeat(vehicle, -1) == Game.PlayerPed.Handle)
      //    {
      //      var vehicleSpeed = GetEntitySpeed(vehicle);
      //      if ((IsControlPressed(0, (int)actionKey) || IsDisabledControlPressed(0, (int)actionKey)) && (vehicleSpeed * kmh_multiplier) <= speed_limit)
      //      {
      //        if (isDrifterEnabled)
      //          SetVehicleReduceGrip(vehicle, true);
      //        else if (isBoosterEnabled)
      //          SetVehicleForwardSpeed(vehicle, vehicleSpeed * 1.2f);
      //      }
      //      else
      //      {
      //        if (isDrifterEnabled)
      //          SetVehicleReduceGrip(vehicle, false);
      //      }
      //    }
      //  }
      //}
    }

  }
}
