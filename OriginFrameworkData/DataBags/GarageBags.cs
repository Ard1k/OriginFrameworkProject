using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
	public class GarageBag
	{
		public static DimensionsBag DefaultSpot = new DimensionsBag()
		{
      Width = 2.3f,
      Length = 4.5f,
      Height = 2.0f
    };
    public static DimensionsBag LargeSpot = new DimensionsBag()
    {
      Width = 3.3f,
      Length = 8.5f,
      Height = 2.0f
    };

    public enum eParkingSpotType : int
    { 
      TakeOut = 0,
      Return = 1
    }

    public class ParkingSpot
    {
      public PosBag Center { get; set; }
      public DimensionsBag Dimensions { get; set; }
      public eParkingSpotType Type { get; set; }
    }

    public string Place { get; set; }
    public string Name { get; set; }
    public PosBag GarageLocation { get; set; }
    public List<ParkingSpot> Parkings { get; set; }
	}
}
