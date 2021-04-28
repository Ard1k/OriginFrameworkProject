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
  public class LuxuryCarsDeliveryServer : BaseScript
  {
    private List<LCDMissionDefinitionBag> Missions = new List<LCDMissionDefinitionBag>
    {
      new LCDMissionDefinitionBag
      {
        Targets = new LCDTargetVehicleBag[]
        {
          new LCDTargetVehicleBag { Identifier = "Car1", Position = new PosBag { X = -710.0746f, Y = 641.9553f, Z = 154.3442f, Heading = 349.0895f } }
        },
        DeliverySpot = new PosBag { X = -162.0009f, Y = -2707.5093f, Z = 5.0071f, Heading = 267.0062f }
      }
    };

    private List<LCDJobStateBag> JobStates = new List<LCDJobStateBag>();

    public LuxuryCarsDeliveryServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
    }

    private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        await Delay(0);

    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
      
    }

    [EventHandler("ofw_lcd:GetJobStateToRestore")]
    private async void GetJobStateToRestore([FromSource] Player source, NetworkCallbackDelegate callback)
    {
      int oidSource = OIDServer.GetOriginServerID(source);
      if (oidSource == -1)
        Debug.WriteLine("ofw_lcd:GetJobStateToRestore: unresolved player oid");

      var jobState = JobStates.Where(j => j.PlayersOnJob.Contains(oidSource)).FirstOrDefault();

      _ = callback(jobState != null ? JsonConvert.SerializeObject(jobState) : null);
    }

    [EventHandler("ofw_lcd:StartJob")]
    private async void StartJob([FromSource] Player source)
    {
      int oidSource = OIDServer.GetOriginServerID(source);
      if (oidSource == -1)
      {
        Debug.WriteLine("ofw_lcd:StartJob: unresolved player oid, cannot start job");
        return;
      }

      LCDMissionDefinitionBag selectedMission = null;
      var unplayedMissions = Missions.Where(m => m.LastRun == null).ToList();
      if (unplayedMissions != null && unplayedMissions.Count > 0)
      {
        int random = MainServer.rngGen.Next(0, unplayedMissions.Count);
        selectedMission = unplayedMissions.ElementAt(random);
      }
      else
      {
        selectedMission = unplayedMissions.OrderBy(m => m.LastRun).FirstOrDefault();
      }

      if (selectedMission == null)
      {
        Debug.WriteLine("ofw_lcd:StartJob: no suitable mission, job not started");
      }

      var members = GroupServer.GetAllGroupMembersServerID(Int32.Parse(source.Handle));
      if (members == null || members.Length <= 0)
        members = new int[] { Int32.Parse(source.Handle) };

      var players = Players.Where(p => members.Contains(int.Parse(p.Handle))).ToArray();

      var oids = OIDServer.GetOIDsFromServerIds(players);

      var js = new LCDJobStateBag
      {
        CurrentState = LCDState.VehicleHunt,
        DeliverySpot = selectedMission.DeliverySpot,
        PlayersOnJob = oids,
        TargetVehicles = selectedMission.Targets
      };

      JobStates.Add(js);
      selectedMission.LastRun = DateTime.Now;
      foreach (var p in players)
      {
        p.TriggerEvent("ofw_lcd:NewJobStateSent", JsonConvert.SerializeObject(js));
      }
    }

  }
}
