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

      if (!await InternalDependencyManager.CanStart(eScriptArea.MainServer))
        return;

      RegisterCommand("identcopy", new Action<int, List<object>, string>((source, args, raw) =>
      {
        var sourcePlayer = Players.Where(p => p.Handle == source.ToString()).FirstOrDefault();
        if (sourcePlayer == null)
          return;

        if (args == null || args.Count != 1)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Natplatne server ID hrace");
          return;
        }

        var player = Players.Where(p => p.Handle == args[0].ToString()).FirstOrDefault();
        if (player == null)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Nepodarilo se najit hrace");
          return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"serverID: [{player.Handle}] OID: [{OIDServer.GetOriginServerID(player)}] Name: [{player.Name}]");
        sb.AppendLine();
        sb.AppendLine("Known idetifiers:");

        foreach (var ident in player.Identifiers)
        {
          sb.AppendLine(ident);
        }

        sourcePlayer.TriggerEvent("ofw_misc:CopyStringToClipboard", sb.ToString());
      }), false);

      InternalDependencyManager.Started(eScriptArea.MainServer);
    }

    [EventHandler("ofw_misc:GetJobGrades")]
    private async void GetJobGrades([FromSource] Player source, string jobname, NetworkCallbackDelegate callback)
    {
      if (source == null)
        return;

      var param = new Dictionary<string, object>();
      param.Add("@jobname", jobname);
      var result = await VSql.FetchAllAsync("SELECT * from `job_grades` where `job_name` = @jobname order by grade asc ", param);

      _ = callback(result != null ? JsonConvert.SerializeObject(result) : null);
    }
  }
}
