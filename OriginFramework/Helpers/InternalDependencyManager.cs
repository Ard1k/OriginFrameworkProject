using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFramework
{
  internal static class InternalDependencyManager
  {
    private static List<eScriptArea> loadedScriptAreas = new List<eScriptArea>();
    private static List<eScriptArea> essentialScripts = new List<eScriptArea>
    {
      eScriptArea.Main,
      eScriptArea.NativeMenuManager,
      eScriptArea.Misc
    };
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
    DontStart = 0,
    VehicleClient = 1,
    NPCClient = 2,
    NoClip = 3,
    NativeMenuManager = 4,
    Misc = 5,
    Main = 6,
    LuxuryCarsDelivery = 7,
    GroupManager = 8,
    FontsManager = 9,
    FlashHeadlightsAndHorn = 10,
    DrifterAndBooster = 11,
    MapClient = 12,
    Login = 13,
    CharacterCreator = 14,
    CharacterCaretaker = 15,
    InventoryManager = 16,
    OrganizationClient = 17,
    MMenu = 18,
    AtmClient = 19,
    CarryClient = 20,
    BlipClient = 21,
    TaxClient = 22,
  }
}
