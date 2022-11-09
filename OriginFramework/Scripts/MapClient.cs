using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using Newtonsoft.Json;
using OriginFramework.Helpers;
using OriginFramework.Menus;
using OriginFrameworkData.DataBags;
using static CitizenFX.Core.Native.API;

namespace OriginFramework.Scripts
{
  public class MapClient : BaseScript
  {
    private bool isLocked = false;
    private List<MapBag> loadedMaps = new List<MapBag>();
    private double distanceLimit = 200;
    private int mapPropLimit = 50;

    public MapClient()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.MapClient))
        return;

      Tick += KeepObjectsUp;

      TriggerServerEvent("ofw_map:GetAllMaps");

      RegisterCommand("loadmap", new Action<int, List<object>, string>((source, args, raw) =>
      {
        if (args == null || args.Count <= 0)
        {
          Notify.Error("You must enter map name");
          return;
        }
        TriggerServerEvent("ofw_map:LoadMap", args[0]);
      }), false);

      RegisterCommand("destroymap", new Action<int, List<object>, string>((source, args, raw) =>
      {
        if (args == null || args.Count <= 0)
        {
          Notify.Error("You must enter map name");
          return;
        }
        TriggerServerEvent("ofw_map:DestroyMap", args[0]);
      }), false);

      RegisterCommand("mapproplimit", new Action<int, List<object>, string>((source, args, raw) =>
      {
        if (args == null || args.Count <= 0)
        {
          Notify.Error("You must enter map name");
          return;
        }
        if (args[0] is string newPropLimit)
          mapPropLimit = Int32.Parse(newPropLimit);
      }), false);

      InternalDependencyManager.Started(eScriptArea.MapClient);
    }

    private async Task KeepObjectsUp()
    {
      await Delay(200);
      var playerPos = Game.PlayerPed.Position;

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
              int _id = prop.LocalID;
              DeleteObject(ref _id);
              prop.LocalID = _id;
            }
          }

          loadedMaps.Remove(loadedMaps[i]);
        }
      }

      foreach (var map in loadedMaps)
      {
        if (isLocked)
          break;
        foreach (var prop in map.Props)
        {
          if (prop.IsNetworked)
            prop.PlayerDistance = 999999999999999999999f; //blah
          prop.PlayerDistance = Vector3.Distance(new Vector3(prop.EntityPosX, prop.EntityPosY, prop.EntityPosZ), playerPos);
        }
        map.Props = map.Props.OrderBy(p => p.PlayerDistance).ToList();
        
        for (int i = 0; i < map.Props.Count; i++)
        {
          if (map.Props[i].IsNetworked)
            continue;

          if (map.Props[i].PlayerDistance < distanceLimit && i < mapPropLimit)
          {
            if (map.Props[i].LocalID <= 0)
            {
              map.Props[i].LocalID = CreateObject(map.Props[i].PropHash, map.Props[i].EntityPosX, map.Props[i].EntityPosY, map.Props[i].EntityPosZ, false, false, false);
              SetEntityRotation(map.Props[i].LocalID, map.Props[i].EntityPitch, map.Props[i].EntityRoll, map.Props[i].EntityYaw, 0, true);
            }
          }
          else
          {
            if (map.Props[i].LocalID > 0)
            {
              int _id = map.Props[i].LocalID;
              DeleteObject(ref _id);
              map.Props[i].LocalID = _id;
            }
          }
        }
      }
    }

    [EventHandler("ofw_map:MapLoaded")]
    private void MapLoaded(string mapData)
    {
      if (string.IsNullOrEmpty(mapData))
      {
        Debug.WriteLine("OFW_MAP: invalid map to load");
        return;
      }

      var map = JsonConvert.DeserializeObject<MapBag>(mapData);

      isLocked = true;
      loadedMaps.Add(map);
      isLocked = false;
    }

    [EventHandler("ofw_map:MapDestroyed")]
    private void MapDestroyed(string mapName)
    {
      if (string.IsNullOrEmpty(mapName))
      {
        Debug.WriteLine("OFW_MAP: invalid map to destroy");
        return;
      }

      var map = loadedMaps.Where(m => m.Name.Equals(mapName)).FirstOrDefault();
      if (map != null)
        map.IsDestroy = true;
    }

    [EventHandler("ofw_map:AllMapsSync")]
    private void AllMapsSync(string mapData)
    {
      if (string.IsNullOrEmpty(mapData))
      {
        Debug.WriteLine("OFW_MAP: invalid map sync data");
        return;
      }

      List<MapBag> maps = JsonConvert.DeserializeObject<List<MapBag>>(mapData);
      if (maps != null)
      {
        isLocked = true;
        foreach (var m in maps)
        {
          if (!loadedMaps.Any(l => l.Name.Equals(m.Name)))
            loadedMaps.Add(m);
        }
        isLocked = false;
      }
    }
  }
}
