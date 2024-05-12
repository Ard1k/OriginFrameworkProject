using CitizenFX.Core;
using CitizenFX.Core.UI;
using OriginFramework.Menus;
using OriginFrameworkData;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
  public enum eMiniGameType
  {
    Wsad,
    VehicleTuning
  }

  public enum eMiniGameStatus
  {
    Running,
    Success,
    Fail,
    Cancelled
  }

  public interface IMiniGame
  {
    eMiniGameType Type { get; }
    eMiniGameStatus Tick();
  }

  public class MiniGame : BaseScript
  {
    private static int _startedAt = 0;
    private static Action _successCallback = null;
    private static Func<Task<bool>> _asyncSuccessCallback = null;
    private static Action _failCallback = null;
    private static Func<Task<bool>> _asyncFailCallback = null;
    private static bool _canBeCancelled = false;

    public static IMiniGame CurrentGame { get; private set; } = null;
    public static eMiniGameStatus LastStatus { get; private set; } = eMiniGameStatus.Running;
    public static bool IsActive { get { return CurrentGame != null; } }

    public MiniGame()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      RegisterCommand("minitest", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        if (args == null || args.Count <= 0)
          return;

        ProgressBar.Start(Int32.Parse(args[0].ToString()), args[1].ToString(), true, null);
      }), false);

      Tick += OnTick;
    }

    private async Task OnTick()
    {
      if (IsActive)
      {
        Game.DisableControlThisFrame(0, Control.FrontendPauseAlternate);

        if (Game.IsControlJustPressed(0, Control.FrontendPauseAlternate) == true) //todo cancellable
        {
          Cancel();
        }

        LastStatus = CurrentGame.Tick();

        if (LastStatus != eMiniGameStatus.Running)
        {
          switch (LastStatus)
          {
            case eMiniGameStatus.Success:
              _successCallback?.Invoke();
              if (_asyncSuccessCallback != null)
                await _asyncSuccessCallback();
              break;
            case eMiniGameStatus.Fail:
              _failCallback?.Invoke();
              if (_asyncFailCallback != null)
                await _asyncFailCallback();
              break;
          }

          _successCallback = null;
          _asyncSuccessCallback = null;
          _failCallback = null;
          _asyncFailCallback = null;
          CurrentGame = null;
        }
      }
      else
      {
        await Delay(250);
      }
    }

    private void Finished()
    {
      _startedAt = 0;
    }

    private static void Cancel()
    {
      _startedAt = 0;
    }

    protected static bool Start(IMiniGame game, Action successCallback, Func<Task<bool>> asyncSuccessCallback, Action failCallback, Func<Task<bool>> asyncFailCallback, bool cancellable = true)
    {
      if (IsActive)
      {
        if (_canBeCancelled)
          Cancel();
        else
          return false;
      }

      _startedAt = GetGameTimer();
      _canBeCancelled = cancellable;
      _successCallback = successCallback;
      _asyncSuccessCallback = asyncSuccessCallback;
      _failCallback = failCallback;
      _asyncFailCallback = asyncFailCallback;

      CurrentGame = game;

      return true;
    }

    public static bool StartVehicleTuning(Func<Task<bool>> asyncSuccessCallback, int vehId)
    {
      if (IsActive && _canBeCancelled == false)
        return false;

      var game = new MiniGameVehicleTuning(vehId);
      return Start(game, null, asyncSuccessCallback, null, null);
    }
  }

  public class MiniGameWsad : IMiniGame
  {
    public eMiniGameType Type { get { return eMiniGameType.Wsad; } }

    public eMiniGameStatus Tick()
    {
      if (Game.IsControlJustPressed(0, Control.MoveUp))
      {
        return eMiniGameStatus.Success;
      }

      return eMiniGameStatus.Running;
    }
  }

  public class MiniGameVehicleTuning : IMiniGame
  {
    private int vehId = 0;

    public MiniGameVehicleTuning(int vehId)
    {
      this.vehId = vehId;
    }

    public eMiniGameType Type { get { return eMiniGameType.VehicleTuning; } }

    public eMiniGameStatus Tick()
    {
      if (Game.IsControlJustPressed(0, Control.PhoneCameraGrid)) //G
      {
        return eMiniGameStatus.Success;
      }
      return eMiniGameStatus.Running;
    }

    private void HandleInput()
    {

    }

    private void Draw()
    {
      //todo
    }
  }
}
