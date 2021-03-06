using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
	public class PosBag
	{
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }
		public float Heading { get; set; }
	}

	public class PosCameraBag : PosBag
	{
		public float RotationX { get; set; }
		public float RotationY { get; set; }
		public float RotationZ { get; set; }
	}
}
