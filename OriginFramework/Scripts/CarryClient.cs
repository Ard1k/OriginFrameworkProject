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
  public class CarryClient : BaseScript
  {

    public CarryClient()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.CarryClient, eScriptArea.InventoryManager))
        return;

      Tick += OnTick;

      RegisterCommand("carry", new Action<int, List<object>, string>((source, args, raw) =>
      {
        if (args == null || args.Count <= 0)
        {
          Notify.Error("You must enter map name");
          return;
        }
      }), false);

      InternalDependencyManager.Started(eScriptArea.CarryClient);
    }

    private async Task OnTick()
    {
      await Delay(200);
      
    }

    [EventHandler("ofw_carry:Event")]
    private void MapLoaded(string mapData)
    {

    }

    
  }
}
