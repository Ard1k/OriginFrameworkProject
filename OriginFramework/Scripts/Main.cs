using CitizenFX.Core;
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

    public static PlayerList LocalPlayers { get; protected set; }
    public static int MyOriginId { get; set; } = -1;

    public static Control MenuToggleKey { get { return MenuController.MenuToggleKey; } private set { MenuController.MenuToggleKey = value; } }

		#region Menu static variables
		public static Menu OriginMainMenu { get; set; }
    public static Group GroupMenu { get; set; }
    public static About AboutMenu { get; set; }
    public static Tools ToolsMenu { get; set; }
    public static Toys ToysMenu { get; set; }
    #endregion

    public bool firstTick = true;

    public Main()
    {
      LocalPlayers = Players;

      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
      EventHandlers["ofw:ValidationErrorNotification"] += new Action<string>(ValidationErrorNotification);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        await Delay(0);

      MenuController.MenuAlignment = SettingsManager.Settings.MenuAlignRight ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left;
      MenuToggleKey = (Control)SettingsManager.Settings.MenuKey;

      Debug.WriteLine($"Waiting for oid...");
      int oid = -1;
      bool oid_returned = false;

      Func<int, bool> OIDCallback = (soid) =>
      {
        oid = soid;
        oid_returned = true;
        return true;
      };

      TriggerServerEvent("ofw_oid:GetMyOriginID", OIDCallback);

      while (!oid_returned)
      {
        await Delay(0);
      }

      MyOriginId = oid;
      Debug.WriteLine($"OID retrieved: {oid}");

      #region register commands
      //RegisterCommand("testik", new Action<int, List<object>, string>((source, args, raw) =>
      //{
      //  BaseScript.TriggerEvent("chat:addMessage", new
      //  {
      //    color = new[] { 255, 0, 0 },
      //    args = new[] { "[CarSpawner]", $"I wish I could spawn this {(args.Count > 0 ? $"{args[0]} or" : "")} adder but my owner was too lazy. :(" + CitizenFX.Core.Native.API.GetCurrentResourceName() }
      //  });

      //}), false);

      //RegisterCommand("mcar", new Action<int, List<object>, string>((source, args, raw) =>
      //{
      //  var ped = Game.PlayerPed;
      //  Debug.WriteLine("S: " + source + "P: " + ped?.Heading);
      //  RequestModel((uint)GetHashKey(args[0].ToString()));
      //  CreateVehicle((uint)GetHashKey(args[0].ToString()), ped.Position.X, ped.Position.Y, ped.Position.Z, ped.Heading, true, true);

      //  BaseScript.TriggerEvent("chat:addMessage", new
      //  {
      //    color = new[] { 255, 0, 0 },
      //    args = new[] { "[CarSpawner]", $"wtf" }
      //  });
      //}), false);

      //RegisterCommand("testdynmenu", new Action<int, List<object>, string>((source, args, raw) =>
      //{
      //  var mDefSub = new DynamicMenuDefinition
      //  {
      //    Name = "TestSub",
      //    Items = new List<DynamicMenuItem> { new DynamicMenuItem { TextLeft = "TestSub1" }, new DynamicMenuItem { TextLeft = "TestSub2" }, new DynamicMenuItem { TextLeft = "TestSub3" } }
      //  };

      //  var mDef = new DynamicMenuDefinition
      //  {
      //    Name = "Testmain",
      //    Items = new List<DynamicMenuItem> { new DynamicMenuItem { TextLeft = "Test1" }, new DynamicMenuItem { TextLeft = "Test2" }, new DynamicMenuItem { TextLeft = "TestSub", Submenu = mDefSub }, new DynamicMenuItem { TextLeft = "Test3" } }
      //  };

      //  var menu = new DynamicMenu(mDef);
      //  menu.Menu.OpenMenu();

      //}), false);

      //  RegisterCommand("testdb", new Action<int, List<object>, string>(async (source, args, raw) =>
      //  {
      //    string ret = String.Empty;
      //    bool completed = false;

      //    Func<string, bool> CallbackFunction = (data) =>
      //    {
      //      ret = data;
      //      completed = true;
      //      return true;
      //    };

      //    TriggerServerEvent("ofw:TestDB", CallbackFunction);

      //    while (!completed)
      //    {
      //      await Delay(0);
      //    }

      //    Debug.WriteLine(ret);
      //  }), false);

      #endregion

      Tick += OnTick;
    }

    private async void ValidationErrorNotification(string message)
    {
      Notify.Error(message);
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
        OriginMainMenu = new Menu("OriginRP", "Main Menu");

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
      MenuItem button1 = new MenuItem("Skupina", "Zorganizuj si svoji skupinu. Nektere mise od NPC s ni umi pracovat. Pokud ano, zjistis to, kdyz si s nima pokecas. Pro spusteni mise jako skupina musi byt vsichni clenove skupiny do vzdalenosti 20m (zeleni v party)")
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

      ToysMenu = new Toys();
      Menu menu2 = ToysMenu.GetMenu();
      MenuItem button2 = new MenuItem("Blbustky", "Blbiny a vychytavky")
      {
        Label = "→→→"
      };
      AddMenu(OriginMainMenu, menu2, button2);

      ToolsMenu = new Tools();
      Menu menu3 = ToolsMenu.GetMenu();
      MenuItem button3 = new MenuItem("Dev Nastroje", "Nastroje pro vyvoj. Nic moc zajimavyho")
      {
        Label = "→→→"
      };
      AddMenu(OriginMainMenu, menu3, button3);

      AboutMenu = new About();
      Menu menu4 = AboutMenu.GetMenu();
      MenuItem button4 = new MenuItem("About OFW", "Informace o origin frameworku")
      {
        Label = "→→→"
      };
      AddMenu(OriginMainMenu, menu4, button4);

    }
    #endregion

    public static void UnlockFun()
    {
      ToolsMenu.UnlockFunTools();
    }
  }
}
