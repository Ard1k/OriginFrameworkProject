using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkServer
{
  internal static class InternalDependencyManager
  {
    private static List<eScriptArea> loadedScriptAreas = new List<eScriptArea>();
    private static List<eScriptArea> essentialScripts = new List<eScriptArea>() { eScriptArea.VSql, };
    private static bool essentialsLoaded = false;

    public static async Task<bool> CanStart(eScriptArea script, params eScriptArea[] dependsOn)
    {
      int counter = 0;

      while (!CheckForEssentials(script) || (dependsOn != null && dependsOn.Length > 0 && dependsOn.Any(it => !loadedScriptAreas.Contains(it))))
      {
        await BaseScript.Delay(0);
        counter++;

        if (counter > 300)
        {
          Debug.WriteLine($"!!! Dependency Issue: {script.ToString("g")} could not load!");
          return false;
        }
      }

      return true;
    }

    public static void Started(eScriptArea script)
    {
      Debug.WriteLine($"started: {script.ToString()}");
      loadedScriptAreas.Add(script);
    }

    private static bool CheckForEssentials(eScriptArea script)
    {
      if (SettingsManager.Settings == null)
        return false;

      if (essentialScripts.Contains(script) || essentialsLoaded)
        return true;

      if (essentialScripts.Any(it => !loadedScriptAreas.Contains(it)))
        return false;

      essentialsLoaded = true;
      return true;
    }
  }

  public enum eScriptArea : int
  {
    GroupServer = 1,
    VSql = 2,
    LuxuryCarsDeliveryServer = 3,
    MainServer = 4,
    NPCServer = 5,
    OIDServer = 6,
    PersistentVehiclesServer = 7,
    MapServer = 8,
    InventoryServer = 9,
    OrganizationServer = 10,
    VehicleVendorServer = 11,
    CarryServer = 12,
    InstanceServer = 13,
    IdentityServer = 14,
    TaxServer = 15,
  }
}
