using CitizenFX.Core;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.UI.Screen;

namespace OriginFramework
{
	public static class OfwFunctions
	{
		#region blocked park spots
		public static int GetParkingSpotBlockingEntity(Vector3 center, float heading)
		{
			return GetParkingSpotBlockingEntity(center, heading, 2.8f, 5.5f, 2.0f);
		}

		public static int GetParkingSpotBlockingEntity(Vector3 center, float heading, float width, float length, float height)
		{
			//--2->auta
			//-- 4->peds
			//-- 16->objekty
			//-- 256->rostliny
			int flags = 2;
			int ray = StartShapeTestBox(
				center.X, center.Y, center.Z,
				width, length, height,
				0.0f, 0.0f, heading, 2,
				flags,
				0, //entita, kterou má raycast ignorovat(např.PlayerPedId() pokud chceme ignorovat sveho peda)
				4); //neznamy parametr

			bool hit = false;
			Vector3 endCoords = new Vector3();
			Vector3 surfaceNormal = new Vector3();
			int entityHit = 0;

			GetShapeTestResult(ray, ref hit, ref endCoords, ref surfaceNormal, ref entityHit);

			return entityHit;
		}
		#endregion
	}
}
