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
using static OriginFrameworkServer.OfwServerFunctions;
using System.IO;
using Newtonsoft.Json.Linq;

namespace OriginFrameworkServer
{
  public class InstanceServer : BaseScript
  {
    private Dictionary<int, List<int>> _instances = new Dictionary<int, List<int>>();

    public InstanceServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
    }

		#region event handlers

		private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.InstanceServer))
        return;

      InternalDependencyManager.Started(eScriptArea.InstanceServer);

      RegisterCommand("mybucket", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        var sourcePlayer = Players.Where(p => p.Handle == source.ToString()).FirstOrDefault();
        if (sourcePlayer == null)
          return;

        if (!CharacterCaretakerServer.HasPlayerAdminLevel(sourcePlayer, 10))
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Nedostatecne opravneni");
          return;
        }

        TriggerClientEvent("chat:addMessage", $"My routing bucket id: {GetPlayerRoutingBucket(sourcePlayer.Handle)}");
      }), false);

      RegisterCommand("setbucket", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        var sourcePlayer = Players.Where(p => p.Handle == source.ToString()).FirstOrDefault();
        if (sourcePlayer == null)
          return;

        if (!CharacterCaretakerServer.HasPlayerAdminLevel(sourcePlayer, 10))
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Nedostatecne opravneni");
          return;
        }

        int bucketId = 0;
        if (args.Count != 1 || !Int32.TryParse(args[0].ToString(), out bucketId))
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neplatne id bucketu");
        }

        SetPlayerRoutingBucket(source.ToString(), bucketId);

        TriggerClientEvent("chat:addMessage", $"My new routing bucket id: {GetPlayerRoutingBucket(sourcePlayer.Handle)}");
      }), false);
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return; 
    }

    [EventHandler("ofw_instance:TransferToPrivateInstance")]
    private async void TransferToPrivateInstance([FromSource] Player source)
    {
      if (source == null)
        return;

      if (GetPlayerRoutingBucket(source.Handle) == Int32.Parse(source.Handle))
        return; //uz tam je

      SetPlayerRoutingBucket(source.Handle, Int32.Parse(source.Handle));
    }

    [EventHandler("ofw_instance:TransferToPublicInstance")]
    private async void TransferToPublicInstance([FromSource] Player source)
    {
      if (source == null)
        return;

      if (GetPlayerRoutingBucket(source.Handle) == 0)
        return; //uz tam je

      SetPlayerRoutingBucket(source.Handle, 0);
    }

    #endregion

    #region private


    #endregion
  }
}
