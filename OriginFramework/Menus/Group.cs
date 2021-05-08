using CitizenFX.Core;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static OriginFramework.OfwFunctions;
using OriginFrameworkData;
using OriginFrameworkData.DataBags;

namespace OriginFramework.Menus
{
  public class Group
  {
    Menu groupMenu = null;

    MenuItem createItem = null;
    MenuItem inviteItem = null;
    MenuItem startWaitItem = null;
    MenuItem stopWaitItem = null;
    MenuItem leaveItem = null;

    private void CreateMenu()
    {
      groupMenu = new Menu("OriginRP", "Skupina");
      createItem = new MenuItem("Zalozit skupinu", "Vytvori novou skupinu do ktere bude mozne pozvat az dalsi 3 hrace");
      stopWaitItem = new MenuItem("Prestat cekat na pozvanku", "Nepujde te pridat do skupiny");
      startWaitItem = new MenuItem("Cekat na pozvanku", "Clenove jine skupiny te budou moci pridat");
      inviteItem = new MenuItem("Pozvat", "Pozvat hrace do skupiny");
      leaveItem = new MenuItem("Opustit", "Opustis skupinu");

      groupMenu.AddMenuItem(createItem);
      groupMenu.AddMenuItem(stopWaitItem);
      groupMenu.AddMenuItem(startWaitItem);
      groupMenu.AddMenuItem(inviteItem);
      groupMenu.AddMenuItem(leaveItem);


      groupMenu.OnItemSelect += async (sender, item, index) =>
      {
        if (item == createItem)
        {
          //Debug.WriteLine($"GroupMenu: ProcessingOnClick, triggering server event ofw_grp:CreateGroup");
          TriggerServerEvent("ofw_grp:CreateGroup");
        }
        else if (item == inviteItem)
        {
          var localPlayers = Main.LocalPlayers.ToList();

          var dynMenuItems = new List<DynamicMenuItem>();

          foreach (var p in localPlayers)
          {
            if (p.ServerId == Game.Player.ServerId)
              continue;

            dynMenuItems.Add(new DynamicMenuItem { TextLeft = $"{p.Name} ID:{p.ServerId}", OnClick = () => { TriggerServerEvent("ofw_grp:InviteToGroup", p.ServerId); MenuController.CloseAllMenus(); } });
          }

          if (dynMenuItems.Count <= 0)
          {
            Notify.Info("Zadni hraci pobliz");
          }
          else
          {
            var dynMenuDef = new DynamicMenuDefinition
            {
              Name = "Invite",
              Items = dynMenuItems
            };

            var dynMenu = new DynamicMenu(dynMenuDef);
            MenuController.CloseAllMenus();
            dynMenu.Menu.OpenMenu();
          }
        }
        else if (item == startWaitItem)
        {
          if (GroupManager.Group == null || !GroupManager.Group.IsInAGroup)
            GroupManager.IsWaitingForGroup = true;
        }
        else if (item == stopWaitItem)
        {
          GroupManager.IsWaitingForGroup = false;
        }
        else if (item == leaveItem)
        {
          TriggerServerEvent("ofw_grp:LeaveGroup");
        }

        groupMenu.GoBack();
      };
    }

    public async Task RefreshMenu()
    {
      List<MenuItem> toFilter = new List<MenuItem>();

      if (GroupManager.Group == null || !GroupManager.Group.IsInAGroup)
      {
        toFilter.Add(createItem);
        if (GroupManager.IsWaitingForGroup)
        {
          toFilter.Add(stopWaitItem);
        }
        else
        {
          toFilter.Add(startWaitItem);
        }
      }
      else
      {
        if (GroupManager.Group != null && GroupManager.Group.Members != null && GroupManager.Group.Members.Length < 4)
          toFilter.Add(inviteItem);
        toFilter.Add(leaveItem);
      }

      groupMenu.FilterMenuItems(mi => toFilter.Contains(mi));
    }

    public Menu GetMenu()
    {
      if (groupMenu == null)
        CreateMenu();
      return groupMenu;
    }
  }
}
