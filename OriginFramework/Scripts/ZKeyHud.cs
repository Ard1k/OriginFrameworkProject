using CitizenFX.Core;
using OriginFrameworkData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
  public class ZKeyHud : BaseScript
  {
    private float renderDist = 15f;
    private float idScale = 0.4f;
    private float displayIDHeight = 1.2f; //Height of ID above players head (starts at center body mass)

    public ZKeyHud()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      Tick += OnTick;
    }

    private async Task OnTick()
    {
      if (Game.IsControlPressed(0, Control.MultiplayerInfo) || Game.IsDisabledControlPressed(0, Control.MultiplayerInfo)) //Z
      {
        foreach (var p in this.Players)
        {
          var ped = Ped.FromHandle(GetPlayerPed(p.Handle));

          if (ped == null)
            continue;

          float dist = Vector3.Distance(Game.PlayerPed.Position, ped.Position);
          
          if (dist > renderDist)
            continue;

          var adjustedCoords = AdjustCoords(ped, ped.Position);

          if (NetworkIsPlayerTalking(p.ServerId))
            TextUtils.Draw3dTextNonPrespective(adjustedCoords, displayIDHeight, idScale, p.ServerId.ToString(), 79, 146, 255, 200);
          else
            TextUtils.Draw3dTextNonPrespective(adjustedCoords, displayIDHeight, idScale, p.ServerId.ToString(), 255, 255, 255, 200);
        }

        if (CharacterCaretaker.LoggedCharacter?.AdminLevel > 0)
        {
          TextUtils.DrawTextOnScreen($"X:{Game.PlayerPed.Position.X.ToString("0.0")}, Y:{Game.PlayerPed.Position.Y.ToString("0.0")}, Z:{Game.PlayerPed.Position.Z.ToString("0.0")}", 0.5f, 0.95f, 0.3f, CitizenFX.Core.UI.Alignment.Center); 
        }
      }
    }

    private Vector3 AdjustCoords(Entity ped, Vector3 pos)
    {
      int veh = GetVehiclePedIsIn(ped.Handle, false);

      if (veh <= 0)
        return pos;

      string[] seatBones = { "seat_dside_f", "seat_pside_f", "seat_dside_r", "seat_pside_r" };
      int seats = GetVehicleModelNumberOfSeats((uint)GetEntityModel(veh));

      for (int i = -1; i <= Math.Min(2, seats); i++)
      {
        if (i <= 2 && GetPedInVehicleSeat(veh, i) == ped.Handle)
        {
          int idx = GetEntityBoneIndexByName(veh, seatBones[i + 1]);

          if (idx > 0)
          {
            return GetWorldPositionOfEntityBone(veh, idx);
          }
          else
            return pos + new Vector3(0,0,(i+1)/10);
        }
      }

      return pos;
    }
  }
}
