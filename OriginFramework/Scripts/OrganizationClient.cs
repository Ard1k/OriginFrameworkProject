using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using Newtonsoft.Json;
using OriginFramework.Helpers;
using OriginFramework.Menus;
using OriginFrameworkData.DataBags;
using static CitizenFX.Core.Native.API;

namespace OriginFramework.Scripts
{
  public class OrganizationClient : BaseScript
  {
    public static int? CharOrganizationID
    {
      get
      {
        return CharacterCaretaker.LoggedCharacter?.OrganizationId;
      }
    }

    public string MenuName { get { return "organization_menu"; } }
    public string MenuInviteName { get { return "organization_invite_menu"; } }

    public static OrganizationBag CurrentOrganization { get; set; }

    public OrganizationClient()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.OrganizationClient))
        return;

      Tick += OnTick;

      InternalDependencyManager.Started(eScriptArea.OrganizationClient);
    }

    private async Task OnTick()
    {
      if (Game.IsControlJustPressed(0, Control.ReplayShowhotkey)) //K
      {
        NativeMenuManager.ToggleMenu(MenuName, getOrganizationMenu);
      }
    }

    private static NativeMenu getOrganizationMenu()
    {
      if (CurrentOrganization == null || CharOrganizationID == null)
      {
        return new NativeMenu
        {
          MenuTitle = "Menu organizace",
          Items = new List<NativeMenuItem> 
          {
            new NativeMenuItem
            {
              Name = "Nejsi v žádné organizaci",
              IsClose = true
            }
          }
        };
      }
      var menu = new NativeMenu
      {
        MenuTitle = CurrentOrganization.Name,
        Items = new List<NativeMenuItem>
          {
            new NativeMenuItem
            {
              Name = "Auta",
              GetSubMenuAsync = getOrganizationCarsMenu
            },
            new NativeMenuItem
            {
              Name = "Opustit organizaci",
              SubMenu = new NativeMenu
              {
                MenuTitle = "Opravdu opustit organizaci?",
                Items = new List<NativeMenuItem>
                {
                  new NativeMenuItem
                  {
                    Name = "Ano",
                    OnSelected = (item) =>
                    {
                      TriggerServerEvent("ofw_org:LeaveOrganization");
                    },
                    IsClose = true
                  },
                  new NativeMenuItem
                  {
                    Name = "Ne",
                    IsBack = true
                  }
                }
              }
            },
            new NativeMenuItem
            {
              Name = "Zavřít",
              IsClose = true
            }
          }
      };

      if (CurrentOrganization.Owner == CharacterCaretaker.LoggedCharacter?.Id || CurrentOrganization.Managers.Any(m => m.CharId == CharacterCaretaker.LoggedCharacter?.Id))
      menu.Items.Insert(0, new NativeMenuItem
      {
        Name = "Správa organizace",
        SubMenu = new NativeMenu 
        { 
          MenuTitle = "Správa organizace",
          Items = new List<NativeMenuItem>
          {
            new NativeMenuItem
            {
              Name = "Přidat člena",
              GetSubMenuAsync = getOrganizationInviteMenu
            },
            new NativeMenuItem
            {
              Name = "Členové",
              GetSubMenu = getMembersMenu
            },
            new NativeMenuItem
            {
              Name = "Zpět",
              IsBack = true
            },
          }
        },
      });

      return menu;
    }

    public static NativeMenu getMembersMenu()
    {
      var menu = new NativeMenu
      {
        MenuTitle = "Členové organizace",
        Items = new List<NativeMenuItem>
        {
          new NativeMenuItem
          {
            Name = "Zpět",
            IsBack = true
          }
        }
      };

      if (CurrentOrganization?.Members == null)
        return menu;

      foreach (var member in CurrentOrganization.Members)
      {
        menu.Items.Insert(0, new NativeMenuItem {
          Name = member.CharName,
          GetSubMenu = () => { return getMemberMenu(member.CharId, member.CharName); }
        });
      }

      return menu;
    }

    public static NativeMenu getMemberMenu(int charId, string name)
    {
      return new NativeMenu
      {
        MenuTitle = name,
        Items = new List<NativeMenuItem>
        {
          new NativeMenuItem
          {
            Name = "Manager",
            NameRight = CurrentOrganization.Managers.Any(m => m.CharId == charId) ? "✅" : "❌",
            OnSelected = async (item) => 
            {
              bool isManager = CurrentOrganization.Managers.Any(m => m.CharId == charId);
              var res = await Callbacks.ServerAsyncCallbackToSync<bool>("ofw_org:SetManager", charId, !isManager);
              if (res == true)
                item.NameRight = !isManager ? "✅" : "❌";
            }
          },
          new NativeMenuItem
          { 
            Name = "Vyhodit",
            SubMenu = new NativeMenu
            { 
              MenuTitle = $"Vyhodit {name}?",
              Items = new List<NativeMenuItem>
              {
                new NativeMenuItem
                {
                  Name = "Ne",
                  IsBack = true
                },
                new NativeMenuItem
                {
                  Name = "Ano",
                  OnSelected = (item) =>
                  {
                    TriggerServerEvent("ofw_org:KickMember", charId);
                  },
                  IsClose = true
                }
              }
            },
          },
          new NativeMenuItem
          {
            Name = "Zpět",
            IsBack = true
          }
        }
      };
    }

    public static async Task<NativeMenu> getOrganizationCarsMenu()
    {
      NativeMenu menu = new NativeMenu
      {
        MenuTitle = "Auta organizace",
        Items = new List<NativeMenuItem>
        {
          new NativeMenuItem
            {
              Name = "Zpět",
              IsBack = true
            },
        }
      };

      var vehiclesString = await Callbacks.ServerAsyncCallbackToSync<string>("ofw_garage:GetOrganizationVehicles");
      if (string.IsNullOrEmpty(vehiclesString))
        return menu;

      var vehicles = JsonConvert.DeserializeObject<List<GarageVehicleBag>>(vehiclesString);

      if (vehicles == null || vehicles.Count <= 0)
        return menu;

      foreach (var veh in vehicles)
      {
        var item = new NativeMenuItem
        {
          Name = $"{veh.Plate.ToUpper()} - {GetDisplayNameFromVehicleModel((uint)veh.Properties.model)}",
          NameRight = veh.IsOut ? "V provozu" : (GarageClient.Garages.Where(g => g.Place == veh.Place).FirstOrDefault()?.Name ?? veh.Place),
          //OnSelected = async (item) => {
          //  await Delay(0);
          //  BaseScript.TriggerServerEvent("ofw_org:InvitePlayerToOrg", p.ServerId);
          //},
          //IsClose = true
        };

        if (CharacterCaretaker.LoggedCharacter?.Id == CurrentOrganization?.Owner || CurrentOrganization.Managers.Any(m => m.CharId == CharacterCaretaker.LoggedCharacter?.Id))
        {
          item.GetSubMenuAsync = async () => { return await getVehicleRightsMenu(veh.Id, veh.Plate); };
        }

        menu.Items.Insert(0, item);
          
      }

      return menu;
    }

    public static async Task<NativeMenu> getVehicleRightsMenu(int vehicleId, string vehicleName)
    {
      NativeMenu menu = new NativeMenu
      {
        MenuTitle = $"Oprávnění: {vehicleName}",
        Items = new List<NativeMenuItem>
        {

        }
      };

      if (CurrentOrganization?.Members == null || CurrentOrganization.Members.Count <= 0)
        return menu;

      await Delay(0);
      string rightDataString = await Callbacks.ServerAsyncCallbackToSync<string>("ofw_org:GetOrgVehicleRights", vehicleId);
      List<OrganizationVehicleRightsBag> rights = null;
      if (!string.IsNullOrEmpty(rightDataString))
      {
        rights = JsonConvert.DeserializeObject<List<OrganizationVehicleRightsBag>>(rightDataString);
      }

      foreach (var member in CurrentOrganization.Members)
      {
        menu.Items.Add(new NativeMenuItem 
        {
          MetaData = new OrganizationVehicleRightsBag { CharId = member.CharId, VehicleId = vehicleId },
          Name = member.CharName,
          NameRight = rights?.Any(r => r.CharId == member.CharId) == true ? "✅" : "❌",
          OnSelected = async (item) =>
          {
            var bag = item.MetaData as OrganizationVehicleRightsBag;
            if (bag == null)
              return;

            await Delay(0);
            var res = await Callbacks.ServerAsyncCallbackToSync<bool>("ofw_org:SetVehicleRights", bag.VehicleId, bag.CharId, item.NameRight == "✅" ? false : true);
            item.NameRight = res ? "✅" : "❌";
          }
        });
      }

      return menu;
    }

    public static async Task<NativeMenu> getOrganizationInviteMenu()
    {
      NativeMenu menu = new NativeMenu
      {
        MenuTitle = "Přidat člena",
        Items = new List<NativeMenuItem>()
      };

      var localPlayers = Main.LocalPlayers.ToList();

      foreach (var p in localPlayers)
      {
        if (p.ServerId == Game.Player.ServerId)
          continue;

        if (GetEntityCoords(GetPlayerPed(p.Handle), false).DistanceToSquared2D(Game.PlayerPed.Position) > 100)
          continue;

        //var charDataString = await Callbacks.ServerAsyncCallbackToSync<string>("ofw_char:GetPlayerCharacter", p.ServerId);
        //if (string.IsNullOrEmpty(charDataString))
        //  continue;

        //var charData = JsonConvert.DeserializeObject<CharacterBag>(charDataString);

        menu.Items.Add(
          new NativeMenuItem
          {
            Name = $"ID:{p.ServerId}",
            OnSelected = async (item) => {
              await Delay(0);
              BaseScript.TriggerServerEvent("ofw_org:InvitePlayerToOrg", p.ServerId);
            },
            IsClose = true
          });
      }

      string backSuffix = menu.Items.Count <= 0 ? " - nikdo poblíž není" : string.Empty;
      menu.Items.Add(new NativeMenuItem { Name = "Zpět" + backSuffix, IsBack = true });

      return menu;
    }

    [EventHandler("ofw_org:OrganizationUpdated")]
    private void OrganizationUpdated(string orgData)
    {
      if (string.IsNullOrEmpty(orgData))
      {
        CurrentOrganization = null;
      }

      CurrentOrganization = JsonConvert.DeserializeObject<OrganizationBag>(orgData);
    }

    [EventHandler("ofw_org:OrganizationInvited")]
    private void OrganizationInvited(string orgName)
    {
      NativeMenuManager.OpenNewMenu(MenuInviteName, () => {
        return new NativeMenu
        {
          MenuTitle = $"Pozvánka do organizace",
          SelectedIndex = 2,
          SelectedIndexVisible = 2,
          Items = new List<NativeMenuItem>
          {
            new NativeMenuItem {
              Name = orgName,
              IsUnselectable = true,
            },
            new NativeMenuItem {
              IsUnselectable = true,
            },
            new NativeMenuItem
            {
              Name = "Přijmout",
              OnSelected = async (item) =>
              {
                await Delay(0);
                BaseScript.TriggerServerEvent("ofw_org:AcceptInvite");
              },
              IsClose = true
            },
            new NativeMenuItem
            {
              Name = "Odmítnout",
              OnSelected = async (item) =>
              {
                await Delay(0);
                BaseScript.TriggerServerEvent("ofw_org:DeclineInvite");
              },
              IsClose = true
            }
          }
        };
      });
    }
  }
}
