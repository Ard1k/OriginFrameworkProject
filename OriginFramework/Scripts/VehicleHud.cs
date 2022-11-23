using CitizenFX.Core;
using CitizenFX.Core.UI;
using OriginFrameworkData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
  public class VehicleHud : BaseScript
  {
    public bool IsHudEnabled { get; protected set; }
    public int speed = 0;

    private int calcX = 0, calcY = 0;
    private float x1 = 0f, y1 = 0f, x2 = 0f, y2 = 0f;

    public VehicleHud()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      Tick += OnTick;
      Tick += OnMidTick;
      Tick += OnSlowTick;
    }

    private async Task OnSlowTick()
    {
      int veh = GetVehiclePedIsIn(Game.PlayerPed.Handle, false);

      IsHudEnabled = veh > 0;

      if (IsHudEnabled)
        RecalcMinimapPos(); //tohle delat jen pri zmene velikosti minimapy

      await Delay(500);
    }

    private async Task OnMidTick()
    {
      if (IsHudEnabled)
      {
        int veh = GetVehiclePedIsIn(Game.PlayerPed.Handle, false);
        if (veh > 0)
          speed = (int)Math.Floor(GetEntitySpeed(veh) * 3.6f / 1.609344f);
      }

      await Delay(100);
    }

    private async Task OnTick()
    {
      if (IsHudEnabled)
      {
        DrawUtils.DrawRect2(x2 - 0.025f, y2 - 0.06f, x2, y2 - 0.03f, 0, 0, 0, 150);
        TextUtils.DrawTextOnScreen(speed.ToString(), x2 - 0.002f, ((y2 - 0.03f) - 0.015f) - 0.35f / TextUtils.TextHalfHConst, 0.35f, CitizenFX.Core.UI.Alignment.Right);
      }
    }

    private void RecalcMinimapPos()
    {
      //if (calcX == Screen.Width && calcY == Screen.Height)
      //  return;

      GetActiveScreenResolution(ref calcX, ref calcY);
      float safezone = GetSafeZoneSize();
      float safezone_x = 1f / 20f;
      float safezone_y = 1f / 20f;
      float xScale = 1f / calcX;
      float yScale = 1f / calcY;

      float width = xScale * (calcX / (4.001f * Screen.AspectRatio));
      float height = yScale * (calcY / 5.674f);
      x1 = xScale * (calcX * (safezone_x * (Math.Abs(safezone - 1.0f) * 10)));
      y2 = 1.0f - yScale * (calcY * (safezone_y * (Math.Abs(safezone - 1.0f) * 10)));
      x2 = x1 + width;
      y1 = y2 - height;
    }
  }
}
