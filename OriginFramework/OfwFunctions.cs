using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
	public static class OfwFunctions
	{
		public static int CreateBlip(Vector3 coords, string name, int sprite, int color, float scale)
		{
			var blip = AddBlipForCoord(coords.X, coords.Y, coords.Z);
			SetBlipSprite(blip, sprite);
			SetBlipDisplay(blip, 4);
			SetBlipColour(blip, color);
			SetBlipAsShortRange(blip, false);
			SetBlipScale(blip, scale);
			BeginTextCommandSetBlipName("STRING");
			AddTextComponentString(name);
			EndTextCommandSetBlipName(blip);

			return blip;
		}

		public static int CreateVehicleBlip(int entity, string name, int sprite, int color, float scale)
		{
			var old = GetBlipFromEntity(entity);
			if (old > 0)
				RemoveBlip(ref old);

			var blip = AddBlipForEntity(entity);
			SetBlipSprite(blip, sprite);
			SetBlipDisplay(blip, 4);
			SetBlipColour(blip, color);
			SetBlipAsShortRange(blip, false);
			SetBlipScale(blip, scale);
			BeginTextCommandSetBlipName("STRING");
			AddTextComponentString(name);
			EndTextCommandSetBlipName(blip);

			return blip;
		}

		public static int CreateRadiusBlip(Vector3 coords, float radius, int color)
		{
			var blip = AddBlipForRadius(coords.X, coords.Y, coords.Z, radius);
			SetBlipDisplay(blip, 4);
			SetBlipColour(blip, color);
			SetBlipAsShortRange(blip, false);

			return blip;
		}

		public static async Task<dynamic> DeserializeToExpando(string serialized)
		{
			dynamic ret = null;
			bool completed = false;
			Func<dynamic, bool> CallbackFunction = (data) => { ret = data; completed = true; return true;	};
			BaseScript.TriggerServerEvent("ofw_deserializer:DeserializeExpandoClient", serialized, CallbackFunction);
			while (!completed)
			{
				await Main.Delay(0);
			}

			return ret;
		}

		public static async Task<bool> IsVehicleWithPlateOutOfGarageSpawned(string plate, dynamic esx)
		{
			bool ret = false;
			bool completed = false;
			Func<bool, bool> CallbackFunction = (data) => { ret = data; completed = true; return true; };
			BaseScript.TriggerServerEvent("ofw_veh:IsVehicleWithPlateOutOfGarage", plate, CallbackFunction);
			while (!completed)
			{
				await Main.Delay(0);
			}

			if (ret)
				return ret;

			//Projet jeste blizky auta

			return ret;
		}
	}
}
