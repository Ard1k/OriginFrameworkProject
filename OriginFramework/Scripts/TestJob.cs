using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;


namespace OriginFramework
{
  public class TestJob : BaseScript
  {
    public TestJob()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        await Delay(0);

      if (!SettingsManager.Settings.TestJobActive)
        return;

      RegisterCommand("tveh", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        int ret = -1;
        bool completed = false;

        BaseScript.TriggerEvent("chat:addMessage", new
        {
          color = new[] { 255, 0, 0 },
          args = new[] { "Test" }
        });

        Func<int, bool> CallbackFunction = (data) =>
        {
          ret = data;
          completed = true;
          return true;
        };

        BaseScript.TriggerServerEvent("ofw_veh:SpawnServerVehicle", args[0], Game.PlayerPed.Position, Game.PlayerPed.Heading, CallbackFunction);

        while (!completed)
        {
          await Delay(0);
        }

        BaseScript.TriggerEvent("chat:addMessage", new
        {
          color = new[] { 255, 0, 0 },
          args = new[] { "NetID: " + ret }
        });

      }), false);

      Tick += OnTick;
    }

    private async Task OnTick()
		{
    }

  }
}
