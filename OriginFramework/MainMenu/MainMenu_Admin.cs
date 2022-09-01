using OriginFramework.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static OriginFramework.OfwFunctions;

namespace OriginFramework.Menus
{
  public static class MainMenu_Admin
  {
    public static NativeMenu GenerateMenu()
    {
      NativeMenu menu = new NativeMenu
      {
        MenuTitle = "Admin menu",
        Items = new List<NativeMenuItem>()
      };

      menu.Items.Add(new NativeMenuItem
      {
        Name = "NoClip",
        NameRight = NoClip.IsNoclipActive() ? "ZAP" : "VYP",
        OnSelected = (item) =>
        {
          NoClip.SetNoclipSwitch();
          item.NameRight = NoClip.IsNoclipActive() ? "ZAP" : "VYP";
        }
      }) ;

      menu.Items.Add(new NativeMenuItem
      {
        Name = "GodMode",
        NameRight = Misc.IsPlayerGodModeEnabled ? "ZAP" : "VYP",
        OnSelected = (item) =>
        {
          Misc.TogglePlayerGodMode();
          item.NameRight = Misc.IsPlayerGodModeEnabled ? "ZAP" : "VYP";
        }
      });

      menu.Items.Add(new NativeMenuItem
      {
        Name = "Booster",
        NameRight = DrifterAndBooster.IsBoosterEnabled ? "ZAP" : "VYP",
        OnSelected = (item) =>
        {
          if (DrifterAndBooster.IsBoosterEnabled)
            DrifterAndBooster.DisableBooster();
          else
            DrifterAndBooster.EnableBooster();

          item.NameRight = DrifterAndBooster.IsBoosterEnabled ? "ZAP" : "VYP";
        }
      });

      return menu;
    }
  }
}
