using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFramework.Menus
{
  public static class MainMenu_Default
  {
    public static NativeMenu GenerateMenu()
    {
      NativeMenu menu = new NativeMenu
      {
        MenuTitle = "Hlavní menu",
        Items = new List<NativeMenuItem>
          {
            new NativeMenuItem { Name = "Skupina", NameRight = ">>>", GetSubMenu = MainMenu_Group.GenerateMenu },
            new NativeMenuItem { Name = "Blbůstky", NameRight = ">>>", GetSubMenu = MainMenu_FunTools.GenerateMenu },
          }
      };

      if (CharacterCaretaker.LoggedCharacter?.AdminLevel > 0)
      {
        menu.Items.Insert(0, new NativeMenuItem
        {
          Name = "Admin menu",
          NameRight = ">>>",
          GetSubMenu = MainMenu_Admin.GenerateMenu
        });

        menu.Items.Insert(1, new NativeMenuItem
        {
          Name = "Dev Tools",
          NameRight = ">>>",
          GetSubMenu = MainMenu_DevTools.GenerateMenu
        });
      }

      return menu;
    }
  }
}
