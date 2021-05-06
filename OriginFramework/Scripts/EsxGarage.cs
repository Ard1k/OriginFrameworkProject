using CitizenFX.Core;
using OriginFrameworkData;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework.Scripts
{
	public class EsxGarage : BaseScript
	{
		private dynamic ESX = null;
		private GarageBag[] Garages = null;
		private float menuMarkerSize = 2f;
		private float vehMarkerSize = 6f;

		public EsxGarage()
		{
			EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
		}

		private async void OnClientResourceStart(string resourceName)
		{
			if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

			while (SettingsManager.Settings == null)
				await Delay(0);

			Garages = SettingsManager.Settings.Garages;

			AddTextEntry("OFW_ESXGARAGE_OPENMENU", $"Stiskni ~INPUT_CONTEXT~ pro otevreni garaze");
			AddTextEntry("OFW_ESXGARAGE_SAVEVEH", $"Stiskni ~INPUT_CONTEXT~ pro zaparkovani auta");

			while (ESX == null)
			{
				TriggerEvent("esx:getSharedObject", new object[] { new Action<dynamic>(esx => { ESX = esx; }) });
				await Delay(0);
			}

			foreach (var garage in Garages)
			{
				var garageBlip = AddBlipForCoord(garage.MenuPosition.X, garage.MenuPosition.Y, garage.MenuPosition.Z);

				SetBlipSprite(garageBlip, 290);
				SetBlipDisplay(garageBlip, 4);
				SetBlipScale(garageBlip, 0.7f);
				SetBlipColour(garageBlip, -1);
				SetBlipAsShortRange(garageBlip, true);
				BeginTextCommandSetBlipName("STRING");
				AddTextComponentString("Parking " + garage.Id);
				EndTextCommandSetBlipName(garageBlip);
			}

			Tick += OnTick;
		}

		public async Task OnTick()
		{
			bool sleep = true;

			var pedCoords = Game.PlayerPed.Position;
			bool draw = false;
			float markerSize = 1f;

			foreach (var garage in Garages)
			{
				var dstMenu = Vector3.Distance(pedCoords, new Vector3(garage.MenuPosition.X, garage.MenuPosition.Y, garage.MenuPosition.Z));
				if (dstMenu < 50f)
				{
					sleep = false;
					if (dstMenu < menuMarkerSize && GarageMenu.IsHidden)
					{
						DisplayHelpTextThisFrame("OFW_ESXGARAGE_OPENMENU", false);
						if (IsControlJustPressed(0, 38))
						{
							Game.DisableControlThisFrame(0, Control.Pickup);
							GarageMenu.ShowGarageMenu(garage);
						}
					}

					if (GarageMenu.IsHidden)
						DrawMarker(27, garage.MenuPosition.X, garage.MenuPosition.Y, garage.MenuPosition.Z, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0f, menuMarkerSize, menuMarkerSize, menuMarkerSize, 0, 255, 0, 255, false, false, 2, false, null, null, false);
				}

				var dstVeh = Vector3.Distance(pedCoords, new Vector3(garage.VehiclePosition.X, garage.VehiclePosition.Y, garage.VehiclePosition.Z));
				if (dstVeh < 50f)
				{
					sleep = false;
					if (dstVeh < vehMarkerSize && GarageMenu.IsHidden && DoIDriveAVehicle())
					{
						if (IsControlJustPressed(0, 38))
							GarageMenu.SaveInGarage(garage);
						DisplayHelpTextThisFrame("OFW_ESXGARAGE_SAVEVEH", false);
					}

					if (GarageMenu.IsHidden)
						DrawMarker(27, garage.VehiclePosition.X, garage.VehiclePosition.Y, garage.VehiclePosition.Z, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0f, vehMarkerSize, vehMarkerSize, vehMarkerSize, 255, 0, 0, 255, false, false, 2, false, null, null, false);
				}
			}

			if (sleep)
				await Delay(500);
		}

		public bool DoIDriveAVehicle()
		{
			if (IsPedInAnyVehicle(Game.PlayerPed.Handle, false))
			{
				var vehicle = GetVehiclePedIsIn(Game.PlayerPed.Handle, true);
				if (GetPedInVehicleSeat(vehicle, -1) == Game.PlayerPed.Handle)
					return true;
				else
					return false;
			}
			return false;
		}
	}
}
