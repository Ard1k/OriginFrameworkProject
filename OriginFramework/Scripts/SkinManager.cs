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
  public class SkinManager : BaseScript
  {
    private static Dictionary<string, SkinComponent> _components = null;
    public static Dictionary<string, SkinComponent> Components 
    { 
      get 
      {
        if (_components == null)
          _loadComponents();

        return _components;
      } 
    }

    public static string[] ComponentsAll 
    {
      get
      {
        return Components.Keys.ToArray();
      }
    }
    public static string[] ClothesAll { get; private set; } = new string[] {
      "tshirt_1", "tshirt_2", "torso_1", "torso_2", "arms_1", "arms_2", "decals_1", "decals_2", "bproof_1", "bproof_2", //torso
      "pants_1", "pants_2", //kalhoty
      "shoes_1", "shoes_2", //boty
      "mask_1", "mask_2", //maska
      "chain_1", "chain_2", //retizek
      "helmet_1", "helmet_2", //cepice/helma
      "glasses_1", "glasses_2", //bryle
      "watches_1", "watches_2", "bracelets_1", "bracelets_2", //zapesti
      "bags_1", "bags_2", //batohy
      "ears_1", "ears_2", //nausnice
    };

    public static string[] AppearanceAll { get; private set; } = new string[] {
      "face", "skin", 
      "hair_1", "hair_2", "hair_color_1", "hair_color_2", 
      "eye_color", "eyebrows_1", "eyebrows_2", "eyebrows_3", "eyebrows_4",
      "makeup_1", "makeup_2", "makeup_3", "makeup_4",
      "lipstick_1", "lipstick_2", "lipstick_3", "lipstick_4",
      "chest_1", "chest_2", "chest_3",
      "bodyb_1", "bodyb_2", "age_1", "age_2", "blemishes_1", "blemishes_2",
      "blush_1", "blush_2", "blush_3",
      "complexion_1", "complexion_2", "sun_1", "sun_2",
      "moles_1", "moles_2", 
      "beard_1", "beard_2", "beard_3", "beard_4"
    };

    public static string[] GetClothesForSlot(eSpecialSlotType slotType)
    {
      switch (slotType)
      {
        case eSpecialSlotType.Head: return new string[] { "helmet_1", "helmet_2", };
        case eSpecialSlotType.Mask: return new string[] { "mask_1", "mask_2", };
        case eSpecialSlotType.Glasses: return new string[] { "glasses_1", "glasses_2", };
        case eSpecialSlotType.Necklace: return new string[] { "chain_1", "chain_2", };
        case eSpecialSlotType.Earring: return new string[] { "ears_1", "ears_2", };
        case eSpecialSlotType.Wrist: return new string[] { "watches_1", "watches_2", "bracelets_1", "bracelets_2", };
        case eSpecialSlotType.Torso: return new string[] { "tshirt_1", "tshirt_2", "torso_1", "torso_2", "arms_1", "arms_2", "decals_1", "decals_2", "bproof_1", "bproof_2", };
        case eSpecialSlotType.Legs: return new string[] { "pants_1", "pants_2", };
        case eSpecialSlotType.Boots: return new string[] { "shoes_1", "shoes_2", };
        default: return new string[0];
      }
    }

    private static int[] equippedItemsCache = new int[9];
    public static void UpdateSkinFromInv(List<InventoryItemBag> items)
    {
      bool isFemale = Game.PlayerPed.Model.Hash == GetHashKey("mp_f_freemode_01");

      for (int i = 0; i < equippedItemsCache.Length; i++)
      {
        var it = items.Where(invIt => invIt.X == -1 && invIt.Y == i).FirstOrDefault();

        if (it == null)
        {
          if (equippedItemsCache[i] <= 0)
            continue;
          else
          {
            SetDefaultSkin(GetClothesForSlot((eSpecialSlotType)i));
            equippedItemsCache[i] = 0;
          }
        }
        else
        {
          if (it.ItemId == equippedItemsCache[i])
            continue;
          else
          {
            SetDefaultSkin(GetClothesForSlot((eSpecialSlotType)i));
            if (isFemale && ItemsDefinitions.Items[it.ItemId].FemaleSkin != null)
              ApplySkin(ItemsDefinitions.Items[it.ItemId].FemaleSkin);
            else if (!isFemale && ItemsDefinitions.Items[it.ItemId].MaleSkin != null)
              ApplySkin(ItemsDefinitions.Items[it.ItemId].MaleSkin);
            else
              Notify.Info("Tyhle hadry ti nepadnou");
            equippedItemsCache[i] = it.ItemId;
          }
        }
      }
    }

    public static void ClearCache()
    {
      equippedItemsCache = new int[9];
    }

    private static void _loadComponents()
    {
      _components = new Dictionary<string, SkinComponent>();
      _components.Add("face", new SkinComponent { Label = "Obličej", Name = "face", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.6f, CamOffset = 0.65f });
      _components.Add("skin", new SkinComponent { Label = "Kůže", Name = "skin", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.6f, CamOffset = 0.65f });
      _components.Add("hair_1", new SkinComponent { Label = "Vlasy 1", Name = "hair_1", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.6f, CamOffset = 0.65f, TextureFrom = "hair_2" });
      _components.Add("hair_2", new SkinComponent { Label = "Vlasy 2", Name = "hair_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.6f, CamOffset = 0.65f, TextureOf = "hair_1" });
      _components.Add("hair_color_1", new SkinComponent { Label = "Barva vlasů 1", Name = "hair_color_1", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.6f, CamOffset = 0.65f });
      _components.Add("hair_color_2", new SkinComponent { Label = "Barva vlasů 2", Name = "hair_color_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.6f, CamOffset = 0.65f });
      _components.Add("tshirt_1", new SkinComponent { Label = "Tričko", Name = "tshirt_1", DefaultValue = 15, DefaultFemale = 14, MinValue = 0, ZoomOffset = 0.75f, CamOffset = 0.15f, TextureFrom = "tshirt_2" });
      _components.Add("tshirt_2", new SkinComponent { Label = "Varianta trička", Name = "tshirt_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.75f, CamOffset = 0.15f, TextureOf = "tshirt_1" });
      _components.Add("torso_1", new SkinComponent { Label = "Torzo", Name = "torso_1", DefaultValue = 15, DefaultFemale = 101/*82*/, MinValue = 0, ZoomOffset = 0.75f, CamOffset = 0.15f, TextureFrom = "torso_2" });
      _components.Add("torso_2", new SkinComponent { Label = "Varianta torza", Name = "torso_2", DefaultValue = 0, DefaultFemale = 5, MinValue = 0, ZoomOffset = 0.75f, CamOffset = 0.15f, TextureOf = "torso_1" });
      _components.Add("decals_1", new SkinComponent { Label = "Decals", Name = "decals_1", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.75f, CamOffset = 0.15f, TextureFrom = "decals_2" });
      _components.Add("decals_2", new SkinComponent { Label = "Varianta decals", Name = "decals_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.75f, CamOffset = 0.15f, TextureOf = "decals_1" });
      _components.Add("arms_1", new SkinComponent { Label = "Ruce", Name = "arms_1", DefaultValue = 15, MinValue = 0, ZoomOffset = 0.75f, CamOffset = 0.15f, TextureFrom = "arms_2" });
      _components.Add("arms_2", new SkinComponent { Label = "Varianta rukou", Name = "arms_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.75f, CamOffset = 0.15f, TextureOf = "arms_1" });
      _components.Add("pants_1", new SkinComponent { Label = "Kalhoty", Name = "pants_1", DefaultValue = 21, DefaultFemale = 56, MinValue = 0, ZoomOffset = 0.8f, CamOffset = -0.5f, TextureFrom = "pants_2" });
      _components.Add("pants_2", new SkinComponent { Label = "Varianta kalhot", Name = "pants_2", DefaultValue = 0, DefaultFemale = 5, MinValue = 0, ZoomOffset = 0.8f, CamOffset = -0.5f, TextureOf = "pants_1" });
      _components.Add("shoes_1", new SkinComponent { Label = "Boty", Name = "shoes_1", DefaultValue = 34, DefaultFemale = 35, MinValue = 0, ZoomOffset = 0.8f, CamOffset = -0.8f, TextureFrom = "shoes_2" });
      _components.Add("shoes_2", new SkinComponent { Label = "Varianta bot", Name = "shoes_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.8f, CamOffset = -0.8f, TextureOf = "shoes_1" });
      _components.Add("mask_1", new SkinComponent { Label = "Maska", Name = "mask_1", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.6f, CamOffset = 0.65f, TextureFrom = "mask_2" });
      _components.Add("mask_2", new SkinComponent { Label = "Varianta masky", Name = "mask_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.6f, CamOffset = 0.65f, TextureOf = "mask_1" });
      _components.Add("bproof_1", new SkinComponent { Label = "Vesta", Name = "bproof_1", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.75f, CamOffset = 0.15f, TextureFrom = "bproof_2" });
      _components.Add("bproof_2", new SkinComponent { Label = "Varianta vesty", Name = "bproof_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.75f, CamOffset = 0.15f, TextureOf = "bproof_1" });
      _components.Add("chain_1", new SkinComponent { Label = "Řetízek", Name = "chain_1", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.6f, CamOffset = 0.65f, TextureFrom = "chain_2" });
      _components.Add("chain_2", new SkinComponent { Label = "Varianta řetízku", Name = "chain_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.6f, CamOffset = 0.65f, TextureOf = "chain_1" });
      _components.Add("helmet_1", new SkinComponent { Label = "Helma", Name = "helmet_1", DefaultValue = -1, MinValue = -1, ZoomOffset = 0.6f, CamOffset = 0.65f, TextureFrom = "helmet_2" });
      _components.Add("helmet_2", new SkinComponent { Label = "Varianta helmy", Name = "helmet_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.6f, CamOffset = 0.65f, TextureOf = "helmet_1" });
      _components.Add("glasses_1", new SkinComponent { Label = "Brýle", Name = "glasses_1", DefaultValue = -1, MinValue = -1, ZoomOffset = 0.6f, CamOffset = 0.65f, TextureFrom = "glasses_2" });
      _components.Add("glasses_2", new SkinComponent { Label = "Varianta brýlí", Name = "glasses_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.6f, CamOffset = 0.65f, TextureOf = "glasses_1" });
      _components.Add("watches_1", new SkinComponent { Label = "Hodinky", Name = "watches_1", DefaultValue = -1, MinValue = -1, ZoomOffset = 0.75f, CamOffset = 0.15f, TextureFrom = "watches_2" });
      _components.Add("watches_2", new SkinComponent { Label = "Varianta hodinel", Name = "watches_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.75f, CamOffset = 0.15f, TextureOf = "watches_1" });
      _components.Add("bracelets_1", new SkinComponent { Label = "Náramek", Name = "bracelets_1", DefaultValue = -1, MinValue = -1, ZoomOffset = 0.75f, CamOffset = 0.15f, TextureFrom = "bracelets_2" });
      _components.Add("bracelets_2", new SkinComponent { Label = "Varianta náramku", Name = "bracelets_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.75f, CamOffset = 0.15f, TextureOf = "bracelets_1" });
      _components.Add("bags_1", new SkinComponent { Label = "Batoh", Name = "bags_1", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.75f, CamOffset = 0.15f, TextureFrom = "bags_2" });
      _components.Add("bags_2", new SkinComponent { Label = "Varianta batohu", Name = "bags_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.75f, CamOffset = 0.15f, TextureOf = "bags_1" });
      _components.Add("eye_color", new SkinComponent { Label = "Barva očí", Name = "eye_color", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("eyebrows_1", new SkinComponent { Label = "Obočí", Name = "eyebrows_1", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("eyebrows_2", new SkinComponent { Label = "Velikost obočí", Name = "eyebrows_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("eyebrows_3", new SkinComponent { Label = "Barva obočí 1", Name = "eyebrows_3", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("eyebrows_4", new SkinComponent { Label = "Barva obočí 2", Name = "eyebrows_4", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("makeup_1", new SkinComponent { Label = "Makeup", Name = "makeup_1", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("makeup_2", new SkinComponent { Label = "Síla makeupu", Name = "makeup_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("makeup_3", new SkinComponent { Label = "Barva makeupu 1", Name = "makeup_3", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("makeup_4", new SkinComponent { Label = "Barva makeupu 2", Name = "makeup_4", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("lipstick_1", new SkinComponent { Label = "Rtěnka", Name = "lipstick_1", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("lipstick_2", new SkinComponent { Label = "Výraznost rtěnky", Name = "lipstick_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("lipstick_3", new SkinComponent { Label = "Barva rtěnky 1", Name = "lipstick_3", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("lipstick_4", new SkinComponent { Label = "Barva rtěnky 2", Name = "lipstick_4", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("ears_1", new SkinComponent { Label = "Náušnice", Name = "ears_1", DefaultValue = -1, MinValue = -1, ZoomOffset = 0.4f, CamOffset = 0.65f, TextureFrom = "ears_2" });
      _components.Add("ears_2", new SkinComponent { Label = "Varianta náušnic", Name = "ears_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f, TextureOf = "ears_1" });
      _components.Add("chest_1", new SkinComponent { Label = "Chlupy", Name = "chest_1", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.75f, CamOffset = 0.15f });
      _components.Add("chest_2", new SkinComponent { Label = "Výraznost chlupů", Name = "chest_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.75f, CamOffset = 0.15f });
      _components.Add("chest_3", new SkinComponent { Label = "Barcha chlupů", Name = "chest_3", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.75f, CamOffset = 0.15f });
      _components.Add("bodyb_1", new SkinComponent { Label = "Tělo", Name = "bodyb_1", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.75f, CamOffset = 0.15f });
      _components.Add("bodyb_2", new SkinComponent { Label = "Velikost těla", Name = "bodyb_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.75f, CamOffset = 0.15f });
      _components.Add("age_1", new SkinComponent { Label = "Vrásky", Name = "age_1", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("age_2", new SkinComponent { Label = "Výraznost vrásek", Name = "age_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("blemishes_1", new SkinComponent { Label = "Blemishes 1", Name = "blemishes_1", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("blemishes_2", new SkinComponent { Label = "Blemishes 2", Name = "blemishes_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("blush_1", new SkinComponent { Label = "Blush 1", Name = "blush_1", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("blush_2", new SkinComponent { Label = "Blush 2", Name = "blush_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("blush_3", new SkinComponent { Label = "Blush 3", Name = "blush_3", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("complexion_1", new SkinComponent { Label = "Complexion 1", Name = "complexion_1", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("complexion_2", new SkinComponent { Label = "Complexion 2", Name = "complexion_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("sun_1", new SkinComponent { Label = "Sun 1", Name = "sun_1", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("sun_2", new SkinComponent { Label = "Sun 2", Name = "sun_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("moles_1", new SkinComponent { Label = "Moles 1", Name = "moles_1", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("moles_2", new SkinComponent { Label = "Moles 2", Name = "moles_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("beard_1", new SkinComponent { Label = "Vousy", Name = "beard_1", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("beard_2", new SkinComponent { Label = "Délka vousů", Name = "beard_2", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("beard_3", new SkinComponent { Label = "Barva vousů 1", Name = "beard_3", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
      _components.Add("beard_4", new SkinComponent { Label = "Barva vousů 2", Name = "beard_4", DefaultValue = 0, MinValue = 0, ZoomOffset = 0.4f, CamOffset = 0.65f });
    }

    public SkinManager()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
    }

    public static void SetDefaultSkin(string[] features)
    {
      bool isFemale = Game.PlayerPed.Model.Hash == GetHashKey("mp_f_freemode_01");

      var values = new Dictionary<string, int>();
      foreach (var f in features)
        values.Add(f, (isFemale && Components[f].DefaultFemale != null) ? Components[f].DefaultFemale.Value : Components[f].DefaultValue);

      ApplySkin(values);
    }

    public static void ApplySkin(Dictionary<string, int> features)
    {
      if (features == null)
        return;

      //TheBugger.DebugLog($"PED HASH:{Game.PlayerPed.Model.Hash} MALE HASH:{GetHashKey("mp_m_freemode_01")}");

      if (Game.PlayerPed.Model.Hash != GetHashKey("mp_m_freemode_01") && Game.PlayerPed.Model.Hash != GetHashKey("mp_f_freemode_01"))
      {
        Notify.Alert("Nepodporovaný model postavy");
        return;
      }

      if (features.ContainsKey("face") && features.ContainsKey("skin"))
        SetPedHeadBlendData(Game.PlayerPed.Handle, features["face"], features["face"], features["face"], features["skin"], features["skin"], features["skin"], 1f, 1f, 1f, true);

      if (features.ContainsKey("beard_1") && features.ContainsKey("beard_2"))
        SetPedHeadOverlay(Game.PlayerPed.Handle, 1, features["beard_1"], (float)features["beard_2"] / (float)10);
      if (features.ContainsKey("eyebrows_1") && features.ContainsKey("eyebrows_2"))
        SetPedHeadOverlay(Game.PlayerPed.Handle, 2, features["eyebrows_1"], (float)features["eyebrows_2"] / (float)10);
      if (features.ContainsKey("age_1") && features.ContainsKey("age_2"))
        SetPedHeadOverlay(Game.PlayerPed.Handle, 3, features["age_1"], (float)features["age_2"] / (float)10);
      if (features.ContainsKey("makeup_1") && features.ContainsKey("makeup_2"))
        SetPedHeadOverlay(Game.PlayerPed.Handle, 4, features["makeup_1"], (float)features["makeup_2"] / (float)10);
      if (features.ContainsKey("blush_1") && features.ContainsKey("blush_2"))
        SetPedHeadOverlay(Game.PlayerPed.Handle, 5, features["blush_1"], (float)features["blush_2"] / (float)10);
      if (features.ContainsKey("complexion_1") && features.ContainsKey("complexion_2"))
        SetPedHeadOverlay(Game.PlayerPed.Handle, 6, features["complexion_1"], (float)features["complexion_2"] / (float)10);
      if (features.ContainsKey("sun_1") && features.ContainsKey("sun_2"))
        SetPedHeadOverlay(Game.PlayerPed.Handle, 7, features["sun_1"], (float)features["sun_2"] / (float)10);
      if (features.ContainsKey("lipstick_1") && features.ContainsKey("lipstick_2"))
        SetPedHeadOverlay(Game.PlayerPed.Handle, 8, features["lipstick_1"], (float)features["lipstick_2"] / (float)10);
      if (features.ContainsKey("moles_1") && features.ContainsKey("moles_2"))
        SetPedHeadOverlay(Game.PlayerPed.Handle, 9, features["moles_1"], (float)features["moles_2"] / (float)10);
      if (features.ContainsKey("chest_1") && features.ContainsKey("chest_2"))
        SetPedHeadOverlay(Game.PlayerPed.Handle, 10, features["chest_1"], (float)features["chest_2"] / (float)10);
      if (features.ContainsKey("bodyb_1") && features.ContainsKey("bodyb_2"))
        SetPedHeadOverlay(Game.PlayerPed.Handle, 11, features["bodyb_1"], (float)features["bodyb_2"] / (float)10);

      if (features.ContainsKey("eye_color"))
        SetPedEyeColor(Game.PlayerPed.Handle, features["eye_color"]);

      if (features.ContainsKey("hair_color_1") && features.ContainsKey("hair_color_2"))
        SetPedHairColor(Game.PlayerPed.Handle, features["hair_color_1"], features["hair_color_2"]);

      if (features.ContainsKey("beard_3") && features.ContainsKey("beard_4"))
        SetPedHeadOverlayColor(Game.PlayerPed.Handle, 1, 1, features["beard_3"], features["beard_3"]);
      if (features.ContainsKey("eyebrows_3") && features.ContainsKey("eyebrows_4"))
        SetPedHeadOverlayColor(Game.PlayerPed.Handle, 2, 1, features["eyebrows_3"], features["eyebrows_4"]);
      if (features.ContainsKey("makeup_3") && features.ContainsKey("makeup_4"))
        SetPedHeadOverlayColor(Game.PlayerPed.Handle, 4, 1, features["makeup_3"], features["makeup_4"]);
      if (features.ContainsKey("blush_3"))
        SetPedHeadOverlayColor(Game.PlayerPed.Handle, 5, 2, features["blush_3"], 0);
      if (features.ContainsKey("lipstick_1") && features.ContainsKey("lipstick_2"))
        SetPedHeadOverlayColor(Game.PlayerPed.Handle, 8, 2, features["lipstick_1"], features["lipstick_2"]);
      if (features.ContainsKey("chest_3"))
        SetPedHeadOverlayColor(Game.PlayerPed.Handle, 10, 1, features["chest_3"], 0);

      if (features.ContainsKey("mask_1") && features.ContainsKey("mask_2"))
        SetPedComponentVariation(Game.PlayerPed.Handle, 1, features["mask_1"], features["mask_2"], 2);
      if (features.ContainsKey("hair_1") && features.ContainsKey("hair_2"))
        SetPedComponentVariation(Game.PlayerPed.Handle, 2, features["hair_1"], features["hair_2"], 2);
      if (features.ContainsKey("arms_1") && features.ContainsKey("arms_2"))
        SetPedComponentVariation(Game.PlayerPed.Handle, 3, features["arms_1"], features["arms_2"], 2);
      if (features.ContainsKey("pants_1") && features.ContainsKey("pants_2"))
        SetPedComponentVariation(Game.PlayerPed.Handle, 4, features["pants_1"], features["pants_2"], 2);
      if (features.ContainsKey("bags_1") && features.ContainsKey("bags_2"))
        SetPedComponentVariation(Game.PlayerPed.Handle, 5, features["bags_1"], features["bags_2"], 2);
      if (features.ContainsKey("shoes_1") && features.ContainsKey("shoes_2"))
        SetPedComponentVariation(Game.PlayerPed.Handle, 6, features["shoes_1"], features["shoes_2"], 2);
      if (features.ContainsKey("chain_1") && features.ContainsKey("chain_2"))
        SetPedComponentVariation(Game.PlayerPed.Handle, 7, features["chain_1"], features["chain_2"], 2);
      if (features.ContainsKey("tshirt_1") && features.ContainsKey("tshirt_2"))
        SetPedComponentVariation(Game.PlayerPed.Handle, 8, features["tshirt_1"], features["tshirt_2"], 2);
      if (features.ContainsKey("bproof_1") && features.ContainsKey("bproof_2"))
        SetPedComponentVariation(Game.PlayerPed.Handle, 9, features["bproof_1"], features["bproof_2"], 2);
      if (features.ContainsKey("decals_1") && features.ContainsKey("decals_2"))
        SetPedComponentVariation(Game.PlayerPed.Handle, 10, features["decals_1"], features["decals_2"], 2);
      if (features.ContainsKey("torso_1") && features.ContainsKey("torso_2"))
        SetPedComponentVariation(Game.PlayerPed.Handle, 11, features["torso_1"], features["torso_2"], 2);

      if (features.ContainsKey("ears_1") && features["ears_1"] == -1)
        ClearPedProp(Game.PlayerPed.Handle, 2);
      else if (features.ContainsKey("ears_1") && features.ContainsKey("ears_2"))
        SetPedPropIndex(Game.PlayerPed.Handle, 2, features["ears_1"], features["ears_2"], true);

      if (features.ContainsKey("helmet_1") && features["helmet_1"] == -1)
        ClearPedProp(Game.PlayerPed.Handle, 0);
      else if (features.ContainsKey("helmet_1") && features.ContainsKey("helmet_2"))
        SetPedPropIndex(Game.PlayerPed.Handle, 0, features["helmet_1"], features["helmet_2"], true);

      if (features.ContainsKey("glasses_1") && features["glasses_1"] == -1)
        ClearPedProp(Game.PlayerPed.Handle, 1);
      else if (features.ContainsKey("glasses_1") && features.ContainsKey("glasses_2"))
        SetPedPropIndex(Game.PlayerPed.Handle, 1, features["glasses_1"], features["glasses_2"], true);

      if (features.ContainsKey("watches_1") && features["watches_1"] == -1)
        ClearPedProp(Game.PlayerPed.Handle, 6);
      else if (features.ContainsKey("watches_1") && features.ContainsKey("watches_2"))
        SetPedPropIndex(Game.PlayerPed.Handle, 6, features["watches_1"], features["watches_2"], true);

      if (features.ContainsKey("bracelets_1") && features["bracelets_1"] == -1)
        ClearPedProp(Game.PlayerPed.Handle, 7);
      else if (features.ContainsKey("bracelets_1") && features.ContainsKey("bracelets_2"))
        SetPedPropIndex(Game.PlayerPed.Handle, 7, features["bracelets_1"], features["bracelets_2"], true);
    }

    public static int GetComponentMaxValue(string componentName, int parentComponentValue)
    {
      switch (componentName)
      {
        case "face": return 45;
        case "skin": return 45;
        case "age_1": return GetNumHeadOverlayValues(3) - 1;
        case "age_2": return 10;
        case "beard_1": return GetNumHeadOverlayValues(1) - 1;
        case "beard_2": return 10;
        case "beard_3": return GetNumHairColors() - 1;
        case "beard_4": return GetNumHairColors() - 1;
        case "hair_1": return GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 2) - 1;
        case "hair_2": return GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, 2, parentComponentValue) - 1; //hair_1
        case "hair_color_1":
        case "hair_color_2": return GetNumHairColors() - 1;
        case "eye_color": return 31;
        case "eyebrows_1": return GetNumHeadOverlayValues(2) - 1;
        case "eyebrows_2": return 10;
        case "eyebrows_3": return GetNumHeadOverlayValues(2) - 1;
        case "eyebrows_4": return GetNumHeadOverlayValues(2) - 1;
        case "makeup_1": return GetNumHeadOverlayValues(4) - 1;
        case "makeup_2": return 10;
        case "makeup_3": return GetNumHairColors() - 1;
        case "makeup_4": return GetNumHairColors() - 1;
        case "lipstick_1": return GetNumHeadOverlayValues(8) - 1;
        case "lipstick_2": return 10;
        case "lipstick_3": return GetNumHairColors() - 1;
        case "lipstick_4": return GetNumHairColors() - 1;
        case "blemishes_1": return GetNumHeadOverlayValues(0) - 1;
        case "blemishes_2": return 10;
        case "blush_1": return GetNumHeadOverlayValues(5) - 1;
        case "blush_2": return 10;
        case "blush_3": return GetNumHairColors() - 1;
        case "complexion_1": return GetNumHeadOverlayValues(6) - 1;
        case "complexion_2": return 10;
        case "sun_1": return GetNumHeadOverlayValues(7) - 1;
        case "sun_2": return 10;
        case "moles_1": return GetNumHeadOverlayValues(9) - 1;
        case "moles_2": return 10;
        case "chest_1": return GetNumHeadOverlayValues(10) - 1;
        case "chest_2": return 10;
        case "chest_3": return GetNumHairColors() - 1;
        case "bodyb_1": return GetNumHeadOverlayValues(11) - 1;
        case "bodyb_2": return 10;
        case "ears_1": return GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, 1) - 1;
        case "ears_2": return GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, 1, parentComponentValue); //ears_1
        case "tshirt_1": return GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 8) - 1;
        case "tshirt_2": return GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, 8, parentComponentValue) - 1; //hair_1
        case "torso_1": return GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 11) - 1;
        case "torso_2": return GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, 11, parentComponentValue) - 1; //torso_1
        case "decals_1": return GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 10) - 1;
        case "decals_2": return GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, 10, parentComponentValue) - 1; //decals_1
        case "arms_1": return GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 3) - 1;
        case "arms_2": return GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, 3, parentComponentValue) - 1; //arms_1
        case "pants_1": return GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 4) - 1;
        case "pants_2": return GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, 4, parentComponentValue) - 1; //pants_1
        case "shoes_1": return GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 6) - 1;
        case "shoes_2": return GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, 6, parentComponentValue) - 1; //shoes_1
        case "mask_1": return GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 1) - 1;
        case "mask_2": return GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, 1, parentComponentValue) - 1; //mask_1
        case "bproof_1": return GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 9) - 1;
        case "bproof_2": return GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, 9, parentComponentValue) - 1; //bproof_1
        case "chain_1": return GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 7) - 1;
        case "chain_2": return GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, 7, parentComponentValue) - 1; //shoes_1
        case "bags_1": return GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 5) - 1;
        case "bags_2": return GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, 5, parentComponentValue) - 1; //bags_1
        case "helmet_1": return GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, 0) - 1;
        case "helmet_2": return GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, 0, parentComponentValue) - 1; //helmet_1
        case "glasses_1": return GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, 1) - 1;
        case "glasses_2": return GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, 1, parentComponentValue) - 1; //glasses_1
        case "watches_1": return GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, 6) - 1;
        case "watches_2": return GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, 6, parentComponentValue) - 1; //watches_1
        case "bracelets_1": return GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, 7) - 1;
        case "bracelets_2": return GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, 7, parentComponentValue) - 1; //helmet_1
        default: return 0;
      }
    }

    public class SkinComponent
    {
      public string Label { get; set; }
      public string Name { get; set; }
      public string TextureOf { get; set; }
      public string TextureFrom { get; set; }
      public int DefaultValue { get; set; }
      public int? DefaultFemale { get; set; }
      public int MinValue { get; set; }
      public float ZoomOffset { get; set; }
      public float CamOffset { get; set; }
    }
  }
}
