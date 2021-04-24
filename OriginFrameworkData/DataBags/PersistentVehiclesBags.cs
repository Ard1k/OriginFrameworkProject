using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
	public class PersistentVehicleDatabaseBag
	{
		public List<PersistentVehicleBag> Vehicles { get; set; }
	}

	public class PersistentVehicleBag
	{
		public int LocalID { get; set; }
		public int NetID { get; set; }
		public int ModelHash { get; set; }
		public VehiclePosBag LastKnownPos { get; set; }
	}

	public class VehiclePosBag
	{
		public VehiclePosBag() { }
		public VehiclePosBag(float x, float y, float z, float heading)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
			this.Heading = heading;
		}

		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }
		public float Heading { get; set; }
	}
}
