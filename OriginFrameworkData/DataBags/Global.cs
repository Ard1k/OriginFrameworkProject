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

		public PosBag() { }
		public PosBag(float x, float y, float z, float heading)
		{
      X = x;
      Y = y;
      Z = z;
      Heading = heading;
    }

		public PosBag GetInstanceCopy()
		{
			return new PosBag(this.X, this.Y, this.Z, this.Heading);
		}
  }

	public class DimensionsBag
	{
		public float Width { get; set; }
		public float Length { get; set; }
		public float Height { get; set; }
	}

	public class ColorBag
	{
		public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }
    public int A { get; set; }
	}

	public class PosCameraBag : PosBag
	{
		public float RotationX { get; set; }
		public float RotationY { get; set; }
		public float RotationZ { get; set; }
	}
}
