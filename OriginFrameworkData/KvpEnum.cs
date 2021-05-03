using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData
{
	public enum KvpEnum : int
	{
		MiscDrawEntityInfo = 1,
		ShowClosesVehicleTrunkInfo = 2,
		DrifterEnabled = 3,
		FlashOnHorn = 4,

		ServerPersistenVehicleDB = 1000
	}

	public class KvpManager
	{
		public static string getKvpString(KvpEnum kvp)
		{
			switch (kvp)
			{
				case KvpEnum.MiscDrawEntityInfo: return "drawentityinfo";
				case KvpEnum.ShowClosesVehicleTrunkInfo: return "closesvehtrnkinfo";
				case KvpEnum.ServerPersistenVehicleDB: return "server_persvehdb";
				case KvpEnum.DrifterEnabled: return "drifterenabled";
				case KvpEnum.FlashOnHorn: return "flashonhorn";
				default: return "undefined";
			}
		}
	}
}
