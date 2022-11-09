using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using Newtonsoft.Json;
using OriginFramework.Menus;
using OriginFrameworkData.DataBags;
using static CitizenFX.Core.Native.API;

namespace OriginFramework.Scripts
{
  public class MapClient : BaseScript
  {
    private List<MapBag> loadedMaps = new List<MapBag>();
    private double distanceLimit = 200;
    private int propLimit = 50;

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

      RegisterCommand("loadmap", new Action<int, List<object>, string>((source, args, raw) =>
      {
        if (args == null || args.Count <= 0)
        {
          Notify.Error("You must enter map name");
          return;
        }
        TriggerServerEvent("ofw_map:LoadMap", args[0]);
      }), false);

      InternalDependencyManager.Started(eScriptArea.MapClient);
    }

    private async Task KeepObjectsUp()
    {
      var playerPos = Game.PlayerPed.Position;

      foreach (var map in loadedMaps)
      {
        foreach (var prop in map.Props)
        {
          if (prop.IsNetworked)
            continue;

          if (Vector3.Distance(new Vector3(prop.EntityPosX, prop.EntityPosY, prop.EntityPosZ), playerPos) < distanceLimit)
          {
            if (prop.LocalID <= 0)
            {
              prop.LocalID = CreateObject(prop.PropHash, prop.EntityPosX, prop.EntityPosY, prop.EntityPosZ, false, false, false);
              SetEntityRotation(prop.LocalID, prop.EntityPitch, prop.EntityRoll, prop.EntityYaw, 0, true);
            }
          }
          else
          {
            if (prop.LocalID > 0)
            {
              int _id = prop.LocalID;
              DeleteObject(ref _id);
              prop.LocalID = _id;
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
      loadedMaps.Add(map);
      Debug.WriteLine($"OFW_MAP: loaded {map.Name}");
    }
  }
}
