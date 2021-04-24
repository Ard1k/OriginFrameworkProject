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
				default: return "undefined";
			}
		}
	}
}
