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
    private const float drawDistSq = 400f;
    public static IdentityShowCardBag DisplayingCard = null;

    public IdentityCards()
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
      if (DisplayingCard == null)
      {
        await Delay(250);
        return;
      }

      var dc = DisplayingCard;

      if (dc.TextureHandle == 0)
      {
        var texture = await GetIdentityPedTexture(dc.Model, dc.CardSkin);
        dc.TextureHandle = texture.Handle;
        dc.TextureName = texture.TextureString;
      }

      DrawSprite(dc.TextureName, dc.TextureName, 0.5f, 0.5f, 0.2f, 0.2f, 0.0f, 255, 255, 255, 255);
    }

    private async Task OnSlowTick()
    {
      await Delay(250);
      if (isLocked)
        return;

      if (ShowingCards.Count <= 0)
        return;

      var myPos = Game.PlayerPed.Position;
      float shortestSqDist = float.NaN;
      IdentityShowCardBag closestCard = null;

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

        if (distSq < drawDistSq && (float.IsNaN(shortestSqDist) || distSq < shortestSqDist))
        {
          shortestSqDist = distSq;
          closestCard = k.Value;
        }
      }

      DisplayingCard = closestCard;
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
      }
      isLocked = false;
    }
  }
}
