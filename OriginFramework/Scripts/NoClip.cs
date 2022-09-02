using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using MenuAPI;
using OriginFramework.Menus;
using static CitizenFX.Core.Native.API;

namespace OriginFramework.Scripts
{
  public class NoClip : BaseScript
  {
    private static bool NoclipActive { get; set; } = false;
    private static int MovingSpeed { get; set; } = 0;
    private static int Scale { get; set; } = -1;
    private static bool FollowCamMode { get; set; } = true;


    private List<string> speeds = new List<string>()
        {
            "Šnek",
            "Normální",
            "Turbo",
        };

    public NoClip()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.NoClip))
        return;

      #region register commands
      RegisterCommand("noclip", new Action<int, List<object>, string>((source, args, raw) =>
      {
        if (Main.IsAdmin)
          NoclipActive = !NoclipActive;
      }), false);
      #endregion

      Tick += NoClipHandler;

      InternalDependencyManager.Started(eScriptArea.NoClip);
    }

    internal static void SetNoclipActive(bool active)
    {
      NoclipActive = active;
    }

    internal static void SetNoclipSwitch()
    {
      NoclipActive = !NoclipActive;
    }

    internal static bool IsNoclipActive()
    {
      return NoclipActive;
    }

    private async Task NoClipHandler()
    {
      if (NoclipActive)
      {
        Scale = RequestScaleformMovie("INSTRUCTIONAL_BUTTONS");
        while (!HasScaleformMovieLoaded(Scale))
        {
          await Delay(0);
        }

        DrawScaleformMovieFullscreen(Scale, 255, 255, 255, 0, 0);
      }
      while (NoclipActive)
      {
        if (!IsHudHidden())
        {
          BeginScaleformMovieMethod(Scale, "CLEAR_ALL");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
          ScaleformMovieMethodAddParamInt(0);
          PushScaleformMovieMethodParameterString("~INPUT_SPRINT~");
          PushScaleformMovieMethodParameterString($"{FontsManager.FiraSansString}Zmenit rychlost ({speeds[MovingSpeed]})");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
          ScaleformMovieMethodAddParamInt(1);
          PushScaleformMovieMethodParameterString("~INPUT_MOVE_LR~");
          PushScaleformMovieMethodParameterString($"{FontsManager.FiraSansString}Pohyb Levá/Pravá");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
          ScaleformMovieMethodAddParamInt(2);
          PushScaleformMovieMethodParameterString("~INPUT_MOVE_UD~");
          PushScaleformMovieMethodParameterString($"{FontsManager.FiraSansString}Pohyb");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
          ScaleformMovieMethodAddParamInt(3);
          PushScaleformMovieMethodParameterString("~INPUT_MULTIPLAYER_INFO~");
          PushScaleformMovieMethodParameterString($"{FontsManager.FiraSansString}Dolu");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
          ScaleformMovieMethodAddParamInt(4);
          PushScaleformMovieMethodParameterString("~INPUT_COVER~");
          PushScaleformMovieMethodParameterString($"{FontsManager.FiraSansString}Nahoru");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
          ScaleformMovieMethodAddParamInt(5);
          PushScaleformMovieMethodParameterString("~INPUT_VEH_HEADLIGHT~");
          PushScaleformMovieMethodParameterString($"{FontsManager.FiraSansString}Cam Mód");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "SET_DATA_SLOT");
          ScaleformMovieMethodAddParamInt(6);
          PushScaleformMovieMethodParameterString(GetControlInstructionalButton(0, 344, 1));
          PushScaleformMovieMethodParameterString($"{FontsManager.FiraSansString}Vypnout");
          EndScaleformMovieMethod();

          BeginScaleformMovieMethod(Scale, "DRAW_INSTRUCTIONAL_BUTTONS");
          ScaleformMovieMethodAddParamInt(0);
          EndScaleformMovieMethod();

          DrawScaleformMovieFullscreen(Scale, 255, 255, 255, 255, 0);
        }

        var noclipEntity = Game.PlayerPed.IsInVehicle() ? Game.PlayerPed.CurrentVehicle.Handle : Game.PlayerPed.Handle;

        FreezeEntityPosition(noclipEntity, true);
        SetEntityInvincible(noclipEntity, true);

        Vector3 newPos;
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
        Game.DisableControlThisFrame(0, Control.VehicleHeadlight);
        if (Game.PlayerPed.IsInVehicle())
          Game.DisableControlThisFrame(0, Control.VehicleRadioWheel);

        var yoff = 0.0f;
        var zoff = 0.0f;

        if (Game.CurrentInputMode == InputMode.MouseAndKeyboard && UpdateOnscreenKeyboard() != 0 && !Game.IsPaused)
        {
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
            yoff = 0.5f;
          }
          if (Game.IsDisabledControlPressed(0, Control.MoveDownOnly))
          {
            yoff = -0.5f;
          }
          if (!FollowCamMode && Game.IsDisabledControlPressed(0, Control.MoveLeftOnly))
          {
            SetEntityHeading(Game.PlayerPed.Handle, GetEntityHeading(Game.PlayerPed.Handle) + 3f);
          }
          if (!FollowCamMode && Game.IsDisabledControlPressed(0, Control.MoveRightOnly))
          {
            SetEntityHeading(Game.PlayerPed.Handle, GetEntityHeading(Game.PlayerPed.Handle) - 3f);
          }
          if (Game.IsDisabledControlPressed(0, Control.Cover))
          {
            zoff = 0.21f;
          }
          if (Game.IsDisabledControlPressed(0, Control.MultiplayerInfo))
          {
            zoff = -0.21f;
          }
          if (Game.IsDisabledControlJustPressed(0, Control.VehicleHeadlight))
          {
            FollowCamMode = !FollowCamMode;
          }
        }

        float moveSpeed = MovingSpeed * MovingSpeed + 1;
        moveSpeed = moveSpeed / (1f / GetFrameTime()) * 60;
        newPos = GetOffsetFromEntityInWorldCoords(noclipEntity, 0f, yoff * (moveSpeed + 0.3f), zoff * (moveSpeed + 0.3f));

        var heading = GetEntityHeading(noclipEntity);
        SetEntityVelocity(noclipEntity, 0f, 0f, 0f);
        SetEntityRotation(noclipEntity, 0f, 0f, 0f, 0, false);
        SetEntityHeading(noclipEntity, FollowCamMode ? GetGameplayCamRelativeHeading() : heading);
        SetEntityCollision(noclipEntity, false, false);
        SetEntityCoordsNoOffset(noclipEntity, newPos.X, newPos.Y, newPos.Z, true, true, true);

        SetEntityVisible(noclipEntity, false, false);
        SetLocalPlayerVisibleLocally(true);
        SetEntityAlpha(noclipEntity, (int)(255 * 0.2), 0);

        SetEveryoneIgnorePlayer(Game.PlayerPed.Handle, true);
        SetPoliceIgnorePlayer(Game.PlayerPed.Handle, true);

        // After the next game tick, reset the entity properties.
        await Delay(0);
        FreezeEntityPosition(noclipEntity, false);
        if (!Misc.IsPlayerGodModeEnabled)
          SetEntityInvincible(noclipEntity, false);
        SetEntityCollision(noclipEntity, true, true);

        // If the player is not set as invisible by PlayerOptions or if the noclip entity is not the player ped, reset the visibility
        SetEntityVisible(noclipEntity, true, false);
        SetLocalPlayerVisibleLocally(true);

        // Always reset the alpha.
        ResetEntityAlpha(noclipEntity);

        SetEveryoneIgnorePlayer(Game.PlayerPed.Handle, false);
        SetPoliceIgnorePlayer(Game.PlayerPed.Handle, false);
      }

      await Task.FromResult(0);
    }
  }
}
