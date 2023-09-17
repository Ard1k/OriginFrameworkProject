using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
	public class PersistentVehicleBag
	{
		public Guid UniqueId { get; set; }
		public int? GarageId { get; set; }
		public int? VehicleVendorSlot { get; set; }
		public bool IsServerRespawning { get; set; }
    public int NetID { get; set; }
		public int ModelHash { get; set; }
		public PosBag LastKnownPos { get; set; }
		public string Plate { get; set; }
		public string Properties { get; set; }
		public bool IsInPropertiesSync { get; set; }
		public long? LastPropertiesSync { get; set; }
		public string Damage { get; set; }
		public bool KeepUnlocked { get; set; }
		public bool BrokenLock { get; set; }

		public PersistentVehicleBag() 
		{
			UniqueId = Guid.NewGuid();
		}
	}
}
