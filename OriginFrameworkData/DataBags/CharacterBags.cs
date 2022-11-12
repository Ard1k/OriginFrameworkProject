using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
	public class CharacterBag
	{
		public int Id { get; set; }
		public string UserIdentifier { get; set; }
		public string Name { get; set; }
		public int Model { get; set; }
		public int AdminLevel { get; set; }
		public PlayerPosBag LastKnownPos { get; set; }
		public bool IsDead { get; set; }
		[JsonIgnore]
		public int? DiedGameTime { get; set; }
		public DateTime? DiedServerTime { get; set; }
	}

	public class PlayerPosBag
	{
		public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
		public float H { get; set; }
  }
}
