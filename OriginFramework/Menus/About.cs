using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OriginFramework.vMenuFunctions;

namespace OriginFramework.Menus
{
  public class About
  {
    // Variables
    private Menu menu;

    private void CreateMenu()
    {
      // Create the menu.
      menu = new Menu("Origin Framework", "About");

      // Create menu items.
      MenuItem version = new MenuItem("Verze", $" ~b~~h~{Main.Version}~h~~s~.")
      {
        Label = $"~h~{Main.Version}~h~"
      };
      MenuItem credits = new MenuItem("About", "Custom featurky pro server");

      MenuItem serverInfo = new MenuItem("Server Info", "Origin");
      serverInfo.Label = "Origin RP";

      menu.AddMenuItem(serverInfo);
      menu.AddMenuItem(version);
      menu.AddMenuItem(credits);

      menu.OnItemSelect += async (sender, item, index) =>
      {
        if (item == version)
        {
          string result = await GetUserInput(windowTitle: "Kdo je tvuj nejoblibenejsi prezident a proc prave Zeman?");
          if (result != null && result == "iwantsomefun")
          {
            Main.UnlockFun();
          }
        }
      };
    }

    /// <summary>
    /// Create the menu if it doesn't exist, and then returns it.
    /// </summary>
    /// <returns>The Menu</returns>
    public Menu GetMenu()
    {
      if (menu == null)
      {
        CreateMenu();
      }
      return menu;
    }
  }
}
