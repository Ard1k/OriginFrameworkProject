using CitizenFX.Core;
using MenuAPI;
using Newtonsoft.Json;
using OriginFramework.Menus;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
  public class GroupManager : BaseScript
  {
    public static bool IsWaitingForGroup { get; set; } = false;
    public static GroupBag Group { get; set; }
    private const float questDistance = 20f;

    #region Tick tasks
    private async Task GroupDisplay()
    {
      if (IsWaitingForGroup || (Group != null && Group.IsInAGroup))
      {
        DrawGroup();
      }
      else
      {
        await Delay(1000);
      }
    }

		private async Task ProcessGroupDistance()
    {
      var myPos = Game.PlayerPed.Position;
      var myPedNetId = Game.PlayerPed.NetworkId;
      var players = Players.ToList();

      if (Group != null && Group.Members != null)
      {
        for (int i = 0; i < Group.Members.Length; i++)
        {
          var member = Group.Members[i];
          
          if (member.NetPedID == myPedNetId)
          {
            member.IsInRange = true;
            member.Distance = 0f;
            continue;
          }

          var memberPed = NetToPed(member.NetPedID);

          if (memberPed <= 0)
          {
            member.IsInRange = false;
            member.Distance = 999f;
            continue;
          }

          var memberCoords = GetEntityCoords(memberPed, true);

          member.IsInRange = true;
          member.Distance = Vector3.Distance(myPos, memberCoords);
          if (member.Distance <= questDistance)
            member.IsInQuestRange = true;
        }
      }

      await Delay(500);
    }

    private float timeCounter = 0;
    private async Task PeriodicGroupRefresh()
    {
      timeCounter += (GetFrameTime() * 1000);
      if (timeCounter > 12000)
      {
        TriggerServerEvent("ofw_grp:RequestGroupInfoRefresh");
        timeCounter = 0;
      }
    }

		#endregion

		#region event handlers
		private void RefreshGroupInfo(string groupJson)
    {
      Group = JsonConvert.DeserializeObject<GroupBag>(groupJson);
    }

    private void NotifyError(string message)
    {
      Notify.Error(message);
    }

    private void NotifySuccess(string message)
    {
      Notify.Info(message);
    }
    private void ConfirmGroupInvite(int requestingPlayerHash)
    {
      TriggerServerEvent("ofw_grp:ConfirmGroupInviteResponse", requestingPlayerHash, IsWaitingForGroup);
    }
    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        await Delay(0);

      Tick += GroupDisplay;
      Tick += ProcessGroupDistance;
      Tick += PeriodicGroupRefresh;
    }
    #endregion

    #region private metody
    private void DrawGroup()
    {
      float x = 0.995f;
      float y = 0.5f;
      float width = 0.18f;
      float height = 0.2f;

      var s1 = new StringBuilder();
      var s2 = new StringBuilder();

      s1.Append("~s~PARTY~n~");

      if (Group != null && Group.IsInAGroup)
      {
        for (int i = 0; i < Group.Members.Length; i++)
        {
          var p = Group.Members[i];
          if (p == null)
            continue;

          if (i < 2)
            s1.AppendFormat("{0}ID:{1} {2} [{3:0}m]~n~", (p.IsInQuestRange) ? "~g~" : (p.IsInRange) ? "~o~" : "~r~", p.ServerPlayerID, p.DisplayName, p.Distance);
          else
            s2.AppendFormat("{0}ID:{1} {2} [{3:0}m]~n~", (p.IsInQuestRange) ? "~g~" : (p.IsInRange) ? "~o~" : "~r~", p.ServerPlayerID, p.DisplayName, p.Distance);
        }
      }
      else if (IsWaitingForGroup)
      {
        s2.Append("~g~Waiting for invite");
      }
      //DrawRect(x + (width / 2) - width, y + (height / 2), width, height, 255, 0, 0, 100);

      SetTextScale(0f, 0.25f);
      SetTextFont(0);
      SetTextProportional(true);
      SetTextWrap(x - width, x);
      SetTextJustification(2);
      SetTextColour(255, 255, 255, 255);
      SetTextOutline();
      SetTextEntry("TWOSTRINGS");
      SetTextCentre(false);
      AddTextComponentString(s1.ToString());
      AddTextComponentString2(s2.ToString());

      EndTextCommandDisplayText(x, y);
    }

    #endregion

    #region public metody
    public static bool CheckGroupInQuestDistance(Vector3 pos)
    {
      if (Group == null || Group.Members == null || Group.Members.Length <= 0)
        return true;

      foreach (var m in Group.Members)
      {
        if (m.Distance > questDistance)
        {
          Notify.Alert("Clenove party jsou moc daleko!");
          return false;
        }
      }

      return true;
    }

    #endregion

    public GroupManager()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
      EventHandlers["ofw_grp:RefreshGroupInfo"] += new Action<string>(RefreshGroupInfo);
      EventHandlers["ofw_grp:NotifyError"] += new Action<string>(NotifyError);
      EventHandlers["ofw_grp:NotifySuccess"] += new Action<string>(NotifySuccess);
      EventHandlers["ofw_grp:ConfirmGroupInvite"] += new Action<int>(ConfirmGroupInvite);

      TriggerServerEvent("ofw_grp:RequestGroupInfoRefresh");
    }

    
  }
}
