using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework.Menus
{
  public static class Vehicle_HandlingInspector
  {
    public static int Veh = 0;
    private const string className = "CHandlingData";
    private static string[] handlingFloats = 
    { 
      "fMass", 
      "fInitialDragCoeff",
      "fPercentSubmerged",
      "fDriveBiasFront", 
      "fInitialDriveForce",
      "fDriveInertia",
      "fClutchChangeRateScaleUpShift",
      "fClutchChangeRateScaleDownShift",
      "fInitialDriveMaxFlatVel",
      "fBrakeForce",
      "fBrakeBiasFront",
      "fHandBrakeForce",
      "fSteeringLock",
      "fTractionCurveMax",
      "fTractionCurveMin",
      "fTractionCurveLateral",
      "fTractionSpringDeltaMax",
      "fLowSpeedTractionLossMult",
      "fCamberStiffnesss",
      "fTractionBiasFront",
      "fTractionLossMult",
      "fSuspensionForce",
      "fSuspensionCompDamp",
      "fSuspensionReboundDamp",
      "fSuspensionUpperLimit",
      "fSuspensionLowerLimit",
      "fSuspensionRaise",
      "fSuspensionBiasFront",
      "fAntiRollBarForce",
      "fAntiRollBarBiasFront",
      "fRollCentreHeightFront",
      "fRollCentreHeightRear",
      "fCollisionDamageMult",
      "fWeaponDamageMult",
      "fDeformationDamageMult",
      "fEngineDamageMult",
      "fPetrolTankVolume",
      "fOilVolume",
      "fSeatOffsetDistX",
      "fSeatOffsetDistY",
      "fSeatOffsetDistZ",
      "nMonetaryValue",
    };

    public static NativeMenu GenerateMenu()
    {
      int Veh = GetVehiclePedIsIn(Game.PlayerPed.Handle, false);

      NativeMenu menu = new NativeMenu
      {
        MenuTitle = "HandlingTuning",
        Items = new List<NativeMenuItem>
          {
            new NativeMenuItem { Name = "Zpět", IsBack = true },
            new NativeMenuItem { Name = "Obnovit", IsRefresh = true, OnSelected = (item) => 
            { 
              Veh = GetVehiclePedIsIn(Game.PlayerPed.Handle, false);
              SetVehicleWheelType(Veh, 4);
              SetVehicleModKit(Veh, 0);

              int i = 1;
              while (GetVehicleMod(Veh, 23) == -1 && i < 50)
              {
                SetVehicleMod(Veh, 23, i, false);
                //SetVehicleMod(Veh, 24, i, true);
                i++;
              }

              TheBugger.DebugLog($"VEH: [{Veh}] {GetVehicleWheelType(Veh)} / {GetVehicleMod(Veh, 23)} / {GetVehicleMod(Veh, 24)}");
            } },
            new NativeMenuItem { IsUnselectable = true }
          }
      };

      menu.Items.Add(new NativeMenuItem
      {
        Name = "Wheel_width",
        NameRight = Veh > 0 ? GetVehicleWheelWidth(Veh).ToString("0.0000") : "---",
        IsTextInput = true,
        OnTextInput = (item, input) =>
        {
          if (Veh < 0)
          {
            item.NameRight = "---";
            return;
          }
          float inputFloat;
          if (float.TryParse(input, out inputFloat))
          {
            SetVehicleWheelWidth(Veh, inputFloat);
            item.NameRight = GetVehicleWheelWidth(Veh).ToString("0.0000");
          }
          else
            Notify.Error("Neplatny float");
        },
        OnRefresh = (item) =>
        {
          if (Veh < 0)
          {
            item.NameRight = "---";
            return;
          }
          item.NameRight = GetVehicleWheelWidth(Veh).ToString("0.0000");
        }
      });

      menu.Items.Add(new NativeMenuItem
      {
        Name = "Wheel_size",
        NameRight = Veh > 0 ? GetVehicleWheelSize(Veh).ToString("0.0000") : "---",
        IsTextInput = true,
        OnTextInput = (item, input) =>
        {
          if (Veh < 0)
          {
            item.NameRight = "---";
            return;
          }
          float inputFloat;
          if (float.TryParse(input, out inputFloat))
          {
            SetVehicleWheelSize(Veh, inputFloat);
            item.NameRight = GetVehicleWheelSize(Veh).ToString("0.0000");
          }
          else
            Notify.Error("Neplatny float");
        },
        OnRefresh = (item) =>
        {
          if (Veh < 0)
          {
            item.NameRight = "---";
            return;
          }
          item.NameRight = GetVehicleWheelSize(Veh).ToString("0.0000");
        }
      });

      foreach (var h in handlingFloats)
      {
        menu.Items.Add(new NativeMenuItem
        {
          Name = h,
          NameRight = Veh > 0 ? GetVehicleHandlingFloat(Veh, className, h).ToString("0.0000") : "---",
          IsTextInput = true,
          OnTextInput = (item, input) =>
          {
            if (Veh < 0)
            {
              item.NameRight = "---";
              return;
            }
            float inputFloat;
            if (float.TryParse(input, out inputFloat))
            {
              SetVehicleHandlingFloat(Veh, className, h, inputFloat);
              item.NameRight = GetVehicleHandlingFloat(Veh, className, h).ToString("0.0000");
            }
            else
              Notify.Error("Neplatny float");
          },
          OnRefresh = (item) =>
          {
            if (Veh < 0)
            {
              item.NameRight = "---";
              return;
            }
            item.NameRight = GetVehicleHandlingFloat(Veh, className, h).ToString("0.0000");
          }
        });
      }

      return menu;
    }
  }
}
