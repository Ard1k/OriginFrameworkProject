using CitizenFX.Core;
using Newtonsoft.Json;
using OriginFramework.Menus;
using OriginFrameworkData;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
	public class TuningCatalogClient : BaseScript
	{
    public static string MenuName { get { return "tuning_catalog_menu"; } }
    public static int CurrentVehicle = 0;
    public static int CurrentVehicleClass = 0;
    public static int CurrentVehicleModel = 0;
    public static VehiclePropertiesBag OriginalProperties = null;
    public static VehiclePropertiesBag RequestedProperties = null;

    public TuningCatalogClient()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      Tick += OnTick;
    }

    private async Task OnTick()
		{
      if (CurrentVehicle == 0)
      {
        if (NativeMenuManager.IsMenuOpen(MenuName))
          NativeMenuManager.CloseAndUnlockMenu(MenuName);

        await Delay(250);
        return;
      }

      int vehCheck = GetVehiclePedIsIn(Game.PlayerPed.Handle, false);

      if (vehCheck != CurrentVehicle || !NativeMenuManager.IsMenuOpen(MenuName))
      {
        if (OriginalProperties != null)
          Vehicles.SetVehicleProperties(CurrentVehicle, OriginalProperties);
        if (RequestedProperties != null)
        {
          TriggerServerEvent("ofw_veh:UpdateRequestedTuning", VehToNet(CurrentVehicle), GetEntityModel(CurrentVehicle), JsonConvert.SerializeObject(RequestedProperties));
          RequestedProperties = null;
        }
        CurrentVehicle = 0;
        OriginalProperties = null;
      }
		}

    public static async void OpenCatalog()
    {
      CurrentVehicle = GetVehiclePedIsIn(Game.PlayerPed.Handle, false);
      if (CurrentVehicle == 0)
      {
        Notify.Alert("Pro otevření katalogu musíš být ve vozidle");
        return;
      }
      CurrentVehicleModel = GetEntityModel(CurrentVehicle);
      CurrentVehicleClass = GetVehicleClass(CurrentVehicle);
      SetVehicleModKit(CurrentVehicle, 0);
      OriginalProperties = Vehicles.GetVehicleProperties(CurrentVehicle);

      string reqData = await Callbacks.ServerAsyncCallbackToSync<string>("ofw_veh:GetRequestedTuning", VehToNet(CurrentVehicle), CurrentVehicleModel);
      if (!string.IsNullOrEmpty(reqData) && reqData != "nodata")
      {
        RequestedProperties = JsonConvert.DeserializeObject<VehiclePropertiesBag>(reqData);
        Vehicles.SetVehicleProperties(CurrentVehicle, RequestedProperties);
      }
      else
        RequestedProperties = new VehiclePropertiesBag();
      NativeMenuManager.OpenNewMenu(MenuName, getCatalogMenu);
    }

    private static NativeMenu getCatalogMenu()
    {
      if (CurrentVehicle == 0)
      {
        return null;
      }

      var menu = new NativeMenu
      {
        MenuTitle = "Katalog",
        Items = new List<NativeMenuItem>
          {
            new NativeMenuItem
            {
              Name = "Barvy karoserie",
              NameRight = ">>>",
              GetSubMenu = getColorsMenu
            },
            new NativeMenuItem
            {
              Name = "Kola",
              NameRight = ">>>",
              GetSubMenu = getWheelsMenu
            },
            new NativeMenuItem
            {
              Name = "Výkon",
              NameRight = ">>>",
              GetSubMenu = getPerformanceMenu
            },
            new NativeMenuItem
            {
              Name = "Vzhled",
              NameRight = ">>>",
              GetSubMenu = getVisualMenu
            },
          }
      };

      return menu;
    }

    #region performance
    private static NativeMenu getPerformanceMenu()
    {
      int[] tuningSubs = { 11, 12, 13, 15, 18 };

      var menu = new NativeMenu
      {
        MenuTitle = "Výkon",
        Items = new List<NativeMenuItem>
          {
          }
      };

      foreach (var tuningType in tuningSubs)
      {
        var it = getTuningMenuItem(tuningType);
        if (it != null)
          menu.Items.Add(it);
      }

      return menu;
    }
    #endregion

    #region visual
    private static NativeMenu getVisualMenu()
    {
      int[] tuningSubs = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 25, 32, 33, 36, 37, 42, 43, 44, 45, 46, 47, 48 };

      var menu = new NativeMenu
      {
        MenuTitle = "Vzhled",
        Items = new List<NativeMenuItem>
        {
        }
      };

      foreach (var tuningType in tuningSubs)
      {
        var it = getTuningMenuItem(tuningType);
        if (it != null)
          menu.Items.Add(it);
      }

      return menu;
    }
    #endregion

    private static NativeMenuItem getTuningMenuItem(int tuningType)
    {
      VehTuningTypeDefinition tuning = null;
      if (VehTuningTypeDefinition.Defined.ContainsKey(tuningType))
        tuning = VehTuningTypeDefinition.Defined[tuningType];

      if (tuning == null)
        return null;

      string menu_name = GetModSlotName(CurrentVehicle, tuningType);
      if (string.IsNullOrWhiteSpace(menu_name))
        menu_name = tuning?.Name ?? "Unknown";
      if (menu_name.Contains("_"))
      {
        var lbl = GetLabelText(menu_name);
        if (lbl != "NULL")
          menu_name = lbl;
      }

      if (tuning != null && !tuning.IsToggle && GetNumVehicleMods(CurrentVehicle, tuningType) <= 0)
        return null;

      return new NativeMenuItem
      {
        Name = menu_name,
        NameRight = ">>>",
        GetSubMenu = () => { return tuning.IsToggle ? getGenericToggleSubmenu(tuningType) : getGenericTuningSubmenu(tuningType); },
        OnRefresh = (item) =>
        {
          if (tuning.IsToggle)
          {
            ToggleVehicleMod(CurrentVehicle, tuningType, (bool?)(RequestedProperties.tunings[tuningType] ?? OriginalProperties.tunings[tuningType]) ?? false);
          }
          else
            SetVehicleMod(CurrentVehicle, tuningType, (int)(RequestedProperties.tunings[tuningType] ?? OriginalProperties.tunings[tuningType] ?? -1), false);
        }
      };
    }

    private static NativeMenu getGenericTuningSubmenu(int tuningType)
    {
      string menu_name = GetModSlotName(CurrentVehicle, tuningType);
      if (string.IsNullOrWhiteSpace(menu_name))
        menu_name = VehTuningTypeDefinition.Defined[tuningType]?.Name ?? "Unknown";
      if (menu_name.Contains("_"))
      {
        var lbl = GetLabelText(menu_name);
        if (lbl != "NULL")
          menu_name = lbl;
      }

      var menu = new NativeMenu
      {
        MenuTitle = $"{menu_name}",
        Items = new List<NativeMenuItem>
        {
        }
      };

      int modCount = GetNumVehicleMods(CurrentVehicle, tuningType);
      if (modCount > 0)
      {
        for (int i = -1; i < modCount; i++)
        {
          int i2 = i;

          string mod_label = GetModTextLabel(CurrentVehicle, tuningType, i2);
          if (mod_label == null || mod_label == string.Empty)
            mod_label = i2 == -1 ? "Stock/None" : $"{VehTuningTypeDefinition.Defined[tuningType].Name} - {i2 + 1}";
          if (mod_label.Contains("_"))
            mod_label = GetLabelText(mod_label);

          menu.Items.Add(new NativeMenuItem
          {
            Name = mod_label,
            NameRight = ((RequestedProperties.tunings[tuningType] ?? OriginalProperties.tunings[tuningType]) as int? == i2) ? "✅" : String.Empty,
            OnHover = () => { SetVehicleMod(CurrentVehicle, tuningType, i2, false); },
            OnSelected = (item) => { RequestedProperties.tunings[tuningType] = i2; },
            OnRefresh = (item) => { item.NameRight = ((RequestedProperties.tunings[tuningType] ?? OriginalProperties.tunings[tuningType]) as int? == i2) ? "✅" : String.Empty; },
            IsRefresh = true,
          });
        }
      }

      return menu;
    }

    private static NativeMenu getGenericToggleSubmenu(int tuningType)
    {
      string menu_name = GetModSlotName(CurrentVehicle, tuningType);
      if (string.IsNullOrWhiteSpace(menu_name))
        menu_name = VehTuningTypeDefinition.Defined[tuningType]?.Name ?? "Unknown";
      if (menu_name.Contains("_"))
      {
        var lbl = GetLabelText(menu_name);
        if (lbl != "NULL")
          menu_name = lbl;
      }

      var menu = new NativeMenu
      {
        MenuTitle = $"{menu_name}",
        Items = new List<NativeMenuItem>
        {
        }
      };

      menu.Items.Add(new NativeMenuItem
      {
        Name = "Stock/None",
        NameRight = ((bool?)(RequestedProperties.tunings[tuningType] ?? OriginalProperties.tunings[tuningType]) ?? false) == false ? "✅" : String.Empty,
        OnHover = () => { ToggleVehicleMod(CurrentVehicle, tuningType, false); },
        OnSelected = (item) => { RequestedProperties.tunings[tuningType] = false; },
        OnRefresh = (item) => { item.NameRight = ((bool?)(RequestedProperties.tunings[tuningType] ?? OriginalProperties.tunings[tuningType]) ?? false) == false ? "✅" : String.Empty; },
        IsRefresh = true,
      });

      menu.Items.Add(new NativeMenuItem
      {
        Name = $"{menu_name}",
        NameRight = ((bool?)(RequestedProperties.tunings[tuningType] ?? OriginalProperties.tunings[tuningType]) ?? false) == true ? "✅" : String.Empty,
        OnHover = () => { ToggleVehicleMod(CurrentVehicle, tuningType, true); },
        OnSelected = (item) => { RequestedProperties.tunings[tuningType] = true; },
        OnRefresh = (item) => { item.NameRight = ((bool?)(RequestedProperties.tunings[tuningType] ?? OriginalProperties.tunings[tuningType]) ?? false) == true ? "✅" : String.Empty; },
        IsRefresh = true,
      });

      return menu;
    }

    #region colors
    private static NativeMenu getColorsMenu()
    {
      var menu = new NativeMenu
      {
        MenuTitle = "Barvy",
        Items = new List<NativeMenuItem>
          {
            new NativeMenuItem
            {
              Name = "Primární",
              NameRight = ">>>",
              GetSubMenu = () => { return getColorMenu(0, "Primární"); },
              OnRefresh = (item) =>
              {
                SetVehicleColours(CurrentVehicle, RequestedProperties?.color1 ?? OriginalProperties?.color1 ?? 0, RequestedProperties?.color2 ?? OriginalProperties?.color2 ?? 0);
              }
            },
            new NativeMenuItem
            {
              Name = "Sekundární",
              NameRight = ">>>",
              GetSubMenu = () => { return getColorMenu(1, "Sekundární"); },
              OnRefresh = (item) =>
              {
                //refresh obstara primarni polozka
              }
            },
            new NativeMenuItem
            {
              Name = "Perleť",
              NameRight = ">>>",
              GetSubMenu = () => { return getColorMenu(2, "Perleť"); },
              OnRefresh = (item) =>
              {
                SetVehicleExtraColours(CurrentVehicle, RequestedProperties?.pearlescentColor ?? OriginalProperties?.pearlescentColor ?? 0, RequestedProperties?.wheelColor ?? OriginalProperties?.wheelColor ?? 0);
              }
            },
          }
      };

      return menu;
    }

    private static NativeMenu getColorMenu(int type, string title)
    {
      var menu = new NativeMenu
      {
        MenuTitle = title,
        Items = new List<NativeMenuItem>
        {
        }
      };

      foreach (eVehColorCategory colorCat in Enum.GetValues(typeof(eVehColorCategory)))
      {
        var colors = VehColor.Defined.Where(c => c.Category == colorCat && c.IsUnused != true).ToList();
        if (colors == null || colors.Count <= 0)
          continue;

        switch (type)
        {
          case 0:
            menu.Items.Add(new NativeMenuItem
            {
              Name = $"{colorCat}",
              NameRight = (colors.Any(col => col.ColorIndex == (RequestedProperties?.color1 ?? OriginalProperties?.color1 ?? 0))) ? "✅" : String.Empty,
              OnSelected = (item) =>
              {
                SetVehicleColours(CurrentVehicle, colors[0].ColorIndex, RequestedProperties?.color2 ?? OriginalProperties?.color2 ?? 0);
              },
              GetSubMenu = () => { return getColorSubmenu(colors, type); },
              OnRefresh = (item) =>
              {
                item.NameRight = (colors.Any(col => col.ColorIndex == (RequestedProperties?.color1 ?? OriginalProperties?.color1 ?? 0))) ? "✅" : String.Empty;
              },
              IsRefresh = true,
            });
            break;
          case 1:
            menu.Items.Add(new NativeMenuItem
            {
              Name = $"{colorCat}",
              NameRight = (colors.Any(col => col.ColorIndex == (RequestedProperties?.color2 ?? OriginalProperties?.color2 ?? 0))) ? "✅" : String.Empty,
              OnSelected = (item) =>
              {
                SetVehicleColours(CurrentVehicle, RequestedProperties?.color1 ?? OriginalProperties?.color1 ?? 0, colors[0].ColorIndex);
              },
              GetSubMenu = () => { return getColorSubmenu(colors, type); },
              OnRefresh = (item) =>
              {
                item.NameRight = (colors.Any(col => col.ColorIndex == (RequestedProperties?.color2 ?? OriginalProperties?.color2 ?? 0))) ? "✅" : String.Empty;
              },
              IsRefresh = true,
            });
            break;
          case 2:
            menu.Items.Add(new NativeMenuItem
            {
              Name = $"{colorCat}",
              NameRight = (colors.Any(col => col.ColorIndex == (RequestedProperties?.pearlescentColor ?? OriginalProperties?.pearlescentColor ?? 0))) ? "✅" : String.Empty,
              OnSelected = (item) =>
              {
                SetVehicleExtraColours(CurrentVehicle, colors[0].ColorIndex, RequestedProperties?.wheelColor ?? OriginalProperties?.wheelColor ?? 0);
              },
              GetSubMenu = () => { return getColorSubmenu(colors, type); },
              OnRefresh = (item) =>
              {
                item.NameRight = (colors.Any(col => col.ColorIndex == (RequestedProperties?.pearlescentColor ?? OriginalProperties?.pearlescentColor ?? 0))) ? "✅" : String.Empty;
              },
              IsRefresh = true,
            });
            break;
        }
      }

      return menu;
    }

    private static NativeMenu getColorSubmenu(List<VehColor> colors, int type)
    {
      var menu = new NativeMenu
      {
        MenuTitle = $"{colors[0].Category}",
        Items = new List<NativeMenuItem>
        {
        }
      };

      foreach (var c in colors)
      {
        switch (type)
        {
          case 0:
            menu.Items.Add(new NativeMenuItem
            {
              Name = $"[{c.ColorIndex}]{c.ColorName}",
              NameRight = ((RequestedProperties?.color1 ?? OriginalProperties?.color1 ?? 0) == c.ColorIndex) ? "✅" : String.Empty,
              OnSelected = (item) =>
              {
                RequestedProperties.color1 = c.ColorIndex;
              },
              OnHover = () =>
              {
                SetVehicleColours(CurrentVehicle, c.ColorIndex, RequestedProperties?.color2 ?? OriginalProperties?.color2 ?? 0);
              },
              OnRefresh = (item) =>
              {
                item.NameRight = ((RequestedProperties?.color1 ?? OriginalProperties?.color1 ?? 0) == c.ColorIndex) ? "✅" : String.Empty;
              },
              IsRefresh = true,
            });
            break;
          case 1:
            menu.Items.Add(new NativeMenuItem
            {
              Name = $"[{c.ColorIndex}]{c.ColorName}",
              NameRight = ((RequestedProperties?.color2 ?? OriginalProperties?.color2 ?? 0) == c.ColorIndex) ? "✅" : String.Empty,
              OnSelected = (item) =>
              {
                RequestedProperties.color2 = c.ColorIndex;
              },
              OnHover = () =>
              {
                SetVehicleColours(CurrentVehicle, RequestedProperties?.color1 ?? OriginalProperties?.color1 ?? 0, c.ColorIndex);
              },
              OnRefresh = (item) =>
              {
                item.NameRight = ((RequestedProperties?.color2 ?? OriginalProperties?.color2 ?? 0) == c.ColorIndex) ? "✅" : String.Empty;
              },
              IsRefresh = true,
            });
            break;
          case 2:
            menu.Items.Add(new NativeMenuItem
            {
              Name = $"[{c.ColorIndex}]{c.ColorName}",
              NameRight = ((RequestedProperties?.pearlescentColor ?? OriginalProperties?.pearlescentColor ?? 0) == c.ColorIndex) ? "✅" : String.Empty,
              OnSelected = (item) =>
              {
                RequestedProperties.pearlescentColor = c.ColorIndex;
              },
              OnHover = () =>
              {
                SetVehicleExtraColours(CurrentVehicle, c.ColorIndex, RequestedProperties?.wheelColor ?? OriginalProperties?.wheelColor ?? 0);
              },
              OnRefresh = (item) =>
              {
                item.NameRight = ((RequestedProperties?.pearlescentColor ?? OriginalProperties?.pearlescentColor ?? 0) == c.ColorIndex) ? "✅" : String.Empty;
              },
              IsRefresh = true,
            });
            break;
        }
      }

      return menu;
    }
    #endregion

    #region wheels
    private static NativeMenu getWheelsMenu()
    {
      var menu = new NativeMenu
      {
        MenuTitle = "Kola",
        Items = new List<NativeMenuItem>
          {
            new NativeMenuItem
            {
              Name = "Změnit kola",
              NameRight = ">>>",
              GetSubMenu = getWheelTypeMenu,
              OnRefresh = (item) =>
              {
                if (RequestedProperties?.wheels != null)
                  SetVehicleWheelType(CurrentVehicle, RequestedProperties.wheels ?? 0);
                else
                  SetVehicleWheelType(CurrentVehicle, OriginalProperties?.wheels ?? 0);

                if (RequestedProperties?.tunings[23] != null)
                  SetVehicleMod(CurrentVehicle, 23, (int)(RequestedProperties.tunings[23] ?? -1), false);
                else
                  SetVehicleMod(CurrentVehicle, 23, (int)(OriginalProperties?.tunings[23] ?? -1), false);
              }
            },
            new NativeMenuItem
            {
              Name = "Změnit barvu",
              NameRight = ">>>",
              GetSubMenu = getWheelColorMenu,
              OnRefresh = (item) =>
              {
                SetVehicleExtraColours(CurrentVehicle, RequestedProperties?.pearlescentColor ?? OriginalProperties?.pearlescentColor ?? 0, RequestedProperties?.wheelColor ?? OriginalProperties?.wheelColor ?? 0);
              }
            },
          }
      };

      return menu;
    }

    private static NativeMenu getWheelColorMenu()
    {
      var menu = new NativeMenu
      {
        MenuTitle = "Barva kol",
        Items = new List<NativeMenuItem>
        {
        }
      };

      foreach (eVehColorCategory colorCat in Enum.GetValues(typeof(eVehColorCategory)))
      {
        var colors = VehColor.Defined.Where(c => c.Category == colorCat && c.IsUnused != true).ToList();
        if (colors == null || colors.Count <= 0)
          continue;

        menu.Items.Add(new NativeMenuItem
        {
          Name = $"{colorCat}",
          NameRight = (colors.Any(col => col.ColorIndex == (RequestedProperties?.wheelColor ?? OriginalProperties?.wheelColor ?? 0))) ? "✅" : String.Empty,
          OnSelected = (item) =>
          {
            SetVehicleExtraColours(CurrentVehicle, RequestedProperties?.pearlescentColor ?? OriginalProperties?.pearlescentColor ?? 0, colors[0].ColorIndex);
          },
          GetSubMenu = () => { return getWheelColorSubmenu(colors); },
          OnRefresh = (item) =>
          {
            item.NameRight = (colors.Any(col => col.ColorIndex == (RequestedProperties?.wheelColor ?? OriginalProperties?.wheelColor ?? 0))) ? "✅" : String.Empty;
          },
          IsRefresh = true,
        });
      }

      return menu;
    }

    private static NativeMenu getWheelColorSubmenu(List<VehColor> colors)
    {
      var menu = new NativeMenu
      {
        MenuTitle = $"{colors[0].Category}",
        Items = new List<NativeMenuItem>
        {
        }
      };

      //SetVehicleExtraColours(CurrentVehicle, RequestedProperties?.pearlescentColor ?? OriginalProperties?.pearlescentColor ?? 0, colors[0].ColorIndex);

      foreach (var c in colors)
      {
        menu.Items.Add(new NativeMenuItem
        {
          Name = $"[{c.ColorIndex}]{c.ColorName}",
          NameRight = ((RequestedProperties?.wheelColor ?? OriginalProperties?.wheelColor ?? 0) == c.ColorIndex) ? "✅" : String.Empty,
          OnSelected = (item) =>
          {
            RequestedProperties.wheelColor = c.ColorIndex;
          },
          OnHover = () =>
          {
            SetVehicleExtraColours(CurrentVehicle, RequestedProperties?.pearlescentColor ?? OriginalProperties?.pearlescentColor ?? 0, c.ColorIndex);
          },
          OnRefresh = (item) =>
          {
            item.NameRight = ((RequestedProperties?.wheelColor ?? OriginalProperties?.wheelColor ?? 0) == c.ColorIndex) ? "✅" : String.Empty;
          },
          IsRefresh = true,
        });
      }

      return menu;
    }

    private static NativeMenu getWheelTypeMenu()
    {
      string[] wheelTypeNames =
      {
        "Sportovní",
        "Muscle",
        "Lowrider",
        "SUV",
        "Offroad",
        "Tuner",
        "Bike",
        "Highend",
        "Benny's Original",
        "Benny's Bespoke",
        "Formula",
        "Street",
        "Závodní"
      };

      var menu = new NativeMenu
      {
        MenuTitle = "Změnit kola",
        Items = new List<NativeMenuItem>
          {
          }
      };

      for (int i = 0; i <= 12; i++)
      {
        int i2 = i; //reference... zase :D


        menu.Items.Add(new NativeMenuItem
        {
          Name = wheelTypeNames[i2],
          NameRight = (RequestedProperties?.wheels ?? OriginalProperties?.wheels ?? 0) == i2 ? "✅" : String.Empty,
          OnRefresh = (item) => 
          {
            item.NameRight = (RequestedProperties?.wheels ?? OriginalProperties?.wheels ?? 0) == i2 ? "✅" : String.Empty;
          },
          GetSubMenu = () => { return getWheelTypeMenu(wheelTypeNames[i2], i2); },
          IsRefresh = true,
        });
      }

      return menu;
    }

    private static NativeMenu getWheelTypeMenu(string title, int wheelType)
    {
      var menu = new NativeMenu
      {
        MenuTitle = title,
        Items = new List<NativeMenuItem>
        {
        }
      };

      SetVehicleWheelType(CurrentVehicle, wheelType);
      SetVehicleMod(CurrentVehicle, 23, -1, false);
      int maxWheels = GetNumVehicleMods(CurrentVehicle, 23);

      for (int i = -1; i < maxWheels; i++)
      {
        int i2 = i;

        string wheelName = null;
        if (i2 == -1)
          wheelName = "Stock";
        else
          wheelName = GetModTextLabel(CurrentVehicle, 23, i2);

        if (wheelName != null && wheelName.Contains("_"))
          wheelName = GetLabelText(wheelName);

        menu.Items.Add(new NativeMenuItem
        {
          Name = wheelName,
          NameRight = (((RequestedProperties?.wheels ?? OriginalProperties?.wheels ?? -1) == wheelType) && ((int)(RequestedProperties?.tunings[23] ?? OriginalProperties?.tunings[23] ?? -1) == i2)) ? "✅" : String.Empty,
          OnSelected = (item) =>
          {
            RequestedProperties.wheels = GetVehicleWheelType(CurrentVehicle);
            RequestedProperties.tunings[23] = i2;
          },
          OnHover = () => 
          {
            SetVehicleMod(CurrentVehicle, 23, i2, false);
          },
          OnRefresh = (item) =>
          {
            item.NameRight = (((RequestedProperties?.wheels ?? OriginalProperties?.wheels ?? -1) == wheelType) && ((int)(RequestedProperties?.tunings[23] ?? OriginalProperties?.tunings[23] ?? -1) == i2)) ? "✅" : String.Empty;
          },
          IsRefresh = true,
        });
      }

      return menu;
    }
    #endregion
  }
}
