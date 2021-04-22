﻿using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using OriginFramework.Menus;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
  public class Main : BaseScript
  {
    public static string _version = null;
    public static string Version 
    { 
      get 
      {
        if (_version != null)
          return _version;

        _version = CitizenFX.Core.Native.API.GetResourceMetadata(CitizenFX.Core.Native.API.GetCurrentResourceName(), "version", 0);
        return _version;
      } 
    }

    public static Control MenuToggleKey { get { return MenuController.MenuToggleKey; } private set { MenuController.MenuToggleKey = value; } }

		#region Menu static variables
		public static Menu OriginMainMenu { get; set; }
    public static Group GroupMenu { get; set; }
    public static About AboutMenu { get; set; }
    public static Tools ToolsMenu { get; set; }
    #endregion

    public bool firstTick = true;

    public Main()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        await Delay(0);

      MenuController.MenuAlignment = SettingsManager.Settings.MenuAlignRight ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left;
      MenuToggleKey = (Control)SettingsManager.Settings.MenuKey;

      #region register commands
      RegisterCommand("testik", new Action<int, List<object>, string>((source, args, raw) =>
      {
        BaseScript.TriggerEvent("chat:addMessage", new
        {
          color = new[] { 255, 0, 0 },
          args = new[] { "[CarSpawner]", $"I wish I could spawn this {(args.Count > 0 ? $"{args[0]} or" : "")} adder but my owner was too lazy. :(" + CitizenFX.Core.Native.API.GetCurrentResourceName() }
        });


      }), false);

      RegisterCommand("testdynmenu", new Action<int, List<object>, string>((source, args, raw) =>
      {
        var mDefSub = new DynamicMenuDefinition
        {
          Name = "TestSub",
          Items = new List<DynamicMenuItem> { new DynamicMenuItem { TextLeft = "TestSub1" }, new DynamicMenuItem { TextLeft = "TestSub2" }, new DynamicMenuItem { TextLeft = "TestSub3" } }
        };

        var mDef = new DynamicMenuDefinition
        {
          Name = "Testmain",
          Items = new List<DynamicMenuItem> { new DynamicMenuItem { TextLeft = "Test1" }, new DynamicMenuItem { TextLeft = "Test2" }, new DynamicMenuItem { TextLeft = "TestSub", Submenu = mDefSub }, new DynamicMenuItem { TextLeft = "Test3" } }
        };

        var menu = new DynamicMenu(mDef);
        menu.Menu.OpenMenu();

      }), false);

      RegisterCommand("testdb", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        string ret = String.Empty;
        bool completed = false;

        Func<string, bool> CallbackFunction = (data) =>
        {
          ret = data;
          completed = true;
          return true;
        };

        TriggerServerEvent("ofw:TestDB", CallbackFunction);

        while (!completed)
        {
          await Delay(0);
        }

        Debug.WriteLine(ret);
      }), false);

      #endregion

      Tick += OnTick;
    }

    private async Task OnTick()
    {
      #region FirstTick
      if (firstTick)
      {
        firstTick = false;
        
        //Wait for data loaded
        while (Game.Player.Name == "**Invalid**" || Game.Player.Name == "** Invalid **")
        {
          await Delay(0);
        }

        // Create the main menu.
        OriginMainMenu = new Menu(Game.Player.Name, "Main Menu");

        // Add the main menu to the menu pool.
        MenuController.AddMenu(OriginMainMenu);
        MenuController.MainMenu = OriginMainMenu;

        // Create all (sub)menus.
        CreateSubmenus();

        Debug.WriteLine("Initialized");
      }
      #endregion

      if (!firstTick)
      {
        // Menu toggle button.
        Game.DisableControlThisFrame(0, MenuToggleKey);
      }
    }

    #region Add Menu Function
    /// <summary>
    /// Add the menu to the menu pool and set it up correctly.
    /// Also add and bind the menu buttons.
    /// </summary>
    /// <param name="submenu"></param>
    /// <param name="menuButton"></param>
    private void AddMenu(Menu parentMenu, Menu submenu, MenuItem menuButton)
    {
      parentMenu.AddMenuItem(menuButton);
      MenuController.BindMenuItem(parentMenu, submenu, menuButton);
      submenu.RefreshIndex();
    }
    #endregion

    #region Create Submenus
    /// <summary>
    /// Creates all the submenus depending on the permissions of the user.
    /// </summary>
    private void CreateSubmenus()
    {
      GroupMenu = new Group();
      Menu menu1 = GroupMenu.GetMenu();
      MenuItem button1 = new MenuItem("Group", "Organize group")
      {
        Label = "→→→"
      };
      AddMenu(OriginMainMenu, menu1, button1);
      OriginMainMenu.OnItemSelect += async (sender, item, index) =>
      {
        if (item == button1)
        {
          await GroupMenu.RefreshMenu();
          menu1.RefreshIndex();
        }
      };

      ToolsMenu = new Tools();
      Menu menu2 = ToolsMenu.GetMenu();
      MenuItem button2 = new MenuItem("Tools", "Custom tools")
      {
        Label = "→→→"
      };
      AddMenu(OriginMainMenu, menu2, button2);

      AboutMenu = new About();
      Menu menu3 = AboutMenu.GetMenu();
      MenuItem button3 = new MenuItem("About OFW", "Information about OFW.")
      {
        Label = "→→→"
      };
      AddMenu(OriginMainMenu, menu3, button3);

    }
    #endregion

    public static void UnlockFun()
    {
      ToolsMenu.UnlockFunTools();
    }
  }
}
