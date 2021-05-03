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
  public class Tools
  {
    Menu toolsMenu = new Menu("OriginRP", "Dev Tools");
    Menu checkpointTool = new Menu("OriginRP", "Checkpoint tool");
    MenuCheckboxItem boostItem = new MenuCheckboxItem("Shift boost", "Press SHIFT to boost your car.", false);

    bool isInitialized = false;

    private async void CreateMenus()
    {
      MenuController.AddMenu(toolsMenu);
      MenuController.AddMenu(checkpointTool);

      MenuItem checkpointToolItem = new MenuItem("Checkpoint tool", "Create checkpoint definition to clickboard");
      checkpointToolItem.Label = "→→→";
      toolsMenu.AddMenuItem(checkpointToolItem);
      MenuCheckboxItem drawEntityInfoItem = new MenuCheckboxItem("Draw entity info", "Draws info on all entities", Misc.IsEntityInfoEnabled);
      toolsMenu.AddMenuItem(drawEntityInfoItem);
      MenuCheckboxItem markClosestVehAndTrunkItem = new MenuCheckboxItem("Mark closes vehicle trunk", "Draws marker over closest vehicle and marker over trunk colored depending on trunk open state", Misc.IsClosestVehicleTrunkInfoEnabled);
      toolsMenu.AddMenuItem(markClosestVehAndTrunkItem);

      MenuController.BindMenuItem(toolsMenu, checkpointTool, checkpointToolItem);

      #region tools checkpoint menu

      MenuItem checkpointBasicItem = new MenuItem("Indirectional", "Create checkpoint without heading and copy it to clipboard");
      MenuItem checkpointDirectionalItem = new MenuItem("Directional", "Create checkpoint with heading and copy it to clipboard");

      checkpointTool.AddMenuItem(checkpointBasicItem);
      checkpointTool.AddMenuItem(checkpointDirectionalItem);

      checkpointTool.RefreshIndex();

      checkpointTool.OnItemSelect += (sender, item, index) =>
      {
        if (item == checkpointBasicItem)
        {
          Checkpointer.SetCheckpointerActivate(Checkpointer.ExportMode.Indirectional);
          MenuController.CloseAllMenus();
        }
        else if (item == checkpointDirectionalItem)
        {
          Checkpointer.SetCheckpointerActivate(Checkpointer.ExportMode.Directional);
          MenuController.CloseAllMenus();
        }
      };

      #endregion

      toolsMenu.OnCheckboxChange += (sender, item, index, _checked) =>
      {
        if (item == boostItem)
        {
          if (_checked)
            DrifterAndBooster.EnableBooster();
          else
            DrifterAndBooster.DisableBooster();
        }

        if (item == drawEntityInfoItem)
        {
          Misc.IsEntityInfoEnabled = _checked;
          SetResourceKvpInt(KvpManager.getKvpString(KvpEnum.MiscDrawEntityInfo), _checked ? 1 : 0);
        }

        if (item == markClosestVehAndTrunkItem)
        {
          Misc.IsClosestVehicleTrunkInfoEnabled = _checked;
          SetResourceKvpInt(KvpManager.getKvpString(KvpEnum.ShowClosesVehicleTrunkInfo), _checked ? 1 : 0);
        }
      };

      isInitialized = true;
    }

    public void UnlockFunTools()
    {
      if (!isInitialized)
        return;

      toolsMenu.AddMenuItem(boostItem);
      toolsMenu.RefreshIndex();
    }

    public Menu GetMenu()
    {
      CreateMenus();
      return toolsMenu;
    }
  }
}
