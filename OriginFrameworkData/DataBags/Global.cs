using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
	public struct PosBag
	{
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }
		public float Heading { get; set; }

		//public PosBag() { }
		public PosBag(float x, float y, float z, float heading)
		{
      X = x;
      Y = y;
      Z = z;
      Heading = heading;
    }

		public bool IsEmpty()
		{
			return X + Y + Z == 0;
		}

		//pozustatek kdyz to byla trida
		public PosBag GetInstanceCopy()
		{
			return new PosBag(this.X, this.Y, this.Z, this.Heading);
		}
  }

	public struct DimensionsBag
	{
		public float Width { get; set; }
		public float Length { get; set; }
		public float Height { get; set; }
	}

	public struct ColorBag
	{
		public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }
    public int A { get; set; }
	}

	//public struct PosCameraBag : PosBag
	//{
	//	public float RotationX { get; set; }
	//	public float RotationY { get; set; }
	//	public float RotationZ { get; set; }
	//}

  public struct MinMaxBag
  {
    public int Min { get; set; }
    public int Max { get; set; }

    public MinMaxBag(int min, int max)
    {
      Min = min;
      Max = max;
    }
  }

	public struct CompoundAreaBag
	{
		
	}
}
