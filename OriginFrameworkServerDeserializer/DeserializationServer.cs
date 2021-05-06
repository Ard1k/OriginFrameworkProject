using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.Native;
using Newtonsoft.Json.Converters;
using System.Dynamic;

namespace OriginFrameworkServerDeserializer
{
  public class DeserializationServer : BaseScript
  {
    public DeserializationServer()
    {
      EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
    }

    private async void OnResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;
    }

    [EventHandler("ofw_deserializer:DeserializeExpandoClient")]
    private async void DeserializeExpandoClient([FromSource] Player source, string serialized, NetworkCallbackDelegate callback)
    {
      var result = JsonConvert.DeserializeObject<ExpandoObject>(serialized, new ExpandoObjectConverter());
      _ = callback(result);
    }

    [EventHandler("ofw_deserializer:DeserializeExpandoServer")]
    private async void DeserializeExpandoServer([FromSource] Player source, string serialized, CallbackDelegate callback)
    {
      var result = JsonConvert.DeserializeObject<ExpandoObject>(serialized, new ExpandoObjectConverter());
      _ = callback(result);
    }
  }
}
