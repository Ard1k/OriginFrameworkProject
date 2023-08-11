using CitizenFX.Core;
using Newtonsoft.Json;
using OriginFramework.Menus;
using OriginFramework.Scripts;
using OriginFrameworkData;
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
  public class VehicleVendorClient : BaseScript
  {
    public static List<VehicleVendorSlot> Slots = new List<VehicleVendorSlot>();
    public const string MenuName = "vehicle_vendor_menu";

    public VehicleVendorClient()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      var slotsString = await Callbacks.ServerAsyncCallbackToSync<string>("ofw_vehvendor:SyncSlots");
      if (slotsString != null)
      { 
        var slots = JsonConvert.DeserializeObject<List<VehicleVendorSlot>>(slotsString);
        if (slots != null && slots.Count > 0)
        {
          foreach (var slot in slots)
          {
            var existingSlot = Slots.Where(s => s.SlotId == slot.SlotId).FirstOrDefault();
            if (existingSlot != null)
              Slots.Remove(existingSlot);

            Slots.Add(slot);
          }
        }
      }

      Tick += OnTick;
      Tick += OnSlowTick;
      Tick += DrawTextOnVehTick;

    }

    private async Task OnTick()
    {
      RemoveVehiclesFromGeneratorsInArea(-74.0333f, - 1120.7878f, 24.9239f, - 5.3972f, - 1073.6084f, 44.7104f, 0); //nespawnovani aut v PDM

      if (Game.PlayerPed.CurrentVehicle == null || GetPedInVehicleSeat(Game.PlayerPed.CurrentVehicle.Handle, -1) != Game.PlayerPed.Handle)
      {
        NativeMenuManager.CloseAndUnlockMenu(MenuName);
        return;
      }

      int vehId = Game.PlayerPed.CurrentVehicle.Handle;

      if (!NetworkGetEntityIsNetworked(vehId))
        return;

      int vehNetId = VehToNet(vehId);

      var inSlot = Slots.Where(s => s.SpawnedNetId != null && s.SpawnedNetId == vehNetId).FirstOrDefault();

      if (inSlot == null)
      {
        NativeMenuManager.CloseAndUnlockMenu(MenuName);
        return;
      }

      EnsureVendorMenu(inSlot);
    }

    private async Task DrawTextOnVehTick()
    {
      var playerPos = Game.PlayerPed.Position;

      for (int i = 0; i < Slots.Count; i++)
      {
        if (Slots[i].SpawnedNetId == null)
          continue;

        if (!NetworkDoesNetworkIdExist(Slots[i].SpawnedNetId.Value) || !NetworkDoesEntityExistWithNetworkId(Slots[i].SpawnedNetId.Value))
          continue;

        var vehId = NetworkGetEntityFromNetworkId(Slots[i].SpawnedNetId.Value);

        if (vehId <= 0)
          continue;

        var veh = new Vehicle(vehId);

        var distance = Vector3.DistanceSquared(veh.Position, playerPos);
        if (distance < 2500 && veh.IsOnScreen)
        {
          if (HasEntityClearLosToEntity(Game.PlayerPed.Handle, vehId, 17)) //Hoodne draha operace, co na to vykon scriptu?
            DrawVendorVehicleName(veh.Position, distance, $"{FontsManager.FiraSansString}NA PRODEJ");
        }
      }
    }

    private async Task OnSlowTick()
    {
      await Delay(500);

      
    }

    [EventHandler("ofw_vehvendor:SlotUpdated")]
    private async void SlotUpdated(string slotData)
    {
      if (slotData != null)
      {
        var slot = JsonConvert.DeserializeObject<VehicleVendorSlot>(slotData);
        if (slot != null)
        {
          var existingSlot = Slots.Where(s => s.SlotId == slot.SlotId).FirstOrDefault();
          if (existingSlot != null)
            Slots.Remove(existingSlot);

          Slots.Add(slot);
        }
      }
    }

    public void EnsureVendorMenu(VehicleVendorSlot slot)
    {
      if (NativeMenuManager.IsMenuOpen(MenuName))
        return;

      if (!HasStreamedTextureDictLoaded("inventory_textures"))
        RequestStreamedTextureDict("inventory_textures", true);

      if (slot.CurrentVehicle == null)
        return;

      NativeMenuManager.OpenNewMenu(MenuName, () => {
        var menu = new NativeMenu
        {
          MenuTitle = $"{GetDisplayNameFromVehicleModel((uint)GetHashKey(slot.CurrentVehicle.Model))}",
          Items = new List<NativeMenuItem>
          {
            
          }
        };

        menu.Items.Add(new NativeMenuItem
        {
          Name = "Platnost nabídky do",
          NameRight = $"{slot.RepopulateAt?.ToString("dd.M HH:mm") ?? "Neučeno"}",
          IsUnselectable = true
        });

        if (slot.CurrentVehicle.Price != null && slot.CurrentVehicle.PriceItemId != null)
        {
          menu.Items.Add(new NativeMenuItem
          {
            Name = slot.CurrentVehicle.PriceItemId == 17 ? "Hotově" : $"Výměnou za {ItemsDefinitions.Items[slot.CurrentVehicle.PriceItemId.Value].Name}",
            NameRight = $"{ItemsDefinitions.Items[slot.CurrentVehicle.PriceItemId.Value].FormatAmount(slot.CurrentVehicle.Price.Value)}",
            IconRight = ItemsDefinitions.Items[slot.CurrentVehicle.PriceItemId.Value].Texture,
            IconRightTextureDict = "inventory_textures",
            IsUnselectable = true
          });
        }

        if (slot.CurrentVehicle.BankMoneyPrice != null)
        {
          menu.Items.Add(new NativeMenuItem
          {
            Name = "Převodem",
            NameRight = $"{ItemsDefinitions.Items[17].FormatAmount(slot.CurrentVehicle.BankMoneyPrice.Value)}",
            IconRight = ItemsDefinitions.Items[17].Texture,
            IconRightTextureDict = "inventory_textures",
            IsUnselectable = true
          });
        }

        menu.Items.Add(new NativeMenuItem { IsUnselectable = true });
        menu.Items.Add(new NativeMenuItem 
        { 
          Name = "Koupit pro sebe",
          GetSubMenu = () => { return getPurchasePaymentMenu(slot, false); }
        });

        if (OrganizationClient.CurrentOrganization != null &&
            (CharacterCaretaker.LoggedCharacter.Id == OrganizationClient.CurrentOrganization.Owner || OrganizationClient.CurrentOrganization.Managers.Any(m => m.CharId == CharacterCaretaker.LoggedCharacter.Id)))
        {
          menu.Items.Add(new NativeMenuItem 
          { 
            Name = "Koupit na firmu",
            GetSubMenu = () => { return getPurchasePaymentMenu(slot, true); }
          });
        }

        return menu;
      } );
    }

    private NativeMenu getPurchasePaymentMenu(VehicleVendorSlot slot, bool isCompany)
    {
      var menu = new NativeMenu
      {
        MenuTitle = "Způsob platby",
        Items = new List<NativeMenuItem>()
      };

      if (slot.CurrentVehicle.Price != null && slot.CurrentVehicle.PriceItemId != null)
      {
        menu.Items.Add(new NativeMenuItem
        {
          Name = slot.CurrentVehicle.PriceItemId == 17 ? "Hotově" : $"Výměnou za {ItemsDefinitions.Items[slot.CurrentVehicle.PriceItemId.Value].Name}",
          NameRight = $"{ItemsDefinitions.Items[slot.CurrentVehicle.PriceItemId.Value].FormatAmount(slot.CurrentVehicle.Price.Value)}",
          IconRight = ItemsDefinitions.Items[slot.CurrentVehicle.PriceItemId.Value].Texture,
          IconRightTextureDict = "inventory_textures",
          GetSubMenu = () => { return getPurchaseConfirmMenu(slot, isCompany, false); }
        });
      }

      if (slot.CurrentVehicle.BankMoneyPrice != null)
      {
        menu.Items.Add(new NativeMenuItem
        {
          Name = "Převodem",
          NameRight = $"{ItemsDefinitions.Items[17].FormatAmount(slot.CurrentVehicle.BankMoneyPrice.Value)}",
          IconRight = ItemsDefinitions.Items[17].Texture,
          IconRightTextureDict = "inventory_textures",
          GetSubMenu = () => { return getPurchaseConfirmMenu(slot, isCompany, true); }
        });
      }

      return menu;
    }

    private NativeMenu getPurchaseConfirmMenu(VehicleVendorSlot slot, bool isCompany, bool isBankTransaction)
    {
      var menu = new NativeMenu
      {
        MenuTitle = "Potvrzení nákupu",
        Items = new List<NativeMenuItem>()
      };

      menu.Items.Add(new NativeMenuItem
      {
        Name = "Odběratel",
        NameRight = isCompany ? "Firma" : "Soukromá osoba",
        IsUnselectable = true
      });

      menu.Items.Add(new NativeMenuItem
      {
        Name = "Způsob úhrady",
        NameRight = isBankTransaction ? "Bankovní převod" : (slot.CurrentVehicle.PriceItemId == 17 ? "Hotově" : "Směna"),
        IsUnselectable = true
      });

      if (!isBankTransaction)
      {
        menu.Items.Add(new NativeMenuItem
        {
          Name = "Cena",
          NameRight = $"{ItemsDefinitions.Items[slot.CurrentVehicle.PriceItemId.Value].FormatAmount(slot.CurrentVehicle.Price.Value)}",
          IconRight = ItemsDefinitions.Items[slot.CurrentVehicle.PriceItemId.Value].Texture,
          IconRightTextureDict = "inventory_textures",
          IsUnselectable = true
        });
      }
      else
      {
        menu.Items.Add(new NativeMenuItem
        {
          Name = "Cena",
          NameRight = $"{ItemsDefinitions.Items[17].FormatAmount(slot.CurrentVehicle.BankMoneyPrice.Value)}",
          IconRight = ItemsDefinitions.Items[17].Texture,
          IconRightTextureDict = "inventory_textures",
          IsUnselectable = true
        });
      }

      menu.Items.Add(new NativeMenuItem { IsUnselectable = true });

      menu.Items.Add(new NativeMenuItem
      {
        Name = "Zrušit",
        IsClose = true
      });
      menu.Items.Add(new NativeMenuItem
      {
        Name = "Potvrdit",
        OnSelected = (item) => 
        {
          TriggerServerEvent("ofw_vehvendor:PurchaseVehicle", slot.SlotId, isCompany, isBankTransaction);
        },
        IsClose = true,
      });

      return menu;
    }

    private void DrawVendorVehicleName(Vector3 pos, float distanceSquared, string name)
    {
      var distance = (float)Math.Sqrt(distanceSquared);

      if (distance < 5f)
        distance = 5f;

      const float perspectiveScale = 1.8f;
      float _x = 0, _y = 0;
      World3dToScreen2d(pos.X, pos.Y, pos.Z + 1f, ref _x, ref _y);
      var p = GetGameplayCamCoords();
      //var fov = (1 / GetGameplayCamFov()) * 75;
      var scale = ((1 / distance) * perspectiveScale) /* * fov*/;

      SetTextScale(1, scale);
      SetTextFont(0);
      SetTextProportional(true);
      SetTextColour(0, 127, 255, 200);
      SetTextOutline();
      SetTextEntry("STRING");
      SetTextCentre(true);
      AddTextComponentString(name);
      DrawText(_x, _y);
    }
  }
}
