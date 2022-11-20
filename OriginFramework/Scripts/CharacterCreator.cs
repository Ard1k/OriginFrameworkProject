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
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
	public class CharacterCreator : BaseScript
	{
    public static bool IsInCharacterCreator { get; private set; } = false;
    private const string charCreatorMenuName = "characterCreatorMenu";
    private static CharacterBag NewCharacter = null;

    private int selectedModelIndex = 0;
    private static List<Tuple<string, string>> availablePedModels = new List<Tuple<string, string>>
    {
      new Tuple<string, string>("mp_m_freemode_01", "Muž"),
      new Tuple<string, string>("mp_f_freemode_01", "Žena"),
      new Tuple<string, string>("a_c_retriever", "Peso more"),
    };

    public CharacterCreator()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.CharacterCreator))
        return;

      Tick += OnTick;

      InternalDependencyManager.Started(eScriptArea.CharacterCreator);
    }

    private async Task OnTick()
		{
      if (IsInCharacterCreator)
      {
        if (!NativeMenuManager.IsMenuOpen(charCreatorMenuName))
          NativeMenuManager.EnsureMenuOpen(charCreatorMenuName, getCreatorMenu);
      }
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

    private NativeMenu getCreatorMenu()
    {
      if (NewCharacter == null)
      {
        selectedModelIndex = 0;
        NewCharacter = new CharacterBag(GetHashKey(availablePedModels[selectedModelIndex].Item1));
      }

      var menu = new NativeMenu
      {
        MenuTitle = "Character creator",
        Items = new List<NativeMenuItem>
          {
            new NativeMenuItem
            {
              Name = "Jméno",
              ExtraLeft = NewCharacter.Name ?? "---",
              IsTextInput = true,
              TextInputMaxLength = 50,
              TextInputRequest = "Zadejte jméno postavy",
              OnTextInput = (item, input) =>
                {
                  NewCharacter.Name = input;
                  item.ExtraLeft = NewCharacter.Name;
                }
            },
            new NativeMenuItem
            {
              Name = "Pohlaví",
              NameRight = "←→",
              ExtraLeft = availablePedModels[selectedModelIndex].Item2 ?? "---",
              OnLeft = (item) => 
              {
                if (selectedModelIndex <= 0)
                  return;

                selectedModelIndex--;
              },
              OnRight = (item) =>
              {
                if (selectedModelIndex >= availablePedModels.Count - 1)
                  return;

                selectedModelIndex++;
              },
              OnRefresh = (item) =>
              {
                item.ExtraLeft = availablePedModels[selectedModelIndex].Item2 ?? "---";
                NewCharacter.Model = GetHashKey(availablePedModels[selectedModelIndex].Item1);
              }
            },
            new NativeMenuItem { IsUnselectable = true },
            new NativeMenuItem
            {
              Name = "Odejít",
              OnSelected = (item) =>
              {
                NativeMenuManager.UnlockMenu(charCreatorMenuName);
                IsInCharacterCreator = false;
                Login.ReturnToLogin();
              }
            },
            new NativeMenuItem
            {
              Name = "Potvrdit a uložit",
              OnSelected = (item) =>
              {
                CreateCharacter();
              }
            }   
          }
      };

      return menu;
    }

    public static async void EnterCreator()
    {
      NewCharacter = null;
      while (NativeMenuManager.LockInMenu(charCreatorMenuName) == false)
        await Delay(0);
      IsInCharacterCreator = true;
    }

    public static async void CreateCharacter()
    {
      var result = await Callbacks.ServerAsyncCallbackToSyncWithErrorMessage("ofw_login:CreateCharacter", JsonConvert.SerializeObject(NewCharacter));

      if (result == true)
      {
        NativeMenuManager.UnlockMenu(charCreatorMenuName);
        IsInCharacterCreator = false;
        Login.ReturnToLogin();
      }
    }
  }
}
