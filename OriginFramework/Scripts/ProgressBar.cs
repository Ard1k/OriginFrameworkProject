using CitizenFX.Core;
using CitizenFX.Core.UI;
using OriginFramework.Menus;
using OriginFrameworkData;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
  public class ProgressBar : BaseScript
  {
    private static string _text = null;
    private static int _showFor = 0;
    private static int _elapsed = 0;
    private static int _startedAt = 0;
    private static Action _callback = null;
    private static AnimationBag _anim = null;
    private static bool _canBeCancelled = false;

    public float yPos = 0.85f;
    public float xPos = 0.5f;
    public float width = 0.15f;
    public float height = 0.025f;
    public float textScale = 0.3f;

    public static bool IsActive { get { return _showFor > 0; } }

    public ProgressBar()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      RegisterCommand("pbtest", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        if (args == null || args.Count <= 0)
          return;

        ProgressBar.Start(Int32.Parse(args[0].ToString()), args[1].ToString(), true, null);
      }), false);

      RegisterCommand("anim", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        if (args == null || args.Count <= 0)
          return;

        ProgressBar.Start(Int32.Parse(args[0].ToString()), args[1].ToString(), true, null, new AnimationBag { AnimDict = args[1].ToString(), AnimName = args[2].ToString(), Time = Int32.Parse(args[0].ToString()) });
      }), false);

      Tick += OnTick;
    }

    private async Task OnTick()
    {
      if (IsActive)
      {
        Game.DisableControlThisFrame(0, Control.FrontendPauseAlternate);

        _elapsed = GetGameTimer() - _startedAt;

        DrawRect(xPos, yPos, width, height, 0, 0, 0, 200);
        if (!string.IsNullOrEmpty(_text))
          TextUtils.DrawTextOnScreen($"~h~{_text}", xPos, yPos - textScale / TextUtils.TextHalfHConst, textScale, CitizenFX.Core.UI.Alignment.Center);
        DrawRect(xPos, yPos, width * ((float)_elapsed / (float)_showFor), height, 0, 145, 255, 200);

        if (_elapsed >= _showFor)
          Finished();
      }
    }

    private void Finished()
    {
      _text = null;
      _showFor = 0;
      _elapsed = 0;
      _startedAt = 0;
      if (_callback != null)
        _callback();
      _callback = null;
      _anim = null;
    }

    private static void Cancel()
    {
      _text = null;
      _showFor = 0;
      _elapsed = 0;
      _startedAt = 0;
      _callback = null;
      _anim = null;
    }


    public static bool Start(int showFor, string text, bool cancellable, Action callback)
    {
      return Start(showFor, text, cancellable, callback, null);
    }

    public static bool Start(int showFor, string text, bool cancellable, Action callback, AnimationBag anim)
    {
      if (IsActive)
      {
        if (_canBeCancelled)
          Cancel();
        else
          return false;
      }

      _startedAt = GetGameTimer();
      _text = text;
      _callback = callback;
      _showFor = showFor;
      _elapsed = 0;
      _anim = anim;
      _canBeCancelled = cancellable;

      if (anim != null)
        Game.PlayerPed.Task.PlayAnimation(
          _anim.AnimDict, 
          _anim.AnimName, 
          anim.Speed ?? 8f, 
          8f, 
          anim.Time, 
          (anim.IsAllowRotation ? AnimationFlags.AllowRotation : AnimationFlags.None) | (anim.IsUpperBodyOnly ? AnimationFlags.UpperBodyOnly : AnimationFlags.None), 
          0f);

      return true;
    }
  }
}
