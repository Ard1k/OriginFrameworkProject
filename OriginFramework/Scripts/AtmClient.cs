using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using OriginFramework.Helpers;
using OriginFramework.Menus;
using OriginFrameworkData.DataBags;
using static CitizenFX.Core.Native.API;

namespace OriginFramework.Scripts
{
  public class AtmClient : BaseScript
  {
    public class AtmExtendedBag : AtmBag
    {
      public Vector3 PosVector3 { get; set; }

      public AtmExtendedBag(AtmBag atm)
      {
        this.PropName = atm.PropName;
        this.Pos = atm.Pos;
        this.Id = atm.Id;
      }
    }

    private static List<AtmExtendedBag> CachedAtms = new List<AtmExtendedBag>();
    private static AtmExtendedBag NearestAtm = null;
    private static float DistLimit = 4f;
    private static float DistSquaredLimit = DistLimit * DistLimit;
    private const string MenuName = "atm_menu";
    private static bool wasMenuOpen = false;

    public AtmClient()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.AtmClient))
        return;

      if (CachedAtms.Count <= 0)
      {
        foreach (var a in AtmBag.DefinedAtms)
        {
          var extendedAtm = new AtmExtendedBag(a);
          extendedAtm.PosVector3 = OfwFunctions.PosBagToVector3(a.Pos);

          CachedAtms.Add(extendedAtm);
        }
      }

      Tick += OnTick;
      Tick += OnSlowTick250;
      AddTextEntry("ATMCLIENT_OPEN_ATM", FontsManager.FiraSansString + "~INPUT_PICKUP~ Použít bankomat");

      InternalDependencyManager.Started(eScriptArea.AtmClient);
    }

    private async Task OnSlowTick250()
    {
      //Debug.WriteLine($"Nearest ATM: {NearestAtm?.Id ?? -1}");
      await Delay(250);

      var pos = Game.PlayerPed.Position;
      NearestAtm = CachedAtms.Where(atm => atm.PosVector3.DistanceToSquared2D(pos) <= DistSquaredLimit).OrderBy(atm => atm.PosVector3.DistanceToSquared2D(pos)).FirstOrDefault();
    }

    private async Task OnTick()
    {
      if (wasMenuOpen && !NativeMenuManager.IsMenuOpen(MenuName))
      {
        Debug.WriteLine("ATM Clear");
        ClearPedTasks(Game.PlayerPed.Handle);
        wasMenuOpen = false;
      }

      if (NativeMenuManager.CurrentMenuName == MenuName || NearestAtm == null)
        return;

      DisplayHelpTextThisFrame("ATMCLIENT_OPEN_ATM", true);

      if (Game.IsControlJustPressed(0, Control.Pickup)) //E
      {
        await MovePedToAtm(NearestAtm);
        await AnimUseAtm();

        NativeMenuManager.OpenNewMenu(MenuName, getAtmMenu);
        wasMenuOpen = true;
      }
    }

    private async Task<bool> MovePedToAtm(AtmExtendedBag atm)
    {
      int obj = GetClosestObjectOfType(atm.Pos.X, atm.Pos.Y, atm.Pos.Z, DistLimit, (uint)GetHashKey(atm.PropName), false, false, false);
      float heading = GetEntityHeading(obj) - 90.0f;

      Vector3 vec = new Vector3(
          (float)Math.Cos(Math.PI / 180.0 * heading),
          (float)Math.Sin(Math.PI / 180.0 * heading),
          0.0f
      ) * 0.6f;

      Vector3 coords = GetEntityCoords(obj, false);
      Vector3 standPos = coords + vec;
      int ped = PlayerPedId();
      await OfwFunctions.MovePedToCoordForSeconds(ped, standPos, heading + 90.0f, 1.0f, 3.5f);

      return true;
    }

    public static async Task<bool> AnimUseAtm()
    {
      int ped = Game.Player.Character.Handle;

      int frameLimit = 60;
      string a1 = "amb@prop_human_atm@male@enter";
      string a2 = "amb@prop_human_atm@male@idle_a";

      RequestAnimDict(a1);
      RequestAnimDict(a2);

      while ((!HasAnimDictLoaded(a1) || !HasAnimDictLoaded(a2)) && frameLimit > 0)
      {
        frameLimit--;
        await Delay(0);
      }

      ClearPedTasksImmediately(ped);

      TaskPlayAnim(ped, a1, "enter", 8.0f, 1.0f, 3500, 2, 0, false, false, false);

      await Delay(3500);

      ClearPedTasks(ped);

      TaskPlayAnim(ped, a2, "idle_a", 8.0f, 1.0f, -1, 1, 0, false, false, false);

      return true;
    }

    private NativeMenu getAtmMenu()
    {
      return new NativeMenu
      {
        MenuTitle = $"ATM ID: {NearestAtm.Id}",
        Items = new List<NativeMenuItem>()
        {
          new NativeMenuItem { 
            Name = "Osobní účet"
          },
          new NativeMenuItem {
            Name = "Firemní účet"
          },
          new NativeMenuItem { 
            Name = "Zavřít",
            IsClose = true
          }
        }
      };
    }

    [EventHandler("ofw_atm:AtmEventHandler")]
    private void AtmEventHandler(string mapData)
    {
      if (string.IsNullOrEmpty(mapData))
      {
        Debug.WriteLine("OFW_MAP: invalid map to load");
        return;
      }
    }

  }
}
