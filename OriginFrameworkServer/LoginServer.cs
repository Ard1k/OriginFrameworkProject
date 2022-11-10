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

namespace OriginFrameworkServer
{
  public class LoginServer : BaseScript
  {
    public LoginServer()
    {
    }

    [EventHandler("ofw_login:GetCharacters")]
    private async void GetCharacters([FromSource] Player source, NetworkCallbackDelegate callback)
    {
      //Kontrola / registrace usera
      //Vraceni existujicich charu

      //while (NPCs == null)
      //  await Delay(0);

      //_ = callback(JsonConvert.SerializeObject(NPCs));
    }

  }
}
