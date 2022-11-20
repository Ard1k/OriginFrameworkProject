using CitizenFX.Core;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.UI.Screen;

namespace OriginFramework
{
  public class TextUtils
  {
    public static string[] StringToArray(string inputString)
    {
      return CitizenFX.Core.UI.Screen.StringToArray(inputString);
    }

    public const float TxtHConst = 14f;
    public const float TextHalfHConst = 28f;


    #region Draw text
    public static void DrawTextOnScreen(string text, float xPosition, float yPosition) =>
        DrawTextOnScreen(text, xPosition, yPosition, size: 0.48f);

    public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size) =>
        DrawTextOnScreen(text, xPosition, yPosition, size, CitizenFX.Core.UI.Alignment.Left);

    public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification) =>
        DrawTextOnScreen(text, xPosition, yPosition, size, justification, 6);

    public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font) =>
        DrawTextOnScreen(text, xPosition, yPosition, size, justification, font, false);

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
        AddTextComponentSubstringPlayerName(FontsManager.FiraSansString + text);
        EndTextCommandDisplayText(xPosition, yPosition);
      }
    }
    public static void DrawCenterScreenText(string text, int red, int green, int blue, int alpha)
    {
      SetTextFont(4);
      SetTextScale(0.0f, 0.5f);
      SetTextColour(red, green, blue, alpha);
      SetTextDropshadow(0, 0, 0, 0, 255);
      SetTextEdge(1, 0, 0, 0, 255);
      SetTextDropShadow();
      SetTextOutline();
      SetTextCentre(true);
      BeginTextCommandDisplayText("STRING");
      AddTextComponentSubstringPlayerName(text);
      EndTextCommandDisplayText(0.5f, 0.5f);
    }

    public static void Draw3dTextNonPrespective(Vector3 pos, float zOffset, float scale, string text, int r, int g, int b, int a)
    {
      float _x = 0, _y = 0;
      World3dToScreen2d(pos.X, pos.Y, pos.Z + zOffset, ref _x, ref _y);

      SetTextScale(1, scale);
      SetTextFont(0);
      SetTextProportional(true);
      SetTextColour(r, g, b, a);
      SetTextOutline();
      SetTextEntry("STRING");
      SetTextCentre(true);
      AddTextComponentString(FontsManager.FiraSansString + text);
      DrawText(_x, _y);
    }

    public static float GetScaledStringFiraSansWidth(string text, float scale)
    {
      return (Text.GetScaledStringWidth(FontsManager.FiraSansString + text, 0, scale) / Screen.ScaledWidth) * 1.1f;
    }

    public static int GetTextLinesCount(float x, float y, string text, float scale)
    {
      BeginTextCommandLineCount("STRING");
      AddTextComponentSubstringPlayerName(text);
      SetTextScale(1.0f, scale);
      return EndTextCommandLineCount(x, y);
    }
    #endregion

    public static async Task<string> GetUserInput(string windowTitle, string defaultText, int maxInputLength)
    {
      // Create the window title string.
      var spacer = "\t";
      AddTextEntry($"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", $"{FontsManager.FiraSansString}{windowTitle ?? "Enter"}");

      // Display the input box.
      DisplayOnscreenKeyboard(1, $"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", "", defaultText ?? "", "", "", "", maxInputLength);
      await BaseScript.Delay(0);
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
            await BaseScript.Delay(0);
            break;
        }
      }
    }
  }
}
