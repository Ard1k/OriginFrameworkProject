using CitizenFX.Core;
using MenuAPI;
using OriginFramework.Menus;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;


namespace OriginFramework
{
  public class LuxuryCarDelivery : BaseScript
  {
    //car pos: -710.0746 641.9553 154.3442 349.0895

    private Dictionary<string, bool> JobNpcList = new Dictionary<string, bool> { { "LuxuryCarDelivery_01", false }, { "kokos", false } };
    private Control actionKey = Control.Context;

    public LuxuryCarDelivery()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
    }


    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (NPCClient.NPCs == null)
        await Delay(0);

      foreach (var n in NPCClient.NPCs)
      {
        switch (n.UniqueName)
        {
          case "LuxuryCarDelivery_01":
            n.OnInteraction = LuxuryCarDelivery_01_Interaction;
            break;
          default: break;
        }
      }
    }

    private void LuxuryCarDelivery_01_Interaction(int pid, string pname)
    {
      var localPlayers = Main.LocalPlayers.ToList();

      var dynMenuItems = new List<DynamicMenuItem>();

      dynMenuItems.Add(new DynamicMenuItem
      {
        TextLeft = $"Ses curak!",
        TextDescription = $"{pname} rika: Co chces? Praci? Myslis si ze na to mas? Neco bych mel, ale bylo by to dostr drsny.",
        OnClick = () => {
          var ped = new Ped(pid);
          SetPedScream(pid);
          MenuController.CloseAllMenus();
        }
      });

      dynMenuItems.Add(new DynamicMenuItem
      {
        TextLeft = $"Nic nechci!",
        TextDescription = $"{pname} rika: Co chces? Praci? Myslis si ze na to mas? Neco bych mel, ale bylo by to dostr drsny.",
        OnClick = () => {
          MenuController.CloseAllMenus();
        }
      });

      var dynMenuDef = new DynamicMenuDefinition
      {
        MainName = pname,
        Name = $"Co reknes?",
        Items = dynMenuItems
      };

      var dynMenu = new DynamicMenu(dynMenuDef);
      MenuController.CloseAllMenus();
      dynMenu.Menu.OpenMenu();
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
      
      
    }
  }
}
