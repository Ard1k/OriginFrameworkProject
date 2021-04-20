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
  public class Checkpointer : BaseScript
  {
    private static bool CheckpointerActive { get; set; } = false;
    private static int MovingSpeed { get; set; } = 0;
    private static int Scale { get; set; } = -1;
    private static Vector3 MarkerPosition { get; set; } = new Vector3();
    private static float MarkerHeading { get; set; } = 0f;
    private static float MarkerSize { get; set; } = 2f;
    private static ExportMode Mode { get; set; } = ExportMode.Indirectional;


    private List<string> speeds = new List<string>()
        {
            "Very Slow",
            "Slow",
            "Normal",
            "Fast"
        };

    public Checkpointer()
    {
      Tick += CheckpointerHandler;
    }

    internal static void SetCheckpointerActivate(ExportMode mode)
    {
      CheckpointerActive = true;

      Mode = mode;

      var isInVeh = Game.PlayerPed.IsInVehicle();
      var coordsEntity = isInVeh ? Game.PlayerPed.CurrentVehicle.Handle : Game.PlayerPed.Handle;

      MarkerPosition = GetEntityCoords(coordsEntity, true);
      MarkerHeading = GetEntityHeading(coordsEntity);
      MarkerSize = isInVeh ? 6f : 2f;
    }

    internal static void SetCheckpointerDeactivate()
    {
      CheckpointerActive = false;
    }

    internal static bool IsCheckpointerActive()
    {
      return CheckpointerActive;
    }

    private async Task CheckpointerHandler()
    {
      if (CheckpointerActive)
      {
        Scale = RequestScaleformMovie("INSTRUCTIONAL_BUTTONS");
        while (!HasScaleformMovieLoaded(Scale))
        {
          await Delay(0);
        }
      }
      while (CheckpointerActive)
      {
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
          PushScaleformMovieMethodParameterString($"Rotate R");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
          ScaleformMovieMethodAddParamInt(6);
          PushScaleformMovieMethodParameterString("~INPUT_VEH_HORN~");
          PushScaleformMovieMethodParameterString($"Rotate L");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
          ScaleformMovieMethodAddParamInt(7);
          PushScaleformMovieMethodParameterString("~INPUT_VEH_EXIT~");
          PushScaleformMovieMethodParameterString($"Confirm");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "DRAW_INSTRUCTIONAL_BUTTONS");
          ScaleformMovieMethodAddParamInt(0);
          EndScaleformMovieMethod();

          DrawScaleformMovieFullscreen(Scale, 255, 255, 255, 255, 0);
        }
        
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

        if (Game.CurrentInputMode == InputMode.MouseAndKeyboard && UpdateOnscreenKeyboard() != 0 && !Game.IsPaused)
        {
          if (Game.IsDisabledControlJustPressed(0, Control.VehicleExit))
          {
            SetCheckpointerDeactivate();

            if (Mode == ExportMode.Indirectional)
            {
              var message = new
              {
                type = "copyCoords",
                x = MarkerPosition.X,
                y = MarkerPosition.Y,
                z = MarkerPosition.Z
              };
              SendNuiMessage(JsonConvert.SerializeObject(message));

              Notify.Info("Marker copied to clipboard!");
            }
            else if (Mode == ExportMode.Directional)
            {
              var message = new
              {
                type = "copyCoordsAndHeading",
                x = MarkerPosition.X,
                y = MarkerPosition.Y,
                z = MarkerPosition.Z,
                h = MarkerHeading
              };
              SendNuiMessage(JsonConvert.SerializeObject(message));

              Notify.Info("Marker copied to clipboard!");
            }
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
          if (Game.IsDisabledControlPressed(0, Control.VehicleHorn))
          {
            rotoff = 6f;
          }
          if (Game.IsDisabledControlPressed(0, Control.Reload) || Game.IsDisabledControlPressed(0, Control.MeleeAttackLight))
          {
            rotoff = -6f;
          }
        }

        float moveSpeed = (float)(MovingSpeed + 1);
        moveSpeed = (moveSpeed / (1f / GetFrameTime())) * 5;

        MarkerHeading += moveSpeed * rotoff;

        movementVector = new Vector3((moveSpeed) * xoff, (moveSpeed) * yoff, (moveSpeed) * zoff);

        var headingRad = (MarkerHeading * Math.PI) / 180;

        rotatedMovementVector = new Vector3(
          (float)(Math.Cos(headingRad) * movementVector.X - Math.Sin(headingRad) * movementVector.Y),
          (float)(Math.Sin(headingRad) * movementVector.X + Math.Cos(headingRad) * movementVector.Y),
          movementVector.Z
          );

        MarkerPosition = Vector3.Add(MarkerPosition, rotatedMovementVector);

        DrawMarker(Mode == ExportMode.Directional ? 26 : 25, MarkerPosition.X, MarkerPosition.Y, MarkerPosition.Z, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, MarkerHeading, MarkerSize, MarkerSize, MarkerSize, 255, 0, 255, 255, false, false, 2, false, null, null, false);

        await Delay(0);
      }

      await Task.FromResult(0);
    }

    public enum ExportMode : int
    {
      Indirectional = 0,
      Directional = 1
    }
  }
}

