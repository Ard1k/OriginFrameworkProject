using CitizenFX.Core;
using Newtonsoft.Json;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkServer
{
	public static class SettingsManager
	{
		private static SettingsBag _settings;
		public static SettingsBag Settings
		{
			get
			{
				if (_settings == null)
				{
					try
					{
						var resName = CitizenFX.Core.Native.API.GetCurrentResourceName();
						var file = CitizenFX.Core.Native.Function.Call<string>(CitizenFX.Core.Native.Hash.LOAD_RESOURCE_FILE, resName, "config.json");
						_settings = JsonConvert.DeserializeObject<SettingsBag>(file);
					}
					catch (Exception ex)
					{
						Console.WriteLine("OFW - settings load exception: " + ex.ToString());
						_settings = new SettingsBag();
					}
				}

				return _settings;
			}
		}
	}
}
