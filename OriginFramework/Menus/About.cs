using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFramework.Menus
{
  public class About
  {
    private Menu menu;

    private void CreateMenu()
    {
      menu = new Menu("Origin Framework", "About");

      MenuItem version = new MenuItem("Verze", $" ~b~~h~{Main.Version}~h~~s~")
      {
        Label = $"~h~{Main.Version}~h~"
      };
      MenuItem about = new MenuItem("About", "Custom scripty pro server");
      MenuItem credits = new MenuItem("Credits", "MenuAPI~n~https://github.com/TomGrobbe/MenuAPI~n~~n~VSql~n~https://github.com/warxander/vSql");

      MenuItem serverInfo = new MenuItem("Server Info", "OriginPLAY CZ/SK Komunita");
      serverInfo.Label = "OriginRP";

      menu.AddMenuItem(serverInfo);
      menu.AddMenuItem(version);
      menu.AddMenuItem(about);
      menu.AddMenuItem(credits);

      menu.OnItemSelect += async (sender, item, index) =>
      {
        if (item == version)
        {
          string result = await OfwFunctions.GetUserInput(windowTitle: "Nebud citlivy, ok?");
          if (result != null && result == "iwantsomefun")
          {
            Main.UnlockFun();
          }
        }
      };
    }

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
