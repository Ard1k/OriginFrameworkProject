using CitizenFX.Core;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using OriginFrameworkData;

namespace OriginFramework.Menus
{
  public class DynamicMenu
  {
    public Menu Menu { get; set; }

    public DynamicMenu(DynamicMenuDefinition menuDefinition)
    {
      this.Menu = BuildMenu(menuDefinition);
      //Notify.Info("Existing menus: " + MenuController.Menus.Count.ToString());
    }

    public static List<Menu> OwnedMenus { get; set; } = new List<Menu>();

    protected void AddItem(Menu menu, DynamicMenuItem it)
    {
      if (it == null)
        return;

      var mItem = new MenuItem(it.TextLeft ?? "???", it.TextDescription);
      if (it.TextRight != null)
        mItem.Label = it.TextRight;

      menu.AddMenuItem(mItem);

      if (it.Submenu != null)
      {
        var subMenu = BuildMenu(it.Submenu);
        if (subMenu != null)
          MenuController.BindMenuItem(menu, subMenu, mItem);
      }

      if (it.OnClick != null)
      {
        menu.OnItemSelect += (sender, item, index) =>
        {
          if (item == mItem)
          {
            it.OnClick();
          }
        };
      }
    }

    protected Menu BuildMenu(DynamicMenuDefinition dynDef)
    {
      if (dynDef == null)
        return null;

      Menu _menu = new Menu(dynDef.MainName ?? "OriginRP", dynDef.Name);
      MenuController.AddMenu(_menu);
      OwnedMenus.Add(_menu);

      if (dynDef.Items != null && dynDef.Items.Count() > 0)
      {
        foreach (var it in dynDef.Items)
        {
          AddItem(_menu, it);
        }
      }

      return _menu;
    }
  }

  public class DynamicMenuDefinition
  {
    public string Name { get; set; }
    public string MainName { get; set; }
    public IEnumerable<DynamicMenuItem> Items { get; set; }
  }

  public class DynamicMenuItem
  {
    public string TextLeft { get; set; }
    public string TextRight { get; set; }
    public string TextDescription { get; set; }
    public Action OnClick { get; set; } 
    public DynamicMenuDefinition Submenu { get; set; }
  }
}
