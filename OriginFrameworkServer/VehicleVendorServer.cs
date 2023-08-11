using CitizenFX.Core;
using OriginFrameworkData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;
using OriginFrameworkData.DataBags;
using CitizenFX.Core.Native;
using static OriginFrameworkServer.OfwServerFunctions;
using System.IO;
using Newtonsoft.Json.Linq;

namespace OriginFrameworkServer
{
  public class VehicleVendorServer : BaseScript
  {
    public static List<VehicleVendorSlot> Slots = new List<VehicleVendorSlot>
    {
      //PDM u okna
      new VehicleVendorSlot(eVehicleVendor.PDM, new PosBag { X = -48.9077f, Y = -1100.5353f, Z = 25.5363f, Heading = 300f }),
      new VehicleVendorSlot(eVehicleVendor.PDM, new PosBag { X = -46.3535f, Y = -1102.3384f, Z = 25.5363f, Heading = 300f }),

      //PDM naproti oknu
      new VehicleVendorSlot(eVehicleVendor.PDM, new PosBag { X = -47.7329f, Y = -1093.4386f, Z = 25.5138f, Heading = 200f }),
      new VehicleVendorSlot(eVehicleVendor.PDM, new PosBag { X = -43.8934f, Y = -1094.8088f, Z = 25.4759f, Heading = 200f }),
      new VehicleVendorSlot(eVehicleVendor.PDM, new PosBag { X = -40.7237f, Y = -1095.4818f, Z = 25.4835f, Heading = 200f }),

      //PDM v cele
      new VehicleVendorSlot(eVehicleVendor.PDM, new PosBag { X = -51.2714f, Y = -1095.1326f, Z = 25.4835f, Heading = 200f }),
    };
    
    private static Random rand = new Random();
    private static List<VehColor> cachedColors = null;
    private static string[] firstCars = { "blista", "ingot", "asea", "emperor2", "primo", "stanier", "regina", "rhapsody", "kanjo", "dynasty", "asbo" };
    bool isFirstTick = true;

    public VehicleVendorServer()
    {
      rand.Next();
      rand.Next();
      rand.Next();
      cachedColors = VehColor.Defined.Where(c => c.IsUnused == false).ToList();

      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
    }

		#region event handlers

		private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.VehicleVendorServer, eScriptArea.VSql, eScriptArea.PersistentVehiclesServer))
        return;

      Tick += OnTick;

      RegisterCommand("resetvehvend", new Action<int, List<object>, string>((source, args, raw) =>
      {
        foreach (var slot in Slots)
        {
          slot.RepopulateAt = null;
        }
      }), false);

      InternalDependencyManager.Started(eScriptArea.VehicleVendorServer);
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
      
    }

    private async Task OnTick()
    {
      if (isFirstTick)
      {
        isFirstTick = false;
        return;
      }

      foreach (var slot in Slots)
      {
        if (slot.RepopulateAt == null || slot.RepopulateAt <= DateTime.Now)
        {
          var stock = VehicleVendorStock.Stock.Where(s => s.Vendor == slot.VendorType)?.ToList();

          if (stock == null || stock.Count <= 0)
          {
            //nemam co tu spawnout, tak si to vyradim at porad neco nehledam
            slot.RepopulateAt = DateTime.MaxValue;
            continue;
          }

          VehicleVendorVehicle chosenOne = null;
          while (chosenOne == null)
          {
            var randomItem = stock[rand.Next(stock.Count)];

            if (randomItem == null)
              continue;

            if (rand.NextDouble() <= (double)randomItem.SpawnChance)
              chosenOne = randomItem;
          }

          if (chosenOne != null)
          {
            string plate = null;
            while (plate == null)
            {
              string genPlate = $"PDM{rand.Next(10000, 100000)}"; //TODO lepsi reseni
              bool plateExists = await VehicleServer.DoesPlateExist(genPlate, false);
              if (!plateExists)
                plate = genPlate;
            }
            await Delay(0);
            int modelHash = GetHashKey(chosenOne.Model);
            int color1 = cachedColors[rand.Next(cachedColors.Count)].ColorIndex;
            int color2 = cachedColors[rand.Next(cachedColors.Count)].ColorIndex;
            int colorp = cachedColors[rand.Next(cachedColors.Count)].ColorIndex;
            int colorw = cachedColors[rand.Next(cachedColors.Count)].ColorIndex;
            VehicleServer.PopulateVendorSlot(slot.SlotId, slot.Position.GetInstanceCopy(), plate, modelHash, JsonConvert.SerializeObject(new VehiclePropertiesBag { plate = plate, model = modelHash, color1 = color1, color2 = color2, pearlescentColor = colorp, wheelColor = colorw }));
            slot.CurrentVehicle = chosenOne;
            slot.SpawnedNetId = null;
            slot.RepopulateAt = DateTime.Now.Add(chosenOne.SpawnForTime);
            TriggerClientEvent("ofw_vehvendor:SlotUpdated", JsonConvert.SerializeObject(slot));
          }
        }
      }
      await (Delay(1000));
    }

    public static void SlotNetIdUpdated(int slotId, int netId)
    {
      var foundItem = Slots.Where(slot => slot.SlotId == slotId).FirstOrDefault();

      if (foundItem != null) 
      {
        foundItem.SpawnedNetId = netId;
      }

      TriggerClientEvent("ofw_vehvendor:SlotUpdated", JsonConvert.SerializeObject(foundItem));
    }

    [EventHandler("ofw_vehvendor:SyncSlots")]
    private async void SyncSlots([FromSource] Player source, NetworkCallbackDelegate callback)
    {
     _ = callback(JsonConvert.SerializeObject(Slots));
    }

    

    [EventHandler("ofw_vehvendor:PurchaseVehicle")]
    private async void PurchaseVehicle([FromSource] Player source, int slotId, bool isOrganization, bool isBankTransaction)
    {
      if (source == null)
      {
        return;
      }

      var sourcePlayer = Players.Where(p => p.Handle == source.Handle).FirstOrDefault();
      if (sourcePlayer == null)
      {
        Debug.WriteLine($"ofw_vehvendor:PurchaseVehicle: Nenalezen hrac {source.Handle}");
        return;
      }

      var character = CharacterCaretakerServer.GetPlayerLoggedCharacter(sourcePlayer);
      if (character == null)
      {
        sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neplatná postava");
        return;
      }

      var slot = Slots.Where(s => s.SlotId == slotId).FirstOrDefault();
      if (slot == null || slot.SpawnedNetId == null || slot.CurrentVehicle == null)
      {
        sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Auto nenalezeno");
        return;
      }

      if (isOrganization)
      {
        if (character.OrganizationId == null || !OrganizationServer.Organizations.Any(o => o.Id == character.OrganizationId && (o.Owner == character.Id || o.Managers.Any(m => m.CharId == character.Id))))
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Nejsi manažer");
        }
      }

      if (isBankTransaction)
      {
        if (slot?.CurrentVehicle?.BankMoneyPrice == null)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neplatná definice slotu na serveru");
          return;
        }

        if (isOrganization)
          InventoryServer.PayBankOrganization(sourcePlayer, character.OrganizationId.Value, slot.CurrentVehicle.BankMoneyPrice.Value, (p) => { return CarPurchased(p, character, slot.SlotId, null, character.OrganizationId); }, OnError);
        else
          InventoryServer.PayBankCharacter(sourcePlayer, character.Id, slot.CurrentVehicle.BankMoneyPrice.Value, (p) => { return CarPurchased(p, character, slot.SlotId, character.Id, null); }, OnError);
      }
      else
      {
        if (slot?.CurrentVehicle?.PriceItemId == null || slot?.CurrentVehicle?.Price == null)
        {
          sourcePlayer.TriggerEvent("ofw:ValidationErrorNotification", "Neplatná definice slotu na serveru");
          return;
        }

        if (isOrganization)
          InventoryServer.PayItem(sourcePlayer, $"char_{character.Id}", slot.CurrentVehicle.PriceItemId.Value, slot.CurrentVehicle.Price.Value, (p) => { return CarPurchased(p, character, slot.SlotId, null, character.OrganizationId); }, OnError);
        else
          InventoryServer.PayItem(sourcePlayer, $"char_{character.Id}", slot.CurrentVehicle.PriceItemId.Value, slot.CurrentVehicle.Price.Value, (p) => { return CarPurchased(p, character, slot.SlotId, character.Id, null); }, OnError);
      }
    }

    private async Task<bool> CarPurchased(Player player, CharacterBag characterForKeys, int slotId, int? ownerChar, int? ownerOrg)
    {
      var newPlate = GenerateNewPlate();
      while (newPlate.StartsWith("PDM") || await VehicleServer.DoesPlateExist(newPlate, true))
      {
        newPlate = GenerateNewPlate();
      }

      string keysResult = await InventoryServer.TryGiveCarKeys(characterForKeys, 23, newPlate.ToLowerInvariant(), true);
      await Delay(0);
      if (keysResult != null)
      {
        player.TriggerEvent("ofw:ValidationErrorNotification", keysResult);
        return false;
      }

      var ret = await VehicleServer.MigrateVendorSlotVehToGarageVeh(slotId, newPlate, ownerChar, ownerOrg);
      if (ret == true)
      {
        await Delay(0);
        player.TriggerEvent("ofw:SuccessNotification", "Auto zakoupeno");
        var slot = Slots.Where(s => s.SlotId == slotId).FirstOrDefault();
        if (slot != null)
        {
          slot.CurrentVehicle = null;
          slot.SpawnedNetId = null;
          TriggerClientEvent("ofw_vehvendor:SlotUpdated", JsonConvert.SerializeObject(slot));
        }
      }
      else
      {
        InventoryServer.RemoveCarKeys(newPlate.ToLowerInvariant());
      }

      return ret;
    }

    public static async Task<bool> GiveFirstCar(int charId)
    {
      var newPlate = GenerateNewPlate();
      while (newPlate.StartsWith("PDM") || await VehicleServer.DoesPlateExist(newPlate, true))
      {
        newPlate = GenerateNewPlate();
      }

      Random random = new Random();
      random.Next();
      random.Next();
      var modelHash = GetHashKey(firstCars[random.Next(firstCars.Length)]);
      int color1 = cachedColors[rand.Next(cachedColors.Count)].ColorIndex;
      int color2 = cachedColors[rand.Next(cachedColors.Count)].ColorIndex;
      int colorp = cachedColors[rand.Next(cachedColors.Count)].ColorIndex;
      int colorw = cachedColors[rand.Next(cachedColors.Count)].ColorIndex;

      var props = JsonConvert.SerializeObject(new VehiclePropertiesBag { plate = newPlate, model = modelHash, color1 = color1, color2 = color2, pearlescentColor = colorp, wheelColor = colorw });

      var param = new Dictionary<string, object>();
      param.Add("@ownerChar", charId);
      param.Add("@properties", props);
      param.Add("@modelHash", modelHash);
      param.Add("@plate", newPlate);

      var res = await VSql.ExecuteAsync("insert into `vehicle` (`model`, `plate`, `place`, `properties`, `owner_char`) values (@modelHash, @plate, 'main', @properties, @ownerChar)", param);
      if (res != 1)
      {
        Debug.WriteLine("GiveFirstCar: Error saving VendorVehicle");
        return false;
      }

      return true;
    }

    private void OnError(Player player, string error)
    {
      player.TriggerEvent("ofw:ValidationErrorNotification", error);
    }
    static string GenerateNewPlate()
    {
      Random random = new Random();
      const string letters = "ABCDEFGHIJKLMNPQRSTUVWXYZ";
      const string digits = "0123456789";
      StringBuilder builder = new StringBuilder();

      // Přidáme první tři písmena
      for (int i = 0; i < 3; i++)
      {
        builder.Append(letters[random.Next(letters.Length)]);
      }

      // Přidáme následujících pět číslic
      for (int i = 0; i < 5; i++)
      {
        builder.Append(digits[random.Next(digits.Length)]);
      }

      return builder.ToString();
    }

    #endregion

    #region private


    #endregion
  }
}
