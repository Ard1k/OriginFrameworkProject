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
    private LCDMissionDefinitionBag[] Missions = null;

    private List<LCDJobStateBag> JobStates = new List<LCDJobStateBag>();
    private bool spawningLock = false;
    private dynamic ESX = null;

    public LuxuryCarsDeliveryServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
    }

		#region event handlers

		private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        await Delay(0);

      Missions = SettingsManager.Settings.LCDMissions;

      while (ESX == null)
      {
        TriggerEvent("esx:getSharedObject", new object[] { new Action<dynamic>(esx => { ESX = esx; }) });
        await Delay(0);
      }

      //Debug.WriteLine("OFW_LCD: ESX object loaded!");
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

    //TriggerServerEvent("ofw_lcd:SpawnJobCar", v.Identifier, CallbackFunction);
    [EventHandler("ofw_lcd:SpawnJobCar")]
    private async void SpawnJobCar([FromSource] Player source, string identifier, int blockingNetID, NetworkCallbackDelegate callback)
    {
      //Debug.WriteLine("Car spawn request recieved");
      while (spawningLock)
      {
        await Delay(0);
      }
      spawningLock = true;

      //Zkontolovat, jestli uz v serverstate nemam pro auto netID. Kdyz ne, spawnu. Kdyz jo, tak ho rovnou vratim. Nemusim pak delat zadny harakiri.

      int oidSource = OIDServer.GetOriginServerID(source);
      if (oidSource == -1)
        Debug.WriteLine("ofw_lcd:SpawnJobCar: unresolved player oid");

      if (blockingNetID > 0 && !PersistentVehiclesServer.IsVehicleKnown(blockingNetID))
      {
        //Fuj no... ale chova se to jinak kdyz mazu server entitu udelanou serverem a kdyz mazu random entitu
        try { DeleteEntity(NetworkGetEntityFromNetworkId(blockingNetID)); } catch { }
        try { DeleteEntity(blockingNetID); } catch { }
      }

      var jobState = JobStates.Where(j => j.PlayersOnJob.Contains(oidSource)).FirstOrDefault();

      if (jobState != null && jobState.TargetVehicles != null)
      {
        var vehBag = jobState.TargetVehicles.Where(v => v.Identifier == identifier).FirstOrDefault();
        if (vehBag != null)
        {
          if (vehBag.NetID > 0)
          {
            _ = callback(vehBag.NetID);
          }
          else
          {
            int ret = -1;
            bool completed = false;

            Func<int, bool> CallbackFunction = (data) =>
            {
              ret = data;
              completed = true;
              return true;
            };
            TriggerEvent("ofw_veh:SpawnServerVehicle", vehBag.ModelName, new Vector3(vehBag.Position.X, vehBag.Position.Y, vehBag.Position.Z), vehBag.Position.Heading, CallbackFunction);

            while (!completed)
            {
              await Delay(0);
            }
            //Debug.WriteLine("Vehicle spawned");
            
            vehBag.NetID = ret;
            _ = callback(ret);
          }
        }
      }
      spawningLock = false;
    }

    [EventHandler("ofw_lcd:JobFinished")]
    private async void JobFinished([FromSource] Player source)
    {
      int oidSource = OIDServer.GetOriginServerID(source);
      if (oidSource == -1)
      {
        Debug.WriteLine("ofw_lcd:JobFinished: unresolved player oid, cannot deliver");
        return;
      }

      var jobState = JobStates.Where(j => j.PlayersOnJob.Contains(oidSource)).FirstOrDefault();

      if (jobState != null && jobState.TargetVehicles != null)
      {
        if (jobState.TargetVehicles.Any(v => v.Delivered == false))
        {
          Debug.WriteLine("OFW_LCD: There are some undelivered cars!");
          return;
        }

        var count = jobState.TargetVehicles.Count();
        var membersServerIDs = new List<int>();

        foreach (var oid in jobState.PlayersOnJob)
        {
          membersServerIDs.Add(OIDServer.GetLastKnownServerID(oid));
        }

        var players = Players.Where(p => membersServerIDs.Contains(int.Parse(p.Handle))).ToArray();

        int reward = jobState.RewardPerCar * count;

        foreach (var p in players)
        {
          var xPlayer = ESX.GetPlayerFromId(p.Handle);
          if (xPlayer == null)
          {
            Debug.WriteLine("OFW_LCD: esx player is null!");
          }
          else
          {
            xPlayer.addMoney(reward);
          }

          p.TriggerEvent("ofw_lcd:JobFinishedUpdate", reward);
        }
        DeleteGuards(jobState.Guards);

        JobStates.Remove(jobState);
      }
    }

    [EventHandler("ofw_lcd:JobCancelled")]
    private async void JobCancelled([FromSource] Player source)
    {
      int oidSource = OIDServer.GetOriginServerID(source);
      if (oidSource == -1)
      {
        Debug.WriteLine("ofw_lcd:JobCancelled: unresolved player oid, cannot deliver");
        return;
      }

      var jobState = JobStates.Where(j => j.PlayersOnJob.Contains(oidSource)).FirstOrDefault();

      if (jobState != null)
      {
        var membersServerIDs = new List<int>();

        foreach (var oid in jobState.PlayersOnJob)
        {
          membersServerIDs.Add(OIDServer.GetLastKnownServerID(oid));
        }

        var players = Players.Where(p => membersServerIDs.Contains(int.Parse(p.Handle))).ToArray();

        if (jobState.TargetVehicles != null)
        {
          foreach (var v in jobState.TargetVehicles)
          {
            if (v.NetID > 0 && !v.Delivered)
            {
              //Fuj no... ale chova se to jinak kdyz mazu server entitu udelanou serverem a kdyz mazu random entitu
              try { DeleteEntity(NetworkGetEntityFromNetworkId(v.NetID)); } catch { }
              try { DeleteEntity(v.NetID); } catch { }
            }
          }
        }

        DeleteGuards(jobState.Guards);

        foreach (var p in players)
        {
          p.TriggerEvent("ofw_lcd:JobCancelledUpdate");
        }

        JobStates.Remove(jobState);
      }
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
      var unplayedMissions = Missions.Where(m => m.LastRun == null)?.ToList();
      if (unplayedMissions != null && unplayedMissions.Count > 0)
      {
        int random = MainServer.rngGen.Next(0, unplayedMissions.Count);
        selectedMission = unplayedMissions.ElementAt(random);
      }
      else
      {
        selectedMission = Missions.OrderBy(m => m.LastRun).FirstOrDefault();
      }

      if (selectedMission == null)
      {
        Debug.WriteLine("ofw_lcd:StartJob: no suitable mission, job not started");
      }

      var members = GroupServer.GetAllGroupMembersServerID(source);
      if (members == null || members.Length <= 0)
        members = new int[] { Int32.Parse(source.Handle) };

      var players = Players.Where(p => members.Contains(int.Parse(p.Handle))).ToArray();

      var oids = OIDServer.GetOIDsFromServerIds(players);

      var jobstateTargets = new List<LCDTargetVehicleBag>();
      for (int i = 0; i < oids.Length; i++)
      {
        if (selectedMission.Targets.Length <= i)
          break;

        if (i == 1)
          continue;

        jobstateTargets.Add(selectedMission.Targets[i]);
      }

      var js = new LCDJobStateBag
      {
        CurrentState = LCDState.VehicleHunt,
        DeliverySpot = selectedMission.DeliverySpot,
        PlayersOnJob = oids,
        TargetVehicles = jobstateTargets.ToArray(),
        RewardPerCar = selectedMission.RewardPerCar,
        Guards = selectedMission.Guards,
      };

      foreach (var i in js.TargetVehicles)
      {
        i.Delivered = false;
        i.HasEntityBlip = false;
        i.NetID = -1;
        i.StaticBlip = 0;
      }

      foreach (var i in js.Guards)
      {
        i.NetID = -1;
      }

      if (js.Guards != null)
      {
        foreach (var g in js.Guards)
          SpawnGuard(g);
      }

      JobStates.Add(js);
      selectedMission.LastRun = DateTime.Now;
      foreach (var p in players)
      {
        p.TriggerEvent("ofw_lcd:NewJobStateSent", JsonConvert.SerializeObject(js));
      }
    }

    [EventHandler("ofw_lcd:DeliverVehicle")]
    private async void DeliverVehicle([FromSource] Player source, int netID, NetworkCallbackDelegate callback)
    {
      int oidSource = OIDServer.GetOriginServerID(source);
      if (oidSource == -1)
      {
        Debug.WriteLine("ofw_lcd:DeliverVehicle: unresolved player oid, cannot deliver");
        _ = callback(false);
        return;
      }

      var jobState = JobStates.Where(j => j.PlayersOnJob.Contains(oidSource)).FirstOrDefault();

      if (jobState != null && jobState.TargetVehicles != null)
      {
        var vehBag = jobState.TargetVehicles.Where(v => v.NetID == netID).FirstOrDefault();

        if (vehBag != null)
        {
          // TODO: Validace na pozici i ze strany serveru... ale asi fuck it? :D
          vehBag.Delivered = true;

          //Fuj no... ale chova se to jinak kdyz mazu server entitu udelanou serverem a kdyz mazu random entitu
          try { DeleteEntity(NetworkGetEntityFromNetworkId(netID)); } catch { }
          try { DeleteEntity(netID); } catch { }

          var members = GroupServer.GetAllGroupMembersServerID(source);
          if (members == null || members.Length <= 0)
            members = new int[] { Int32.Parse(source.Handle) };

          var players = Players.Where(p => members.Contains(int.Parse(p.Handle))).ToArray();
          foreach (var p in players)
          {
            p.TriggerEvent("ofw_lcd:VehicleDeliveredUpdate", vehBag.Identifier);
          }

          _ = callback(true);
          return;
        }
      }

      _ = callback(false);
    }
		#endregion

		#region private
		private void DeleteGuards(LCDLocationGuard[] guards)
    {
      if (guards == null)
        return;

      foreach (var i in guards)
      {
        if (i.NetID <= 0)
        {
          continue;
        }

        var pid = NetworkGetEntityFromNetworkId(i.NetID);
        if (pid > 0)
        {
          try
          {
            DeleteEntity(pid);
          }
          catch
          { 
          }
        }
      }
    }

    private async Task<bool> SpawnGuard(LCDLocationGuard n)
    {
      var p = CreatePed(0, (uint)GetHashKey(n.ModelName), n.Position.X, n.Position.Y, n.Position.Z, n.Position.Heading, true, true);

      while (!DoesEntityExist(p))
        await Delay(0);

      var ped = new Ped(p);
      n.NetID = ped.NetworkId;
      return true;
    }

    #endregion
  }
}
