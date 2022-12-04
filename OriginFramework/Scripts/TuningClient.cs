using CitizenFX.Core;
using Newtonsoft.Json;
using OriginFramework.Menus;
using OriginFrameworkData;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
	public class TuningClient : BaseScript
	{
    public static string MenuName { get { return "tuning_catalog_menu"; } }
    public static int CurrentVehicle = 0;
    public static VehiclePropertiesBag OriginalProperties = null;
    public static VehiclePropertiesBag RequestedProperties = null;
    public static int colorType = 0;

    public TuningClient()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.CharacterCreator))
        return;

      Tick += OnTick;

      InternalDependencyManager.Started(eScriptArea.CharacterCreator);
    }

    private async Task OnTick()
		{
      if (CurrentVehicle == 0)
      {
        await Delay(250);
        return;
      }

      int vehCheck = GetVehiclePedIsIn(Game.PlayerPed.Handle, false);

      if (vehCheck != CurrentVehicle || !NativeMenuManager.IsMenuOpen(MenuName))
      {
        if (OriginalProperties != null)
          Vehicles.SetVehicleProperties(CurrentVehicle, OriginalProperties);
        if (RequestedProperties != null)
          RequestedProperties = null; //TODO - sync na server
        CurrentVehicle = 0;
        OriginalProperties = null;
      }
		}

    public static void OpenCatalog()
    {
      CurrentVehicle = GetVehiclePedIsIn(Game.PlayerPed.Handle, false);
      if (CurrentVehicle == 0)
      {
        Notify.Alert("Pro otevření katalogu musíš být ve vozidle");
        return;
      }
      SetVehicleModKit(CurrentVehicle, 0);
      OriginalProperties = Vehicles.GetVehicleProperties(CurrentVehicle);
      RequestedProperties = new VehiclePropertiesBag(); //TODO - sync ze serveru
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
              Name = "Kola",
              NameRight = ">>>",
              GetSubMenu = getWheelsMenu
            },
           
          }
      };

      return menu;
    }

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

                if (RequestedProperties?.modFrontWheels != null)
                  SetVehicleMod(CurrentVehicle, 23, RequestedProperties.modFrontWheels ?? -1, false);
                else
                  SetVehicleMod(CurrentVehicle, 23, OriginalProperties?.modFrontWheels ?? -1, false);
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
            new NativeMenuItem
            {
              Name = "Změnit barvu 2",
              NameRight = ">>>",
              GetSubMenu = getWheelColorMenu2,
              OnRefresh = (item) =>
              {
                //SetVehicleExtraColours(CurrentVehicle, RequestedProperties?.pearlescentColor ?? OriginalProperties?.pearlescentColor ?? 0, RequestedProperties?.wheelColor ?? OriginalProperties?.wheelColor ?? 0);
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

      SetVehicleColours(CurrentVehicle, colors[0].ColorIndex, colors[0].ColorIndex);
      SetVehicleExtraColours(CurrentVehicle, RequestedProperties?.pearlescentColor ?? OriginalProperties?.pearlescentColor ?? 0, colors[0].ColorIndex);

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
            SetVehicleColours(CurrentVehicle, c.ColorIndex, c.ColorIndex);
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

    private static NativeMenu getWheelColorMenu2()
    {
      var menu = new NativeMenu
      {
        MenuTitle = "Barva kol",
        Items = new List<NativeMenuItem>
        {
        }
      };

      for (int i = 0; i < 200; i++)
      {
        int i2 = i;

        var c = VehColor.Defined.Where(it => it.ColorIndex == i2).FirstOrDefault();
        string colorInfo = (c == null) ? "NotFound" : $"{c.Category}|{c.ColorName}";

        menu.Items.Add(new NativeMenuItem
        {
          Name = $"[{i2}] {colorInfo}",
          NameRight = ((RequestedProperties?.wheelColor ?? OriginalProperties?.wheelColor ?? 0) == i2) ? "✅" : String.Empty,
          OnSelected = (item) =>
          {
            RequestedProperties.wheelColor = i2;
          },
          OnHover = () =>
          {
            SetVehicleColours(CurrentVehicle, i2, i2);
            SetVehicleExtraColours(CurrentVehicle, RequestedProperties?.pearlescentColor ?? OriginalProperties?.pearlescentColor ?? 0, i2);
          },
          OnRefresh = (item) =>
          {
            item.NameRight = ((RequestedProperties?.wheelColor ?? OriginalProperties?.wheelColor ?? 0) == i2) ? "✅" : String.Empty;
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
          NameRight = (((RequestedProperties?.wheels ?? OriginalProperties?.wheels ?? -1) == wheelType) && ((RequestedProperties?.modFrontWheels ?? OriginalProperties?.modFrontWheels ?? -1) == i2)) ? "✅" : String.Empty,
          OnSelected = (item) =>
          {
            RequestedProperties.wheels = GetVehicleWheelType(CurrentVehicle);
            RequestedProperties.modFrontWheels = i2;
          },
          OnHover = () => 
          {
            SetVehicleMod(CurrentVehicle, 23, i2, false);
          },
          OnRefresh = (item) =>
          {
            item.NameRight = (((RequestedProperties?.wheels ?? OriginalProperties?.wheels ?? -1) == wheelType) && ((RequestedProperties?.modFrontWheels ?? OriginalProperties?.modFrontWheels ?? -1) == i2)) ? "✅" : String.Empty;
          },
          IsRefresh = true,
        });
      }

      return menu;
    }
    
  }
}
