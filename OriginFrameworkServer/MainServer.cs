using CitizenFX.Core;
using OriginFrameworkData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;
using OriginFrameworkData.DataBags;
using CitizenFX.Core.Native;

namespace OriginFrameworkServer
{
  public class MainServer : BaseScript
  {
    public static Random rngGen = new Random();

    public MainServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
    }

    private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        await Delay(0);

      RegisterCommand("idcopy", new Action<int, List<object>, string>((source, args, raw) =>
      {
        if (args == null || args.Count != 1)
        {
          return;
          //Invalid arguments
        }

        var plr = Players.Where(p => p.Handle == source.ToString()).FirstOrDefault();

        var id = OIDServer.GetOriginServerID(plr);

        Debug.WriteLine("returned ID: " + id + " SID: " + source);
      }), false);
    }
  }
}
