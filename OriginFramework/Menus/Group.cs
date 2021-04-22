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
      groupMenu = new Menu("Group", "");
      createItem = new MenuItem("Create group", "Create new group");
      stopWaitItem = new MenuItem("Stop waiting for invite", "Stop waiting for invite");
      startWaitItem = new MenuItem("Wait for invite", "Set your status as waiting for invite");
      inviteItem = new MenuItem("Invite", "Invite another member to group");
      leaveItem = new MenuItem("Leave", "Leave group");

      groupMenu.AddMenuItem(createItem);
      groupMenu.AddMenuItem(stopWaitItem);
      groupMenu.AddMenuItem(startWaitItem);
      groupMenu.AddMenuItem(inviteItem);
      groupMenu.AddMenuItem(leaveItem);


      groupMenu.OnItemSelect += async (sender, item, index) =>
      {
        if (item == createItem)
        {
          Debug.WriteLine($"GroupMenu: ProcessingOnClick, triggering server event ofw_grp:CreateGroup");
          TriggerServerEvent("ofw_grp:CreateGroup");
        }
        if (item == inviteItem)
        {
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
        if (item == startWaitItem)
        {
          if (GroupManager.Group == null || !GroupManager.Group.IsInAGroup)
            GroupManager.IsWaitingForGroup = true;
        }
        if (item == stopWaitItem)
        {
          GroupManager.IsWaitingForGroup = false;
        }
        if (item == leaveItem)
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
