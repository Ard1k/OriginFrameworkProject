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
  public class OIDServer : BaseScript
  {
    private static int index = 10000;
    private static bool allowNosteamName = true;
    public static List<OIDBag> OriginServerIdentifiers { get; private set; } = new List<OIDBag>();

    public OIDServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
    }

    private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.OIDServer))
        return;

      InternalDependencyManager.Started(eScriptArea.OIDServer);
    }

    [EventHandler("ofw_oid:GetMyOriginID")]
    private void GetMyOriginID([FromSource] Player source, NetworkCallbackDelegate callback)
    {
      if (source == null)
      {
        Debug.WriteLine("ofw_oid:GetMyOriginID: This event can't be called from server!");
        callback(-1, false);
      }

      var playerOID = GetOriginServerID(source);
      var isAdmin = IsPlayerAceAllowed(source.Handle, "command");

      _ = callback(playerOID.OID, isAdmin);
    }

    public static OIDBag GetOriginServerID(Player p)
    {
      if (p == null)
      {
        Debug.WriteLine("OID: Player is null, cannot return OID");
        return null;
      }

      var lic = p.Identifiers.ToArray().Where(l => l.StartsWith("license:")).FirstOrDefault();
      var steam = p.Identifiers.ToArray().Where(l => l.StartsWith("steam:")).FirstOrDefault();
      var discord = p.Identifiers.ToArray().Where(l => l.StartsWith("discord:")).FirstOrDefault();
      var ip = p.Identifiers.ToArray().Where(l => l.StartsWith("ip:")).FirstOrDefault();
      string primary_identifier = steam;

      if (primary_identifier == null)
      {
        if (allowNosteamName)
          primary_identifier = p.Name;
        else
        {
          p.Drop("Nepodařilo se spojit se službou steam!");
          return null;
        }
      }

      var known = OriginServerIdentifiers.Where(oid => oid.PrimaryIdentifier == primary_identifier).FirstOrDefault();

      if (known != null)
      {
        known.LastServerID = Int32.Parse(p.Handle);
        return known;
      }

      var newOID = new OIDBag { PrimaryIdentifier = primary_identifier, Discord = discord, IP = ip, License = lic, Steam = steam, LastServerID = Int32.Parse(p.Handle), OID = ++index };
      OriginServerIdentifiers.Add(newOID);

      Debug.WriteLine($"OID: Created new OID:[{newOID.OID}]");
      return newOID;
    }

    public static int[] GetOIDsFromServerIds(Player[] players)
    {
      List<int> oids = new List<int>();

      foreach (var p in players)
      {
        oids.Add(GetOriginServerID(p)?.OID ?? -1);
      }

      return oids.ToArray();
    }

    public static int GetLastKnownServerID(int oid)
    {
      var oidBag = OriginServerIdentifiers.Where(o => o.OID == oid).FirstOrDefault();

      if (oidBag == null)
        return -1;

      return oidBag.LastServerID;
    }
  }
}
