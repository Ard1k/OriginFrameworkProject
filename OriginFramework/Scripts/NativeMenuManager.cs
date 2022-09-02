using CitizenFX.Core;
using MenuAPI;
using Newtonsoft.Json;
using OriginFramework.Menus;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
  public class NativeMenuManager : BaseScript
  {
    public static bool IsHidden { get; set; } = true;
    public static NativeMenu CurrentMenu { get; set; }
    public static string CurrentMenuName { get; set; }

    private static float xOffset = 0.7f;
    private static float xLenght = 0.15f;
    private static float xExtraLenght = 0.11f;

    private static float yOffset = 0.25f;
    private static float yHeight = 0.03f;
    private static float spacing = 0.0f;

    private static float textScale = 0.36f;
    private static float textYOffset = textScale / 28f;
    private static float textMarginName = 0.004f;
    private static float textMarginNameRight = xLenght * (2f / 3f);
    private static float textMarginExtraLeft = textMarginName;
    private static float textMarginExtraRight = xExtraLenght * (2f / 3f);
    private static float textMarginMenuTitle = textMarginName;

    private static float iconScaleFactor = 0.75f;

    private int control_nav_back = (int)Control.FrontendRright;
    private int control_nav_hide = (int)Control.FrontendPauseAlternate;
    private int control_nav_down = (int)Control.PhoneDown;
    private int control_nav_up = (int)Control.Phone;
    private int control_nav_select = (int)Control.FrontendAccept;
    private int control_nav_select2 = -1;

    private float block_back_timer = 0f;

    #region Tick tasks
    private async Task HandleMenu()
    {
      UpdateSelection();
      RenderMenu();
    }
		#endregion

		#region event handlers
    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.NativeMenuManager))
        return;

      Tick += HandleMenu;

      InternalDependencyManager.Started(eScriptArea.NativeMenuManager);
    }

    #endregion

    #region private metody
    private async void UpdateSelection()
    {
      if (IsHidden)
      {
        if (block_back_timer > 0)
        {
          DisableControlAction(0, control_nav_hide, true);
          block_back_timer -= GetFrameTime();
        }

        return;
      }

      DisableControlAction(0, control_nav_back, true);
      DisableControlAction(0, control_nav_hide, true);

      if (IsDisabledControlJustPressed(0, control_nav_hide))
      {
        block_back_timer = 0.5f;
        IsHidden = true;
        return;
      }

      bool autoPressSelect = false;
      if (IsDisabledControlJustPressed(0, control_nav_back))
      {
        BackOrClose();
        return;

        // Nepamatuju si k cemu sem to kdysi potreboval, ale je to stupidni

        //bool foundBackKey = false;

        //foreach (var it in CurrentMenu.Items)
        //{
        //  if (it.IsBack || it.IsNavBack)
        //  {
        //    foundBackKey = true;
        //    break;
        //  }
        //}

        //if (foundBackKey)
        //{
        //  while (CurrentMenu.Items[CurrentMenu.SelectedIndex].IsBack != true && CurrentMenu.Items[CurrentMenu.SelectedIndex].IsNavBack != true)
        //  {
        //    if (CurrentMenu.SelectedIndex < CurrentMenu.Items.Count - 1)
        //    {
        //      CurrentMenu.SelectedIndex++;
        //      if (CurrentMenu.SelectedIndexVisible < (CurrentMenu.MaxMenuItemsVisible - 1))
        //        CurrentMenu.SelectedIndexVisible++;
        //    }
        //    else
        //    {
        //      CurrentMenu.SelectedIndex = 0;
        //      CurrentMenu.SelectedIndexVisible = 0;
        //    }
        //  }

        //  autoPressSelect = true;
        //}
        //else
        //  BackOrClose();
      }

      if (IsControlJustPressed(0, control_nav_down))
      {
        PlaySound(-1, "NAV_UP_DOWN", "HUD_MINI_GAME_SOUNDSET", false, 0, true);
        do
        {
          if (CurrentMenu.SelectedIndex < CurrentMenu.Items.Count - 1)
          {
            CurrentMenu.SelectedIndex++;
            if (CurrentMenu.SelectedIndexVisible < CurrentMenu.MaxMenuItemsVisible - 1)
              CurrentMenu.SelectedIndexVisible++;
          }
          else
          {
            CurrentMenu.SelectedIndex = 0;
            CurrentMenu.SelectedIndexVisible = 0;
          }

          if (CurrentMenu.Items[CurrentMenu.SelectedIndex].OnHover != null)
            CurrentMenu.Items[CurrentMenu.SelectedIndex].OnHover();
        } while (CurrentMenu.Items[CurrentMenu.SelectedIndex].IsUnselectable == true);
      }
      else if (IsControlJustPressed(0, control_nav_up))
      {
        PlaySound(-1, "NAV_UP_DOWN", "HUD_MINI_GAME_SOUNDSET", false, 0, true);
        do
        {
          if (CurrentMenu.SelectedIndex > 0)
          {
            CurrentMenu.SelectedIndex--;
            if (CurrentMenu.SelectedIndexVisible > 0)
              CurrentMenu.SelectedIndexVisible--;
          }
          else
          {
            CurrentMenu.SelectedIndex = CurrentMenu.Items.Count - 1;
            if (CurrentMenu.Items.Count <= CurrentMenu.MaxMenuItemsVisible)
              CurrentMenu.SelectedIndexVisible = CurrentMenu.Items.Count - 1;
            else
              CurrentMenu.SelectedIndexVisible = CurrentMenu.MaxMenuItemsVisible - 1;
          }

          if (CurrentMenu.Items[CurrentMenu.SelectedIndex].OnHover != null)
            CurrentMenu.Items[CurrentMenu.SelectedIndex].OnHover();
        } while (CurrentMenu.Items[CurrentMenu.SelectedIndex].IsUnselectable == true);
      }
      else if (autoPressSelect || IsControlJustPressed(0, control_nav_select) || (control_nav_select2 > 0 && IsControlJustPressed(0, control_nav_select2)))
      {
        PlaySound(-1, "SELECT", "HUD_MINI_GAME_SOUNDSET", false, 0, true);
        bool isBack = CurrentMenu.Items[CurrentMenu.SelectedIndex].IsBack == true;
        bool isHide = CurrentMenu.Items[CurrentMenu.SelectedIndex].IsHide == true;
        Action<NativeMenuItem> funcOnSelected = null;
        var currentItem = CurrentMenu.Items[CurrentMenu.SelectedIndex];

        if (currentItem.OnSelected != null)
          funcOnSelected = currentItem.OnSelected;

        if (currentItem.IsTextInput)
        {
          string prefill = currentItem.TextInputPrefill;

          switch (prefill)
          {
            case "NameLeft": prefill = currentItem.Name; break;
            case "NameRight": prefill = currentItem.NameRight; break;
            case "ExtraLeft": prefill = currentItem.ExtraLeft; break;
            case "ExtraRight": prefill = currentItem.ExtraRight; break;
          }

          AddTextEntry("FMMC_KEY_TIP1", string.Format("{0}{1}[max length:{2}]", FontsManager.FiraSansString, currentItem.TextInputRequest, currentItem.TextInputMaxLength));
          DisplayOnscreenKeyboard(1, "FMMC_KEY_TIP1", "", prefill, "", "", "", currentItem.TextInputMaxLength);

          while (UpdateOnscreenKeyboard() != 1 && UpdateOnscreenKeyboard() != 2)
            await Delay(0);

          if (UpdateOnscreenKeyboard() == 1)
          {
            string result = GetOnscreenKeyboardResult();
            await Delay(50);
            if (currentItem.OnTextInput != null)
              currentItem.OnTextInput(result);
            Refresh();
          }
          else
            await Delay(0);
        }

        if (currentItem.SubMenu != null)
        {
          var prevMenu = CurrentMenu;
          CurrentMenu = currentItem.SubMenu;
          CurrentMenu.PreviousMenu = prevMenu;
          Refresh();
        }

        if (currentItem.GetSubMenu != null)
        {
          var prevMenu = CurrentMenu;
          CurrentMenu = currentItem.GetSubMenu();
          CurrentMenu.PreviousMenu = prevMenu;
          Refresh();
        }

        if (funcOnSelected != null)
        {
          funcOnSelected(currentItem);
        }

        if (isHide)
          IsHidden = true;

        if (isBack)
        {
          BackOrClose();
          return;
        }
      }

      for (int i = 0; i < CurrentMenu.Items.Count; i++)
        CurrentMenu.Items[i].IsActive = (i == CurrentMenu.SelectedIndex);
    }

    private void RenderMenu()
    {
      if (CurrentMenu == null || IsHidden)
        return;

      int screen_w = 0, screen_h = 0;
      GetActiveScreenResolution(ref screen_w, ref screen_h);
      float htow = (float)screen_h / (float)screen_w;
      float wtoh = (float)screen_w / (float)screen_h;

      int indexMin = CurrentMenu.SelectedIndex - CurrentMenu.SelectedIndexVisible;
      int indexMax = CurrentMenu.SelectedIndex + (CurrentMenu.MaxMenuItemsVisible - 1 - CurrentMenu.SelectedIndexVisible);

      if (!HasStreamedTextureDictLoaded("menu_textures"))
        RequestStreamedTextureDict("menu_textures", true);

      if (CurrentMenu.Sprite != null)
      {
        if (!HasStreamedTextureDictLoaded(CurrentMenu.Sprite.TextureDict))
          RequestStreamedTextureDict(CurrentMenu.Sprite.TextureDict, true);

        if (CurrentMenu.Sprite.X <= 0)
          CurrentMenu.Sprite.X = 1;
        if (CurrentMenu.Sprite.Y <= 0)
          CurrentMenu.Sprite.Y = 1;

        float ytox = (float)CurrentMenu.Sprite.Y / (float)CurrentMenu.Sprite.X;
        DrawSprite(CurrentMenu.Sprite.TextureDict, CurrentMenu.Sprite.TextureName, xOffset, yOffset - (spacing / 2f) - (xLenght * ytox * wtoh / 2f) - (yHeight / 2f), xLenght, xLenght * ytox * wtoh, 0, 255, 255, 255, 255);
      }

      if (!String.IsNullOrEmpty(CurrentMenu.MenuTitle))
      {
        float extraYOffset = 0f;
        float scaleModifier = 1f;

        if (CurrentMenu.Sprite == null)
          SetTextColour(0, 0, 0, 255);
        else
        {
          SetTextColour(255, 255, 255, 255);
          scaleModifier = 1.2f;
          extraYOffset = 0.004f;
        }
        
        SetTextScale(textScale * scaleModifier, textScale * scaleModifier);
        SetTextEntry("STRING");
        AddTextComponentString("~h~" + FontsManager.FiraSansString + CurrentMenu.MenuTitle ?? "");
        DrawText(xOffset - xLenght / 2.0f + textMarginMenuTitle, (yOffset - yHeight - (textYOffset * scaleModifier) - extraYOffset));

        if (CurrentMenu.Sprite == null)
          DrawRect(xOffset, yOffset - yHeight, xLenght, yHeight - spacing, 255, 255, 255, 200);
      }

      if (indexMin > 1)
        DrawSprite("menu_textures", "arrow_b256", xOffset - (((yHeight + spacing) / 2.0f) * htow + xLenght / 2.0f), yOffset, (yHeight - spacing) * htow, yHeight - spacing, 180.0f, 255, 255, 255, 150);

      if (indexMax < CurrentMenu.Items.Count - 1)
        DrawSprite("menu_textures", "arrow_b256", xOffset - (((yHeight + spacing) / 2.0f) * htow + xLenght / 2.0f), (yOffset + yHeight * (CurrentMenu.MaxMenuItemsVisible - 1)), (yHeight - spacing) * htow, yHeight - spacing, 0.0f, 255, 255, 255, 150);

      for (int i = 0; i < CurrentMenu.Items.Count; i++)
      {
        if (i >= indexMin && i <= indexMax)
        {
          int[] boxColor = new int[4];

          if (CurrentMenu.Items[i].IsActive)
          {
            if (CurrentMenu.Items[i].ColorOverride != null)
              boxColor = new int[] { CurrentMenu.Items[i].ColorOverride[0], CurrentMenu.Items[i].ColorOverride[1], CurrentMenu.Items[i].ColorOverride[2], 233 };
            else if (CurrentMenu.Items[i].IsColorAsUnavailable)
              boxColor = new int[] { 60, 20, 20, 233 };
            else
              boxColor = new int[] { 13, 11, 10, 233 };
          }
          else
          {
            if (CurrentMenu.Items[i].ColorOverride != null)
              boxColor = new int[] { CurrentMenu.Items[i].ColorOverride[3], CurrentMenu.Items[i].ColorOverride[4], CurrentMenu.Items[i].ColorOverride[5], CurrentMenu.Items[i].IsUnselectable ? 0 : 230 };
            else if (CurrentMenu.Items[i].IsColorAsUnavailable)
              boxColor = new int[] { 120, 45, 45, CurrentMenu.Items[i].IsUnselectable ? 0 : 230 };
            else
              boxColor = new int[] { 45, 45, 45, CurrentMenu.Items[i].IsUnselectable ? 0 : 230 };
          }

          DrawRect(xOffset, yOffset + (yHeight * (i - indexMin)), xLenght, yHeight - spacing, boxColor[0], boxColor[1], boxColor[2], boxColor[3]);
          if (!string.IsNullOrEmpty(CurrentMenu.Items[i].ExtraLeft) || !string.IsNullOrEmpty(CurrentMenu.Items[i].ExtraRight))
            DrawRect(xOffset + (xLenght + xExtraLenght) / 2.0f + spacing, yOffset + (yHeight * (i - indexMin)), xExtraLenght, yHeight - spacing, 255, 255, 255, 199);

          float iconOffset = 0f;
          if (!string.IsNullOrEmpty(CurrentMenu.Items[i].Icon))
          {
            DrawSprite("menu_textures", CurrentMenu.Items[i].Icon, xOffset + (((yHeight - spacing) / 2.0f) * htow) - (xLenght / 2.0f), (yOffset + yHeight * (i - indexMin)), (yHeight - spacing) * iconScaleFactor * htow, (yHeight - spacing) * iconScaleFactor, 0f, 255, 255, 255, 200);
            iconOffset = (yHeight - spacing) * htow - textMarginName;
          }

          if (!string.IsNullOrEmpty(CurrentMenu.Items[i].Name) || CurrentMenu.Items[i].IsActive)
          {
            SetTextFont(4);
            SetTextScale(textScale, textScale);
            SetTextColour(255, 255, 255, 255);
            SetTextEntry("STRING");
            AddTextComponentString((CurrentMenu.Items[i].IsActive ? " " : String.Empty) + FontsManager.FiraSansString + (CurrentMenu.Items[i].Name ?? String.Empty));
            DrawText(xOffset - xLenght / 2.0f + iconOffset + textMarginName, (yOffset + (yHeight * (i - indexMin)) - textYOffset));
          }

          if (!string.IsNullOrEmpty(CurrentMenu.Items[i].NameRight))
          {
            SetTextFont(4);
            SetTextScale(textScale, textScale);
            SetTextColour(255, 255, 255, 255);
            SetTextEntry("STRING");
            AddTextComponentString(FontsManager.FiraSansString + CurrentMenu.Items[i].NameRight);
            DrawText(xOffset - xLenght / 2.0f + textMarginNameRight, (yOffset + (yHeight * (i - indexMin)) - textYOffset));
          }

          if (!string.IsNullOrEmpty(CurrentMenu.Items[i].ExtraLeft))
          {
            SetTextFont(4);
            SetTextScale(textScale, textScale);
            SetTextColour(11, 11, 11, 255);
            SetTextEntry("STRING");
            AddTextComponentString(FontsManager.FiraSansString + CurrentMenu.Items[i].ExtraLeft);
            DrawText(xOffset + xLenght / 2.0f + spacing + textMarginExtraLeft, (yOffset + (yHeight * (i - indexMin)) - textYOffset));
          }

          if (!string.IsNullOrEmpty(CurrentMenu.Items[i].ExtraRight))
          {
            SetTextFont(4);
            SetTextScale(textScale, textScale);
            SetTextColour(11, 11, 11, 255);
            SetTextEntry("STRING");
            AddTextComponentString(FontsManager.FiraSansString + CurrentMenu.Items[i].ExtraRight);
            DrawText(xOffset + xLenght / 2.0f + spacing + textMarginExtraRight, (yOffset + (yHeight * (i - indexMin)) - textYOffset));
          }
        }
      }
    }

    private void Refresh()
    {
      if (IsHidden)
        return;

      for (int i = 0; i < CurrentMenu.Items.Count; i++)
      {
        if (CurrentMenu.Items[i].OnRefresh != null)
          CurrentMenu.Items[i].OnRefresh(CurrentMenu.Items[i]); //uvidime jak to da
      }
    }

    private void BackOrClose()
    {
      if (IsHidden || CurrentMenu == null)
        return;

      if (CurrentMenu.PreviousMenu != null)
      {
        CurrentMenu = CurrentMenu.PreviousMenu;
        Refresh();
      }
      else
      {
        CurrentMenu = null;
        CurrentMenuName = null;
        IsHidden = true;
      }
    }
    #endregion

    #region public metody
    public static void ToggleMenu(string menuName, Func<NativeMenu> getMenu)
    {
      if (CurrentMenuName == menuName)
      {
        if (!IsHidden)
        {
          IsHidden = true;
          return;
        }
        else if (CurrentMenu != null)
        {
          IsHidden = false;
          return;
        }
      }

      CurrentMenu = getMenu();
      CurrentMenuName = menuName;
      IsHidden = false;
    }
    #endregion

    public NativeMenuManager()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }
  }
}
