using CitizenFX.Core;
using Newtonsoft.Json;
using OriginFramework.Menus;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static OriginFramework.OfwFunctions;


namespace OriginFramework
{
  public class VehicleClient : BaseScript
  {
    public VehicleClient()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }


    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.VehicleClient))
        return;

      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);

      
      RegisterCommand("dv", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        if (CharacterCaretaker.LoggedCharacter?.AdminLevel > 0 == false)
        {
          Notify.Error("Nedostatečné oprávnění!");
        }

        if (Game.PlayerPed.IsInVehicle())
        {
          int veh = Game.PlayerPed.CurrentVehicle.Handle;
          DeleteEntity(ref veh);
        }
      }), false);


      InternalDependencyManager.Started(eScriptArea.VehicleClient);
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
    }
  }
}
