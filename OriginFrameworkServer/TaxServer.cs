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
  public class TaxServer : BaseScript
  {
    public static TaxBag CurrentTaxes = new TaxBag();

    public TaxServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
    }

    private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.TaxServer, eScriptArea.VSql))
        return;

      //Tick += KeepObjectsUp;
      var results = await VSql.FetchAllAsync("SELECT * FROM `tax_rates`", null);
      if (results == null || results.Count <= 0)
      {
        Debug.WriteLine("TAX_SERVER:No tax rates found in database.");
      }
      else
      {
        CurrentTaxes = TaxBag.ParseFromSql(results[0]);

        Debug.WriteLine("TAX_SERVER:Loaded tax rates from database.");

        if (results.Count > 1)
          Debug.WriteLine("TAX_SERVER: !!!! Multiple tax records in DB! Resolve this!!!!!");
      }

      InternalDependencyManager.Started(eScriptArea.TaxServer);
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
    }

    [EventHandler("ofw_tax:GetTaxes")]
    private async void GetTaxes([FromSource] Player source)
    {
      if (source == null)
        return;

      source.TriggerEvent("ofw_tax:UpdateTaxes", JsonConvert.SerializeObject(CurrentTaxes));
    }
  }
}
