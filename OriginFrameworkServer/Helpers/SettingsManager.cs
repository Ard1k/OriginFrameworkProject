using CitizenFX.Core;
using Newtonsoft.Json;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

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
						var read = LoadResourceFile(CitizenFX.Core.Native.API.GetCurrentResourceName(), "config.json");
						_settings = JsonConvert.DeserializeObject<SettingsBag>(read);
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
