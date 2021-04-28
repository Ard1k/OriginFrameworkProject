using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
	public class GroupBag
	{
		public bool IsInAGroup { get; set; }
		public GroupMemberBag[] Members { get; set; }
	}

	public class GroupMemberBag
	{
		public int ServerPlayerID { get; set; }
		public int NetPedID { get; set; }
		public string DisplayName { get; set; }
		public bool IsOnline { get; set; }
		public float Distance { get; set; }
		public bool IsInRange { get; set; }
		public bool IsInQuestRange { get; set; }
	}
}
