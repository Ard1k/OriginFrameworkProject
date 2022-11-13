using CitizenFX.Core;
using Newtonsoft.Json;
using OriginFramework.Menus;
using OriginFrameworkData;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
	public class CharacterCaretaker : BaseScript
	{
    private static bool spawnLock = false;
    private static CharacterBag LoggedCharacter = null;
    private static float[] LoginPedPos = { -241f, -855f, 750f };

    public CharacterCaretaker()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.CharacterCaretaker))
        return;

      Tick += OnTick;
      Tick += ServerSync;

      InternalDependencyManager.Started(eScriptArea.CharacterCaretaker);
    }

    private async Task OnTick()
		{
      if (!Login.IsInLoginScreen && !CharacterCreator.IsInCharacterCreator && LoggedCharacter == null)
      {
        FirstSpawn();
        Login.ReturnToLogin();
        return;
      }

      if (NetworkIsPlayerActive(Game.Player.Handle) && LoggedCharacter != null)
      {
        if (LoggedCharacter.IsDead == true)
        {
          if (LoggedCharacter.DiedGameTime == null)
            LoggedCharacter.DiedGameTime = GetGameTimer();
          else
          {
            if (GetGameTimer() - LoggedCharacter.DiedGameTime > 2000)
            {
              ReviveCharacter();
            }
          }
        }

        if (Game.PlayerPed.IsDead && !LoggedCharacter.IsDead)
        {
          LoggedCharacter.IsDead = true;
          LoggedCharacter.DiedGameTime = GetGameTimer();
          LoggedCharacter.DiedServerTime = DateTime.Now;
        }
      }

      await Delay(100);
    }

    private async Task ServerSync()
    {
      await Delay(5000);

      if (LoggedCharacter == null || spawnLock == true)
        return;

      if (LoggedCharacter.LastKnownPos == null)
        LoggedCharacter.LastKnownPos = new PlayerPosBag();

      LoggedCharacter.LastKnownPos.X = Game.PlayerPed.Position.X;
      LoggedCharacter.LastKnownPos.Y = Game.PlayerPed.Position.Y;
      LoggedCharacter.LastKnownPos.Z = Game.PlayerPed.Position.Z;
      LoggedCharacter.LastKnownPos.H = Game.PlayerPed.Heading;

      TriggerServerEvent("ofw_char_caretaker:UpdateCharacterServer", JsonConvert.SerializeObject(LoggedCharacter));
    }

    private void ReviveCharacter()
    {
      //TODO SERVERSIDE!!!
      if (LoggedCharacter != null)
      {
        LoggedCharacter.IsDead = false;
        LoggedCharacter.DiedGameTime = null;
        LoggedCharacter.DiedServerTime = null;
      }

      NetworkResurrectLocalPlayer(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, Game.PlayerPed.Heading, true, true);
    }

    [EventHandler("onClientMapStart")]
    private async void OnMapStart()
    {
      FirstSpawn();
    }

    [EventHandler("ofw_char_caretaker:ForceRelogin")]
    private async void ForceRelogin()
    {
      Notify.Error("Chyba synchronizace se serverem!");

      FirstSpawn();
      Login.ReturnToLogin();
      return;
    }

    private static void FreezePlayer(bool isFreeze)
    {
      SetPlayerControl(Game.Player.Handle, !isFreeze, 0);

      if (isFreeze)
      {
        if (IsEntityVisible(Game.PlayerPed.Handle))
          SetEntityVisible(Game.PlayerPed.Handle, false, false);

        SetEntityCollision(Game.PlayerPed.Handle, false, false);
        FreezeEntityPosition(Game.PlayerPed.Handle, true);
        SetPlayerInvincible(Game.Player.Handle, true);
      }
      else
      {
        if (!IsEntityVisible(Game.PlayerPed.Handle))
          SetEntityVisible(Game.PlayerPed.Handle, true, false);

        SetEntityCollision(Game.PlayerPed.Handle, true, false);
        FreezeEntityPosition(Game.PlayerPed.Handle, false);
        SetPlayerInvincible(Game.Player.Handle, false);
      }
    }

    private async void FirstSpawn()
    {
      if (spawnLock == true)
        return;

      spawnLock = true;

      uint defaultModel = (uint)GetHashKey("player_two");

      RequestModel(defaultModel);

      while (!HasModelLoaded(defaultModel))
      {
        RequestModel(defaultModel);

        await Delay(0);
      }

      SetPlayerModel(Game.Player.Handle, defaultModel);
      SetModelAsNoLongerNeeded(defaultModel);

      DoScreenFadeOut(500);
      while (!IsScreenFadedOut())
        await Delay(0);

      FreezePlayer(true);

      RequestCollisionAtCoord(LoginPedPos[0], LoginPedPos[1], LoginPedPos[2]);

      SetEntityCoordsNoOffset(Game.PlayerPed.Handle, LoginPedPos[0], LoginPedPos[1], LoginPedPos[2], false, false, false);
      NetworkResurrectLocalPlayer(LoginPedPos[0], LoginPedPos[1], LoginPedPos[2], 0.0f, true, true);

      ClearPedTasksImmediately(Game.PlayerPed.Handle);
      //RemoveAllPedWeapons(Game.PlayerPed.Handle, true);
      //ClearPlayerWantedLevel(Game.Player.Handle);

      int time = GetGameTimer();

      while (!HasCollisionLoadedAroundEntity(Game.PlayerPed.Handle) && (GetGameTimer() - time) < 5000)
        await Delay(0);

      ShutdownLoadingScreen();

      if (IsScreenFadedOut())
        DoScreenFadeIn(500);

      while (!IsScreenFadedIn())
        await Delay(0);

      Login.ReturnToLogin();
      spawnLock = false;
    }

    private static async void RespawnCharacter()
    {
      if (spawnLock == true)
        return;

      spawnLock = true;

      if (LoggedCharacter?.Model != null && LoggedCharacter.Model > 0 && Game.PlayerPed.Model.Hash != LoggedCharacter.Model)
      {
        uint defaultModel = (uint)GetHashKey("player_two");

        RequestModel(defaultModel);

        while (!HasModelLoaded(defaultModel))
        {
          RequestModel(defaultModel);

          await Delay(0);
        }

        SetPlayerModel(Game.Player.Handle, defaultModel);
        SetModelAsNoLongerNeeded(defaultModel);
      }

      DoScreenFadeOut(500);
      while (!IsScreenFadedOut())
        await Delay(0);

      FreezePlayer(true);

      float x = 358.2862f, y = -593.026f, z = 28.2662f, h = 245.8211f;

      if (LoggedCharacter?.LastKnownPos != null)
      {
        x = LoggedCharacter.LastKnownPos.X;
        y = LoggedCharacter.LastKnownPos.Y;
        z = LoggedCharacter.LastKnownPos.Z;
        h = LoggedCharacter.LastKnownPos.H;
      }

      RequestCollisionAtCoord(x, y, z);

      SetEntityCoordsNoOffset(Game.PlayerPed.Handle, x, y, z, false, false, false);
      NetworkResurrectLocalPlayer(x, y, z, h, true, true);

      ClearPedTasksImmediately(Game.PlayerPed.Handle);

      int time = GetGameTimer();

      while (!HasCollisionLoadedAroundEntity(Game.PlayerPed.Handle) && (GetGameTimer() - time) < 5000)
        await Delay(0);

      FreezePlayer(false);

      if (IsScreenFadedOut())
        DoScreenFadeIn(500);

      while (!IsScreenFadedIn())
        await Delay(0);

      spawnLock = false;
    }

    public static void LoggedIn(CharacterBag character)
    {
      LoggedCharacter = character;
      RespawnCharacter();
    }
  }
}
