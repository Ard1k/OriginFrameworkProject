using CitizenFX.Core;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework.Menus
{
  public static class Vehicle_ModMenu
  {
    public static int Veh = 0;

    public static NativeMenu GenerateMenu()
    {
      Veh = GetVehiclePedIsIn(Game.PlayerPed.Handle, false);

      NativeMenu menu = new NativeMenu
      {
        MenuTitle = "Mods",
        Items = new List<NativeMenuItem>
          {
            new NativeMenuItem { Name = "Zpět", IsBack = true },
            
            new NativeMenuItem { IsUnselectable = true }
          }
      };

      if (Veh == 0)
        return menu;

      SetVehicleModKit(Veh, 0);

      for (int i = 0; i < 49; i++)
      {
        int i2 = i;

        VehTuningTypeDefinition tuning = null;
        if (VehTuningTypeDefinition.Defined.ContainsKey(i2))
          tuning = VehTuningTypeDefinition.Defined[i2];

        string menu_name = tuning?.Name ?? "Unknown";          
        if (menu_name.Contains("_"))
        {
          var lbl = GetLabelText(menu_name);
          if (lbl != "NULL")
            menu_name = lbl;
        }
        menu_name = $"[{i2}]" + menu_name;

        if (tuning != null && tuning.IsToggle)
        {
          menu.Items.Add(new NativeMenuItem
          {
            Name = menu_name,
            NameRight = IsToggleModOn(Veh, i2) ? "ON" : "OFF",
            OnSelected = (item) => 
            {
              bool isOn = IsToggleModOn(Veh, i2);
              ToggleVehicleMod(Veh, i2, !isOn);
              item.NameRight = !isOn ? "ON" : "OFF";
            }
          });
        }
        else if (GetNumVehicleMods(Veh, i2) > 0)
        {
          menu.Items.Add(new NativeMenuItem
          {
            Name = menu_name,
            NameRight = ">>>",
            GetSubMenu = () => { return GetModMenu(i2); }
          });
        }
      }

      return menu;
    }

    public static NativeMenu GetModMenu(int modType)
    {
      NativeMenu menu = new NativeMenu
      {
        MenuTitle = GetModSlotName(Veh, modType) ?? "UNK",
        Items = new List<NativeMenuItem>
          {
            new NativeMenuItem { Name = "Zpět", IsBack = true },

            new NativeMenuItem { IsUnselectable = true }
          }
      };

      int modCount = GetNumVehicleMods(Veh, modType);
      if (modCount > 0)
      {
        for (int i = -1; i < modCount; i++)
        {
          int i2 = i;

          string mod_label = GetModTextLabel(Veh, modType, i2);
          if (mod_label == null || mod_label == string.Empty)
            mod_label = i2 == -1 ? "Stock/None" : $"{VehTuningTypeDefinition.Defined[modType].Name} - {i2 + 1}";
          if (mod_label.Contains("_"))
            mod_label = GetLabelText(mod_label);

          menu.Items.Add(new NativeMenuItem
          {
            Name = mod_label,
            NameRight = (GetVehicleMod(Veh, modType) == i2).ToString(),
            OnSelected = (item) => { SetVehicleMod(Veh, modType, i2, false); },
            OnRefresh = (item) => { item.NameRight = (GetVehicleMod(Veh, modType) == i2).ToString(); },
            IsRefresh = true,
          });
        }
      }

      return menu;
    }
  }
}
