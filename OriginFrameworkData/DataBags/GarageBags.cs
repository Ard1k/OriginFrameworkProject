using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
	public class GarageBag
	{
		public string Id { get; set; }
		public PosBag MenuPosition { get; set; }
		public PosBag VehiclePosition { get; set; }
		public PosCameraBag Camera { get; set; }
	}
}
