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
    public Dictionary<int, ItemDefinition> DynamicItemsDefinitions = new Dictionary<int, ItemDefinition>();

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

      while (!VSql.IsReadyToUse)
        await Delay(0);

      var itemDefResult = await VSql.FetchAllAsync("SELECT `id`, `data` from `item_definition`", null);
      if (itemDefResult != null && itemDefResult.Count > 0)
        foreach (var row in itemDefResult)
        {
          var it = JsonConvert.DeserializeObject<ItemDefinition>(row["data"] as string);
          if (it != null)
          {
            DynamicItemsDefinitions.Add(it.ItemId, it);
            ItemsDefinitions.Items[it.ItemId] = it;
          }
        }

      InternalDependencyManager.Started(eScriptArea.MainServer);
    }

    [EventHandler("ofw_core:GetDynamicItemDefinitions")]
    private async void GetDynamicItemDefinitions([FromSource] Player source, NetworkCallbackDelegate callback)
    {
      if (source == null)
        return;

      _ = callback(JsonConvert.SerializeObject(DynamicItemsDefinitions));
    }
    [EventHandler("ofw_core:UpdateDynamicItemDefinitions")]
    private async void UpdateDynamicItemDefinitions([FromSource] Player source, string definition)
    {
      if (source == null)
        return;

      if (!CharacterCaretakerServer.HasPlayerAdminLevel(source, 10))
      {
        source.TriggerEvent("ofw:ValidationErrorNotification", "Nedostatecne opravneni");
        return;
      }

      if (definition == null)
        source.TriggerEvent("ofw:ValidationErrorNotification", "Žádná data");

      var it = JsonConvert.DeserializeObject<ItemDefinition>(definition);
      if (it == null)
        source.TriggerEvent("ofw:ValidationErrorNotification", "Neplatná data");

      if (it.ItemId < 1000)
      {
        for (int i = 1000; i < 2000; i++)
        {
          if (ItemsDefinitions.Items[i] == null)
          {
            it.ItemId = i;
            break;
          }
        }
      }
      if (it.ItemId < 1000)
        source.TriggerEvent("ofw:ValidationErrorNotification", "Žádné volné id");

      ItemsDefinitions.Items[it.ItemId] = it;
      if (DynamicItemsDefinitions.ContainsKey(it.ItemId))
        DynamicItemsDefinitions[it.ItemId] = it;
      else
        DynamicItemsDefinitions.Add(it.ItemId, it);

      var param = new Dictionary<string, object>();
      param.Add("@id", it.ItemId);
      param.Add("@data", JsonConvert.SerializeObject(it));
      VSql.ExecuteAsync("insert into `item_definition` (`id`, `data`) VALUES (@id, @data) ON DUPLICATE KEY UPDATE `data` = @data", param);

      TriggerClientEvent("ofw_core:DynamicItemDefinitionUpdated", JsonConvert.SerializeObject(it));
      source?.TriggerEvent("ofw:SuccessNotification", "Item uložen!");
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
