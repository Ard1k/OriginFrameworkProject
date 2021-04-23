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

namespace OriginFrameworkServer
{
  public class GroupServer : BaseScript
  {
    private int lastGroupIndex = 0;
    private const int securitySalt = 54898745; //uff, je to stupidni, ale aspon neco :D
    private Dictionary<int, int> DictionaryPlayerInGroup = new Dictionary<int, int>();

    public GroupServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
    }

    private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        await Delay(0);
    }

    [EventHandler("ofw_grp:RequestGroupInfoRefresh")]
    private void RequestGrouInfoRefresh([FromSource] Player source)
    {
      SendGroupToSource(source, false);
    }

    

    [EventHandler("ofw_grp:CreateGroup")]
    private void CreateGroup([FromSource] Player source)
    {
      var spH = int.Parse(source.Handle);

      Debug.WriteLine($"triggered ofw_grp:CreateGroup [Handle: {spH}]");

      if (DictionaryPlayerInGroup.ContainsKey(spH))
      {
        source.TriggerEvent("ofw_grp:NotifyError", "You are already in a party!");
        return;
      }

      var newGroupIndex = ++lastGroupIndex;
      DictionaryPlayerInGroup.Add(spH, newGroupIndex);

      source.TriggerEvent("ofw_grp:NotifySuccess", "Party created!");
      SendGroupToSource(source, true);
    }

    [EventHandler("ofw_grp:InviteToGroup")]
    private void InviteToGroup([FromSource] Player source, int targetPlayerID)
    {
      var spH = int.Parse(source.Handle);

      Debug.WriteLine($"triggered ofw_grp:InviteToGroup [Handle: {spH}][TargetPlayer: {targetPlayerID}]");

      if (DictionaryPlayerInGroup.ContainsKey(targetPlayerID))
      {
        source.TriggerEvent("ofw_grp:NotifyError", "Player is already in a party!");
        return;
      }

      if (!DictionaryPlayerInGroup.ContainsKey(spH))
      {
        source.TriggerEvent("ofw_grp:NotifyError", "You are not in a group!");
        return;
      }

      var groupID = DictionaryPlayerInGroup[spH];
      if (groupID <= 0)
      {
        Debug.WriteLine("OFW ERROR: 489742214 - group not found after validation!");
        source.TriggerEvent("ofw_grp:NotifyError", "You are not in a group!");
        return;
      }

      var memberCount = DictionaryPlayerInGroup.Where(g => g.Value == groupID)?.Count();
      if (memberCount != null && memberCount.Value >= 4)
      {
        source.TriggerEvent("ofw_grp:NotifyError", "Group is full");
        return;
      }

      var targetPlayer = Players.Where(p => p.Handle == targetPlayerID.ToString()).FirstOrDefault();
      if (targetPlayer == null)
      {
        Debug.WriteLine("OFW ERROR: 544548778814 - cannot find player object!");
        source.TriggerEvent("ofw_grp:NotifyError", "Internal server error");
        return;
      }

      targetPlayer.TriggerEvent("ofw_grp:ConfirmGroupInvite", (spH + securitySalt));
    }

    [EventHandler("ofw_grp:ConfirmGroupInviteResponse")]
    private void ConfirmGroupInviteResponse([FromSource] Player source, int inviteSenderHash, bool accepted)
    {
      var senderH = inviteSenderHash - securitySalt;
      var sourceId = int.Parse(source.Handle);

      Debug.WriteLine($"triggered ofw_grp:InviteToGroup [OriginalSender: {senderH}][AcceptingPlayer: {source.Handle}]");
      
      var senderPlayer = Players.Where(p => p.Handle == senderH.ToString()).FirstOrDefault();
      if (senderPlayer == null)
      {
        Debug.WriteLine("OFW ERROR: 878548778814 - cannot find player object!");
        return;
      }

      if (!accepted)
      {
        senderPlayer.TriggerEvent("ofw_grp:NotifySuccess", "Player is not waiting for invite!");
        return;
      }

      if (!DictionaryPlayerInGroup.ContainsKey(senderH))
      {
        Debug.WriteLine("ofw_grp:ConfirmGroupInviteResponse - inviting player doesnt have a group!");
        return;
      }

      var groupId = DictionaryPlayerInGroup[senderH];

      if (groupId <= 0)
      {
        Debug.WriteLine("ofw_grp:ConfirmGroupInviteResponse - invalid group ID");
        return;
      }

      if (DictionaryPlayerInGroup.ContainsKey(sourceId))
      {
        source.TriggerEvent("ofw_grp:NotifyError", "Invited player has already a group!");
        return;
      }

      DictionaryPlayerInGroup.Add(sourceId, groupId);
      senderPlayer.TriggerEvent("ofw_grp:NotifySuccess", "Invite accepted!");
      SendGroupToSource(source, true);
    }

    [EventHandler("ofw_grp:LeaveGroup")]
    private void LeaveGroup([FromSource] Player source)
    {
      var spH = int.Parse(source.Handle);

      if (!DictionaryPlayerInGroup.ContainsKey(spH))
      {
        source.TriggerEvent("ofw_grp:NotifyError", "You are not member of a party!");
        return;
      }

      int groupId = DictionaryPlayerInGroup[spH];
      DictionaryPlayerInGroup.Remove(spH);
      SendGroupToSource(source, false);
      source.TriggerEvent("ofw_grp:NotifySuccess", "You left party!");

      var anyMember = DictionaryPlayerInGroup.Where(gk => gk.Value == groupId).Select(gk => gk.Key).FirstOrDefault();
      if (anyMember > 0)
      {
        var anyMemberPlayer = Players[anyMember];

        SendGroupToSource(anyMemberPlayer, true);
      }
    }

    private void SendGroupToSource(Player source, bool sendToAllMembers)
    {
      if (source == null || !Players.Contains(source)) //Neni online
      {
        Debug.WriteLine($"SendGroupToSource: Player not online [Handle{source?.Handle ?? "null"}]");
        return;
      }

      int spH = int.Parse(source.Handle);

      int groupId = -1;
      if (DictionaryPlayerInGroup.ContainsKey(spH))
        groupId = DictionaryPlayerInGroup[spH];

      if (groupId <= 0)
      {
        Debug.WriteLine($"SendGroupToSource: Player not in group, returning empty [Handle{source?.Handle ?? "null"}]");
        source.TriggerEvent("ofw_grp:RefreshGroupInfo", JsonConvert.SerializeObject(new GroupBag { IsInAGroup = false }));
        return;
      }

      var members = DictionaryPlayerInGroup.Where(gk => gk.Value == groupId).Select(gk => gk.Key).ToList();

      var memberPlayers = new List<Player>();
      foreach (var m in members)
      {
        if (Players[m] != null)
          memberPlayers.Add(Players[m]);
        else
          DictionaryPlayerInGroup.Remove(m); //Neni online, cistime

      }

      var memberBags = new List<GroupMemberBag>();

      foreach (var m in memberPlayers)
      {
        memberBags.Add(new GroupMemberBag { ServerPlayerID = int.Parse(m.Handle), DisplayName = m.Name, IsOnline = true, NetPedID = m.Character.NetworkId });
      }

      var bag = new GroupBag
      {
        IsInAGroup = true,
        Members = memberBags.ToArray()
      };

      if (sendToAllMembers)
      {
        foreach (var m in memberPlayers)
        {
          Debug.WriteLine($"SendGroupToSource: Sending for each player [Current Handle{m?.Handle ?? "null"}]");
          m.TriggerEvent("ofw_grp:RefreshGroupInfo", JsonConvert.SerializeObject(bag));
        }
      }
      else
      {
        Debug.WriteLine($"SendGroupToSource: Sending only for source [Handle{source?.Handle ?? "null"}]");
        source.TriggerEvent("ofw_grp:RefreshGroupInfo", JsonConvert.SerializeObject(bag));
      }
    }
  }
}
