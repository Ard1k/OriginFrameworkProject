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
    public static int carryEntity = 0;

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
      await Delay(500);
      var handsIt = InventoryManager.PlayerInventoryCache.Items.Where(it => it.X == -1 && it.Y == 100).FirstOrDefault();
      if (handsIt != null && carryEntity == 0)
      {
        await SetItemCarry(ItemsDefinitions.Items[handsIt.ItemId]);
      }
      else if (handsIt == null && carryEntity != 0)
      {
        DeleteObject(ref carryEntity);
        carryEntity = 0;
        ClearPedTasksImmediately(Game.PlayerPed.Handle);
      }
    }

    private async Task SetItemCarry(ItemDefinition itDef)
    {
      if (itDef.CarryType != eItemCarryType.Hands || itDef.CarryInfo == null)
        return;

      await LoadAnimDict(itDef.CarryInfo.CarryAnim);
      TaskPlayAnim(Game.PlayerPed.Handle, itDef.CarryInfo.CarryAnim, "idle", 4.0f, 1.0f, -1, 49, 0f, false, false, false);
      carryEntity = CreateObject(GetHashKey(itDef.CarryInfo.PropName), 0f, 0f, 0f, true, false, false);
      SetEntityCollision(carryEntity, false, false);

      AttachEntityToEntity(carryEntity, Game.PlayerPed.Handle, GetPedBoneIndex(Game.PlayerPed.Handle, itDef.CarryInfo.PedBoneId),
        itDef.CarryInfo.XPos, itDef.CarryInfo.YPos, itDef.CarryInfo.ZPos,
        itDef.CarryInfo.XRot, itDef.CarryInfo.YRot, itDef.CarryInfo.ZRot,
        true, true, false, true, 1, true);
    }

    private async Task LoadAnimDict(string anim)
    {
      RequestAnimDict(anim);
      while (!HasAnimDictLoaded(anim))
      {
        await Delay(50);
      }
    }

    [EventHandler("ofw_carry:Event")]
    private void MapLoaded(string mapData)
    {

    }

    
  }
}
