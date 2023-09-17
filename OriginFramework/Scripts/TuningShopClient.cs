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
  public class TuningShopClient : BaseScript
  {
    public static List<TuningShopBag> Shops = new List<TuningShopBag>
    {
      new TuningShopBag
      { 
        Area = new DimensionsBag {  }
      },
    };

    public TuningShopClient()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.TuningShopClient))
        return;

      //TriggerServerEvent("ofw_tax:GetTaxes");

      //RegisterCommand("debugtax", new Action<int, List<object>, string>((source, args, raw) =>
      //{
      //  Debug.WriteLine("TAXES:");
      //  Debug.WriteLine($"Food {CurrentTaxes.Food}%");
      //  Debug.WriteLine($"Cars {CurrentTaxes.Cars}%");
      //  Debug.WriteLine($"Medical {CurrentTaxes.Medical}%");
      //  Debug.WriteLine($"Clothes {CurrentTaxes.Clothes}%");
      //  Debug.WriteLine($"Fuel {CurrentTaxes.Fuel}%");
      //  Debug.WriteLine($"Weapons {CurrentTaxes.Weapons}%");
      //  Debug.WriteLine($"Income {CurrentTaxes.Income}%");
      //}), false);

      InternalDependencyManager.Started(eScriptArea.TuningShopClient);
    }

    //[EventHandler("ofw_tax:UpdateTaxes")]
    //private void UpdateTaxes(string taxData)
    //{
    //  CurrentTaxes = JsonConvert.DeserializeObject<TaxBag>(taxData);
    //}
  }
}
