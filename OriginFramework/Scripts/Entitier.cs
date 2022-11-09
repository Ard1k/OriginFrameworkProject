using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using OriginFramework.Menus;
using OriginFramework.Helpers;

namespace OriginFramework
{
  public class Entitier : BaseScript
  {
    private static bool EntitierActive { get; set; } = false;
    private static int MovingSpeed { get; set; } = 0;
    private static int Scale { get; set; } = -1;
    private static Vector3 EntityPosition { get; set; } = new Vector3();
    private static float EntityYaw { get; set; } = 0f;
    private static float EntityPitch { get; set; } = 0f;
    private static float EntityRoll { get; set; } = 0f;
    private static int EntityId { get; set; } = -1;
    private static int ModelHash { get; set; } = 1317858860;
    private static EntityMode Mode { get; set; } = EntityMode.Spawn;
    private static float RotationOffset { get; set; } = 90f;
    private static float RotationOffsetRad { get { return (float)(RotationOffset * Math.PI) / 180; } }
    private static float ViewDistance { get; set; } = 10f;
    private static MapHelper MapHelper { get; set; }


    private List<float> speeds = new List<float>()
        {
            0.1f, 1f, 10f
        };

    public Entitier()
    {
      Tick += EntitierHandler;
    }

    internal static void SetEntitierActivate(EntityMode mode, MapHelper helper)
    {
      EntitierActive = true;

      Mode = mode;
      MapHelper = helper;

      var isInVeh = Game.PlayerPed.IsInVehicle();
      var coordsEntity = Game.PlayerPed.Handle;

      EntityPosition = GetEntityCoords(coordsEntity, true);
      //EntityYaw = GetEntityHeading(coordsEntity);
    }

    internal static void SetEntitierDeactivate()
    {
      EntitierActive = false;
      //if (Mode == EntityMode.CopyInfo)
      //{
      //  int _id = 0; 
      //  DeleteObject(ref _id);
      //}

      MapHelper = null;
      EntityId = -1;
      SetModelAsNoLongerNeeded((uint)ModelHash);
    }

    internal static void DespawnCurrentEntity()
    {
      int _id = EntityId;
      DeleteObject(ref _id);
      EntityId = -1;
      SetModelAsNoLongerNeeded((uint)ModelHash);
    }

    private async Task EntitierHandler()
    {
      if (EntitierActive)
      {
        Scale = RequestScaleformMovie("INSTRUCTIONAL_BUTTONS");
        while (!HasScaleformMovieLoaded(Scale))
        {
          await Delay(0);
        }
      }
      while (EntitierActive)
      {
        if (EntityId <= 0)
        {
          RequestModel((uint)ModelHash);
          while (!HasModelLoaded((uint)ModelHash))
          {
            await BaseScript.Delay(0);
          }
          EntityId = CreateObject(ModelHash, EntityPosition.X, EntityPosition.Y, EntityPosition.Z, true, true, false);
        }

        if (!IsHudHidden())
        {
          BeginScaleformMovieMethod(Scale, "CLEAR_ALL");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
          ScaleformMovieMethodAddParamInt(0);
          PushScaleformMovieMethodParameterString("~INPUT_SPRINT~");
          PushScaleformMovieMethodParameterString($"Change Speed ({speeds[MovingSpeed]})");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
          ScaleformMovieMethodAddParamInt(1);
          PushScaleformMovieMethodParameterString("~INPUT_MOVE_LR~");
          PushScaleformMovieMethodParameterString($"Move Left/Right");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
          ScaleformMovieMethodAddParamInt(2);
          PushScaleformMovieMethodParameterString("~INPUT_MOVE_UD~");
          PushScaleformMovieMethodParameterString($"Move Front/Back");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
          ScaleformMovieMethodAddParamInt(3);
          PushScaleformMovieMethodParameterString("~INPUT_MULTIPLAYER_INFO~");
          PushScaleformMovieMethodParameterString($"Down");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
          ScaleformMovieMethodAddParamInt(4);
          PushScaleformMovieMethodParameterString("~INPUT_COVER~");
          PushScaleformMovieMethodParameterString($"Up");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
          ScaleformMovieMethodAddParamInt(5);
          PushScaleformMovieMethodParameterString("~INPUT_RELOAD~");
          PushScaleformMovieMethodParameterString($"Select entity");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
          ScaleformMovieMethodAddParamInt(6);
          PushScaleformMovieMethodParameterString("~INPUT_VEH_HORN~");
          PushScaleformMovieMethodParameterString($"Rotate");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
          ScaleformMovieMethodAddParamInt(6);
          PushScaleformMovieMethodParameterString("~INPUT_VEH_EXIT~");
          PushScaleformMovieMethodParameterString($"{(MapHelper != null ? "Add" : "Confirm")}");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "DRAW_INSTRUCTIONAL_BUTTONS");
          ScaleformMovieMethodAddParamInt(0);
          EndScaleformMovieMethod();

          DrawScaleformMovieFullscreen(Scale, 255, 255, 255, 255, 0);
        }

        var playerEntity = Game.PlayerPed.Handle;

        FreezeEntityPosition(playerEntity, true);
        SetEntityAlpha(playerEntity, 255, 0);

        Game.DisableControlThisFrame(0, Control.MoveUpOnly);
        Game.DisableControlThisFrame(0, Control.MoveUp);
        Game.DisableControlThisFrame(0, Control.MoveUpDown);
        Game.DisableControlThisFrame(0, Control.MoveDown);
        Game.DisableControlThisFrame(0, Control.MoveDownOnly);
        Game.DisableControlThisFrame(0, Control.MoveLeft);
        Game.DisableControlThisFrame(0, Control.MoveLeftOnly);
        Game.DisableControlThisFrame(0, Control.MoveLeftRight);
        Game.DisableControlThisFrame(0, Control.MoveRight);
        Game.DisableControlThisFrame(0, Control.MoveRightOnly);
        Game.DisableControlThisFrame(0, Control.Cover);
        Game.DisableControlThisFrame(0, Control.MultiplayerInfo);
        Game.DisableControlThisFrame(0, Control.VehicleExit);
        Game.DisableControlThisFrame(0, Control.VehicleHorn); //E
        Game.DisableControlThisFrame(0, Control.Reload); //R
        Game.DisableControlThisFrame(0, Control.VehicleCinCam); //R
        Game.DisableControlThisFrame(0, Control.MeleeAttackLight); //R

        Game.DisableControlThisFrame(0, Control.VehicleFlyRollLeftOnly); //NUM4
        Game.DisableControlThisFrame(0, Control.VehicleFlyRollRightOnly); //NUM6
        Game.DisableControlThisFrame(0, Control.VehicleFlyPitchUpOnly); //NUM8
        Game.DisableControlThisFrame(0, Control.VehicleFlyPitchDownOnly); //NUM5
        Game.DisableControlThisFrame(0, Control.VehicleFlySelectTargetLeft); //NUM7
        Game.DisableControlThisFrame(0, Control.VehicleFlySelectTargetRight); //NUM9

        Game.DisableControlThisFrame(0, Control.ReplayFOVIncrease); //NUM+
        Game.DisableControlThisFrame(0, Control.ReplayFOVDecrease); //NUM-

        if (Game.PlayerPed.IsInVehicle())
        {
          Game.DisableControlThisFrame(0, Control.VehicleRadioWheel);
          Game.DisableControlThisFrame(0, Control.VehicleAccelerate);
          Game.DisableControlThisFrame(0, Control.VehicleBrake);
        }

        Vector3 movementVector;
        Vector3 rotatedMovementVector;
        var yoff = 0.0f;
        var xoff = 0.0f;
        var zoff = 0.0f;
        var rotoff = 0.0f;

        if (Game.CurrentInputMode == InputMode.MouseAndKeyboard && UpdateOnscreenKeyboard() != 0 && !Game.IsPaused && !NativeMenuManager.IsMenuOpen(null))
        {
          if (Game.IsDisabledControlJustPressed(0, Control.VehicleExit))
          {
            if (MapHelper == null)
            {
              SetEntitierDeactivate();
              return;
            }

            MapHelper.AddProp(ModelHash, EntityPosition.X, EntityPosition.Y, EntityPosition.Z, EntityPitch, EntityRoll, EntityYaw + RotationOffset);
            if (NetworkGetEntityIsNetworked(EntityId))
            {
              var netId = ObjToNet(EntityId);
              if (netId > 0)
                MapHelper.spawnedNetIds.Add(netId);
            }
            EntityId = -1;
          }

          if (Game.IsControlJustPressed(0, Control.Sprint))
          {
            MovingSpeed++;
            if (MovingSpeed == speeds.Count)
            {
              MovingSpeed = 0;
            }
          }
          if (Game.IsDisabledControlPressed(0, Control.MoveUpOnly))
          {
            yoff = 0.3f;
          }
          if (Game.IsDisabledControlPressed(0, Control.MoveDownOnly))
          {
            yoff = -0.3f;
          }
          if (Game.IsDisabledControlPressed(0, Control.MoveLeftOnly))
          {
            xoff = -0.3f;
          }
          if (Game.IsDisabledControlPressed(0, Control.MoveRightOnly))
          {
            xoff = +0.3f;
          }
          if (Game.IsDisabledControlPressed(0, Control.Cover))
          {
            zoff = 0.2f;
          }
          if (Game.IsDisabledControlPressed(0, Control.MultiplayerInfo))
          {
            zoff = -0.2f;
          }
          if (Game.IsDisabledControlJustPressed(0, Control.VehicleHorn))
          {
            RotationOffset += 90f;
            if (RotationOffset >= 360f)
              RotationOffset = 0f;
          }
          if (Game.IsDisabledControlPressed(0, Control.Reload) || Game.IsDisabledControlPressed(0, Control.MeleeAttackLight))
          {
            var menu = new NativeMenu
            {
              MenuTitle = "Props",
              Items = new List<NativeMenuItem>
              {
                new NativeMenuItem
                {
                  Name = "Set custom prop",
                  IsHide = true,
                  IsTextInput = true,
                  OnTextInput = (item, input) =>
                  {
                    int res = -1;
                    int.TryParse(input, out res);

                    DespawnCurrentEntity();
                    ModelHash = res;
                  }
                },
                new NativeMenuItem
                {
                  Name = "Road_MED_x3",
                  OnSelected = (item) =>
                  {
                    DespawnCurrentEntity();
                    ModelHash = 1317858860;
                  }
                },
                new NativeMenuItem
                {
                  Name = "Road_SML_x3",
                  OnSelected = (item) =>
                  {
                    DespawnCurrentEntity();
                    ModelHash = 1261306399;
                  }
                },
                new NativeMenuItem
                {
                  Name = "Ramp_ITS_2",
                  OnSelected = (item) =>
                  {
                    DespawnCurrentEntity();
                    ModelHash = -1061569318;
                  }
                },
              }
            };

            if (MapHelper != null)
            {
              menu.Items.Insert(0, new NativeMenuItem {
                Name = "Finish map",
                IsHide = true,
                OnSelected = (item) => {
                  TriggerServerEvent("ofw_map:SaveMap", JsonConvert.SerializeObject(MapHelper.Map));

                  if (MapHelper.spawnedNetIds != null && MapHelper.spawnedNetIds.Count >= 0)
                  {
                    foreach (var netId in MapHelper.spawnedNetIds)
                    {
                      var localId = NetToEnt(netId);
                      if (localId > 0)
                        DeleteObject(ref localId);
                    }
                  }

                  DespawnCurrentEntity();
                  SetEntitierDeactivate();
                }
              });
            }

            NativeMenuManager.OpenNewMenu("Entitier_Entity", () => { return menu; });
          }

          if (Game.IsDisabledControlJustPressed(0, Control.VehicleFlyRollLeftOnly)) //NUM4
          {
            EntityYaw += 15f;
          }
          if (Game.IsDisabledControlJustPressed(0, Control.VehicleFlyRollRightOnly)) //NUM6
          {
            EntityYaw -= 15f;
          }
          if (Game.IsDisabledControlJustPressed(0, Control.VehicleFlyPitchUpOnly)) //NUM8
          {
            EntityPitch -= 15f * (float)Math.Cos(RotationOffsetRad);
            EntityRoll += 15f * (float)Math.Sin(RotationOffsetRad);
          }
          if (Game.IsDisabledControlJustPressed(0, Control.VehicleFlyPitchDownOnly)) //NUM5
          {
            EntityPitch += 15f * (float)Math.Cos(RotationOffsetRad);
            EntityRoll -= 15f * (float)Math.Sin(RotationOffsetRad);
          }
          if (Game.IsDisabledControlJustPressed(0, Control.VehicleFlySelectTargetLeft)) //NUM7
          {
            EntityRoll -= 15f * (float)Math.Cos(RotationOffsetRad);
            EntityPitch -= 15f * (float)Math.Sin(RotationOffsetRad);
          }
          if (Game.IsDisabledControlJustPressed(0, Control.VehicleFlySelectTargetRight)) //NUM9
          {
            EntityRoll += 15f * (float)Math.Cos(RotationOffsetRad);
            EntityPitch += 15f * (float)Math.Sin(RotationOffsetRad);
          }

          if (Game.IsDisabledControlJustPressed(0, Control.ReplayFOVIncrease)) //NUM+
          {
            ViewDistance += 10f;
          }
          if (Game.IsDisabledControlJustPressed(0, Control.ReplayFOVDecrease)) //NUM-
          {
            ViewDistance -= 10f;
          }
        }

        float moveSpeed = (float)speeds[MovingSpeed];
        moveSpeed = (moveSpeed / (1f / GetFrameTime())) * 5;

        EntityYaw += moveSpeed * rotoff;

        movementVector = new Vector3((moveSpeed) * xoff, (moveSpeed) * yoff, (moveSpeed) * zoff);

        var headingRad = (EntityYaw * Math.PI) / 180;

        rotatedMovementVector = new Vector3(
          (float)(Math.Cos(headingRad) * movementVector.X - Math.Sin(headingRad) * movementVector.Y),
          (float)(Math.Sin(headingRad) * movementVector.X + Math.Cos(headingRad) * movementVector.Y),
          movementVector.Z
          );

        EntityPosition = Vector3.Add(EntityPosition, rotatedMovementVector);

        SetEntityCoords(EntityId, EntityPosition.X, EntityPosition.Y, EntityPosition.Z, false, false, false, false);
        SetEntityRotation(EntityId, EntityPitch, EntityRoll, EntityYaw + RotationOffset, 0, true);

        var shiftedHeadingRad = (EntityYaw + RotationOffset * Math.PI) / 180;

        Vector3 playerOffsetVector = new Vector3(
          //(float)(Math.Cos(headingRad) * -1 * ViewDistance * Math.Abs(Math.Sin(RotationOffsetRad)) - Math.Sin(headingRad) * -1 * ViewDistance * Math.Abs(Math.Cos(RotationOffsetRad))),
          //(float)(Math.Sin(headingRad) * -1 * ViewDistance * Math.Abs(Math.Sin(RotationOffsetRad)) + Math.Cos(headingRad) * -1 * ViewDistance * Math.Abs(Math.Cos(RotationOffsetRad))),
          (float)(Math.Sin(headingRad) * ViewDistance),
          (float)(Math.Cos(headingRad) * -1 * ViewDistance),
          0f
          );

        playerOffsetVector = Vector3.Add(EntityPosition, playerOffsetVector);

        SetEntityVelocity(playerEntity, 0f, 0f, 0f);
        SetEntityRotation(playerEntity, 0f, 0f, EntityYaw, 0, false);
        //SetEntityHeading(playerEntity, EntityYaw);
        SetGameplayCamRelativeHeading(0);
        SetEntityCollision(playerEntity, false, false);
        SetEntityCoordsNoOffset(playerEntity, playerOffsetVector.X, playerOffsetVector.Y, playerOffsetVector.Z, true, true, true);

        SetEntityVisible(playerEntity, false, false);
        SetLocalPlayerVisibleLocally(true);
        SetEntityAlpha(playerEntity, (int)(255 * 0.0), 0);

        SetEveryoneIgnorePlayer(Game.PlayerPed.Handle, true);
        SetPoliceIgnorePlayer(Game.PlayerPed.Handle, true);

        // After the next game tick, reset the entity properties.
        await Delay(0);
        FreezeEntityPosition(playerEntity, false);
        if (!Misc.IsPlayerGodModeEnabled)
          SetEntityInvincible(playerEntity, false);
        SetEntityCollision(playerEntity, true, true);

        // If the player is not set as invisible by PlayerOptions or if the noclip entity is not the player ped, reset the visibility
        SetEntityVisible(playerEntity, true, false);
        SetLocalPlayerVisibleLocally(true);

        // Always reset the alpha.
        ResetEntityAlpha(playerEntity);

        SetEveryoneIgnorePlayer(Game.PlayerPed.Handle, false);
        SetPoliceIgnorePlayer(Game.PlayerPed.Handle, false);
      }

      await Task.FromResult(0);
    }

    public enum EntityMode : int
    {
      Spawn = 0,
      //CopyInfo = 1,
      //SpawnAndCopyInfo = 2,
      MapEditor = 3
    }
  }
}

