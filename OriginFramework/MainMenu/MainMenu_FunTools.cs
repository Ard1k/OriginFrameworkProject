using OriginFrameworkData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework.Menus
{
  public static class MainMenu_FunTools
  {
    public static NativeMenu GenerateMenu()
    {
      NativeMenu menu = new NativeMenu
      {
        MenuTitle = "Blbůstky",
        Items = new List<NativeMenuItem>
          {
            new NativeMenuItem
            {
              Name = "Shift drift",
              NameRight = DrifterAndBooster.IsDrifterEnabled ? "ZAP" : "VYP",
              OnSelected = (item) =>
              {
                if (DrifterAndBooster.IsDrifterEnabled)
                  DrifterAndBooster.DisableDrifter();
                else
                  DrifterAndBooster.EnableDrifter();

                item.NameRight = DrifterAndBooster.IsDrifterEnabled ? "ZAP" : "VYP";
                SetResourceKvpInt(KvpManager.getKvpString(KvpEnum.DrifterEnabled), DrifterAndBooster.IsDrifterEnabled ? 1 : 0);
              }
            },
            new NativeMenuItem
            {
              Name = "Dálkovky při klaksonu",
              NameRight = FlashHeadlightsOnHorn.IsEnabled ? "ZAP" : "VYP",
              OnSelected = (item) =>
              {
                if (FlashHeadlightsOnHorn.IsEnabled)
                  FlashHeadlightsOnHorn.DisableFlashOnHorn();
                else
                  FlashHeadlightsOnHorn.EnableFlashOnHorn();

                item.NameRight = FlashHeadlightsOnHorn.IsEnabled ? "ZAP" : "VYP";
                SetResourceKvpInt(KvpManager.getKvpString(KvpEnum.FlashOnHorn), FlashHeadlightsOnHorn.IsEnabled ? 1 : 0);
              }
            },
          }
      };

      return menu;
    }
  }
}
