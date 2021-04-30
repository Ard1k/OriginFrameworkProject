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
  public class NPCServer : BaseScript
  {
    private NPCDefinitionBag[] NPCs = null;
    private float respawnDistance = 20f;

    public NPCServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
    }

    private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        await Delay(0);

      NPCs = SettingsManager.Settings.NPCs;
      respawnDistance = SettingsManager.Settings.NPCRespawnDistance;

      //foreach (var n in NPCs)
      //{
      //  await SpawnPed(n);
      //}

      Tick += TakeCareOfNPC;
    }

    private async Task TakeCareOfNPC()
    {
      foreach (var i in NPCs)
      {
        if (i.IsSpawning)
          continue;

        if (i.SpawnedNetID <= 0 && (i.NextRespawn == null || i.NextRespawn < DateTime.Now))
        {
          await SpawnPed(i);
          continue;
        }

        var pid = NetworkGetEntityFromNetworkId(i.SpawnedNetID);
        if (pid > 0)
        {
          //ClearPedTasks(pid);
          var ped = new Ped(pid);
          var health = GetEntityHealth(pid);
          var dist = Vector3.Distance(ped.Position, new Vector3(i.Position.X, i.Position.Y, i.Position.Z));
          if (health <= 0)
          {
            DeleteEntity(pid);
            if (i.RespawnTimeMinutes == 0)
            {
              await SpawnPed(i);
            }
            else if (i.RespawnTimeMinutes < 0)
            {
              i.NextRespawn = DateTime.MaxValue;
            }
            else
            {
              i.NextRespawn = DateTime.Now.AddMinutes(i.RespawnTimeMinutes);
            }
          }
          else if (dist > respawnDistance)
          {
            DeleteEntity(pid);
            await SpawnPed(i);
          }
        }
        else
        {
          await SpawnPed(i);
        }
      }

      await Delay(5000);
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      foreach (var i in NPCs)
      {
        if (i.SpawnedNetID <= 0)
          continue;

        var pid = NetworkGetEntityFromNetworkId(i.SpawnedNetID);
        if (pid > 0)
          DeleteEntity(pid);
      }
    }

    private async Task<bool> SpawnPed(NPCDefinitionBag n)
    {
      n.IsSpawning = true;

      var p = CreatePed(0, (uint)GetHashKey(n.ModelName), n.Position.X, n.Position.Y, n.Position.Z, n.Position.Heading, true, true);

      while (!DoesEntityExist(p))
        System.Threading.Thread.Sleep(100);

      var ped = new Ped(p);
      n.SpawnedNetID = ped.NetworkId;
      if (n.HasWeapon)
      {
        GiveWeaponToPed(p, (uint)GetHashKey(n.WeaponModelName), 999999, false, true);
      }      
      n.IsSpawning = false;
      TriggerClientEvent("ofw_npc:UpdateNPCNetID", n.UniqueName, n.SpawnedNetID);
      return true;
    }

    [EventHandler("ofw_npc:GetAllNPC")]
    private async void GetAllNPC([FromSource] Player source, NetworkCallbackDelegate callback)
    {
      while (NPCs == null)
        await Delay(0);

      _ = callback(JsonConvert.SerializeObject(NPCs));
    }

  }
}
