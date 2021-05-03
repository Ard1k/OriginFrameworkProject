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
  public class Toys
  {
    Menu toysMenu = new Menu("OriginRP", "Blbustky");

    bool isInitialized = false;

    private async void CreateMenus()
    {
      MenuController.AddMenu(toysMenu);

      MenuCheckboxItem driftItem = new MenuCheckboxItem("Shift drift", "Zmacknutim SHIFTu bude tvoje auto driftovat. Porad plati pravidla RP, takze pouzivat s rozumem a nevymlouvat se na tuto funkci.", DrifterAndBooster.IsDrifterEnabled);
      toysMenu.AddMenuItem(driftItem);
      MenuCheckboxItem flashItem = new MenuCheckboxItem("Dalkovky pri klaksonu", "Problikne dalkovky pri pouziti klaksonu.", FlashHeadlightsOnHorn.IsEnabled);
      toysMenu.AddMenuItem(flashItem);

      toysMenu.OnCheckboxChange += (sender, item, index, _checked) =>
      {
        if (item == driftItem)
        {
          if (_checked)
            DrifterAndBooster.EnableDrifter();
          else
            DrifterAndBooster.DisableDrifter();

          SetResourceKvpInt(KvpManager.getKvpString(KvpEnum.DrifterEnabled), _checked ? 1 : 0);
        }

        if (item == flashItem)
        {
          if (_checked)
            FlashHeadlightsOnHorn.EnableFlashOnHorn();
          else
            FlashHeadlightsOnHorn.DisableFlashOnHorn();

          SetResourceKvpInt(KvpManager.getKvpString(KvpEnum.FlashOnHorn), _checked ? 1 : 0);
        }
      };

      isInitialized = true;
    }

    public Menu GetMenu()
    {
      CreateMenus();
      return toysMenu;
    }
  }
}
