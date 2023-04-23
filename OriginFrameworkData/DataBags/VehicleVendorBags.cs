using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
	public enum eVehicleVendor : int
	{
		PDM = 0,
	}

	public class VehicleVendorVehicle
	{
		public eVehicleVendor Vendor { get; set; }
		public string Model { get; set; }
		public int? BankMoneyPrice { get; set; }
		public int? Price { get; set; }
		public int? PriceItemId { get; set; }
		public float SpawnChance { get; set; }
		public TimeSpan SpawnForTime { get; set; }
	}

	public static class VehicleVendorStock
	{
		public static List<VehicleVendorVehicle> Stock = new List<VehicleVendorVehicle>
		{
			new VehicleVendorVehicle { Vendor = eVehicleVendor.PDM, Model = "adder", SpawnForTime = TimeSpan.FromHours(1), SpawnChance = 0.2f, BankMoneyPrice = 100000},
      new VehicleVendorVehicle { Vendor = eVehicleVendor.PDM, Model = "t20", SpawnForTime = TimeSpan.FromHours(1), SpawnChance = 0.2f, BankMoneyPrice = 100000, Price = 200000, PriceItemId = 17},
      new VehicleVendorVehicle { Vendor = eVehicleVendor.PDM, Model = "blista", SpawnForTime = TimeSpan.FromHours(1), SpawnChance = 1f, Price = 100, PriceItemId = 1},
      new VehicleVendorVehicle { Vendor = eVehicleVendor.PDM, Model = "everon", SpawnForTime = TimeSpan.FromHours(1), SpawnChance = 1f, Price = 100, PriceItemId = 1, BankMoneyPrice = 10000},
    };
	}

	public class VehicleVendorSlot
	{
		private static int _nextId = 1000;

		public int SlotId { get; set; }
		public eVehicleVendor VendorType { get; set; }
		public PosBag Position { get; set; }
		public DateTime? RepopulateAt { get; set; }
		public VehicleVendorVehicle CurrentVehicle { get; set; }
		public int? SpawnedNetId { get; set; }

		/// <summary>
		/// Konstruktor nepouzivat, pristupny pouze kvuli deserializaci
		/// </summary>
		public VehicleVendorSlot()
		{ 
		}

		public VehicleVendorSlot(eVehicleVendor vendorType, PosBag position)
		{
      SlotId = _nextId++;
			Position = position.GetInstanceCopy();
			VendorType = vendorType;
    }
	}
}
