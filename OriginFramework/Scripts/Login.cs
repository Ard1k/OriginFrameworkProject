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
	public class Login : BaseScript
	{
    private static bool isInLoginScreen = false;
    private const string loginMenuName = "loginMenu";
    private static List<CharacterBag> ExistingCaracters = null;

    public Login()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.Login))
        return;

      Tick += OnTick;

      InternalDependencyManager.Started(eScriptArea.Login);
    }

    private async Task OnTick()
		{
      if (isInLoginScreen)
      {
        if (!NativeMenuManager.IsMenuOpen(loginMenuName))
          NativeMenuManager.EnsureMenuOpen(loginMenuName, getLoginMenu);
      }
		}

    private NativeMenu getLoginMenu()
    {
      var menu = new NativeMenu
      {
        MenuTitle = "Login",
        Items = new List<NativeMenuItem>
          {
            new NativeMenuItem
            {
              Name = "Nová postava",
              OnSelected = (item) => 
              {
                isInLoginScreen = false;
                NativeMenuManager.UnlockMenu(loginMenuName);
                CharacterCreator.EnterCreator();
              }
            }   
          }
      };

      if (ExistingCaracters != null)
      {
        foreach (var c in ExistingCaracters)
        {
          menu.Items.Insert(0, new NativeMenuItem
          {
            Name = c.Name ?? "Unknown",
            OnSelected = (item) =>
            {
              LoginCharacter(c);
            }
          });
        }
      }

      return menu;
    }

    public static async void ReturnToLogin()
    {
      while (NativeMenuManager.LockInMenu(loginMenuName) == false)
        await Delay(0);

      string charData = await OfwFunctions.ServerAsyncCallbackToSync<string>("ofw_login:GetCharacters");
      if (!string.IsNullOrEmpty(charData) && charData != "nochar")
        ExistingCaracters = JsonConvert.DeserializeObject<List<CharacterBag>>(charData);

      if (ExistingCaracters != null)
        ExistingCaracters = ExistingCaracters.OrderByDescending(c => c.Name).ToList();

      isInLoginScreen = true;
    }

    public static async void LoginCharacter(CharacterBag character)
    {
      var result = await OfwFunctions.ServerAsyncCallbackToSyncWithErrorMessage("ofw_login:LoginCharacter", character.Id);

      if (result == true)
      {
        CharacterCaretaker.LoggedIn(character);
        isInLoginScreen = false;
        NativeMenuManager.CloseAndUnlockMenu(loginMenuName);
      }
    }
  }
}
