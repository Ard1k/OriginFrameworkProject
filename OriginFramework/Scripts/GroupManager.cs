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
      if (Group != null && Group.Members != null)
      {
        for (int i = 0; i < Group.Members.Length; i++)
        {
          var p = Group.Members[i];

          if (i == 0)
          {
            p.Distance = 0f;
            p.IsInRange = true;
          }
          if (i == 1)
          {
            p.Distance = 999f;
            p.IsInRange = false;
          }
          if (i == 2)
          {
            p.Distance = 999f;
          }
          if (i == 3)
          {
            p.Distance = 150.34568f;
            p.IsInRange = true;
          }
        }
      }

      await Delay(500);
    }

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
            s1.AppendFormat("{0}ID:{1} {2} [{3:0}m]~n~", (!p.IsOnline) ? "~r~" : (!p.IsInRange) ? "~c~" : "~g~", p.ServerPlayerID, p.DisplayName, p.Distance);
          else
            s2.AppendFormat("{0}ID:{1} {2} [{3:0}m]~n~", (!p.IsOnline) ? "~r~" : (!p.IsInRange) ? "~c~" : "~g~", p.ServerPlayerID, p.DisplayName, p.Distance);
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

    public GroupManager()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
      EventHandlers["ofw_grp:RefreshGroupInfo"] += new Action<string>(RefreshGroupInfo);
      EventHandlers["ofw_grp:NotifyError"] += new Action<string>(NotifyError);
      EventHandlers["ofw_grp:NotifySuccess"] += new Action<string>(NotifySuccess);

      TriggerServerEvent("ofw_grp:RequestGroupInfoRefresh");
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        await Delay(0);

      Tick += GroupDisplay;
      Tick += ProcessGroupDistance;
    }

    
  }
}
