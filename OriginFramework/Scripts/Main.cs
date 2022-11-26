﻿using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OriginFramework.Menus;
using static CitizenFX.Core.Native.API;
using OriginFrameworkData.DataBags;
using System.Text.RegularExpressions;
using OriginFramework.Scripts;
using Newtonsoft.Json;

namespace OriginFramework
{
  public class Main : BaseScript
  {
    public static string _version = null;
    public static string Version
    {
      get
      {
        if (_version != null)
          return _version;

        _version = CitizenFX.Core.Native.API.GetResourceMetadata(CitizenFX.Core.Native.API.GetCurrentResourceName(), "version", 0);
        return _version;
      }
    }

    public static PlayerList LocalPlayers { get; protected set; } //tohle funguje ale je to celkem sketchy. Jakmile se zmeni reference tak papa :D
    public static int MyOriginId { get; set; } = -1;
    public static bool IsAdmin { get; set; } = false;

    public Main()
    {
      LocalPlayers = Players;

      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.Main))
        return;

      Debug.WriteLine($"Waiting for oid...");
      MyOriginId = await Callbacks.ServerAsyncCallbackToSync<int>("ofw_oid:GetMyOriginID");
      Debug.WriteLine($"OID retrieved: {MyOriginId}");

      #region register commands

      RegisterCommand("skin", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        //TODO - opravneni, chcem to vubec?
        if (args == null || args.Count <= 0)
          NativeMenuManager.OpenNewMenu("skin_menu", () => { return SkinMenu.GenerateMenu(SkinManager.ComponentsAll, null); });
        else if (args.Count == 1)
        {
          int slot;
          if (Int32.TryParse((string)args[0], out slot))
            NativeMenuManager.OpenNewMenu("skin_menu", () => { return SkinMenu.GenerateMenu(SkinManager.GetClothesForSlot((eSpecialSlotType)slot), null); });
        }
      }), false);

      #endregion

      var dynamicItemsDefString = await Callbacks.ServerAsyncCallbackToSync<string>("ofw_core:GetDynamicItemDefinitions");
      if (dynamicItemsDefString != null)
      {
        var dynItDefs = JsonConvert.DeserializeObject<Dictionary<int, ItemDefinition>>(dynamicItemsDefString);
        if (dynItDefs != null)
        {
          foreach (var row in dynItDefs)
          {
            ItemsDefinitions.Items[row.Key] = row.Value;
          }
        }
      }

      Tick += OnTick;

      InternalDependencyManager.Started(eScriptArea.Main);
    }

    private async Task OnTick()
    {
      Game.DisableControlThisFrame(0, Control.WeaponWheelUpDown);
      Game.DisableControlThisFrame(0, Control.WeaponWheelNext);
      Game.DisableControlThisFrame(0, Control.WeaponWheelPrev);
      Game.DisableControlThisFrame(0, Control.SelectNextWeapon);
      Game.DisableControlThisFrame(0, Control.SelectPrevWeapon);
      Game.DisableControlThisFrame(0, Control.SelectWeapon); //TAB
      Game.DisableControlThisFrame(0, Control.WeaponSpecial);
      Game.DisableControlThisFrame(0, Control.WeaponSpecial2);
      Game.DisableControlThisFrame(0, Control.VehicleSelectNextWeapon);
      Game.DisableControlThisFrame(0, Control.VehicleSelectPrevWeapon);

      if (IsControlJustPressed(0, 344)) //F11
        NativeMenuManager.ToggleMenu("MainMenu", MainMenu_Default.GenerateMenu);

      if (IsControlJustPressed(0, 57)) //F10
      {
        if (CharacterCaretaker.LoggedCharacter.AdminLevel > 0)
          NoClip.SetNoclipSwitch();
      }
    }

    [EventHandler("ofw_core:DynamicItemDefinitionUpdated")]
    private async void GetDynamicItemDefinitions(string data)
    {
      if (data == null)
        return;

      var it = JsonConvert.DeserializeObject<ItemDefinition>(data);

      if (it != null)
      {
        ItemsDefinitions.Items[it.ItemId] = it;
      }
    }

    [EventHandler("ofw:ValidationErrorNotification")]
    private async void ValidationErrorNotification(string message)
    {
      Notify.Error(message);
    }

    [EventHandler("ofw:SuccessNotification")]
    private async void SuccessNotification(string message)
    {
      Notify.Success(message);
    }
  }
}
