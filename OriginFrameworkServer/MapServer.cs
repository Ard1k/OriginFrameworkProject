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

namespace OriginFrameworkServer
{
  public class MapServer : BaseScript
  {
    private LockObj syncLock = new LockObj("MapServer");
    private List<MapBag> loadedMaps = new List<MapBag>();

    public MapServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
    }

		#region event handlers

		private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.MapServer, eScriptArea.VSql))
        return;

      Tick += KeepObjectsUp;

      InternalDependencyManager.Started(eScriptArea.MapServer);
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
      
    }

    private async Task KeepObjectsUp()
    {
      using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
      {
        if (Players != null && Players.Count() > 0 || Players.Any(p => p.Character != null))
        {
          for (int i = loadedMaps.Count - 1; i >= 0; i--)
          {
            if (loadedMaps[i].IsDestroy)
            {
              foreach (var prop in loadedMaps[i].Props)
              {
                if (prop.IsNetworked)
                  continue;

                if (prop.LocalID > 0)
                {
                  DeleteEntity(prop.LocalID);
                }
              }

              loadedMaps.Remove(loadedMaps[i]);
            }
          }

          foreach (var map in loadedMaps)
          {
            foreach (var prop in map.Props)
            {
              if (!prop.IsNetworked)
                continue;

              if (prop.NetID <= 0)
              {
                prop.NetID = CreateObject(prop.PropHash, prop.EntityPosX, prop.EntityPosY, prop.EntityPosZ, true, false, false);

                
              }
              SetEntityRotation(prop.NetID, prop.EntityPitch, prop.EntityRoll, prop.EntityYaw, 0, true);
            }
          }
        }
      }
      await (Delay(2000));
    }

    [EventHandler("ofw_map:SaveMap")]
    private async void SaveMap([FromSource] Player source, string mapData)
    {
      if (mapData == null)
        return;

      var mapObj = JsonConvert.DeserializeObject<MapBag>(mapData);

      if (mapObj == null || mapObj.Props == null || mapObj.Props.Count <= 0)
        return;

      var param = new Dictionary<string, object>();
      param.Add("@name", mapObj.Name);
      var result = await VSql.FetchAllAsync("SELECT `id`FROM `prop_map` WHERE `name` = @name", param);

      if (result != null && result.Count > 0)
      {
        Debug.WriteLine("ofw_map:SaveMap: Map name duplicity, not saving... ");
        source?.TriggerEvent("ofw:ValidationErrorNotification", "Map duplicity!");
        return;
      }

      var result2 = await VSql.FetchScalarAsync("insert into `prop_map` (`name`) VALUES (@name); select LAST_INSERT_ID();", param);
      int mapId = Convert.ToInt32(result2);
      if (mapId <= 0)
      {
        Debug.WriteLine("ofw_map:SaveMap: Map id error... ");
        return;
      }

      var param2 = new Dictionary<string, object> { };
      param2.Add("@mapid", mapId);

      bool isFirst = true;
      string sql = String.Empty;
      int counter = 0;
      foreach (var prop in mapObj.Props)
      {
        prop.MapId = mapId;

        if (isFirst)
        {
          sql = "insert into prop_map_item(`prop_map_id`, `data`) VALUES (@mapid, @v0)";
          isFirst = false;
        }
        else
          sql += $",(@mapid, @v{counter})";

        param2.Add($"v{counter}", JsonConvert.SerializeObject(prop));
        counter++;
      }

      VSql.ExecuteAsync(sql, param2);
      source?.TriggerEvent("ofw:SuccessNotification", "Map saved");
    }

    [EventHandler("ofw_map:LoadMap")]
    private async void LoadMap([FromSource] Player source, string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        source?.TriggerEvent("ofw:ValidationErrorNotification", "Name not provided!");
      }

      var param = new Dictionary<string, object>();
      param.Add("@name", name);
      var result = await VSql.FetchScalarAsync("select id from `prop_map` where `name` = @name", param);
      int mapId = Convert.ToInt32(result);
      if (mapId <= 0)
      {
        source?.TriggerEvent("ofw:ValidationErrorNotification", "Map name not found!");
        return;
      }

      var param2 = new Dictionary<string, object>();
      param2.Add("@mapid", mapId);
      var result2 = await VSql.FetchAllAsync("SELECT `data`FROM `prop_map_item` WHERE `prop_map_id` = @mapid", param2);

      if (result2 == null || result2.Count <= 0)
      {
        source?.TriggerEvent("ofw:ValidationErrorNotification", "Map dont have any props, not loatding!");
        return;
      }

      var mapObj = new MapBag();
      mapObj.Name = name;
      mapObj.Id = mapId;
      mapObj.Props = new List<MapPropBag>();

      foreach (var res in result2)
      {
        mapObj.Props.Add(JsonConvert.DeserializeObject<MapPropBag>((string)res["data"]));
      }

      using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
      {
        loadedMaps.Add(mapObj);
      }

      source?.TriggerEvent("ofw:SuccessNotification", "Map loaded");
      TriggerClientEvent("ofw_map:MapLoaded", JsonConvert.SerializeObject(mapObj));
    }

    [EventHandler("ofw_map:DestroyMap")]
    private async void DestroyMap([FromSource] Player source, string mapName)
    {
      using (var sl = await SyncLocker.GetLockerWhenAvailible(syncLock))
      {
        var map = loadedMaps.Where(m => m.Name.Equals(mapName)).FirstOrDefault();
        if (map != null)
        {
          map.IsDestroy = true;
          source?.TriggerEvent("ofw:SuccessNotification", "Map destroyed");
          TriggerClientEvent("ofw_map:MapDestroyed", mapName);
        }
        else
        {
          source?.TriggerEvent("ofw:ErrorNotification", "Map not found");
        }
      }
    }

    [EventHandler("ofw_map:GetAllMaps")]
    private async void GetAllMaps([FromSource] Player source)
    {
      if (loadedMaps != null && loadedMaps.Count > 0)
        source?.TriggerEvent("ofw_map:AllMapsSync", JsonConvert.SerializeObject(loadedMaps));
    }
    #endregion

    #region private


    #endregion
  }
}
