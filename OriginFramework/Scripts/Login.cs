using CitizenFX.Core;
using Newtonsoft.Json;
using OriginFramework.Menus;
using OriginFrameworkData;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
	public class Login : BaseScript
	{
    public static bool IsInLoginScreen { get; private set; } = false;
    private const string loginMenuName = "loginMenu";
    private static List<CharacterBag> ExistingCaracters = null;
    private static dynamic ScreenshotBasic = null;

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
      ScreenshotBasic = Exports["screenshot-basic"];

      InternalDependencyManager.Started(eScriptArea.Login);
    }

    private async Task OnTick()
		{
      if (IsInLoginScreen)
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
                IsInLoginScreen = false;
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

      string charData = await Callbacks.ServerAsyncCallbackToSync<string>("ofw_login:GetCharacters");
      if (!string.IsNullOrEmpty(charData) && charData != "nochar")
        ExistingCaracters = JsonConvert.DeserializeObject<List<CharacterBag>>(charData);

      if (ExistingCaracters != null)
        ExistingCaracters = ExistingCaracters.OrderByDescending(c => c.Name).ToList();

      IsInLoginScreen = true;
    }

    public static async void LoginCharacter(CharacterBag character)
    {
      var result = await Callbacks.ServerAsyncCallbackToSyncWithErrorMessage("ofw_login:LoginCharacter", character.Id);

      if (result == true)
      {
        try
        {
          if (ScreenshotBasic != null)
          {
            ScreenshotBasic.requestScreenshot(new { encoding = "jpg", quality = 0.8f } ,new Action<string> ((data) =>
            {
              //Debug.WriteLine(data);
              //TriggerEvent("chat:addMessage", new { template = $"<img src=\"{data}\" style=\"max-width: 300px;\" />" });

              TriggerLatentServerEvent("ofw_core:SaveImageJpg", 20000, data); //20kB /s -> 160kb/s
            }));
          }
        }
        catch
        {
        }

        CharacterCaretaker.LoggedIn(character);
        IsInLoginScreen = false;
        NativeMenuManager.CloseAndUnlockMenu(loginMenuName);
      }
    }
  }
}
