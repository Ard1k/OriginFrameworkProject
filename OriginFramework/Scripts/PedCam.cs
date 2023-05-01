using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using OriginFramework.Helpers;
using OriginFramework.Menus;
using OriginFrameworkData.DataBags;
using static CitizenFX.Core.Native.API;

namespace OriginFramework.Scripts
{
  public class PedCam : BaseScript
  {
    private static float _zoom = 1f;
    private static float _zOffset = 0f;
    private static int _camId = 0;
    private static bool _isStatic = false;
    private static float _angleRadY = 0.0f;
    private static float _angleRadZ = 0.0f;
    private static float _fwdHeadingRad = 0.0f;
    private static float _limitRot = 110f;

    public PedCam()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      Tick += OnTick;
    }

    private async Task OnTick()
    {
      if (_camId == 0)
        return;

      ProcessCamControls();
    }

    public static void StartCam(float limitRot = 110f)
    {
      if (_camId != 0)
        return;

      ClearFocus();

      _camId = CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, 0f, 0f, 0f, GetGameplayCamFov(), true, 0);

      SetCamActive(_camId, true);
      RenderScriptCams(true, true, 1000, true, false);

      _fwdHeadingRad = (GetEntityHeading(Game.PlayerPed.Handle) + 90f) * (float)(Math.PI / 180f);
      _angleRadY = 0f;
      _angleRadZ = _fwdHeadingRad;

      _limitRot = limitRot;
    }

    public static void EndCam()
    {
      if (_camId == 0)
        return;

      ClearFocus();

      RenderScriptCams(false, false, 0, true, false);
      DestroyCam(_camId, false);

      _camId = 0;
    }

    private Vector3 GetPlayerCoords()
    {
      Vector3 coords = Game.PlayerPed.Position;

      return coords + new Vector3(0.0f, 0.0f, _zOffset);
    }

    public static void SetCamZoomAndOffset(float zoom, float zOffset, float limitRot)
    {
      _zoom = zoom;
      _zOffset = zOffset;
      _limitRot = limitRot;
    }

    public static void ResetForward()
    {
      _angleRadZ = _fwdHeadingRad;
      _angleRadY = 0f;
    }

    private void ProcessCamControls()
    {
      int playerPed = PlayerPedId();
      Vector3 playerCoords = GetPlayerCoords();

      // disable 1st person as the 1st person camera can cause some glitches
      DisableFirstPersonCamThisFrame();

      // calculate new position
      Vector3 newPos = ProcessNewPosition();

      if (newPos != null)
      {
        // focus moveableCam area
        SetFocusArea(newPos.X, newPos.Y, newPos.Z, 0.0f, 0.0f, 0.0f);

        // set coords of moveableCam
        SetCamCoord(_camId, newPos.X, newPos.Y, newPos.Z);

        // set rotation
        PointCamAtCoord(_camId, playerCoords.X, playerCoords.Y, playerCoords.Z);
      }
    }

    private Vector3 ProcessNewPosition()
    {
      float mouseX = 0.0f;
      float mouseY = 0.0f;

      // keyboard
      if (!_isStatic)
      {
        if (IsInputDisabled(0))
        {
          // rotation
          mouseX = GetDisabledControlNormal(1, 1) * 8f;
          mouseY = GetDisabledControlNormal(1, 2) * 8f;

          // controller
        }
        else
        {
          // rotation
          mouseX = GetDisabledControlNormal(1, 1) * 1.5f;
          mouseY = GetDisabledControlNormal(1, 2) * 1.5f;
        }
      }

      float newAngleRadZ = _angleRadZ - mouseX * (float)(Math.PI / 180f); // around Z axis (left / right)
      _angleRadY += mouseY * (float)(Math.PI / 180f); // up / down

      float fwdPrev = Math.Abs(_fwdHeadingRad - _angleRadZ);
      float fwd = Math.Abs(_fwdHeadingRad - newAngleRadZ);

      float limit = _limitRot * (float)(Math.PI / 180f);
      if (fwd < fwdPrev || fwd < limit)
      {
        _angleRadZ = newAngleRadZ;
      }

      // limit up / down angle to 90°
      if (_angleRadY > 50.0f * (float)(Math.PI / 180f))
      {
        _angleRadY = 50.0f * (float)(Math.PI / 180f);
      }
      else if (_angleRadY < -15.0f * (float)(Math.PI / 180f))
      {
        _angleRadY = -15.0f * (float)(Math.PI / 180f);
      }

      Vector3 pCoords = GetPlayerCoords();

      float maxRadius = _zoom;
      Vector3 offset = new Vector3(
      (float)((Math.Cos(_angleRadZ) * Math.Cos(_angleRadY)) + (Math.Cos(_angleRadY) * Math.Cos(_angleRadZ))) / 2 * maxRadius,
      (float)((Math.Sin(_angleRadZ) * Math.Cos(_angleRadY)) + (Math.Cos(_angleRadY) * Math.Sin(_angleRadZ))) / 2 * maxRadius,
      (float)((Math.Sin(_angleRadY))) * maxRadius
      );

      Vector3 pos = new Vector3(
          pCoords.X + offset.X,
          pCoords.Y + offset.Y,
          pCoords.Z + offset.Z
      );

      return pos;
    }
  }
}
