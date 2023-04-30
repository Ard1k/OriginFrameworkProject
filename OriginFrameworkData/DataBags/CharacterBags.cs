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
		public int? OrganizationId { get; set; }
		public Dictionary<string,int> Skin { get; set; }
		public bool IsNew { get; set; }

		private CharacterBag()
		{ 
		}
		public CharacterBag(int model)
		{
			Model = model;
		}

		public static CharacterBag ParseFromSql(Dictionary<string, object> row)
		{
			var it = new CharacterBag
			{
				Id = Convert.ToInt32(row["id"]),
				UserIdentifier = Convert.ToString(row["user_identifier"]),
				Name = Convert.ToString(row["name"]),
				AdminLevel = Convert.ToInt32(row["admin_level"]),
				IsNew = Convert.ToBoolean(row["is_new"])
			};

			if (row.ContainsKey("organization_id") && row["organization_id"] != null && row["organization_id"] != DBNull.Value)
				it.OrganizationId = Convert.ToInt32(row["organization_id"]);

			if (row.ContainsKey("pos") && row["pos"] != null)
				it.LastKnownPos = JsonConvert.DeserializeObject<PlayerPosBag>(Convert.ToString(row["pos"]));
			
			if (row.ContainsKey("model") && row["model"] != null && row["model"] != DBNull.Value)
				it.Model = Convert.ToInt32(row["model"]);

			if (row.ContainsKey("skin") && row["skin"] != null && row["skin"] != DBNull.Value)
				it.Skin = JsonConvert.DeserializeObject<Dictionary<string, int>>(Convert.ToString(row["skin"]));

			return it;
    }
	}

	public class PlayerPosBag
	{
		public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
		public float H { get; set; }
  }
}
