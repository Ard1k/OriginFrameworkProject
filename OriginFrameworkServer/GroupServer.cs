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
    private static Dictionary<int, int> DictionaryPlayerInGroup = new Dictionary<int, int>();

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
      //var spH = int.Parse(source.Handle);
      var oid = OIDServer.GetOriginServerID(source);

      //Debug.WriteLine($"triggered ofw_grp:CreateGroup [OID: {oid}]");

      if (DictionaryPlayerInGroup.ContainsKey(oid))
      {
        source.TriggerEvent("ofw_grp:NotifyError", "Uz jsi ve skupine!");
        return;
      }

      var newGroupIndex = ++lastGroupIndex;
      DictionaryPlayerInGroup.Add(oid, newGroupIndex);

      source.TriggerEvent("ofw_grp:NotifySuccess", "Skupina vytvorena!");
      SendGroupToSource(source, true);
    }

    [EventHandler("ofw_grp:InviteToGroup")]
    private void InviteToGroup([FromSource] Player source, int targetPlayerID)
    {
      var spH = int.Parse(source.Handle);
      var targetPlayer = Players.Where(p => p.Handle == targetPlayerID.ToString()).FirstOrDefault();
      if (targetPlayer == null)
      {
        Debug.WriteLine("OFW ERROR: 544548778814 - cannot find player object!");
        source.TriggerEvent("ofw_grp:NotifyError", "Interni chyba");
        return;
      }
      var targetOID = OIDServer.GetOriginServerID(targetPlayer);
      var sourceOID = OIDServer.GetOriginServerID(source);

      //Debug.WriteLine($"triggered ofw_grp:InviteToGroup [Handle: {spH}][TargetPlayer: {targetPlayerID}]");

      if (DictionaryPlayerInGroup.ContainsKey(targetOID))
      {
        source.TriggerEvent("ofw_grp:NotifyError", "Hrac uz je v nejake skupine!");
        return;
      }

      if (!DictionaryPlayerInGroup.ContainsKey(sourceOID))
      {
        source.TriggerEvent("ofw_grp:NotifyError", "Nejsi ve skupine!");
        return;
      }

      var groupID = DictionaryPlayerInGroup[sourceOID];
      if (groupID <= 0)
      {
        Debug.WriteLine("OFW ERROR: 489742214 - group not found after validation!");
        source.TriggerEvent("ofw_grp:NotifyError", "Nejsi ve skupine!");
        return;
      }

      var memberCount = DictionaryPlayerInGroup.Where(g => g.Value == groupID)?.Count();
      if (memberCount != null && memberCount.Value >= 4)
      {
        source.TriggerEvent("ofw_grp:NotifyError", "Skupina je plna");
        return;
      }

      targetPlayer.TriggerEvent("ofw_grp:ConfirmGroupInvite", (spH + securitySalt));
    }

    [EventHandler("ofw_grp:ConfirmGroupInviteResponse")]
    private void ConfirmGroupInviteResponse([FromSource] Player source, int inviteSenderHash, bool accepted)
    {
      var senderH = inviteSenderHash - securitySalt;
      var sourceId = int.Parse(source.Handle);
      var sourceOID = OIDServer.GetOriginServerID(source);

      //Debug.WriteLine($"triggered ofw_grp:InviteToGroup [OriginalSender: {senderH}][AcceptingPlayer: {source.Handle}]");
      
      var senderPlayer = Players.Where(p => p.Handle == senderH.ToString()).FirstOrDefault();
      if (senderPlayer == null)
      {
        Debug.WriteLine("OFW ERROR: 878548778814 - cannot find player object!");
        return;
      }

      var senderOID = OIDServer.GetOriginServerID(senderPlayer);

      if (!accepted)
      {
        senderPlayer.TriggerEvent("ofw_grp:NotifySuccess", "Hrac neceka na pozvanku!");
        return;
      }

      if (!DictionaryPlayerInGroup.ContainsKey(senderOID))
      {
        Debug.WriteLine("ofw_grp:ConfirmGroupInviteResponse - inviting player doesnt have a group!");
        return;
      }

      var groupId = DictionaryPlayerInGroup[senderOID];

      if (groupId <= 0)
      {
        Debug.WriteLine("ofw_grp:ConfirmGroupInviteResponse - invalid group ID");
        return;
      }

      if (DictionaryPlayerInGroup.ContainsKey(sourceOID))
      {
        source.TriggerEvent("ofw_grp:NotifyError", "Hrac uz je v nejake skupine!");
        return;
      }

      DictionaryPlayerInGroup.Add(sourceOID, groupId);
      senderPlayer.TriggerEvent("ofw_grp:NotifySuccess", "Pozvanka prijata!");
      SendGroupToSource(source, true);
    }

    [EventHandler("ofw_grp:LeaveGroup")]
    private void LeaveGroup([FromSource] Player source)
    {
      var oid = OIDServer.GetOriginServerID(source);

      if (!DictionaryPlayerInGroup.ContainsKey(oid))
      {
        source.TriggerEvent("ofw_grp:NotifyError", "Nejsi clenem zadne skupiny!");
        return;
      }

      int groupId = DictionaryPlayerInGroup[oid];
      DictionaryPlayerInGroup.Remove(oid);
      SendGroupToSource(source, false);
      source.TriggerEvent("ofw_grp:NotifySuccess", "Opustil jsi skupinu!");

      var anyMember = DictionaryPlayerInGroup.Where(gk => gk.Value == groupId).Select(gk => gk.Key).FirstOrDefault();
      if (anyMember > 0)
      {
        var anyMemberPlayer = Players[OIDServer.GetLastKnownServerID(anyMember)];
        if (anyMemberPlayer != null)
          SendGroupToSource(anyMemberPlayer, true);
      }
    }

    private void SendGroupToSource(Player source, bool sendToAllMembers)
    {
      if (source == null || !Players.Contains(source)) //Neni online
      {
        //Debug.WriteLine($"SendGroupToSource: Player not online [Handle{source?.Handle ?? "null"}]");
        return;
      }

      //int spH = int.Parse(source.Handle);
      int oid = OIDServer.GetOriginServerID(source);

      int groupId = -1;
      if (DictionaryPlayerInGroup.ContainsKey(oid))
        groupId = DictionaryPlayerInGroup[oid];

      if (groupId <= 0)
      {
        source.TriggerEvent("ofw_grp:RefreshGroupInfo", JsonConvert.SerializeObject(new GroupBag { IsInAGroup = false }));
        return;
      }

      var membersOIDs = DictionaryPlayerInGroup.Where(gk => gk.Value == groupId).Select(gk => gk.Key).ToList();

      var memberPlayers = new List<Player>();
      int offlineCount = 0;

      foreach (var moid in membersOIDs)
      {
        var lsID = OIDServer.GetLastKnownServerID(moid);

        if (lsID <= 0)
          continue;

        var player = Players.Where(p => p.Handle == lsID.ToString()).FirstOrDefault();
        if (player == null)
        {
          offlineCount++;
        }
        else
        {
          memberPlayers.Add(player);
        }
      }

      var memberBags = new List<GroupMemberBag>();
      foreach (var m in memberPlayers)
      {
        memberBags.Add(new GroupMemberBag { ServerPlayerID = int.Parse(m.Handle), DisplayName = m.Name, IsOnline = true, NetPedID = m.Character.NetworkId });
      }

      while (offlineCount > 0)
      {
        memberBags.Add(new GroupMemberBag { ServerPlayerID = -1, DisplayName = "OFFLINE", IsOnline = false, NetPedID = -1 });
        offlineCount--;
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
          //Debug.WriteLine($"SendGroupToSource: Sending for each player [Current Handle{m?.Handle ?? "null"}]");
          m.TriggerEvent("ofw_grp:RefreshGroupInfo", JsonConvert.SerializeObject(bag));
        }
      }
      else
      {
        //Debug.WriteLine($"SendGroupToSource: Sending only for source [Handle{source?.Handle ?? "null"}]");
        source.TriggerEvent("ofw_grp:RefreshGroupInfo", JsonConvert.SerializeObject(bag));
      }
    }

    public static int[] GetAllGroupMembersServerID(Player player)
    {
      var oid = OIDServer.GetOriginServerID(player);

      if (!DictionaryPlayerInGroup.ContainsKey(oid))
      {
        return null;
      }

      int groupId = DictionaryPlayerInGroup[oid];

      var membersOIDs = DictionaryPlayerInGroup.Where(gk => gk.Value == groupId).Select(gk => gk.Key).ToArray();

      var pserverIDs = new List<int>();
      
      foreach (var moid in membersOIDs)
      {
        pserverIDs.Add(OIDServer.GetLastKnownServerID(moid));
      }

      return pserverIDs.ToArray();
    }
  }
}
