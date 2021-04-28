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
    public static List<OIDBag> OriginServerIdentifiers { get; private set; } = new List<OIDBag>();
    public static PlayerList ServerPlayers = null;

    public OIDServer()
    {
      ServerPlayers = Players;
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
    }

    private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        await Delay(0);

    }

    [EventHandler("ofw_oid:GetMyOriginID")]
    private void GetMyOriginID([FromSource] Player source, NetworkCallbackDelegate callback)
    {
      if (source == null)
      {
        Debug.WriteLine("ofw_oid:GetMyOriginID: This event can't be called from server!");
        callback(-1);
      }

      var playerOID = GetOriginServerID(source);

      _ = callback(playerOID);
    }

    public static int GetOriginServerID(Player p)
    {
      if (p == null)
      {
        Debug.WriteLine("OID: Player is null, cannot return OID");
        return -1;
      }

      var lic = p.Identifiers.ToArray().Where(l => l.StartsWith("license:")).FirstOrDefault();

      if (lic == null)
      {
        Debug.WriteLine("OID: Player doesn't have license identifier, return server ID");
        return int.Parse(p.Handle);
      }

      var known = OriginServerIdentifiers.Where(oid => oid.License == lic).FirstOrDefault();

      if (known != null)
        return known.OID;

      var newOID = new OIDBag { License = lic, OID = ++index };
      OriginServerIdentifiers.Add(newOID);

      Debug.WriteLine($"OID: Created new OID:[{newOID.OID}] License:[{newOID.License}]");
      return newOID.OID;
    }

    public static int[] GetOIDsFromServerIds(Player[] players)
    {
      List<int> oids = new List<int>();

      foreach (var p in players)
      {
        oids.Add(GetOriginServerID(p));
      }

      return oids.ToArray();
    }
  }
}
