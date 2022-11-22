using CitizenFX.Core;
using CitizenFX.Core.UI;
using Newtonsoft.Json;
using OriginFramework.Menus;
using OriginFrameworkData;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
	public class SkinEditor : BaseScript
	{
    public static bool IsInSkinEditor { get; private set; } = false;
    private const string skinEditorMenuName = "skin_editor_menu";
    public static ItemDefinition ItemDefinition { get; private set; } = null;
    public static string CurrentPedModel = "mp_m_freemode_01";
    public static eSkinEditState CurrentState { get; protected set; } = eSkinEditState.None;
    public enum eSkinEditState : int
    {
      None = 0,
      ItemEdit = 2,
    }

    public SkinEditor()
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
      if (IsInSkinEditor)
      {
        if (!NativeMenuManager.IsMenuOpen(skinEditorMenuName))
          NativeMenuManager.EnsureMenuOpen(skinEditorMenuName, getSkinEditorMenu);

        EnsurePedModel(0, false);

        if (CurrentState == eSkinEditState.ItemEdit && ItemDefinition.Texture != null)
          DrawSprite("inventory_textures", ItemDefinition.Texture, 0.7f, 0.15f, 0.1f / Screen.AspectRatio, 0.1f, 0f, ItemDefinition.Color?.R ?? 255, ItemDefinition.Color?.G ?? 255, ItemDefinition.Color?.B ?? 255, 255);
      }
		}

    #region menu
    private NativeMenu getSkinEditorMenu()
    {
      switch (CurrentState)
      {
        default:
        case eSkinEditState.None:
          {
            ItemDefinition = null;
            var menu = new NativeMenu
            {
              MenuTitle = "Skin List",
              Items = new List<NativeMenuItem>
              {
                new NativeMenuItem
                {
                  Name = "Exit",
                  OnSelected = (item) => { ExitEditor(); },
                  IsClose = true,
                },
                new NativeMenuItem
                {
                  IsUnselectable = true
                }
              }
            };

            for (int i = 0; i < 9; i++)
            {
              var iterator = i; //kvuli opozdenemu volani
              menu.Items.Add(new NativeMenuItem 
              {
                Name = $"Slot: [{((eSpecialSlotType)iterator).ToString()}]",
                GetSubMenu = () => { return getSlotMenu((eSpecialSlotType)iterator); }
              });
            }
            return menu;
          }
        case eSkinEditState.ItemEdit:
          {
            return SkinEditorMenu.GenerateMenu(SkinManager.GetClothesForSlot((eSpecialSlotType)ItemDefinition.SpecialSlotType));
          }
      }
    }

    private NativeMenu getSlotMenu(eSpecialSlotType slotType)
    {
      var menu = new NativeMenu
      {
        MenuTitle = "Skin List",
        Items = new List<NativeMenuItem>
              {
                new NativeMenuItem
                {
                  Name = "Nový předmět",
                  OnSelected = (item) => 
                  {
                    ItemDefinition = new ItemDefinition { SpecialSlotType = slotType, StackSize = 1, Color = new InventoryColor(0, 255, 255, 255) };
                    CurrentState = eSkinEditState.ItemEdit;
                    EnsurePedModel(0, true);
                  },
                  IsClose = true,
                },
                new NativeMenuItem
                {
                  IsUnselectable = true
                }
              }
      };

      var slotItems = ItemsDefinitions.Items.Where(it => it != null && it.SpecialSlotType == slotType && it.ItemId > 0).OrderByDescending(it2 => it2.ItemId).ToList();
      if (slotItems != null && slotItems.Count > 0)
      {
        foreach (var si in slotItems)
        {
          menu.Items.Add(new NativeMenuItem 
          {
            Name = $"[{si.ItemId}] {si.Name}",
            OnSelected = (item) => 
            {
              ItemDefinition = si.GetInstanceCopy();
              CurrentState = eSkinEditState.ItemEdit;
              EnsurePedModel(0, true);
            },
            IsClose = true
          });
        }
      }

      return menu;
    }
    #endregion

    public static async void EnsurePedModel(int modelRequest, bool forceDefault)
    {
      if (modelRequest == 0)
        modelRequest = GetHashKey(CurrentPedModel);

      int requestedHash = modelRequest;
      if (Game.PlayerPed.Model.Hash != requestedHash)
      {
        uint model = (uint)requestedHash;

        RequestModel(model);
        while (!HasModelLoaded(model))
        {
          RequestModel(model);

          await BaseScript.Delay(0);
        }
        SetPlayerModel(Game.Player.Handle, model);
        SetPedDefaultComponentVariation(Game.PlayerPed.Handle);
        SetGameplayCamRelativeHeading(180f);
        SetModelAsNoLongerNeeded(model);
      }
      else if (!forceDefault)
        return;

      SkinManager.SetDefaultSkin(SkinManager.ClothesAll);
      if (modelRequest == GetHashKey("mp_m_freemode_01") && ItemDefinition?.MaleSkin != null)
        SkinManager.ApplySkin(ItemDefinition.MaleSkin);
      else if (modelRequest == GetHashKey("mp_f_freemode_01") && ItemDefinition?.FemaleSkin != null)
        SkinManager.ApplySkin(ItemDefinition.FemaleSkin);
    }

    public static async void EnterEditor()
    {
      ItemDefinition = null;
      CurrentPedModel = "mp_m_freemode_01";
      CurrentState = eSkinEditState.None;
      while (NativeMenuManager.LockInMenu(skinEditorMenuName) == false)
        await Delay(0);
      IsInSkinEditor = true;
      EnsurePedModel(0, true);
    }

    public static async void SendItemToServer(bool isMale, bool isFemale)
    {
      if (ItemDefinition.Texture == null)
      {
        Notify.Error("Item nemá texturu");
        return;
      }
      if (string.IsNullOrEmpty(ItemDefinition.Name))
      {
        Notify.Error("Item nemá jméno");
        return;
      }
      if (ItemDefinition.StackSize <= 0)
        ItemDefinition.StackSize = 1;
      if (ItemDefinition.SpecialSlotType == null)
      {
        Notify.Error("Item nemá typ slotu!");
        return;
      }

      var defCopy = ItemDefinition.GetInstanceCopy();
      if (!isMale)
        defCopy.MaleSkin = null;
      if (!isFemale)
        defCopy.FemaleSkin = null;

      TriggerServerEvent("ofw_core:UpdateDynamicItemDefinitions", JsonConvert.SerializeObject(defCopy));
    }

    public static async void ExitEditor()
    {
      ItemDefinition = null;
      CurrentState = eSkinEditState.None;
      NativeMenuManager.UnlockMenu(skinEditorMenuName);
      IsInSkinEditor = false;
      if (CharacterCaretaker.LoggedCharacter?.Model != 0)
        EnsurePedModel(CharacterCaretaker.LoggedCharacter.Model, true);

      SkinManager.ClearCache();
      TriggerServerEvent("ofw_inventory:ReloadInventory", null);
    }
  }
}
