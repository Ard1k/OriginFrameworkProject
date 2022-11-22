using OriginFramework.Helpers;
using OriginFrameworkData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework.Menus
{
  public static class MainMenu_DevTools
  {
    public static NativeMenu GenerateMenu()
    {
      NativeMenu menu = new NativeMenu
      {
        MenuTitle = "Dev nástroje",
        Items = new List<NativeMenuItem>
          {
            new NativeMenuItem { Name = "Checkpoint positioner", NameRight = ">>>", GetSubMenu = GenerateSubMenu_Checkpoint },
            new NativeMenuItem { Name = "Clothes editor", NameRight = ">>>", IsClose = true, OnSelected = (item) => { SkinEditor.EnterEditor(); } },
            new NativeMenuItem { Name = "Entity positioner", NameRight = ">>>", GetSubMenu = GenerateSubMenu_Entitier },
            new NativeMenuItem { Name = "Handling tool", NameRight = ">>>", GetSubMenu = Vehicle_HandlingInspector.GenerateMenu },
            new NativeMenuItem { Name = "Entity info", NameRight = Misc.IsEntityInfoEnabled ? "ZAP" : "VYP",
              OnSelected = (item) => {
                Misc.IsEntityInfoEnabled = !Misc.IsEntityInfoEnabled;
                item.NameRight = Misc.IsEntityInfoEnabled ? "ZAP" : "VYP";
                SetResourceKvpInt(KvpManager.getKvpString(KvpEnum.MiscDrawEntityInfo), Misc.IsEntityInfoEnabled ? 1 : 0);
              }
            },
            new NativeMenuItem { Name = "Vehicle trunk mark", NameRight = Misc.IsClosestVehicleTrunkInfoEnabled ? "ZAP" : "VYP",
            OnSelected = (item) => {
                Misc.IsClosestVehicleTrunkInfoEnabled = !Misc.IsClosestVehicleTrunkInfoEnabled;
                item.NameRight = Misc.IsClosestVehicleTrunkInfoEnabled ? "ZAP" : "VYP";
                SetResourceKvpInt(KvpManager.getKvpString(KvpEnum.MiscDrawEntityInfo), Misc.IsClosestVehicleTrunkInfoEnabled ? 1 : 0);
              }
            },
          }
      };

      return menu;
    }

    public static NativeMenu GenerateSubMenu_Checkpoint()
    {
      NativeMenu menu = new NativeMenu
      {
        MenuTitle = "Checkpoint tool",
        Items = new List<NativeMenuItem>
          {
            new NativeMenuItem { Name = "Indirectional", IsHide = true,
              OnSelected = (item) => {
                Checkpointer.SetCheckpointerActivate(Checkpointer.ExportMode.Indirectional);
              }
            },
            new NativeMenuItem { Name = "Directional", IsHide = true,
            OnSelected = (item) => {
                Checkpointer.SetCheckpointerActivate(Checkpointer.ExportMode.Directional);
              }
            },
          }
      };

      return menu;
    }

    public static NativeMenu GenerateSubMenu_Entitier()
    {
      NativeMenu menu = new NativeMenu
      {
        MenuTitle = "Entitier tool",
        Items = new List<NativeMenuItem>
          {
            new NativeMenuItem { Name = "Spawn", IsHide = true,
              OnSelected = (item) => {
                Entitier.SetEntitierActivate(Entitier.EntityMode.Spawn, null);
              }
            },
            //new NativeMenuItem { Name = "Copy info", IsHide = true,
            //OnSelected = (item) => {
            //    Entitier.SetEntitierActivate(Entitier.EntityMode.CopyInfo, null);
            //  }
            //},
            //new NativeMenuItem { Name = "Spawn and Copy", IsHide = true,
            //OnSelected = (item) => {
            //    Entitier.SetEntitierActivate(Entitier.EntityMode.SpawnAndCopyInfo, null);
            //  }
            //},
            new NativeMenuItem { Name = "Create map", IsHide = true,
            IsTextInput = true,
            OnTextInput = (item, input) => {
                Entitier.SetEntitierActivate(Entitier.EntityMode.MapEditor, MapHelper.InitializeNewMap(input));
              }
            },
          }
      };

      return menu;
    }
  }
}
