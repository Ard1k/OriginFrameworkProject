using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFramework
{
	public enum KvpEnum : int
	{
		MiscDrawEntityInfo = 1,
		ShowClosesVehicleTrunkInfo = 2
	}

	public class KvpManager
	{
		public static string getKvpString(KvpEnum kvp)
		{
			switch (kvp)
			{
				case KvpEnum.MiscDrawEntityInfo: return "drawentityinfo";
				case KvpEnum.ShowClosesVehicleTrunkInfo: return "closesvehtrnkinfo";
				default: Debug.WriteLine($"Unknown kvp: {kvp}"); return "undefined";
			}
		}
	}
}
