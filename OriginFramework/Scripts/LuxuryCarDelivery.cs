using CitizenFX.Core;
using MenuAPI;
using Newtonsoft.Json;
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
    private Dictionary<string, bool> JobNpcList = new Dictionary<string, bool> { { "LuxuryCarDelivery_01", false }, { "kokos", false } };
    private LCDJobStateBag JobState = null;


    public LuxuryCarDelivery()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
      EventHandlers["ofw_lcd:NewJobStateSent"] += new Action<string>(NewJobStateSent);
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

      string ret = null;
      bool completed = false;
      Func<string, bool> CallbackFunction = (data) =>
      {
        ret = data;
        completed = true;
        return true;
      };

      BaseScript.TriggerServerEvent("GetJobStateToRestore", CallbackFunction);

      while (!completed)
      {
        await Delay(0);
      }

      if (ret != null)
        JobState = JsonConvert.DeserializeObject<LCDJobStateBag>(ret);
    }

    private void LuxuryCarDelivery_01_Interaction(int pid, string pname)
    {
      var localPlayers = Main.LocalPlayers.ToList();

      var dynMenuItems = new List<DynamicMenuItem>();

      string npcTalk = $"{pname} rika: Co chces? Praci? Neco bych mel, ale neni to nic pro padavky. Myslis, ze na to mas?";
      if (JobState != null)
      {
        switch (JobState.CurrentState)
        {
          case LCDState.VehicleHunt: npcTalk = $"Tak mas pro me ty auta? Tak co po me chces!"; break;
        }
      }

      if (JobState == null)
      {
        dynMenuItems.Add(new DynamicMenuItem
        {
          TextLeft = $"Jasne, du do toho!",
          TextDescription = npcTalk,
          OnClick = () =>
          {
            var ped = new Ped(pid);
            if (GroupManager.CheckGroupInQuestDistance(Game.PlayerPed.Position))
            {
              TriggerServerEvent("ofw_lcd:StartJob");
            }
            MenuController.CloseAllMenus();
          }
        });
      }

      dynMenuItems.Add(new DynamicMenuItem
      {
        TextLeft = $"Nic nechci!",
        TextDescription = npcTalk,
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

    private async void NewJobStateSent(string sstate)
    {
      Debug.WriteLine(sstate);
      JobState = JsonConvert.DeserializeObject<LCDJobStateBag>(sstate);
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
      
      
    }
  }
}
