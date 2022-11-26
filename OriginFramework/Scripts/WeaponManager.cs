using CitizenFX.Core;
using OriginFrameworkData;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
  public class WeaponManager : BaseScript
  {
    private static ItemDefinition equippedDefinition = null;
    private static int ammoCount = 0;
    private const int _fistHash = -1569615261;
    private static bool wasShooting;
    private static bool shotPrevFrame;
    private static int lastShootTime;

    public WeaponManager()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      Tick += OnTick;
      Tick += OnSlowTick;
    }

    private async Task OnTick()
    {
      if (Game.IsDisabledControlJustPressed(0, Control.SelectWeapon))
        UnequipWeapon();


      if (equippedDefinition != null)
      {
        var timer = GetGameTimer();

        if (IsPedShooting(Game.PlayerPed.Handle))
        {
          wasShooting = true;
          shotPrevFrame = true;
        }
        else if (shotPrevFrame)
        {
          shotPrevFrame = false;
          lastShootTime = timer;
        }

        if (wasShooting && timer - lastShootTime > 1000)
        {
          SyncAmmo();
        }
      }

    }

    private async Task OnSlowTick()
    {
      int bestWeapon = GetBestPedWeapon(Game.PlayerPed.Handle, false);

      if (bestWeapon != _fistHash && (equippedDefinition == null || bestWeapon != equippedDefinition.WeaponHash))
      {
        //TODO - report red flag
        TheBugger.DebugLog("Tuhle zbran nemas mit v ruce");
        if (equippedDefinition != null)
          SyncAmmo(); //pro jistotu kdyby to bylo false positive, tak at nedostanou sync na 0
        RemoveAllPedWeapons(Game.PlayerPed.Handle, true);
      }

      await Delay(1000);
    }

    public static void EquipWeapon(int itemId, int ammo)
    {
      equippedDefinition = ItemsDefinitions.Items[itemId];
      ammoCount = ammo;

      SetPedCanSwitchWeapon(Game.PlayerPed.Handle, true);
      SetWeaponsNoAutoswap(true);
      SetWeaponsNoAutoreload(true);

      RemoveAllPedWeapons(Game.PlayerPed.Handle, true);
      GiveWeaponToPed(Game.PlayerPed.Handle, (uint)equippedDefinition.WeaponHash, ammoCount, true, true);
    }

    public static void UnequipWeapon()
    {
      if (equippedDefinition != null)
      {
        SyncAmmo();
        equippedDefinition = null;
        ammoCount = 0;
      }

      RemoveAllPedWeapons(Game.PlayerPed.Handle, true);
    }

    private static void SyncAmmo()
    {
      wasShooting = false;

      if (ammoCount == 0)
        return; //uz sem mel 0, neni co syncovat

      if (equippedDefinition == null)
      {
        TheBugger.DebugLog("AMMO Sync requested without weapon equipped");
        return;
      }

      int weaponAmmo = GetAmmoInPedWeapon(Game.PlayerPed.Handle, (uint)equippedDefinition.WeaponHash);
      int diff = ammoCount - weaponAmmo;
      if (diff <= 0)
        return; //neni co synchronizovat

      ammoCount -= diff;
      TriggerServerEvent("ofw_inventory:SyncAmmo", equippedDefinition.ItemId, diff);
    }
  }
}
