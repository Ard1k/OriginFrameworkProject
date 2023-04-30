using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
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
    public static CharacterBag LoggedCharacter { get; private set; } = null;
    private static float[] LoginPedPos = { -241f, -855f, 750f };
    public static CharacterCaretaker Instance { get; private set; } = null;
    public static List<Tuple<Vector3, float>> FirstSpawns = new List<Tuple<Vector3, float>>
    {
      new Tuple<Vector3, float>(new Vector3(473.85f, -1154.63f, 28.68f), 89.5f),
      new Tuple<Vector3, float>(new Vector3(180.11f, -1014.34f, 28.45f), 28.5f),
      new Tuple<Vector3, float>(new Vector3(28.8f, -1029.4f, 28.59f), 252.5f),
      new Tuple<Vector3, float>(new Vector3(-20.68f, -225.76f, 45.32f), 150.5f),
      new Tuple<Vector3, float>(new Vector3(-163.34f, -31.13f, 51.87f), 158.5f),
      new Tuple<Vector3, float>(new Vector3(-70.14f, 91.46f, 72.28f), 246.5f),
      new Tuple<Vector3, float>(new Vector3(-310.07f, -1363.99f, 30.44f), 261.5f),
      new Tuple<Vector3, float>(new Vector3(-333.55f, -1401.13f, 29.78f), 192.5f),
      new Tuple<Vector3, float>(new Vector3(28.13f, -1585.35f, 28.34f), 230.5f),
      new Tuple<Vector3, float>(new Vector3(234.68f, -1773.89f, 27.82f), 203.5f),
      new Tuple<Vector3, float>(new Vector3(213.79f, -1870.22f, 25.74f), 231.5f),
      new Tuple<Vector3, float>(new Vector3(506.96f, -1498.2f, 28.44f), 156.5f),
      new Tuple<Vector3, float>(new Vector3(408.97f, -644.14f, 27.65f), 266.5f),
      new Tuple<Vector3, float>(new Vector3(-487.73f, -608.48f, 31.16f), 179.0f),
      new Tuple<Vector3, float>(new Vector3(-487.62f, -614.41f, 31.17f), 1.5f),
      new Tuple<Vector3, float>(new Vector3(-470.98f, -613.06f, 31.17f), 181.5f),
      new Tuple<Vector3, float>(new Vector3(-464.04f, -623.69f, 31.17f), 179.0f),
      new Tuple<Vector3, float>(new Vector3(-759.7f, -905.78f, 19.74f), 91.0f),
      new Tuple<Vector3, float>(new Vector3(-759.34f, -897.24f, 20.26f), 93.5f),
      new Tuple<Vector3, float>(new Vector3(-751.55f, -1041.05f, 12.65f), 118.0f),
      new Tuple<Vector3, float>(new Vector3(-738.22f, -1033.17f, 12.73f), 120.5f),
      new Tuple<Vector3, float>(new Vector3(-727.66f, -1060.62f, 12.35f), 29.5f),
      new Tuple<Vector3, float>(new Vector3(-754.53f, -1061.9f, 11.94f), 208.5f),
      new Tuple<Vector3, float>(new Vector3(-754.74f, -1077.93f, 11.79f), 26.5f),
      new Tuple<Vector3, float>(new Vector3(-812.54f, -1100.98f, 10.83f), 297.5f),
      new Tuple<Vector3, float>(new Vector3(-817.36f, -1094.71f, 10.91f), 304.5f),
      new Tuple<Vector3, float>(new Vector3(-239.62f, -1165.85f, 23.0f), 273.0f),
      new Tuple<Vector3, float>(new Vector3(-237.98f, -1178.47f, 22.99f), 325.0f),
      new Tuple<Vector3, float>(new Vector3(139.81f, -1080.72f, 29.19f), 0.5f),
      new Tuple<Vector3, float>(new Vector3(151.07f, -1082.04f, 29.19f), 357.5f)
    };

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
      Instance = this;

      await Delay(2000);

      //tohle pomuze pri restartu resource misto eventu onClientMapStart
      if (!Login.IsInLoginScreen && !CharacterCreator.IsInCharacterCreator && LoggedCharacter == null)
      {
        Debug.WriteLine("force relogin client");
        LoginSpawn();
        Login.ReturnToLogin();
        return;
      }
    }

    private async Task OnTick()
		{
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
      LoginSpawn();
      Login.ReturnToLogin();
    }


    [EventHandler("ofw_char_caretaker:UpdateOrganization")]
    private async void UpdateOrganization(int? organizationId)
    {
      if (LoggedCharacter == null)
        return;

      LoggedCharacter.OrganizationId = organizationId;

      if (organizationId != null)
        Notify.Alert("Přidal ses k organizaci");
      else
        Notify.Alert("Organizace opuštěna");

      TriggerServerEvent("ofw_org:RequestOrganizationData");
    }

    [EventHandler("ofw_char_caretaker:ForceRelogin")]
    private async void ForceRelogin()
    {
      Notify.Error("Chyba synchronizace se serverem!");

      LoginSpawn();
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

    public async Task LoginSpawn(bool fadeOut = true)
    {
      if (spawnLock == true)
        return;

      spawnLock = true;

      DoScreenFadeOut(500);
      while (!IsScreenFadedOut())
        await Delay(0);

      TriggerServerEvent("ofw_instance:TransferToPrivateInstance");

      uint defaultModel = (uint)GetHashKey("player_two");

      RequestModel(defaultModel);

      while (!HasModelLoaded(defaultModel))
      {
        RequestModel(defaultModel);

        await Delay(0);
      }

      SetPlayerModel(Game.Player.Handle, defaultModel);
      SetModelAsNoLongerNeeded(defaultModel);

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

      if (fadeOut)
      {
        if (IsScreenFadedOut())
          DoScreenFadeIn(500);

        while (!IsScreenFadedIn())
          await Delay(0);
      }

      spawnLock = false;
    }

    private static async Task<bool> SpawnLoggedCharacter(bool isFirstSpawn)
    {
      if (spawnLock == true)
        return false;

      spawnLock = true;
      TheBugger.DebugLog(LoggedCharacter?.Model.ToString() ?? "null char");
      if (LoggedCharacter?.Model != null && LoggedCharacter.Model != 0 && Game.PlayerPed.Model.Hash != LoggedCharacter.Model)
      {
        uint model = (uint)LoggedCharacter.Model;
        
        RequestModel(model);

        while (!HasModelLoaded(model))
        {
          RequestModel(model);

          await Delay(0);
        }

        SetPlayerModel(Game.Player.Handle, model);
        SetPedDefaultComponentVariation(Game.PlayerPed.Handle);
        SetModelAsNoLongerNeeded(model);
      }

      SkinManager.SetDefaultSkin(SkinManager.ClothesAll);
      SkinManager.ApplySkin(LoggedCharacter?.Skin);
      TriggerServerEvent("ofw_inventory:ReloadInventory", null);
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
      else
      {
        var rand = new Random();
        var spawn = FirstSpawns[rand.Next(FirstSpawns.Count)];

        x = spawn.Item1.X;
        y = spawn.Item1.Y;
        z = spawn.Item1.Z;
        h = spawn.Item2;
      }

      SetEntityCoordsNoOffset(Game.PlayerPed.Handle, x, y, z, false, false, false);
      await Delay(0);

      if (Misc.IsInCaioPericoRange(Game.PlayerPed.Position) && !Misc.IslandLoaded)
      {
        while (!Misc.IslandLoaded)
          await Delay(0);
      }

      RequestCollisionAtCoord(x, y, z);

      //SetEntityCoordsNoOffset(Game.PlayerPed.Handle, x, y, z, false, false, false);
      NetworkResurrectLocalPlayer(x, y, z, h, true, true);

      ClearPedTasksImmediately(Game.PlayerPed.Handle);

      int time = GetGameTimer();

      while (!HasCollisionLoadedAroundEntity(Game.PlayerPed.Handle) && (GetGameTimer() - time) < 5000)
        await Delay(0);

      FreezePlayer(false);

      if (!isFirstSpawn)
      {
        if (IsScreenFadedOut())
          DoScreenFadeIn(500);

        while (!IsScreenFadedIn())
          await Delay(0);
      }

      spawnLock = false;
      return true;
    }

    public static async void LoggedIn(CharacterBag character)
    {
      LoggedCharacter = character;
      TriggerServerEvent("ofw_instance:TransferToPublicInstance");
      if (LoggedCharacter.IsNew)
      {
        await SpawnLoggedCharacter(true);

        await Delay(1000);
        await GarageClient.TakeOutFirstVehicle(Game.PlayerPed.Position, Game.PlayerPed.Heading);

        if (IsScreenFadedOut())
          DoScreenFadeIn(500);

        TransitionToBlurred(1);

        ShakeGameplayCam("DRUNK_SHAKE", 2.0f);

        SetEntityInvincible(Game.Player.Character.Handle, true);

        await Delay(1000);

        int time = Game.GameTime;

        while (Game.PlayerPed.CurrentVehicle == null && (Game.GameTime - time) < 10000)
        {
          await Delay(100);
        }

        if (Game.PlayerPed.CurrentVehicle != null)
        {
          TaskSequence taskSequence = new TaskSequence();
          RequestAnimDict("rcmnigel2");
          while (!HasAnimDictLoaded("rcmnigel2"))
          {
            await Delay(50);
          }
          TaskPlayAnim(Game.PlayerPed.Handle, "rcmnigel2", "die_horn", 1.0f, 1.0f, 5000, 0, 1f, false, false, false);
        }

        await Delay(3000);

        StopGameplayCamShaking(false);

        await Delay(2000);

        Notify.Alert("Vzbudil ses v autě... už zase.");
        TransitionFromBlurred(1000f);

        await Delay(3000);
        Notify.Alert("Jen ty víš, co se dělo včera.");

        SetEntityInvincible(Game.Player.Character.Handle, false);
      }
      else
      {
        SpawnLoggedCharacter(false);
      }
      TriggerServerEvent("ofw_org:RequestOrganizationData");
    }
  }
}
