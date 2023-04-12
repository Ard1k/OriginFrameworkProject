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
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace OriginFrameworkServer
{
  public class OrganizationServer : BaseScript
  {
    public static List<OrganizationBag> Organizations = new List<OrganizationBag>();
    public static List<OrganizationInviteBag> OrgInvites = new List<OrganizationInviteBag>();

    public OrganizationServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
    }

		#region event handlers

		private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.OrganizationServer, eScriptArea.VSql))
        return;

      #region load cache
      var organizationResults = await VSql.FetchAllAsync("select * from `organization`", null);
      if (organizationResults != null && organizationResults.Count > 0)
      {
        foreach (var orgRes in organizationResults)
        {
          Organizations.Add(OrganizationBag.ParseFromSql(orgRes));
        }
      }

      Debug.WriteLine($"ofw_org: Organizations loaded: {Organizations.Count}");
      #endregion


      //owner, name, tag, color
      RegisterCommand("createorg", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        var sourcePlayer = Players.Where(p => p.Handle == source.ToString()).FirstOrDefault();
        if (sourcePlayer == null)
          return;

        if (!CharacterCaretakerServer.HasPlayerAdminLevel(sourcePlayer, 10))
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Nedostatečné oprávnění");
          return;
        }

        if (args == null || args.Count != 4)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neplatný počet argumentů");
          return;
        }

        int playerId = 0;
        int characterId = 0;
        OIDBag ownerOID = null;
        Player ownerPlayer = null;
        if (Int32.TryParse((string)args[0], out playerId))
        {
          ownerPlayer = Players.Where(p => p.Handle == playerId.ToString()).FirstOrDefault();
          if (ownerPlayer != null)
          {
            characterId = CharacterCaretakerServer.GetPlayerLoggedCharacterId(ownerPlayer);
            ownerOID = OIDServer.GetOriginServerID(ownerPlayer);
          }
        }

        if (characterId <= 0)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Nenalezen vlastník");
          return;
        }

        var param = new Dictionary<string, object>();
        param.Add("@owner", characterId);
        object ownerCurrentOrg = await VSql.FetchScalarAsync("select `organization_id` from `character` where `id` = @owner", param);
        if (ownerCurrentOrg != null && ownerCurrentOrg != DBNull.Value) 
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Tento hráč už je v organizaci");
          return;
        }

        var orgName = (string)args[1];
        var tag = (string)args[2];

        if (orgName == null || orgName.Length <= 3)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Název musí být delší než 3 znaky");
          return;
        }

        if (tag == null || tag.Length != 3)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Tag musí mít přesně 3 znaky");
          return;
        }

        param.Add("@tag", tag);

        var existingTagResult = await VSql.FetchAllAsync("select `tag` from `organization` where `tag` = @tag", param);
        if (existingTagResult != null && existingTagResult.Count > 0)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Tento tag už existuje");
          return;
        }

        string colorStr = (string)args[3];
        eOrganizationColor orgColor = eOrganizationColor.None;
        if (colorStr != null)
        {
          if (!Enum.TryParse(colorStr, true, out orgColor))
          {
            sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neznámá barva");
            return;
          }
        }

        param.Add("@name", orgName);
        param.Add("@color", (int)orgColor);
        var result = await VSql.ExecuteAsync("insert into `organization` (`name`, `owner`, `color`, `tag`) values (@name, @owner, @color, @tag)", param);

        if (result == 1)
        {
          object objId = await VSql.FetchScalarAsync("select `id` from `organization` where `tag` = @tag", param);
          if (objId != null && objId != DBNull.Value)
          {
            int id = (int)objId;
            param.Add("@orgId", id);
            _ = await VSql.ExecuteAsync("update `character` set `organization_id` = @orgId where `id` = @owner", param);
            CharacterCaretakerServer.UpdateOrganization(ownerOID, ownerPlayer, id);
            
            sourcePlayer.TriggerEvent("ofw:SuccessNotification", $"Organizace založena ID:{objId}");
            return;
          }
        }

        sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Založení organizace se nezdařilo - chyba ukládání do DB");
      }), false);

      Tick += OnTick;

      InternalDependencyManager.Started(eScriptArea.OrganizationServer);
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
    }

    private async Task OnTick()
    {
      await Delay(1000);

      for (int i = OrgInvites.Count - 1; i >= 0; i--)
      {
        if (DateTime.Now.Subtract(OrgInvites[i].Created).TotalSeconds > 20)
          OrgInvites.RemoveAt(i);
      }
    }

    [EventHandler("ofw_org:RequestOrganizationData")]
    private async void RequestOrganizationData([FromSource] Player source)
    {
      var sourcePlayer = Players.Where(p => p.Handle == source.Handle).FirstOrDefault();
      if (sourcePlayer == null)
        return;

      var character = CharacterCaretakerServer.GetPlayerLoggedCharacter(sourcePlayer);
      if (character == null)
        return;

      if (character.OrganizationId != null)
      {
        for (int i = 0; i < Organizations.Count; i++)
        {
          if (Organizations[i].Id == character.OrganizationId)
          {
            sourcePlayer.TriggerEvent("ofw_org:OrganizationUpdated", JsonConvert.SerializeObject(Organizations[i]));
            return;
          }
        }
      }

      sourcePlayer.TriggerEvent("ofw_org:OrganizationUpdated", String.Empty);
    }

    [EventHandler("ofw_org:InvitePlayerToOrg")]
    private async void InvitePlayerToOrg([FromSource] Player source, int invitedPlayerHandle)
    {
      var sourcePlayer = Players.Where(p => p.Handle == source.Handle).FirstOrDefault();
      if (sourcePlayer == null)
        return;

      var character = CharacterCaretakerServer.GetPlayerLoggedCharacter(sourcePlayer);
      if (character == null)
        return;

      OrganizationBag orgBag = null;

      if (character.OrganizationId != null)
      {
        for (int i = 0; i < Organizations.Count; i++)
        {
          if (Organizations[i].Id == character.OrganizationId)
          {
            orgBag = Organizations[i];
            break;
          }
        }
      }

      if (orgBag == null)
      {
        sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Na tohle nemáš oprávnění");
        return;
      }

      var invitedPlayer = Players.Where(p => p.Handle == invitedPlayerHandle.ToString()).FirstOrDefault();
      if (invitedPlayer == null)
      {
        sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Hráč nenalezen");
        return;
      }

      var invitedCharacter = CharacterCaretakerServer.GetPlayerLoggedCharacter(invitedPlayer);
      if (invitedCharacter == null)
      {
        sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Hráč nenalezen");
        return;
      }

      //pošlu teď - nechci aby mohl poznat jestli už má organizaci nebo ne
      sourcePlayer.TriggerEvent("ofw:SuccessNotification", "Pozvánka odeslána");

      if (invitedCharacter.OrganizationId != null)
      {
        invitedPlayer.TriggerEvent("ofw:ValidationErrorNotification", "Dostal jsi pozvánku do organizace, ty už ale nějákou máš");
        return;
      }

      if (OrgInvites.Any(inv => inv.CharId == invitedCharacter.Id))
        return;

      OrgInvites.Add(new OrganizationInviteBag(invitedCharacter.Id, orgBag.Id, source.Handle));
      invitedPlayer.TriggerEvent("ofw_org:OrganizationInvited", orgBag.Name);
    }

    [EventHandler("ofw_org:AcceptInvite")]
    private async void AcceptInvite([FromSource] Player source)
    {
      var sourcePlayer = Players.Where(p => p.Handle == source.Handle).FirstOrDefault();
      if (sourcePlayer == null)
        return;

      var character = CharacterCaretakerServer.GetPlayerLoggedCharacter(sourcePlayer);
      if (character == null)
        return;

      if (character.OrganizationId != null)
        return;

      var invite = OrgInvites.Where(inv => inv.CharId == character.Id).FirstOrDefault();
      if (invite == null)
      {
        sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Platnost pozvánky vypršela");
        return;
      }

      var param = new Dictionary<string, object>();
      param.Add("@charId", character.Id);
      param.Add("@orgId", invite.OrganizationId);
      var result = await VSql.ExecuteAsync("update `character` set `organization_id` = @orgId where `id` = @charId", param);

      if (result != 1)
      {
        sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Chyba při nastavování organizace");
        return;
      }

      var oid = OIDServer.GetOriginServerID(sourcePlayer);
      CharacterCaretakerServer.UpdateOrganization(oid, sourcePlayer, invite.OrganizationId);

      var inviteSenderPlayer = Players.Where(p => p.Handle == invite.InvitedByHandle).FirstOrDefault();
      if (inviteSenderPlayer != null)
      {
        inviteSenderPlayer.TriggerEvent("ofw:SuccessNotification", "Hráč přijal vaši pozvánku");
      }
    }

    [EventHandler("ofw_org:DeclineInvite")]
    private async void DeclineInvite([FromSource] Player source)
    {
      var sourcePlayer = Players.Where(p => p.Handle == source.Handle).FirstOrDefault();
      if (sourcePlayer == null)
        return;

      var character = CharacterCaretakerServer.GetPlayerLoggedCharacter(sourcePlayer);
      if (character == null)
        return;

      if (OrgInvites.Any(inv => inv.CharId == character.Id))
        OrgInvites.Remove(OrgInvites.Where(inv => inv.CharId == character.Id).First());
    }

    #endregion

    #region private


    #endregion
  }
}
