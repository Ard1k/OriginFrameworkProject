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

namespace OriginFramework.Scripts
{
	public class OilIndustriesFaction : ESXFactionScriptBase
	{
		public OilIndustriesFaction() : base(eFaction.OilIndustries)
		{
      DataUpdated += OilIndustriesFaction_DataUpdated;
		}

    protected async override void OnClientResourceStart(string resourceName)
    {
      base.OnClientResourceStart(resourceName);
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      await RegisterNpcOnInteraction("OilIndustries_Manager", OilIndustries_Manager_Interaction);
    }

    private async void OilIndustriesFaction_DataUpdated(object sender, EventArgs e)
    {
      Debug.WriteLine("DataUpdated triggered");
    }

    private void OilIndustries_Manager_Interaction(int pid, string pname)
    {
      var localPlayers = Main.LocalPlayers.ToList();

      var dynMenuItems = new List<DynamicMenuItem>();

      if (DoesPlayerHaveJob())
      {
        dynMenuItems.Add(new DynamicMenuItem
        {
          TextLeft = $"Obstarat palivo",
          TextDescription = "Klienti po nas chteji palivo. Vem si cisternu a bez to obstarat.",
          OnClick = () =>
          {
            var ped = new Ped(pid);
            if (GroupManager.CheckGroupInQuestDistance(Game.PlayerPed.Position))
            {
              TriggerServerEvent("ofw_oilindustries:Test");
            }
            MenuController.CloseAllMenus();
          }
        });
      }

      dynMenuItems.Add(new DynamicMenuItem
      {
        TextLeft = $"Zavrit",
        OnClick = () => {
          MenuController.CloseAllMenus();
        }
      });

      var dynMenuDef = new DynamicMenuDefinition
      {
        MainName = pname,
        Name = $"Oil Industries",
        Items = dynMenuItems
      };

      var dynMenu = new DynamicMenu(dynMenuDef);
      MenuController.CloseAllMenus();
      dynMenu.Menu.OpenMenu();
    }
  }
}
