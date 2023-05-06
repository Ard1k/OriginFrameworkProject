using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using OriginFramework.Helpers;
using OriginFramework.Menus;
using OriginFrameworkData.DataBags;
using static CitizenFX.Core.Native.API;

namespace OriginFramework.Scripts
{
  public struct IdentityPedTexture
  {
    public int Handle { get; set; }
    public string TextureString { get; set; }
  }

  public struct IdentityCardTextures
  {
    public IdentityPedTexture Card { get; set; }
    public IdentityPedTexture Actual { get; set; }
  }

  public class IdentityCards : BaseScript
  {
    public static Dictionary<int, IdentityShowCardBag> ShowingCards = new Dictionary<int, IdentityShowCardBag>();
    private static bool isLocked = false;
    private static bool isDisplayLocked = false;
    private const float drawDistSq = 400f;
    public static List<IdentityShowCardBag> DisplayingCards = new List<IdentityShowCardBag>();

    public IdentityCards()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      AddTextEntry("OFWIDENTITY_HIDE_CARD", $"~INPUT_FRONTEND_CANCEL~ {FontsManager.FiraSansString}Uklidit kartu");

      Tick += OnTick;
      Tick += OnSlowTick;
    }

    private async Task OnTick()
    {
      if (DisplayingCards.Count <= 0)
      {
        await Delay(250);
        return;
      }

      if (ShowingCards.ContainsKey(Game.Player.ServerId))
      {
        DisplayHelpTextThisFrame("OFWIDENTITY_HIDE_CARD", true);

        Game.DisableControlThisFrame(0, Control.FrontendCancel); //ESC
        Game.DisableControlThisFrame(0, Control.FrontendPauseAlternate); //ESC
        Game.DisableControlThisFrame(0, Control.ReplayToggleTimeline); //ESC
        Game.DisableControlThisFrame(0, Control.PhoneCancel); //ESC

        if (InventoryManager.IsInventoryOpen || Game.IsDisabledControlJustPressed(0, Control.FrontendCancel)) //ESC
        {
          TriggerServerEvent("ofw_identity:HideCard");
          OfwFunctions.BlockEsc(500);
        }
      }

      if (isDisplayLocked || InventoryManager.IsInventoryOpen || NativeMenuManager.IsMenuOpen(null))
        return;

      const float DISPLAY_DIST = 15.0f;

      foreach (var c in DisplayingCards)
      {
        int serverId = c.ShowingServerId;

        int showerPlayer = API.GetPlayerFromServerId(serverId);
        int showerPed = API.GetPlayerPed(showerPlayer);

        if (showerPlayer <= 0 || showerPed <= 0)
          continue;

        Vector3 showerCoords = API.GetEntityCoords(showerPed, true);
        float dist = Vector3.Distance(Game.PlayerPed.Position, showerCoords);

        if (c.TextureName != null && dist < DISPLAY_DIST)
        {
          float screenX = 0f;
          float screenY = 0f;
          bool onScreen = GetScreenCoordFromWorldCoord(showerCoords.X, showerCoords.Y, showerCoords.Z, ref screenX, ref screenY);

          if (onScreen)
          {
            float scale = (DISPLAY_DIST - dist) / DISPLAY_DIST;
            if (scale > 0.3f)
            {
              DrawCard(c, screenX, screenY, scale);
            }
          }
        }
      }
    }

    private async Task OnSlowTick()
    {
      await Delay(250);
      if (isLocked)
        return;

      if (ShowingCards.Count <= 0)
      {
        isDisplayLocked = true;
        await Delay(0);
        DisplayingCards.Clear();
        isDisplayLocked = false;
        return;
      }

      var myPos = Game.PlayerPed.Position;

      foreach (var k in ShowingCards)
      {
        var player = GetPlayerFromServerId(k.Key);
        if (player <= 0)
          continue;

        int playerPed = GetPlayerPed(player);
        if (playerPed <= 0)
          continue;
        
        var coords = GetEntityCoords(playerPed, false);
        float distSq = coords.DistanceToSquared2D(myPos);

        if (distSq < drawDistSq && !DisplayingCards.Any(c => c.ShowingServerId == k.Key))
        {
          if (k.Value.TextureHandle == 0)
          {
            var texture = await GetIdentityPedTexture(k.Value.Model, k.Value.CardSkin);
            k.Value.TextureHandle = texture.Handle;
            k.Value.TextureName = texture.TextureString;
            k.Value.ShowingServerId = k.Key;
          }

          isDisplayLocked = true;
          await Delay(0);
          DisplayingCards.Add(k.Value);
          isDisplayLocked = false;
        }
      }
    }

    private void DrawCard(IdentityShowCardBag dc, float screenX, float screenY, float scale)
    {
      float gScale = 0.7f * scale;
      float cardWidth = gScale * 0.25f;
      float cardHeight = gScale * API.GetAspectRatio(true) * 0.16f;

      float picXMargin = gScale * 0.005f;
      float picYMargin = (cardHeight / 4);
      float textYMargin = (cardHeight / 5);
      float picWidth = gScale * 0.07f;
      float picHeight = gScale * API.GetAspectRatio(true) * 0.07f;
      float xMargin = gScale * 0.005f;
      float yMargin = API.GetAspectRatio(true) * xMargin;

      float attrTitleScale = gScale * 0.3f;
      float attrTitleLineHeight = gScale * 0.015f;
      float attrTextScale = gScale * 0.45f;
      float attrTextLineHeight = gScale * 0.0275f;

      SetDrawOrigin(screenX - cardWidth / 2, screenY - cardHeight / 2, 0.5f, 1);

      switch (dc.ItemId)
      {
        case 20: DrawSprite("inventory_textures", "card_driver", cardWidth/2, cardHeight/2, cardWidth, cardHeight, 0.0f, 255, 255, 255, 255); break;
        case 21: DrawSprite("inventory_textures", "card_police", cardWidth/2, cardHeight/2, cardWidth, cardHeight, 0.0f, 255, 255, 255, 255); break;
        case 22: DrawSprite("inventory_textures", "card_weapon", cardWidth/2, cardHeight/2, cardWidth, cardHeight, 0.0f, 255, 255, 255, 255); break;
      }

      DrawSprite(dc.TextureName, dc.TextureName, picXMargin + picWidth /2, picYMargin + picHeight /2, picWidth, picHeight, 0.0f, 255, 255, 255, 255);

      DrawCardText(picXMargin + picWidth + xMargin, textYMargin, attrTitleScale, "Jméno");
      DrawCardText(picXMargin + picWidth + xMargin, textYMargin + attrTitleLineHeight, attrTextScale, dc.CharName);

      DrawCardText(picXMargin + picWidth + xMargin, textYMargin + attrTitleLineHeight + attrTextLineHeight, attrTitleScale, "Datum narození");
      DrawCardText(picXMargin + picWidth + xMargin, textYMargin + 2 * attrTitleLineHeight + attrTextLineHeight, attrTextScale, dc.Born);

      DrawCardText(picXMargin + picWidth + xMargin, textYMargin + 2 * attrTitleLineHeight + 2 * attrTextLineHeight, attrTitleScale, "Vystaveno");
      DrawCardText(picXMargin + picWidth + xMargin, textYMargin + 3 * attrTitleLineHeight + 2 * attrTextLineHeight, attrTextScale, dc.Created);
      DrawCardText(picXMargin + picWidth + xMargin + cardWidth / 3, textYMargin + 2 * attrTitleLineHeight + 2 * attrTextLineHeight, attrTitleScale, "Expirace");
      DrawCardText(picXMargin + picWidth + xMargin + cardWidth / 3, textYMargin + 3 * attrTitleLineHeight + 2 * attrTextLineHeight, attrTextScale, dc.ValidTo);

      DrawCardText(picXMargin + picWidth + xMargin, textYMargin + 3 * attrTitleLineHeight + 3 * attrTextLineHeight, attrTitleScale, "S/N");
      DrawCardText(picXMargin + picWidth + xMargin, textYMargin + 4 * attrTitleLineHeight + 3 * attrTextLineHeight, attrTextScale, dc.Sn);
    }

    public static void DrawCardText(float x, float y, float scale, string text)
    {
      SetTextFont(0);
      SetTextScale(0.0f, scale);
      SetTextColour(0, 0, 0, 255);
      SetTextEntry("STRING");
      AddTextComponentString(FontsManager.FiraSansString + text);
      DrawText(x, y);
    }

    public static async Task<int> CreateIdentityPed(uint model, Dictionary<string, int> appearance)
    {
      RequestModel(model);

      while (!HasModelLoaded(model))
      {
        await Delay(10);
      }
        
      var identityPed = CreatePed(4, model, Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z + 50.0f, 0.0f, false, false);
      SetEntityVisible(identityPed, false, false);
      FreezeEntityPosition(identityPed, true);

      SkinManager.ApplySkin(appearance, identityPed);

      return identityPed;
    }

    public static async Task<IdentityPedTexture> GetIdentityPedTexture(uint model, Dictionary<string, int> appearanceCard)
    {
      int cardPed = await CreateIdentityPed(model, appearanceCard);

      await Delay(200);

      var textureCard = await GetPedHeadshotTexture(cardPed);

      await Delay(200);

      SetEntityAsNoLongerNeeded(ref cardPed);
      DeletePed(ref cardPed);

      return textureCard;
    }

    public static async Task<IdentityPedTexture> GetPedHeadshotTexture(int ped)
    {
      var handle = RegisterPedheadshot_3(ped);

      while (!IsPedheadshotReady(handle))
        await Delay(0);

      var texture = GetPedheadshotTxdString(handle);

      return new IdentityPedTexture { Handle = handle, TextureString = texture };
    }

    [EventHandler("ofw_identity:ShowingCard")]
    private async void ShowingCard(int serverId, string cardData)
    {
      if (string.IsNullOrEmpty(cardData))
      {
        Debug.WriteLine("ofw_identity: invalid card data");
        return;
      }

      isLocked = true;
      await Delay(0);

      var cardObj = JsonConvert.DeserializeObject<IdentityShowCardBag>(cardData);
      if (ShowingCards.ContainsKey(serverId))
      {
        if (ShowingCards[serverId].TextureHandle != 0)
          UnregisterPedheadshot(ShowingCards[serverId].TextureHandle);
        ShowingCards.Remove(serverId);

        if (DisplayingCards.Any(c => c.ShowingServerId == serverId))
        {
          isDisplayLocked = true;
          await Delay(0);
          DisplayingCards.RemoveAll(c => c.ShowingServerId == serverId);
          isDisplayLocked = false;
        }
      }

      ShowingCards.Add(serverId, cardObj);
      isLocked = false;
    }

    [EventHandler("ofw_identity:HidingCard")]
    private async void HidingCard(int serverId)
    {
      isLocked = true;
      await Delay(0);
      if (ShowingCards.ContainsKey(serverId))
      {
        if (ShowingCards[serverId].TextureHandle != 0)
          UnregisterPedheadshot(ShowingCards[serverId].TextureHandle);
        ShowingCards.Remove(serverId);

        if (DisplayingCards.Any(c => c.ShowingServerId == serverId))
        {
          isDisplayLocked = true;
          await Delay(0);
          DisplayingCards.RemoveAll(c => c.ShowingServerId == serverId);
          isDisplayLocked = false;
        }
      }
      isLocked = false;
    }
  }
}
