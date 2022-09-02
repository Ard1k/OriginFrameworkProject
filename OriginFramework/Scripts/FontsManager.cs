using CitizenFX.Core;
using OriginFrameworkData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
  public class FontsManager : BaseScript
  {
    public static int FiraSansFontId = -1;
    public static bool IsFiraSansLoaded { get { return FiraSansFontId > 0; } }
    public static string FiraSansString { get { return IsFiraSansLoaded ? "<FONT FACE=\'Fira Sans\'>" : ""; } }

    public FontsManager()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.FontsManager))
        return;

      RegisterFontFile("fs");
      FiraSansFontId = RegisterFontId("Fira Sans");

      InternalDependencyManager.Started(eScriptArea.FontsManager);
    }

    public static async Task<int> GetFiraSansIdAsync()
    {
      while (FiraSansFontId <= 0)
        Delay(1000);
      
      return FiraSansFontId;
    }
  }
}
