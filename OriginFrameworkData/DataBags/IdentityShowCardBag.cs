using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
  public class IdentityShowCardBag
  {
    public int CardId { get; set; }
    public int ItemId { get; set; }
    public uint Model { get; set; }
    public string CharName { get; set; }
    public string Born { get; set; }
    public string ValidTo { get; set; }
    public string Created { get; set; }
    public string Sn { get; set; }
    public Dictionary<string, int> CardSkin { get; set; }

    [JsonIgnore]
    public int ShowingServerId { get; set; }
    [JsonIgnore]
    public int TextureHandle { get; set; } = 0;
    [JsonIgnore]
    public string TextureName { get; set; }
  }
}
