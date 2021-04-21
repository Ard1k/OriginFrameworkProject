using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
	public class SettingsBag
	{
		public int MenuKey { get; set; } = 344;
		public bool MenuAlignRight { get; set; } = false;
		public bool LoadHeistIsland { get; set; } = false;
		public bool TestJobActive { get; set; } = false;

		public SettingsBag() { }
	}
}
