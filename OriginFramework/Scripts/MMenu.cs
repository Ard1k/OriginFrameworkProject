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
  public class MMenu : BaseScript
  {
    public string MenuName { get { return "mmenu_menu"; } }

    public MMenu()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.MMenu))
        return;

      Tick += OnTick;

      InternalDependencyManager.Started(eScriptArea.MMenu);
    }

    private async Task OnTick()
    {
      if (Game.IsControlJustPressed(0, Control.InteractionMenu)) //M
      {
        NativeMenuManager.ToggleMenu(MenuName, getMMenu);
      }
    }

    private static NativeMenu getMMenu()
    {
      if (CharacterCaretaker.LoggedCharacter == null)
        return null;

      var menu = new NativeMenu
      {
        MenuTitle = "Menu postavy",
        Items = new List<NativeMenuItem>
          {
            new NativeMenuItem
            {
              Name = "Auta",
              GetSubMenuAsync = getCarsMenu
            },
            new NativeMenuItem
            {
              Name = "Zavřít",
              IsClose = true
            }
          }
      };

      return menu;
    }

    public static async Task<NativeMenu> getCarsMenu()
    {
      NativeMenu menu = new NativeMenu
      {
        MenuTitle = "Auta",
        Items = new List<NativeMenuItem>
        {
          new NativeMenuItem
            {
              Name = "Zpět",
              IsBack = true
            },
        }
      };

      var vehiclesString = await Callbacks.ServerAsyncCallbackToSync<string>("ofw_garage:GetCharacterVehicles");
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
        };

        menu.Items.Insert(0, item);
      }

      return menu;
    }

    [EventHandler("ofw_mmenu:eventh")]
    private void OrganizationInvited(string orgName)
    {
      //NativeMenuManager.OpenNewMenu(MenuInviteName, () => {
      //  return new NativeMenu
      //  {
      //    MenuTitle = $"Pozvánka do organizace",
      //    SelectedIndex = 2,
      //    SelectedIndexVisible = 2,
      //    Items = new List<NativeMenuItem>
      //    {
      //      new NativeMenuItem {
      //        Name = orgName,
      //        IsUnselectable = true,
      //      },
      //      new NativeMenuItem {
      //        IsUnselectable = true,
      //      },
      //      new NativeMenuItem
      //      {
      //        Name = "Přijmout",
      //        OnSelected = async (item) =>
      //        {
      //          await Delay(0);
      //          BaseScript.TriggerServerEvent("ofw_org:AcceptInvite");
      //        },
      //        IsClose = true
      //      },
      //      new NativeMenuItem
      //      {
      //        Name = "Odmítnout",
      //        OnSelected = async (item) =>
      //        {
      //          await Delay(0);
      //          BaseScript.TriggerServerEvent("ofw_org:DeclineInvite");
      //        },
      //        IsClose = true
      //      }
      //    }
      //  };
      //});
    }
  }
}
