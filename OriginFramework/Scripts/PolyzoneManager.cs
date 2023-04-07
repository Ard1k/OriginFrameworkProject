using CitizenFX.Core;
using OriginFrameworkData;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static OriginFramework.OfwFunctions;

namespace OriginFramework
{
  public class PolyzoneManager : BaseScript
  {
    private static List<PolyzoneBag> knownPolyzones = new List<PolyzoneBag>();
    private static bool testsFrozen = false;

    public PolyzoneManager()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      Tick += OnTick;
      Tick += OnSlowTick;

      RegisterCommand("tpoly", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        testsFrozen = true;

        if (args?.Count == 4)
          knownPolyzones.Add(new PolyzoneBag()
          {
            Center = new PosBag { X = float.Parse(args[0].ToString()), Y = float.Parse(args[1].ToString()), Z = float.Parse(args[2].ToString()), Heading = float.Parse(args[3].ToString()) },
            Dimensions = new DimensionsBag
            {
              Width = 2.3f,
              Length = 4.5f,
              Height = 2.0f
            }
          });
        else
          knownPolyzones.Clear();

        testsFrozen = false;
      }), false);
    }

    private async Task OnTick()
    {
      if (knownPolyzones.Count <= 0)
      {
        await Delay(1000);
        return;
      }

      foreach (var i in knownPolyzones)
      {
        if (testsFrozen)
          break;

        var blocked = GetBoxMarkerBlockingVehicle(i.Center, i.Dimensions) != 0;
        BoxMarkerSolidDraw(i.Center, i.Dimensions, blocked);
      }
    }

    private async Task OnSlowTick()
    {
      int bestWeapon = GetBestPedWeapon(Game.PlayerPed.Handle, false);



      await Delay(1000);
    }

    public static void AddPolyZone(PolyzoneBag bag)
    {
      testsFrozen = true;
      knownPolyzones.Add(bag);
      testsFrozen = false;
    }
    public static void AddPolyZone(PosBag bag, DimensionsBag dim)
    {
      testsFrozen = true;
      knownPolyzones.Add(new PolyzoneBag { Center = bag, Dimensions = dim });
      testsFrozen = false;
    }
  }
}
