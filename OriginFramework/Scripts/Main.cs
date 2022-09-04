using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OriginFramework.Menus;
using static CitizenFX.Core.Native.API;
using OriginFrameworkData.DataBags;
using System.Text.RegularExpressions;
using OriginFramework.Scripts;

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
    public static bool IsAdmin { get; set; } = false;

    public Main()
    {
      LocalPlayers = Players;

      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
      EventHandlers["ofw:ValidationErrorNotification"] += new Action<string>(ValidationErrorNotification);
      EventHandlers["ofw:SuccessNotification"] += new Action<string>(SuccessNotification);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.Main))
        return;

      Debug.WriteLine($"Waiting for oid...");
      int oid = -1;
      bool isAdmin = false;
      bool oid_returned = false;

      Func<int, bool, bool> OIDCallback = (soid, sisadmin) =>
      {
        oid = soid;
        isAdmin = sisadmin;
        oid_returned = true;
        return true;
      };

      TriggerServerEvent("ofw_oid:GetMyOriginID", OIDCallback);

      while (!oid_returned)
      {
        await Delay(0);
      }

      MyOriginId = oid;
      IsAdmin = isAdmin;
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

      RegisterCommand("menu", new Action<int, List<object>, string>((source, args, raw) =>
      {
        NativeMenuManager.ToggleMenu("MainMenu", MainMenu_Default.GenerateMenu);
      }), false);

      Tick += OnTick;

      InternalDependencyManager.Started(eScriptArea.Main);
    }

    private async Task OnTick()
    {
      if (IsControlJustPressed(0, 344)) //F11
        NativeMenuManager.ToggleMenu("MainMenu", MainMenu_Default.GenerateMenu);

      if (IsControlJustPressed(0, 57)) //F10
        NoClip.SetNoclipSwitch();
    }

    private async void ValidationErrorNotification(string message)
    {
      Notify.Error(message);
    }

    private async void SuccessNotification(string message)
    {
      Notify.Success(message);
    }
  }
}
