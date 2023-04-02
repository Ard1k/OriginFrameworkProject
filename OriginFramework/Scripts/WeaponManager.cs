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
    private static bool _putAwayLock = false;

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
          lastShootTime = timer;
        }
        else if (shotPrevFrame)
        {
          shotPrevFrame = false;
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
        UnequipWeapon();
      }

      await Delay(1000);
    }

    public static async void EquipWeapon(int itemId, int ammo)
    {
      equippedDefinition = ItemsDefinitions.Items[itemId];

      int maxWepAmmo = ammo;
      if (GetMaxAmmo(Game.PlayerPed.Handle, (uint)equippedDefinition.WeaponHash, ref maxWepAmmo))
      {
        if (ammo > maxWepAmmo)
          ammo = maxWepAmmo;
      }

      ammoCount = ammo;

      SetPedCanSwitchWeapon(Game.PlayerPed.Handle, true);
      SetWeaponsNoAutoswap(true);
      SetWeaponsNoAutoreload(true);

      RemoveAllPedWeapons(Game.PlayerPed.Handle, true);
      if (equippedDefinition.UseAnim != null)
      {
        while (!ProgressBar.Start(equippedDefinition.UseAnim.Time, equippedDefinition.Name, false, null, equippedDefinition.UseAnim))
          await Delay(0);

        await Delay(equippedDefinition.UseAnim.Time / 2);
      }
      GiveWeaponToPed(Game.PlayerPed.Handle, (uint)equippedDefinition.WeaponHash, ammoCount, true, true);
    }

    public static async void UnequipWeapon()
    {
      if (_putAwayLock)
        return;

      try
      {
        _putAwayLock = true;

        if (equippedDefinition?.PutAwayAnim != null)
        {
          while (!ProgressBar.Start(equippedDefinition.PutAwayAnim.Time, equippedDefinition.Name, false, null, equippedDefinition.PutAwayAnim))
            await Delay(0);

          await Delay(equippedDefinition.PutAwayAnim.Time / 2);
        }

        if (equippedDefinition != null)
        {
          SyncAmmo();
          equippedDefinition = null;
          ammoCount = 0;
        }

        RemoveAllPedWeapons(Game.PlayerPed.Handle, true);
      }
      finally
      {
        _putAwayLock = false;
      }
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
