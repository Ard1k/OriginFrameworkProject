﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using OriginFramework.ClientDataBags;
using OriginFramework.Helpers;
using OriginFramework.Menus;
using OriginFrameworkData.DataBags;
using static CitizenFX.Core.Native.API;

namespace OriginFramework.Scripts
{
  public class BlipClient : BaseScript
  {
    public static List<BlipBag> Blips { get; set; } = new List<BlipBag>()
    {
      new BlipBag { BlipId = 825, Scale = 1f, Color = 33, PosVector3 = new Vector3(-34f, -1105f, 26.42f), Label = "Regular cars", UniqueId = "STATIC_PDM" },
    };

    public BlipClient()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.BlipClient))
        return;

      foreach (var b in Blips)
        EnsureBlip(b);

      InternalDependencyManager.Started(eScriptArea.BlipClient);
    }

    public static void AddBlip(BlipBag blip)
    {
      if (blip.UniqueId != null)
      {
        if (Blips.Any(b => b.UniqueId == blip.UniqueId))
        {
          var existingBlip = Blips.Where(b => b.UniqueId == blip.UniqueId).First();
          RemoveCurrentBlipIfExists(existingBlip);
          Blips.Remove(existingBlip);
        }

        Blips.Add(blip);
        EnsureBlip(blip);
      }
    }

    private static void EnsureBlip(BlipBag blip)
    {
      RemoveCurrentBlipIfExists(blip);

      blip.Id = AddBlipForCoord(blip.PosVector3.X, blip.PosVector3.Y, blip.PosVector3.Z);
      SetBlipSprite(blip.Id, blip.BlipId);
      SetBlipDisplay(blip.Id, 2);
      SetBlipScale(blip.Id, blip.Scale);
      SetBlipColour(blip.Id, blip.Color);
      SetBlipAsShortRange(blip.Id, true);
      BeginTextCommandSetBlipName("STRING");
      AddTextComponentString(blip.Label ?? "_MissingLabel");
      EndTextCommandSetBlipName(blip.Id);
    }

    private static void RemoveCurrentBlipIfExists(BlipBag blip)
    {
      if (blip.Id != 0 && DoesBlipExist(blip.Id))
      {
        int blipId = blip.Id;
        RemoveBlip(ref blipId);
      }

      blip.Id = 0;
    }
  }
}
