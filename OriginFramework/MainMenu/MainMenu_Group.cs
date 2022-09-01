using CitizenFX.Core;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static OriginFramework.OfwFunctions;

namespace OriginFramework.Menus
{
  public static class MainMenu_Group
  {
    public static NativeMenu GenerateMenu()
    {
      NativeMenu menu = new NativeMenu
      {
        MenuTitle = "Skupina",
        Items = new List<NativeMenuItem>()
      };

      if (GroupManager.Group == null || !GroupManager.Group.IsInAGroup)
      {
        menu.Items.Add(new NativeMenuItem
        {
          Name = "Založit skupinu",
          OnSelected = (item) =>
          {
            TriggerServerEvent("ofw_grp:CreateGroup");
          },
          IsBack = true
        }); ;
        if (GroupManager.IsWaitingForGroup)
        {
          menu.Items.Add(new NativeMenuItem
          {
            Name = "Přestat čekat na pozvánku",
            OnSelected = (item) => {
              GroupManager.IsWaitingForGroup = false;
            },
            IsBack = true,
          });
        }
        else
        {
          menu.Items.Add(new NativeMenuItem
          {
            Name = "Čekat na pozvánku",
            OnSelected = (item) => {
              if (GroupManager.Group == null || !GroupManager.Group.IsInAGroup)
                GroupManager.IsWaitingForGroup = true;
            },
            IsBack = true
          });
        }
      }
      else
      {
        if (GroupManager.Group != null && GroupManager.Group.Members != null && GroupManager.Group.Members.Length < 4)
          menu.Items.Add(new NativeMenuItem
          {
            Name = "Pozvat hráče",
            GetSubMenu = GenerateSubMenu_Invite
          });
        menu.Items.Add(new NativeMenuItem
        {
          Name = "Opustit skupinu",
          OnSelected = (item) => { TriggerServerEvent("ofw_grp:LeaveGroup"); },
          IsBack = true
        });
      }

      return menu;
    }

    public static NativeMenu GenerateSubMenu_Invite()
    {
      NativeMenu menu = new NativeMenu
      {
        MenuTitle = "Pozvat",
        Items = new List<NativeMenuItem>()
      };

      var localPlayers = Main.LocalPlayers.ToList();

      foreach (var p in localPlayers)
      {
        if (p.ServerId == Game.Player.ServerId)
          continue;

        menu.Items.Add(
          new NativeMenuItem
          { 
            Name = $"{p.Name} ID:{p.ServerId}",
            OnSelected = (item) => { 
              TriggerServerEvent("ofw_grp:InviteToGroup", p.ServerId);
              },
            IsBack = true
          });
      }

      if (menu.Items.Count <= 0)
      {
        menu.Items.Add(new NativeMenuItem { Name = "Zpět - nikdo poblíž není", IsBack = true });
      }

      return menu;
    }
  }
}
