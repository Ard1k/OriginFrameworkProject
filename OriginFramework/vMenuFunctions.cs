using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
  public static class vMenuFunctions
  {
    #region Variables
    private static string _currentScenario = "";
    private static Vehicle _previousVehicle;

    internal static bool DriveToWpTaskActive = false;
    internal static bool DriveWanderTaskActive = false;
    #endregion

    #region some misc functions copied from base script
    /// <summary>
    /// Copy of <see cref="BaseScript.TriggerServerEvent(string, object[])"/>
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="args"></param>
    public static void TriggerServerEvent(string eventName, params object[] args)
    {
      BaseScript.TriggerServerEvent(eventName, args);
    }

    /// <summary>
    /// Copy of <see cref="BaseScript.TriggerEvent(string, object[])"/>
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="args"></param>
    public static void TriggerEvent(string eventName, params object[] args)
    {
      BaseScript.TriggerEvent(eventName, args);
    }

    /// <summary>
    /// Copy of <see cref="BaseScript.Delay(int)"/>
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static async Task Delay(int time)
    {
      await BaseScript.Delay(time);
    }
    #endregion

    #region Toggle vehicle alarm
    public static void ToggleVehicleAlarm(Vehicle vehicle)
    {
      if (vehicle != null && vehicle.Exists())
      {
        if (vehicle.IsAlarmSounding)
        {
          // Set the duration to 0;
          vehicle.AlarmTimeLeft = 0;
          vehicle.IsAlarmSet = false;
        }
        else
        {
          // Randomize duration of the alarm and start the alarm.
          vehicle.IsAlarmSet = true;
          vehicle.AlarmTimeLeft = new Random().Next(8000, 45000);
          vehicle.StartAlarm();
        }
      }
    }
    #endregion

    #region lock or unlock vehicle doors
    public static async void LockOrUnlockDoors(Vehicle veh, bool lockDoors)
    {
      if (veh != null && veh.Exists())
      {
        for (int i = 0; i < 2; i++)
        {
          int timer = GetGameTimer();
          while (GetGameTimer() - timer < 50)
          {
            SoundVehicleHornThisFrame(veh.Handle);
            await Delay(0);
          }
          await Delay(50);
        }
        if (lockDoors)
        {
          Subtitle.Custom("Vehicle doors are now locked.");
          SetVehicleDoorsLockedForAllPlayers(veh.Handle, true);
        }
        else
        {
          Subtitle.Custom("Vehicle doors are now unlocked.");
          SetVehicleDoorsLockedForAllPlayers(veh.Handle, false);
        }
      }
    }
    #endregion

    #region Get Localized Vehicle Display Name
    /// <summary>
    /// Get the localized model name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetVehDisplayNameFromModel(string name) => GetLabelText(GetDisplayNameFromVehicleModel((uint)GetHashKey(name)));
    #endregion

    #region DoesModelExist
    /// <summary>
    /// Does this model exist?
    /// </summary>
    /// <param name="modelName">The model name</param>
    /// <returns></returns>
    public static bool DoesModelExist(string modelName) => DoesModelExist((uint)GetHashKey(modelName));

    /// <summary>
    /// Does this model exist?
    /// </summary>
    /// <param name="modelHash">The model hash</param>
    /// <returns></returns>
    public static bool DoesModelExist(uint modelHash) => IsModelInCdimage(modelHash);
    #endregion

    #region GetVehicle from specified player id (if not specified, return the vehicle of the current player)
    /// <summary>
    /// Returns the current or last vehicle of the current player.
    /// </summary>
    /// <param name="lastVehicle"></param>
    /// <returns></returns>
    public static Vehicle GetVehicle(bool lastVehicle = false)
    {
      if (lastVehicle)
      {
        return Game.PlayerPed.LastVehicle;
      }
      else
      {
        if (Game.PlayerPed.IsInVehicle())
        {
          return Game.PlayerPed.CurrentVehicle;
        }
      }
      return null;
    }

    /// <summary>
    /// Returns the current or last vehicle of the selected ped.
    /// </summary>
    /// <param name="ped"></param>
    /// <param name="lastVehicle"></param>
    /// <returns></returns>
    public static Vehicle GetVehicle(Ped ped, bool lastVehicle = false)
    {
      if (lastVehicle)
      {
        return ped.LastVehicle;
      }
      else
      {
        if (ped.IsInVehicle())
        {
          return ped.CurrentVehicle;
        }
      }
      return null;
    }

    /// <summary>
    /// Returns the current or last vehicle of the selected player.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="lastVehicle"></param>
    /// <returns></returns>
    public static Vehicle GetVehicle(Player player, bool lastVehicle = false)
    {
      if (lastVehicle)
      {
        return player.Character.LastVehicle;
      }
      else
      {
        if (player.Character.IsInVehicle())
        {
          return player.Character.CurrentVehicle;
        }
      }
      return null;
    }
    #endregion

    #region GetVehicleModel (uint)(hash) from Entity/Vehicle (int)
    /// <summary>
    /// Get the vehicle model hash (as uint) from the specified (int) entity/vehicle.
    /// </summary>
    /// <param name="vehicle">Entity/vehicle.</param>
    /// <returns>Returns the (uint) model hash from a (vehicle) entity.</returns>
    public static uint GetVehicleModel(int vehicle) => (uint)GetHashKey(GetEntityModel(vehicle).ToString());
    #endregion

    #region Is ped pointing
    /// <summary>
    /// Is ped pointing function returns true if the ped is currently pointing their finger.
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    public static bool IsPedPointing(int handle)
    {
      return N_0x921ce12c489c4c41(handle);
    }

    /// <summary>
    /// Gets the finger pointing camera pitch.
    /// </summary>
    /// <returns></returns>
    public static float GetPointingPitch()
    {
      float pitch = GetGameplayCamRelativePitch();
      if (pitch < -70f)
      {
        pitch = -70f;
      }
      if (pitch > 42f)
      {
        pitch = 42f;
      }
      pitch += 70f;
      pitch /= 112f;

      return pitch;
    }
    /// <summary>
    /// Gets the finger pointing camera heading.
    /// </summary>
    /// <returns></returns>
    public static float GetPointingHeading()
    {
      float heading = GetGameplayCamRelativeHeading();
      if (heading < -180f)
      {
        heading = -180f;
      }
      if (heading > 180f)
      {
        heading = 180f;
      }
      heading += 180f;
      heading /= 360f;
      heading *= -1f;
      heading += 1f;

      return heading;
    }
    #endregion
    #region Drive Tasks (WIP)
    /// <summary>
    /// Drives to waypoint
    /// </summary>
    public static void DriveToWp(int style = 0)
    {
      ClearPedTasks(Game.PlayerPed.Handle);
      DriveWanderTaskActive = false;
      DriveToWpTaskActive = true;

      Vector3 waypoint = World.WaypointPosition;

      Vehicle veh = GetVehicle();
      uint model = (uint)veh.Model.Hash;

      SetDriverAbility(Game.PlayerPed.Handle, 1f);
      SetDriverAggressiveness(Game.PlayerPed.Handle, 0f);

      TaskVehicleDriveToCoordLongrange(Game.PlayerPed.Handle, veh.Handle, waypoint.X, waypoint.Y, waypoint.Z, GetVehicleModelMaxSpeed(model), style, 10f);
    }

    /// <summary>
    /// Drives around the area.
    /// </summary>
    public static void DriveWander(int style = 0)
    {
      ClearPedTasks(Game.PlayerPed.Handle);
      DriveWanderTaskActive = true;
      DriveToWpTaskActive = false;

      Vehicle veh = GetVehicle();
      uint model = (uint)veh.Model.Hash;

      SetDriverAbility(Game.PlayerPed.Handle, 1f);
      SetDriverAggressiveness(Game.PlayerPed.Handle, 0f);

      TaskVehicleDriveWander(Game.PlayerPed.Handle, veh.Handle, GetVehicleModelMaxSpeed(model), style);
    }
    #endregion

    #region Quit session & Quit game
    /// <summary>
    /// Quit the current network session, but leaves you connected to the server so addons/resources are still streamed.
    /// </summary>
    public static void QuitSession() => NetworkSessionEnd(true, true);

    /// <summary>
    /// Quit the game after 5 seconds.
    /// </summary>
    public static async void QuitGame()
    {
      Notify.Info("The game will exit in 5 seconds.");
      Debug.WriteLine("Game will be terminated in 5 seconds, because the player used the Quit Game option in vMenu.");
      await BaseScript.Delay(5000);
      ForceSocialClubUpdate(); // bye bye
    }
    #endregion

    #region Teleport To Coords
    /// <summary>
    /// Teleport to the specified <see cref="pos"/>.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="safeModeDisabled"></param>
    /// <returns></returns>
    public static async Task TeleportToCoords(Vector3 pos, bool safeModeDisabled = false)
    {
      if (!safeModeDisabled)
      {
        // Is player in a vehicle and the driver? Then we'll use that to teleport.
        var veh = GetVehicle();
        bool inVehicle() => veh != null && veh.Exists() && Game.PlayerPed == veh.Driver;

        bool vehicleRestoreVisibility = inVehicle() && veh.IsVisible;
        bool pedRestoreVisibility = Game.PlayerPed.IsVisible;

        // Freeze vehicle or player location and fade out the entity to the network.
        if (inVehicle())
        {
          veh.IsPositionFrozen = true;
          if (veh.IsVisible)
          {
            NetworkFadeOutEntity(veh.Handle, true, false);
          }
        }
        else
        {
          ClearPedTasksImmediately(Game.PlayerPed.Handle);
          Game.PlayerPed.IsPositionFrozen = true;
          if (Game.PlayerPed.IsVisible)
          {
            NetworkFadeOutEntity(Game.PlayerPed.Handle, true, false);
          }
        }

        // Fade out the screen and wait for it to be faded out completely.
        DoScreenFadeOut(500);
        while (!IsScreenFadedOut())
        {
          await Delay(0);
        }

        // This will be used to get the return value from the groundz native.
        float groundZ = 850.0f;

        // Bool used to determine if the groundz coord could be found.
        bool found = false;

        // Loop from 950 to 0 for the ground z coord, and take away 25 each time.
        for (float zz = 950.0f; zz >= 0f; zz -= 25f)
        {
          float z = zz;
          // The z coord is alternating between a very high number, and a very low one.
          // This way no matter the location, the actual ground z coord will always be found the fastest.
          // If going from top > bottom then it could take a long time to reach the bottom. And vice versa.
          // By alternating top/bottom each iteration, we minimize the time on average for ANY location on the map.
          if (zz % 2 != 0)
          {
            z = 950f - zz;
          }

          // Request collision at the coord. I've never actually seen this do anything useful, but everyone keeps telling me this is needed.
          // It doesn't matter to get the ground z coord, and neither does it actually prevent entities from falling through the map, nor does
          // it seem to load the world ANY faster than without, but whatever.
          RequestCollisionAtCoord(pos.X, pos.Y, z);

          // Request a new scene. This will trigger the world to be loaded around that area.
          NewLoadSceneStart(pos.X, pos.Y, z, pos.X, pos.Y, z, 50f, 0);

          // Timer to make sure things don't get out of hand (player having to wait forever to get teleported if something fails).
          int tempTimer = GetGameTimer();

          // Wait for the new scene to be loaded.
          while (IsNetworkLoadingScene())
          {
            // If this takes longer than 1 second, just abort. It's not worth waiting that long.
            if (GetGameTimer() - tempTimer > 1000)
            {
              Log("Waiting for the scene to load is taking too long (more than 1s). Breaking from wait loop.");
              break;
            }

            await Delay(0);
          }

          // If the player is in a vehicle, teleport the vehicle to this new position.
          if (inVehicle())
          {
            SetEntityCoords(veh.Handle, pos.X, pos.Y, z, false, false, false, true);
          }
          // otherwise, teleport the player to this new position.
          else
          {
            SetEntityCoords(Game.PlayerPed.Handle, pos.X, pos.Y, z, false, false, false, true);
          }

          // Reset the timer.
          tempTimer = GetGameTimer();

          // Wait for the collision to be loaded around the entity in this new location.
          while (!HasCollisionLoadedAroundEntity(Game.PlayerPed.Handle))
          {
            // If this takes too long, then just abort, it's not worth waiting that long since we haven't found the real ground coord yet anyway.
            if (GetGameTimer() - tempTimer > 1000)
            {
              Log("Waiting for the collision is taking too long (more than 1s). Breaking from wait loop.");
              break;
            }
            await Delay(0);
          }

          // Check for a ground z coord.
          found = GetGroundZFor_3dCoord(pos.X, pos.Y, z, ref groundZ, false);

          // If we found a ground z coord, then teleport the player (or their vehicle) to that new location and break from the loop.
          if (found)
          {
            Log($"Ground coordinate found: {groundZ}");
            if (inVehicle())
            {
              SetEntityCoords(veh.Handle, pos.X, pos.Y, groundZ, false, false, false, true);

              // We need to unfreeze the vehicle because sometimes having it frozen doesn't place the vehicle on the ground properly.
              veh.IsPositionFrozen = false;
              veh.PlaceOnGround();
              // Re-freeze until screen is faded in again.
              veh.IsPositionFrozen = true;
            }
            else
            {
              SetEntityCoords(Game.PlayerPed.Handle, pos.X, pos.Y, groundZ, false, false, false, true);
            }
            break;
          }

          // Wait 10ms before trying the next location.
          await Delay(10);
        }

        // If the loop ends but the ground z coord has not been found yet, then get the nearest vehicle node as a fail-safe coord.
        if (!found)
        {
          var safePos = pos;
          GetNthClosestVehicleNode(pos.X, pos.Y, pos.Z, 0, ref safePos, 0, 0, 0);

          // Notify the user that the ground z coord couldn't be found, so we will place them on a nearby road instead.
          Notify.Alert("Could not find a safe ground coord. Placing you on the nearest road instead.");
          Log("Could not find a safe ground coord. Placing you on the nearest road instead.");

          // Teleport vehicle, or player.
          if (inVehicle())
          {
            SetEntityCoords(veh.Handle, safePos.X, safePos.Y, safePos.Z, false, false, false, true);
            veh.IsPositionFrozen = false;
            veh.PlaceOnGround();
            veh.IsPositionFrozen = true;
          }
          else
          {
            SetEntityCoords(Game.PlayerPed.Handle, safePos.X, safePos.Y, safePos.Z, false, false, false, true);
          }
        }

        // Once the teleporting is done, unfreeze vehicle or player and fade them back in.
        if (inVehicle())
        {
          if (vehicleRestoreVisibility)
          {
            NetworkFadeInEntity(veh.Handle, true);
            if (!pedRestoreVisibility)
            {
              Game.PlayerPed.IsVisible = false;
            }
          }
          veh.IsPositionFrozen = false;
        }
        else
        {
          if (pedRestoreVisibility)
          {
            NetworkFadeInEntity(Game.PlayerPed.Handle, true);
          }
          Game.PlayerPed.IsPositionFrozen = false;
        }

        // Fade screen in and reset the camera angle.
        DoScreenFadeIn(500);
        SetGameplayCamRelativePitch(0.0f, 1.0f);
      }

      // Disable safe teleporting and go straight to the specified coords.
      else
      {
        RequestCollisionAtCoord(pos.X, pos.Y, pos.Z);

        // Teleport directly to the coords without trying to get a safe z pos.
        if (Game.PlayerPed.IsInVehicle() && GetVehicle().Driver == Game.PlayerPed)
        {
          SetEntityCoords(GetVehicle().Handle, pos.X, pos.Y, pos.Z, false, false, false, true);
        }
        else
        {
          SetEntityCoords(Game.PlayerPed.Handle, pos.X, pos.Y, pos.Z, false, false, false, true);
        }
      }
    }

    /// <summary>
    /// Teleports to the player's waypoint. If no waypoint is set, notify the user.
    /// </summary>
    public static async void TeleportToWp()
    {
      if (Game.IsWaypointActive)
      {
        var pos = World.WaypointPosition;
        await TeleportToCoords(pos);
      }
      else
      {
        Notify.Error("You need to set a waypoint first!");
      }
    }
    #endregion

    #region Cycle Through Vehicle Seats
    /// <summary>
    /// Cycle to the next available seat.
    /// </summary>
    public static void CycleThroughSeats()
    {

      // Create a new vehicle.
      Vehicle vehicle = GetVehicle();

      // If there are enough empty seats, continue.
      if (AreAnyVehicleSeatsFree(vehicle.Handle))
      {
        // Get the total seats for this vehicle.
        var maxSeats = GetVehicleModelNumberOfSeats((uint)GetEntityModel(vehicle.Handle));

        // If the player is currently in the "last" seat, start from the driver's position and loop through the seats.
        if (GetPedInVehicleSeat(vehicle.Handle, maxSeats - 2) == Game.PlayerPed.Handle)
        {
          // Loop through all seats.
          for (var seat = -1; seat < maxSeats - 2; seat++)
          {
            // If the seat is free, get in it and stop the loop.
            if (vehicle.IsSeatFree((VehicleSeat)seat))
            {
              TaskWarpPedIntoVehicle(Game.PlayerPed.Handle, vehicle.Handle, seat);
              break;
            }
          }
        }
        // If the player is not in the "last" seat, loop through all the seats starrting from the driver's position.
        else
        {
          var switchedPlace = false;
          var passedCurrentSeat = false;
          // Loop through all the seats.
          for (var seat = -1; seat < maxSeats - 1; seat++)
          {
            // If this seat is the one the player is sitting on, set passedCurrentSeat to true.
            // This way we won't just keep placing the ped in the 1st available seat, but actually the first "next" available seat.
            if (!passedCurrentSeat && GetPedInVehicleSeat(vehicle.Handle, seat) == Game.PlayerPed.Handle)
            {
              passedCurrentSeat = true;
            }

            // Only if the current seat has been passed, check if the seat is empty and if so teleport into it and stop the loop.
            if (passedCurrentSeat && IsVehicleSeatFree(vehicle.Handle, seat))
            {
              switchedPlace = true;
              TaskWarpPedIntoVehicle(Game.PlayerPed.Handle, vehicle.Handle, seat);
              break;
            }
          }
          // If the player was not switched, then that means there are not enough empty vehicle seats "after" the player, and the player was not sitting in the "last" seat.
          // To fix this, loop through the entire vehicle again and place them in the first available seat.
          if (!switchedPlace)
          {
            // Loop through all seats, starting at the drivers seat (-1), then moving up.
            for (var seat = -1; seat < maxSeats - 1; seat++)
            {
              // If the seat is free, take it and break the loop.
              if (IsVehicleSeatFree(vehicle.Handle, seat))
              {
                TaskWarpPedIntoVehicle(Game.PlayerPed.Handle, vehicle.Handle, seat);
                break;
              }
            }
          }
        }
      }
      else
      {
        Notify.Alert("There are no more available seats to cycle through.");
      }
    }
    #endregion

    #region Spawn Vehicle
    #region Overload Spawn Vehicle Function
    /// <summary>
    /// Simple custom vehicle spawn function.
    /// </summary>
    /// <param name="vehicleName">Vehicle model name. If "custom" the user will be asked to enter a model name.</param>
    /// <param name="spawnInside">Warp the player inside the vehicle after spawning.</param>
    /// <param name="replacePrevious">Replace the previous vehicle of the player.</param>
    //public static async void SpawnVehicle(string vehicleName = "custom", bool spawnInside = false, bool replacePrevious = false)
    //{
    //  if (vehicleName == "custom")
    //  {
    //    // Get the result.
    //    string result = await GetUserInput(windowTitle: "Enter Vehicle Name");
    //    // If the result was not invalid.
    //    if (!string.IsNullOrEmpty(result))
    //    {
    //      // Convert it into a model hash.
    //      uint model = (uint)GetHashKey(result);
    //      SpawnVehicle(vehicleHash: model, spawnInside: spawnInside, replacePrevious: replacePrevious, skipLoad: false, vehicleInfo: new VehicleInfo(),
    //          saveName: null);
    //    }
    //    // Result was invalid.
    //    else
    //    {
    //      Notify.Error(CommonErrors.InvalidInput);
    //    }
    //  }
    //  // Spawn the specified vehicle.
    //  else
    //  {
    //    SpawnVehicle(vehicleHash: (uint)GetHashKey(vehicleName), spawnInside: spawnInside, replacePrevious: replacePrevious, skipLoad: false,
    //        vehicleInfo: new VehicleInfo(), saveName: null);
    //  }
    //}
    #endregion

    #region Main Spawn Vehicle Function
    /// <summary>
    /// Spawns a vehicle.
    /// </summary>
    /// <param name="vehicleHash">Model hash of the vehicle to spawn.</param>
    /// <param name="spawnInside">Teleports the player into the vehicle after spawning.</param>
    /// <param name="replacePrevious">Replaces the previous vehicle of the player with the new one.</param>
    /// <param name="skipLoad">Does not attempt to load the vehicle, but will spawn it right a way.</param>
    /// <param name="vehicleInfo">All information needed for a saved vehicle to re-apply all mods.</param>
    /// <param name="saveName">Used to get/set info about the saved vehicle data.</param>
    //public static async void SpawnVehicle(uint vehicleHash, bool spawnInside, bool replacePrevious, bool skipLoad, VehicleInfo vehicleInfo, string saveName = null)
    //{
    //  float speed = 0f;
    //  float rpm = 0f;
    //  if (Game.PlayerPed.IsInVehicle())
    //  {
    //    Vehicle tmpOldVehicle = GetVehicle();
    //    speed = GetEntitySpeedVector(tmpOldVehicle.Handle, true).Y; // get forward/backward speed only
    //    rpm = tmpOldVehicle.CurrentRPM;
    //  }


    //  //var vehClass = GetVehicleClassFromName(vehicleHash);
    //  int modelClass = GetVehicleClassFromName(vehicleHash);
    //  if (!VehicleSpawner.allowedCategories[modelClass])
    //  {
    //    Notify.Alert("You are not allowed to spawn this vehicle, because it belongs to a category which is restricted by the server owner.");
    //    return;
    //  }

    //  if (!skipLoad)
    //  {
    //    bool successFull = await LoadModel(vehicleHash);
    //    if (!successFull || !IsModelAVehicle(vehicleHash))
    //    {
    //      // Vehicle model is invalid.
    //      Notify.Error(CommonErrors.InvalidModel);
    //      return;
    //    }
    //  }

    //  Log("Spawning of vehicle is NOT cancelled, if this model is invalid then there's something wrong.");

    //  // Get the heading & position for where the vehicle should be spawned.
    //  Vector3 pos = (spawnInside) ? GetEntityCoords(Game.PlayerPed.Handle, true) : GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 0f, 8f, 0f);
    //  float heading = GetEntityHeading(Game.PlayerPed.Handle) + (spawnInside ? 0f : 90f);

    //  // If the previous vehicle exists...
    //  if (_previousVehicle != null)
    //  {
    //    // And it's actually a vehicle (rather than another random entity type)
    //    if (_previousVehicle.Exists() && _previousVehicle.PreviouslyOwnedByPlayer &&
    //        (_previousVehicle.Occupants.Count() == 0 || _previousVehicle.Driver.Handle == Game.PlayerPed.Handle))
    //    {
    //      // If the previous vehicle should be deleted:
    //      if (replacePrevious || !IsAllowed(Permission.VSDisableReplacePrevious))
    //      {
    //        // Delete it.
    //        _previousVehicle.PreviouslyOwnedByPlayer = false;
    //        SetEntityAsMissionEntity(_previousVehicle.Handle, true, true);
    //        _previousVehicle.Delete();
    //      }
    //      // Otherwise
    //      else
    //      {
    //        if (!vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.Setting.vmenu_keep_spawned_vehicles_persistent))
    //        {
    //          // Set the vehicle to be no longer needed. This will make the game engine decide when it should be removed (when all players get too far away).
    //          SetEntityAsMissionEntity(_previousVehicle.Handle, false, false);
    //          //_previousVehicle.IsPersistent = false;
    //          //_previousVehicle.PreviouslyOwnedByPlayer = false;
    //          //_previousVehicle.MarkAsNoLongerNeeded();
    //        }
    //      }
    //      _previousVehicle = null;
    //    }
    //  }

    //  if (Game.PlayerPed.IsInVehicle() && (replacePrevious || !IsAllowed(Permission.VSDisableReplacePrevious)))
    //  {
    //    if (GetVehicle().Driver == Game.PlayerPed)// && IsVehiclePreviouslyOwnedByPlayer(GetVehicle()))
    //    {
    //      var tmpveh = GetVehicle();
    //      SetVehicleHasBeenOwnedByPlayer(tmpveh.Handle, false);
    //      SetEntityAsMissionEntity(tmpveh.Handle, true, true);

    //      if (_previousVehicle != null)
    //      {
    //        if (_previousVehicle.Handle == tmpveh.Handle)
    //        {
    //          _previousVehicle = null;
    //        }
    //      }
    //      tmpveh.Delete();
    //      Notify.Info("Your old car was removed to prevent your new car from glitching inside it. Next time, get out of your vehicle before spawning a new one if you want to keep your old one.");
    //    }
    //  }

    //  if (_previousVehicle != null)
    //    _previousVehicle.PreviouslyOwnedByPlayer = false;

    //  if (Game.PlayerPed.IsInVehicle())
    //    pos = GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 0, 8f, 0.1f);

    //  // Create the new vehicle and remove the need to hotwire the car.
    //  Vehicle vehicle = new Vehicle(CreateVehicle(vehicleHash, pos.X, pos.Y, pos.Z + 1f, heading, true, false))
    //  {
    //    NeedsToBeHotwired = false,
    //    PreviouslyOwnedByPlayer = true,
    //    IsPersistent = true,
    //    IsStolen = false,
    //    IsWanted = false
    //  };

    //  Log($"New vehicle, hash:{vehicleHash}, handle:{vehicle.Handle}, force-re-save-name:{(saveName ?? "NONE")}, created at x:{pos.X} y:{pos.Y} z:{(pos.Z + 1f)} " +
    //      $"heading:{heading}");

    //  // If spawnInside is true
    //  if (spawnInside)
    //  {
    //    // Set the vehicle's engine to be running.
    //    vehicle.IsEngineRunning = true;

    //    // Set the ped into the vehicle.
    //    new Ped(Game.PlayerPed.Handle).SetIntoVehicle(vehicle, VehicleSeat.Driver);

    //    // If the vehicle is a helicopter and the player is in the air, set the blades to be full speed.
    //    if (vehicle.ClassType == VehicleClass.Helicopters && GetEntityHeightAboveGround(Game.PlayerPed.Handle) > 10.0f)
    //    {
    //      SetHeliBladesFullSpeed(vehicle.Handle);
    //    }
    //    // If it's not a helicopter or the player is not in the air, set the vehicle on the ground properly.
    //    else
    //    {
    //      vehicle.PlaceOnGround();
    //    }
    //  }

    //  // If mod info about the vehicle was specified, check if it's not null.
    //  if (saveName != null)
    //  {
    //    ApplyVehicleModsDelayed(vehicle, vehicleInfo, 500);
    //  }

    //  // Set the previous vehicle to the new vehicle.
    //  _previousVehicle = vehicle;
    //  //vehicle.Speed = speed; // retarded feature that randomly breaks for no fucking reason
    //  if (!vehicle.Model.IsTrain) // to be extra fucking safe
    //  {
    //    // workaround of retarded feature above:
    //    SetVehicleForwardSpeed(vehicle.Handle, speed);
    //  }
    //  vehicle.CurrentRPM = rpm;

    //  await Delay(1); // Mandatory delay - without it radio station will not set properly

    //  // Set the radio station to default set by player in Vehicle Menu
    //  vehicle.RadioStation = (RadioStation)UserDefaults.VehicleDefaultRadio;

    //  // Discard the model.
    //  SetModelAsNoLongerNeeded(vehicleHash);
    //}

    ///// <summary>
    ///// Waits for the given delay before applying the vehicle mods
    ///// </summary>
    ///// <param name="vehicle"></param>
    ///// <param name="vehicleInfo"></param>
    //private static async void ApplyVehicleModsDelayed(Vehicle vehicle, VehicleInfo vehicleInfo, int delay)
    //{
    //  if (vehicle != null && vehicle.Exists())
    //  {
    //    vehicle.Mods.InstallModKit();
    //    // set the extras
    //    foreach (var extra in vehicleInfo.extras)
    //    {
    //      if (DoesExtraExist(vehicle.Handle, extra.Key))
    //        vehicle.ToggleExtra(extra.Key, extra.Value);
    //    }

    //    SetVehicleWheelType(vehicle.Handle, vehicleInfo.wheelType);
    //    SetVehicleMod(vehicle.Handle, 23, 0, vehicleInfo.customWheels);
    //    if (vehicle.Model.IsBike)
    //    {
    //      SetVehicleMod(vehicle.Handle, 24, 0, vehicleInfo.customWheels);
    //    }
    //    ToggleVehicleMod(vehicle.Handle, 18, vehicleInfo.turbo);
    //    SetVehicleTyreSmokeColor(vehicle.Handle, vehicleInfo.colors["tyresmokeR"], vehicleInfo.colors["tyresmokeG"], vehicleInfo.colors["tyresmokeB"]);
    //    ToggleVehicleMod(vehicle.Handle, 20, vehicleInfo.tyreSmoke);
    //    ToggleVehicleMod(vehicle.Handle, 22, vehicleInfo.xenonHeadlights);
    //    SetVehicleLivery(vehicle.Handle, vehicleInfo.livery);

    //    SetVehicleColours(vehicle.Handle, vehicleInfo.colors["primary"], vehicleInfo.colors["secondary"]);
    //    SetVehicleInteriorColour(vehicle.Handle, vehicleInfo.colors["trim"]);
    //    SetVehicleDashboardColour(vehicle.Handle, vehicleInfo.colors["dash"]);

    //    SetVehicleExtraColours(vehicle.Handle, vehicleInfo.colors["pearlescent"], vehicleInfo.colors["wheels"]);

    //    SetVehicleNumberPlateText(vehicle.Handle, vehicleInfo.plateText);
    //    SetVehicleNumberPlateTextIndex(vehicle.Handle, vehicleInfo.plateStyle);

    //    SetVehicleWindowTint(vehicle.Handle, vehicleInfo.windowTint);

    //    vehicle.CanTiresBurst = !vehicleInfo.bulletProofTires;

    //    SetVehicleEnveffScale(vehicle.Handle, vehicleInfo.enveffScale);

    //    VehicleOptions._SetHeadlightsColorOnVehicle(vehicle, vehicleInfo.headlightColor);

    //    vehicle.Mods.NeonLightsColor = System.Drawing.Color.FromArgb(red: vehicleInfo.colors["neonR"], green: vehicleInfo.colors["neonG"], blue: vehicleInfo.colors["neonB"]);
    //    vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Left, vehicleInfo.neonLeft);
    //    vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Right, vehicleInfo.neonRight);
    //    vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Front, vehicleInfo.neonFront);
    //    vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Back, vehicleInfo.neonBack);

    //    void DoMods()
    //    {
    //      vehicleInfo.mods.ToList().ForEach(mod =>
    //      {
    //        if (vehicle != null && vehicle.Exists())
    //          SetVehicleMod(vehicle.Handle, mod.Key, mod.Value, vehicleInfo.customWheels);
    //      });
    //    }

    //    DoMods();
    //    // Performance mods require a delay after setting the modkit,
    //    // so we just do it once first so all the visual mods load instantly,
    //    // and after a small delay we do it again to make sure all performance
    //    // mods have also loaded.
    //    await Delay(delay);
    //    DoMods();
    //  }
    //}
    #endregion
    #endregion

    #region VehicleInfo struct
    /// <summary>
    /// Contains all information for a saved vehicle.
    /// </summary>
    public struct VehicleInfo
    {
      public Dictionary<string, int> colors;
      public bool customWheels;
      public Dictionary<int, bool> extras;
      public int livery;
      public uint model;
      public Dictionary<int, int> mods;
      public string name;
      public bool neonBack;
      public bool neonFront;
      public bool neonLeft;
      public bool neonRight;
      public string plateText;
      public int plateStyle;
      public bool turbo;
      public bool tyreSmoke;
      public int version;
      public int wheelType;
      public int windowTint;
      public bool xenonHeadlights;
      public bool bulletProofTires;
      public int headlightColor;
      public float enveffScale;
    };
    #endregion

    #region Load Model
    /// <summary>
    /// Check and load a model.
    /// </summary>
    /// <param name="modelHash"></param>
    /// <returns>True if model is valid & loaded, false if model is invalid.</returns>
    private static async Task<bool> LoadModel(uint modelHash)
    {
      // Check if the model exists in the game.
      if (IsModelInCdimage(modelHash))
      {
        // Load the model.
        RequestModel(modelHash);
        // Wait until it's loaded.
        while (!HasModelLoaded(modelHash))
        {
          await Delay(0);
        }
        // Model is loaded, return true.
        return true;
      }
      // Model is not valid or is not loaded correctly.
      else
      {
        // Return false.
        return false;
      }
    }
    #endregion

    #region GetUserInput
    /// <summary>
    /// Get a user input text string.
    /// </summary>
    /// <returns></returns>
    public static async Task<string> GetUserInput() => await GetUserInput(null, null, 30);
    /// <summary>
    /// Get a user input text string.
    /// </summary>
    /// <param name="maxInputLength"></param>
    /// <returns></returns>
    public static async Task<string> GetUserInput(int maxInputLength) => await GetUserInput(null, null, maxInputLength);
    /// <summary>
    /// Get a user input text string.
    /// </summary>
    /// <param name="windowTitle"></param>
    /// <returns></returns>
    public static async Task<string> GetUserInput(string windowTitle) => await GetUserInput(windowTitle, null, 30);
    /// <summary>
    /// Get a user input text string.
    /// </summary>
    /// <param name="windowTitle"></param>
    /// <param name="maxInputLength"></param>
    /// <returns></returns>
    public static async Task<string> GetUserInput(string windowTitle, int maxInputLength) => await GetUserInput(windowTitle, null, maxInputLength);
    /// <summary>
    /// Get a user input text string.
    /// </summary>
    /// <param name="windowTitle"></param>
    /// <param name="defaultText"></param>
    /// <returns></returns>
    public static async Task<string> GetUserInput(string windowTitle, string defaultText) => await GetUserInput(windowTitle, defaultText, 30);
    /// <summary>
    /// Get a user input text string.
    /// </summary>
    /// <param name="windowTitle"></param>
    /// <param name="defaultText"></param>
    /// <param name="maxInputLength"></param>
    /// <returns></returns>
    public static async Task<string> GetUserInput(string windowTitle, string defaultText, int maxInputLength)
    {
      // Create the window title string.
      var spacer = "\t";
      AddTextEntry($"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", $"{windowTitle ?? "Enter"}:{spacer}(MAX {maxInputLength} Characters)");

      // Display the input box.
      DisplayOnscreenKeyboard(1, $"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", "", defaultText ?? "", "", "", "", maxInputLength);
      await Delay(0);
      // Wait for a result.
      while (true)
      {
        int keyboardStatus = UpdateOnscreenKeyboard();

        switch (keyboardStatus)
        {
          case 3: // not displaying input field anymore somehow
          case 2: // cancelled
            return null;
          case 1: // finished editing
            return GetOnscreenKeyboardResult();
          default:
            await Delay(0);
            break;
        }
      }
    }
    #endregion

    #region ToProperString()
    /// <summary>
    /// Converts a PascalCaseString to a Propper Case String.
    /// </summary>
    /// <param name="inputString"></param>
    /// <returns>Input string converted to a normal sentence.</returns>
    public static string ToProperString(string inputString)
    {
      var outputString = "";
      var prevUpper = true;
      foreach (char c in inputString)
      {
        if (char.IsLetter(c) && c != ' ' && c == char.Parse(c.ToString().ToUpper()))
        {
          if (prevUpper)
          {
            outputString += $"{c}";
          }
          else
          {
            outputString += $" {c}";
          }
          prevUpper = true;
        }
        else
        {
          prevUpper = false;
          outputString += c.ToString();
        }
      }
      while (outputString.IndexOf("  ") != -1)
      {
        outputString = outputString.Replace("  ", " ");
      }
      return outputString;
    }
    #endregion

    #region Data parsing functions
    /// <summary>
    /// Converts a simple json string (only containing (string) key : (string) value).
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static Dictionary<string, string> JsonToDictionary(string json)
    {
      return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
    }
    #endregion

    #region StringToStringArray
    /// <summary>
    /// Converts the inputString into a string[] (array).
    /// Each string in the array is up to 99 characters long at max.
    /// </summary>
    /// <param name="inputString"></param>
    /// <returns></returns>
    public static string[] StringToArray(string inputString)
    {
      return CitizenFX.Core.UI.Screen.StringToArray(inputString);
    }
    #endregion

    #region Hud Functions
    /// <summary>
    /// Draw text on the screen at the provided x and y locations.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="xPosition">The x position for the text draw origin.</param>
    /// <param name="yPosition">The y position for the text draw origin.</param>
    public static void DrawTextOnScreen(string text, float xPosition, float yPosition) =>
        DrawTextOnScreen(text, xPosition, yPosition, size: 0.48f);

    /// <summary>
    /// Draw text on the screen at the provided x and y locations.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="xPosition">The x position for the text draw origin.</param>
    /// <param name="yPosition">The y position for the text draw origin.</param>
    /// <param name="size">The size of the text.</param>
    public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size) =>
        DrawTextOnScreen(text, xPosition, yPosition, size, CitizenFX.Core.UI.Alignment.Left);

    /// <summary>
    /// Draw text on the screen at the provided x and y locations.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="xPosition">The x position for the text draw origin.</param>
    /// <param name="yPosition">The y position for the text draw origin.</param>
    /// <param name="size">The size of the text.</param>
    /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
    public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification) =>
        DrawTextOnScreen(text, xPosition, yPosition, size, justification, 6);

    /// <summary>
    /// Draw text on the screen at the provided x and y locations.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="xPosition">The x position for the text draw origin.</param>
    /// <param name="yPosition">The y position for the text draw origin.</param>
    /// <param name="size">The size of the text.</param>
    /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
    /// <param name="font">Specify the font to use (0-8).</param>
    public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font) =>
        DrawTextOnScreen(text, xPosition, yPosition, size, justification, font, false);

    /// <summary>
    /// Draw text on the screen at the provided x and y locations.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="xPosition">The x position for the text draw origin.</param>
    /// <param name="yPosition">The y position for the text draw origin.</param>
    /// <param name="size">The size of the text.</param>
    /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
    /// <param name="font">Specify the font to use (0-8).</param>
    /// <param name="disableTextOutline">Disables the default text outline.</param>
    public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font, bool disableTextOutline)
    {
      if (IsHudPreferenceSwitchedOn() && Hud.IsVisible && !IsPlayerSwitchInProgress() && IsScreenFadedIn() && !IsPauseMenuActive() && !IsFrontendFading() && !IsPauseMenuRestarting() && !IsHudHidden())
      {
        SetTextFont(font);
        SetTextScale(1.0f, size);
        if (justification == CitizenFX.Core.UI.Alignment.Right)
        {
          SetTextWrap(0f, xPosition);
        }
        SetTextJustification((int)justification);
        if (!disableTextOutline) { SetTextOutline(); }
        BeginTextCommandDisplayText("STRING");
        AddTextComponentSubstringPlayerName(text);
        EndTextCommandDisplayText(xPosition, yPosition);
      }
    }
    #endregion

    #region ped info struct
    public struct PedInfo
    {
      public int version;
      public uint model;
      public bool isMpPed;
      public Dictionary<int, int> props;
      public Dictionary<int, int> propTextures;
      public Dictionary<int, int> drawableVariations;
      public Dictionary<int, int> drawableVariationTextures;
    };
    #endregion

    #region Get "Header" Menu Item
    /// <summary>
    /// Get a header menu item (text-centered, disabled MenuItem)
    /// </summary>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public static MenuItem GetSpacerMenuItem(string title, string description = null)
    {
      string output = "~h~";
      int length = title.Length;
      int totalSize = 80 - length;

      for (var i = 0; i < totalSize / 2 - (length / 2); i++)
      {
        output += " ";
      }
      output += title;
      MenuItem item = new MenuItem(output, description ?? "")
      {
        Enabled = false
      };
      return item;
    }
    #endregion

    #region Log Function
    /// <summary>
    /// Print data to the console and save it to the CitizenFX.log file. Only when vMenu debugging mode is enabled.
    /// </summary>
    /// <param name="data"></param>
    public static void Log(string data)
    {
      if (false) Debug.WriteLine(@data);
    }
    #endregion

    #region Get Currently Opened Menu
    /// <summary>
    /// Returns the currently opened menu, if no menu is open, it'll return null.
    /// </summary>
    /// <returns></returns>
    public static Menu GetOpenMenu()
    {
      return MenuController.GetCurrentMenu();
    }
    #endregion

    #region Disable Movement Controls
    /// <summary>
    /// Disables all movement and camera related controls this frame.
    /// </summary>
    /// <param name="disableMovement"></param>
    /// <param name="disableCameraMovement"></param>
    public static void DisableMovementControlsThisFrame(bool disableMovement, bool disableCameraMovement)
    {
      if (disableMovement)
      {
        Game.DisableControlThisFrame(0, Control.MoveDown);
        Game.DisableControlThisFrame(0, Control.MoveDownOnly);
        Game.DisableControlThisFrame(0, Control.MoveLeft);
        Game.DisableControlThisFrame(0, Control.MoveLeftOnly);
        Game.DisableControlThisFrame(0, Control.MoveLeftRight);
        Game.DisableControlThisFrame(0, Control.MoveRight);
        Game.DisableControlThisFrame(0, Control.MoveRightOnly);
        Game.DisableControlThisFrame(0, Control.MoveUp);
        Game.DisableControlThisFrame(0, Control.MoveUpDown);
        Game.DisableControlThisFrame(0, Control.MoveUpOnly);
        Game.DisableControlThisFrame(0, Control.VehicleFlyMouseControlOverride);
        Game.DisableControlThisFrame(0, Control.VehicleMouseControlOverride);
        Game.DisableControlThisFrame(0, Control.VehicleMoveDown);
        Game.DisableControlThisFrame(0, Control.VehicleMoveDownOnly);
        Game.DisableControlThisFrame(0, Control.VehicleMoveLeft);
        Game.DisableControlThisFrame(0, Control.VehicleMoveLeftRight);
        Game.DisableControlThisFrame(0, Control.VehicleMoveRight);
        Game.DisableControlThisFrame(0, Control.VehicleMoveRightOnly);
        Game.DisableControlThisFrame(0, Control.VehicleMoveUp);
        Game.DisableControlThisFrame(0, Control.VehicleMoveUpDown);
        Game.DisableControlThisFrame(0, Control.VehicleSubMouseControlOverride);
        Game.DisableControlThisFrame(0, Control.Duck);
        Game.DisableControlThisFrame(0, Control.SelectWeapon);
      }
      if (disableCameraMovement)
      {
        Game.DisableControlThisFrame(0, Control.LookBehind);
        Game.DisableControlThisFrame(0, Control.LookDown);
        Game.DisableControlThisFrame(0, Control.LookDownOnly);
        Game.DisableControlThisFrame(0, Control.LookLeft);
        Game.DisableControlThisFrame(0, Control.LookLeftOnly);
        Game.DisableControlThisFrame(0, Control.LookLeftRight);
        Game.DisableControlThisFrame(0, Control.LookRight);
        Game.DisableControlThisFrame(0, Control.LookRightOnly);
        Game.DisableControlThisFrame(0, Control.LookUp);
        Game.DisableControlThisFrame(0, Control.LookUpDown);
        Game.DisableControlThisFrame(0, Control.LookUpOnly);
        Game.DisableControlThisFrame(0, Control.ScaledLookDownOnly);
        Game.DisableControlThisFrame(0, Control.ScaledLookLeftOnly);
        Game.DisableControlThisFrame(0, Control.ScaledLookLeftRight);
        Game.DisableControlThisFrame(0, Control.ScaledLookUpDown);
        Game.DisableControlThisFrame(0, Control.ScaledLookUpOnly);
        Game.DisableControlThisFrame(0, Control.VehicleDriveLook);
        Game.DisableControlThisFrame(0, Control.VehicleDriveLook2);
        Game.DisableControlThisFrame(0, Control.VehicleLookBehind);
        Game.DisableControlThisFrame(0, Control.VehicleLookLeft);
        Game.DisableControlThisFrame(0, Control.VehicleLookRight);
        Game.DisableControlThisFrame(0, Control.NextCamera);
        Game.DisableControlThisFrame(0, Control.VehicleFlyAttackCamera);
        Game.DisableControlThisFrame(0, Control.VehicleCinCam);
      }
    }
    #endregion

    #region Get safe player name
    /// <summary>
    /// Returns a properly formatted and escaped player name for notifications.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetSafePlayerName(string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        return "";
      }
      return name.Replace("^", @"\^").Replace("~", @"\~").Replace("<", "«").Replace(">", "»");
    }
    #endregion

    #region Draw model dimensions math util functions

    /*
        These util functions are taken from Deltanic's mapeditor resource for FiveM.
        https://gitlab.com/shockwave-fivem/mapeditor/tree/master
        Thank you Deltanic for allowing me to use these functions here.
    */

    /// <summary>
    /// Draws the bounding box for the entity with the provided rgba color.
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <param name="a"></param>
    public static void DrawEntityBoundingBox(Entity ent, int r, int g, int b, int a)
    {
      var box = GetEntityBoundingBox(ent.Handle);
      DrawBoundingBox(box, r, g, b, a);
    }

    /// <summary>
    /// Gets the bounding box of the entity model in world coordinates, used by <see cref="DrawEntityBoundingBox(Entity, int, int, int, int)"/>.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    internal static Vector3[] GetEntityBoundingBox(int entity)
    {
      Vector3 min = Vector3.Zero;
      Vector3 max = Vector3.Zero;

      GetModelDimensions((uint)GetEntityModel(entity), ref min, ref max);
      //const float pad = 0f;
      const float pad = 0.001f;
      var retval = new Vector3[8]
      {
                // Bottom
                GetOffsetFromEntityInWorldCoords(entity, min.X - pad, min.Y - pad, min.Z - pad),
                GetOffsetFromEntityInWorldCoords(entity, max.X + pad, min.Y - pad, min.Z - pad),
                GetOffsetFromEntityInWorldCoords(entity, max.X + pad, max.Y + pad, min.Z - pad),
                GetOffsetFromEntityInWorldCoords(entity, min.X - pad, max.Y + pad, min.Z - pad),

                // Top
                GetOffsetFromEntityInWorldCoords(entity, min.X - pad, min.Y - pad, max.Z + pad),
                GetOffsetFromEntityInWorldCoords(entity, max.X + pad, min.Y - pad, max.Z + pad),
                GetOffsetFromEntityInWorldCoords(entity, max.X + pad, max.Y + pad, max.Z + pad),
                GetOffsetFromEntityInWorldCoords(entity, min.X - pad, max.Y + pad, max.Z + pad)
      };
      return retval;
    }

    /// <summary>
    /// Draws the edge poly faces and the edge lines for the specific box coordinates using the specified rgba color.
    /// </summary>
    /// <param name="box"></param>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <param name="a"></param>
    private static void DrawBoundingBox(Vector3[] box, int r, int g, int b, int a)
    {
      var polyMatrix = GetBoundingBoxPolyMatrix(box);
      var edgeMatrix = GetBoundingBoxEdgeMatrix(box);

      DrawPolyMatrix(polyMatrix, r, g, b, a);
      DrawEdgeMatrix(edgeMatrix, 255, 255, 255, 255);
    }

    /// <summary>
    /// Gets the coordinates for all poly box faces.
    /// </summary>
    /// <param name="box"></param>
    /// <returns></returns>
    private static Vector3[][] GetBoundingBoxPolyMatrix(Vector3[] box)
    {
      return new Vector3[12][]
      {
                new Vector3[3] { box[2], box[1], box[0] },
                new Vector3[3] { box[3], box[2], box[0] },

                new Vector3[3] { box[4], box[5], box[6] },
                new Vector3[3] { box[4], box[6], box[7] },

                new Vector3[3] { box[2], box[3], box[6] },
                new Vector3[3] { box[7], box[6], box[3] },

                new Vector3[3] { box[0], box[1], box[4] },
                new Vector3[3] { box[5], box[4], box[1] },

                new Vector3[3] { box[1], box[2], box[5] },
                new Vector3[3] { box[2], box[6], box[5] },

                new Vector3[3] { box[4], box[7], box[3] },
                new Vector3[3] { box[4], box[3], box[0] }
      };
    }

    /// <summary>
    /// Gets the coordinates for all edge coordinates.
    /// </summary>
    /// <param name="box"></param>
    /// <returns></returns>
    private static Vector3[][] GetBoundingBoxEdgeMatrix(Vector3[] box)
    {
      return new Vector3[12][]
      {
                new Vector3[2] { box[0], box[1] },
                new Vector3[2] { box[1], box[2] },
                new Vector3[2] { box[2], box[3] },
                new Vector3[2] { box[3], box[0] },

                new Vector3[2] { box[4], box[5] },
                new Vector3[2] { box[5], box[6] },
                new Vector3[2] { box[6], box[7] },
                new Vector3[2] { box[7], box[4] },

                new Vector3[2] { box[0], box[4] },
                new Vector3[2] { box[1], box[5] },
                new Vector3[2] { box[2], box[6] },
                new Vector3[2] { box[3], box[7] }
      };
    }

    /// <summary>
    /// Draws the poly matrix faces.
    /// </summary>
    /// <param name="polyCollection"></param>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <param name="a"></param>
    private static void DrawPolyMatrix(Vector3[][] polyCollection, int r, int g, int b, int a)
    {
      foreach (var poly in polyCollection)
      {
        float x1 = poly[0].X;
        float y1 = poly[0].Y;
        float z1 = poly[0].Z;

        float x2 = poly[1].X;
        float y2 = poly[1].Y;
        float z2 = poly[1].Z;

        float x3 = poly[2].X;
        float y3 = poly[2].Y;
        float z3 = poly[2].Z;
        DrawPoly(x1, y1, z1, x2, y2, z2, x3, y3, z3, r, g, b, a);
      }
    }

    /// <summary>
    /// Draws the edge lines for the model dimensions.
    /// </summary>
    /// <param name="linesCollection"></param>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <param name="a"></param>
    private static void DrawEdgeMatrix(Vector3[][] linesCollection, int r, int g, int b, int a)
    {
      foreach (var line in linesCollection)
      {
        float x1 = line[0].X;
        float y1 = line[0].Y;
        float z1 = line[0].Z;

        float x2 = line[1].X;
        float y2 = line[1].Y;
        float z2 = line[1].Z;

        DrawLine(x1, y1, z1, x2, y2, z2, r, g, b, a);
      }
    }
    #endregion

    #region Map (math util) function
    /// <summary>
    /// Maps the <paramref name="value"/> (which is a value between <paramref name="min_in"/> and <paramref name="max_in"/>) to a new value in the range of <paramref name="min_out"/> and <paramref name="max_out"/>.
    /// </summary>
    /// <param name="value">The value to map.</param>
    /// <param name="min_in">The minimum range value of the value.</param>
    /// <param name="max_in">The max range value of the value.</param>
    /// <param name="min_out">The min output range value.</param>
    /// <param name="max_out">The max output range value.</param>
    /// <returns></returns>
    public static float Map(float value, float min_in, float max_in, float min_out, float max_out)
    {
      return (value - min_in) * (max_out - min_out) / (max_in - min_in) + min_out;
    }

    /// <summary>
    /// Maps the <paramref name="value"/> (which is a value between <paramref name="min_in"/> and <paramref name="max_in"/>) to a new value in the range of <paramref name="min_out"/> and <paramref name="max_out"/>.
    /// </summary>
    /// <param name="value">The value to map.</param>
    /// <param name="min_in">The minimum range value of the value.</param>
    /// <param name="max_in">The max range value of the value.</param>
    /// <param name="min_out">The min output range value.</param>
    /// <param name="max_out">The max output range value.</param>
    /// <returns></returns>
    public static double Map(double value, double min_in, double max_in, double min_out, double max_out)
    {
      return (value - min_in) * (max_out - min_out) / (max_in - min_in) + min_out;
    }
    #endregion

    #region Keyfob personal vehicle func
    public static async void PressKeyFob(Vehicle veh)
    {
      Player player = Game.Player;
      if (player != null && !player.IsDead && !player.Character.IsInVehicle())
      {
        uint KeyFobHashKey = (uint)GetHashKey("p_car_keys_01");
        RequestModel(KeyFobHashKey);
        while (!HasModelLoaded(KeyFobHashKey))
        {
          await Delay(0);
        }

        int KeyFobObject = CreateObject((int)KeyFobHashKey, 0, 0, 0, true, true, true);
        AttachEntityToEntity(KeyFobObject, player.Character.Handle, GetPedBoneIndex(player.Character.Handle, 57005), 0.09f, 0.03f, -0.02f, -76f, 13f, 28f, false, true, true, true, 0, true);
        SetModelAsNoLongerNeeded(KeyFobHashKey); // cleanup model from memory

        ClearPedTasks(player.Character.Handle);
        SetCurrentPedWeapon(Game.PlayerPed.Handle, (uint)GetHashKey("WEAPON_UNARMED"), true);
        //if (player.Character.Weapons.Current.Hash != WeaponHash.Unarmed)
        //{
        //    player.Character.Weapons.Give(WeaponHash.Unarmed, 1, true, true);
        //}

        // if (!HasEntityClearLosToEntityInFront(player.Character.Handle, veh.Handle))
        {
          /*
          TODO: Work out how to get proper heading between entities.
          */


          //SetPedDesiredHeading(player.Character.Handle, )
          //float heading = GetHeadingFromVector_2d(player.Character.Position.X - veh.Position.Y, player.Character.Position.Y - veh.Position.X);
          //double x = Math.Cos(player.Character.Position.X) * Math.Sin(player.Character.Position.Y - (double)veh.Position.Y);
          //double y = Math.Cos(player.Character.Position.X) * Math.Sin(veh.Position.X) - Math.Sin(player.Character.Position.X) * Math.Cos(veh.Position.X) * Math.Cos(player.Character.Position.Y - (double)veh.Position.Y);
          //float heading = (float)Math.Atan2(x, y);
          //Debug.WriteLine(heading.ToString());
          //SetPedDesiredHeading(player.Character.Handle, heading);

          ClearPedTasks(Game.PlayerPed.Handle);
          TaskTurnPedToFaceEntity(player.Character.Handle, veh.Handle, 500);
        }

        string animDict = "anim@mp_player_intmenu@key_fob@";
        RequestAnimDict(animDict);
        while (!HasAnimDictLoaded(animDict))
        {
          await Delay(0);
        }
        player.Character.Task.PlayAnimation(animDict, "fob_click", 3f, 1000, AnimationFlags.UpperBodyOnly);
        PlaySoundFromEntity(-1, "Remote_Control_Fob", player.Character.Handle, "PI_Menu_Sounds", true, 0);


        await Delay(1250);
        DetachEntity(KeyFobObject, false, false);
        DeleteObject(ref KeyFobObject);
        RemoveAnimDict(animDict); // cleanup anim dict from memory
      }

      await Delay(0);
    }
    #endregion
  }
}
