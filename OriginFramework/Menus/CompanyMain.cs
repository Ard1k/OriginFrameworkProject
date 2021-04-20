using CitizenFX.Core;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static OriginFramework.vMenuFunctions;
using OriginFrameworkData;

namespace OriginFramework.Menus
{
  public class CompanyMain
  {
    public List<int> PlayersWaypointList = new List<int>();

    Menu companyMainMenu = new Menu("Company", "");
    PlayerCompanyBag playerCompanyData = null;


    /// <summary>
    /// Creates the menu.
    /// </summary>
    private async void RefreshMenu()
    {
      companyMainMenu.ClearMenuItems();

      playerCompanyData = await RequestPlayerCompanyData();

      if (playerCompanyData == null) return;

      MenuItem companyName = new MenuItem("Company:", playerCompanyData.CompanyName ?? "---");
      companyName.Label = playerCompanyData.CompanyCode ?? "---";
      companyMainMenu.AddMenuItem(companyName);

      MenuItem companyRole = new MenuItem("Role:", "Role in company");
      companyRole.Label = playerCompanyData.IsCompanyOwner ? "Owner" : playerCompanyData.IsCompanyManager ? "Manager" : "Employee";
      companyMainMenu.AddMenuItem(companyRole);

      MenuItem manageCompany = new MenuItem("Manage company", "Company management menu");
      manageCompany.Label = "→→→";
      companyMainMenu.AddMenuItem(manageCompany);

      // handle button presses for the specific player's menu.
      companyMainMenu.OnItemSelect += async (sender, item, index) =>
      {
        // send message
        if (item == manageCompany)
        {
          Notify.Alert("Management!");
          var mDefSub = new DynamicMenuDefinition
          {
            Name = "TestSub",
            Items = new List<DynamicMenuItem> { new DynamicMenuItem { TextLeft = "TestSub1" }, new DynamicMenuItem { TextLeft = "TestSub2" }, new DynamicMenuItem { TextLeft = "TestSub3" } }
          };

          var mDef = new DynamicMenuDefinition
          {
            Name = "Testmain",
            Items = new List<DynamicMenuItem> { new DynamicMenuItem { TextLeft = "Test1" }, new DynamicMenuItem { TextLeft = "Test2" }, new DynamicMenuItem { TextLeft = "TestSub", Submenu = mDefSub }, new DynamicMenuItem { TextLeft = "Test3" } }
          };

          var menu = new DynamicMenu(mDef);
          MenuController.CloseAllMenus();
          menu.Menu.OpenMenu();
        }
      };


    }

    public static async Task<PlayerCompanyBag> RequestPlayerCompanyData()
    {
      PlayerCompanyBag bag = null;
      bool completed = false;

      Func<string, bool> CallbackFunction = (data) =>
      {
        bag = JsonConvert.DeserializeObject<PlayerCompanyBag>(data);
        completed = true;
        return true;
      };

      TriggerServerEvent("ofw:GetPlayerCompanyData", Game.Player.ServerId, CallbackFunction);

      while (!completed)
      {
        await Delay(0);
      }

      return bag;
    }

    /// <summary>
    /// Checks if the menu exists, if not then it creates it first.
    /// Then returns the menu.
    /// </summary>
    /// <returns>The Online Players Menu</returns>
    public Menu GetMenu()
    {
      RefreshMenu();
      return companyMainMenu;
    }
  }
}
