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
		public static bool DebugMode { get; set; } = false;

		#region base script functions wrappers
		public static void TriggerServerEvent(string eventName, params object[] args)
		{
			BaseScript.TriggerServerEvent(eventName, args);
		}

		public static void TriggerEvent(string eventName, params object[] args)
		{
			BaseScript.TriggerEvent(eventName, args);
		}

		public static async Task Delay(int time)
		{
			await BaseScript.Delay(time);
		}
		#endregion

		#region Optional debug
		public static void WriteOnlyInDebugMode(string data)
		{
			if (DebugMode) Debug.WriteLine(@data);
		}
		#endregion

		#region blips
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

		#endregion

		#region ofw_deserializer
		public static async Task<dynamic> DeserializeToExpando(string serialized)
		{
			dynamic ret = null;
			bool completed = false;
			Func<dynamic, bool> CallbackFunction = (data) => { ret = data; completed = true; return true; };
			BaseScript.TriggerServerEvent("ofw_deserializer:DeserializeExpandoClient", serialized, CallbackFunction);
			while (!completed)
			{
				await Main.Delay(0);
			}

			return ret;
		}
		#endregion

		#region vehicle related
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

			var vehs = World.GetAllVehicles();
			bool isAnyFound = false;

			if (vehs != null)
			{
				foreach (var veh in vehs)
				{
					var cplate = GetVehicleNumberPlateText(veh.Handle).Trim();
					if (cplate != null && cplate == plate)
						isAnyFound = true;
				}
			}

			return isAnyFound;
		}

		public static VehiclePropertiesBag GetVehicleProperties(int vehicleId)
		{
			if (!DoesEntityExist(vehicleId))
				return null;

			int colorPrimary = 0,	colorSecondary = 0;
			GetVehicleColours(vehicleId, ref colorPrimary, ref colorSecondary);
			int pearlescentColor = 0, wheelColor = 0;
			GetVehicleExtraColours(vehicleId, ref pearlescentColor, ref wheelColor);
			var extras = new Dictionary<string, bool>();
			for (int i = 0; i < 20; i++)
			{
				if (DoesExtraExist(vehicleId, i))
				{
					extras.Add(i.ToString(), IsVehicleExtraTurnedOn(vehicleId, i));
				}
			}
			var neonEnabled = new List<bool>();
			for (int i = 0; i < 4; i++)
			{
				neonEnabled.Add(IsVehicleNeonLightEnabled(vehicleId, i));
			}

			int r = 0, g = 0, b = 0;
			GetVehicleNeonLightsColour(vehicleId, ref r, ref g, ref b);
			var neonColor = new List<int> { r, g, b };

			int r2 = 0, g2 = 0, b2 = 0;
			GetVehicleTyreSmokeColor(vehicleId, ref r2, ref g2, ref b2);
			var tyreSmokeColor = new List<int> { r2, g2, b2 };

			var bag = new VehiclePropertiesBag();

			bag.model = GetEntityModel(vehicleId);
			bag.plate = GetVehicleNumberPlateText(vehicleId).Trim();
			bag.plateIndex = GetVehicleNumberPlateTextIndex(vehicleId);

			bag.bodyHealth = (float)Math.Round(GetVehicleBodyHealth(vehicleId), 1);
			bag.engineHealth = (float)Math.Round(GetVehicleEngineHealth(vehicleId), 1);
			bag.fuelLevel = (float)Math.Round(GetVehicleFuelLevel(vehicleId), 1);
			bag.dirtLevel = (float)Math.Round(GetVehicleDirtLevel(vehicleId), 1);

			bag.color1 = colorPrimary;
			bag.color2 = colorSecondary;
			bag.pearlescentColor = pearlescentColor;
			bag.wheelColor = wheelColor;

			bag.wheels = GetVehicleWheelType(vehicleId);
			bag.windowTint = GetVehicleWindowTint(vehicleId);
			bag.xenonColor = GetVehicleXenonLightsColour(vehicleId);
			bag.neonEnabled = neonEnabled;
			bag.neonColor = neonColor;
			bag.tyreSmokeColor = tyreSmokeColor;
			bag.extras = extras;

			bag.modSpoilers = GetVehicleMod(vehicleId, 0);
			bag.modFrontBumper = GetVehicleMod(vehicleId, 1);
			bag.modRearBumper = GetVehicleMod(vehicleId, 2);
			bag.modSideSkirt = GetVehicleMod(vehicleId, 3);
			bag.modExhaust = GetVehicleMod(vehicleId, 4);
			bag.modFrame = GetVehicleMod(vehicleId, 5);
			bag.modGrille = GetVehicleMod(vehicleId, 6);
			bag.modHood = GetVehicleMod(vehicleId, 7);
			bag.modFender = GetVehicleMod(vehicleId, 8);
			bag.modRightFender = GetVehicleMod(vehicleId, 9);
			bag.modRoof = GetVehicleMod(vehicleId, 10);

			bag.modEngine = GetVehicleMod(vehicleId, 11);
			bag.modBrakes = GetVehicleMod(vehicleId, 12);
			bag.modTransmission = GetVehicleMod(vehicleId, 13);
			bag.modHorns = GetVehicleMod(vehicleId, 14);
			bag.modSuspension = GetVehicleMod(vehicleId, 15);
			bag.modArmor = GetVehicleMod(vehicleId, 16);

			bag.modTurbo = IsToggleModOn(vehicleId, 18);
			bag.modSmokeEnabled = IsToggleModOn(vehicleId, 20);
			bag.modXenon = IsToggleModOn(vehicleId, 22);

			bag.modFrontWheels = GetVehicleMod(vehicleId, 23);
			bag.modBackWheels = GetVehicleMod(vehicleId, 24);

			bag.modPlateHolder = GetVehicleMod(vehicleId, 25);
			bag.modVanityPlate = GetVehicleMod(vehicleId, 26);
			bag.modTrimA = GetVehicleMod(vehicleId, 27);
			bag.modOrnaments = GetVehicleMod(vehicleId, 28);
			bag.modDashboard = GetVehicleMod(vehicleId, 29);
			bag.modDial = GetVehicleMod(vehicleId, 30);
			bag.modDoorSpeaker = GetVehicleMod(vehicleId, 31);
			bag.modSeats = GetVehicleMod(vehicleId, 32);
			bag.modSteeringWheel = GetVehicleMod(vehicleId, 33);
			bag.modShifterLeavers = GetVehicleMod(vehicleId, 34);
			bag.modAPlate = GetVehicleMod(vehicleId, 35);
			bag.modSpeakers = GetVehicleMod(vehicleId, 36);
			bag.modTrunk = GetVehicleMod(vehicleId, 37);
			bag.modHydrolic = GetVehicleMod(vehicleId, 38);
			bag.modEngineBlock = GetVehicleMod(vehicleId, 39);
			bag.modAirFilter = GetVehicleMod(vehicleId, 40);
			bag.modStruts = GetVehicleMod(vehicleId, 41);
			bag.modArchCover = GetVehicleMod(vehicleId, 42);
			bag.modAerials = GetVehicleMod(vehicleId, 43);
			bag.modTrimB = GetVehicleMod(vehicleId, 44);
			bag.modTank = GetVehicleMod(vehicleId, 45);
			bag.modWindows = GetVehicleMod(vehicleId, 46);
			bag.modLivery = GetVehicleMod(vehicleId, 48);
			if (bag.modLivery <= 0)
				bag.modLivery = GetVehicleLivery(vehicleId);

			return bag;
		}

		public static void SetVehicleProperties(int vehicleId, VehiclePropertiesBag props)
		{
			SetVehicleModKit(vehicleId, 0);

			int colorPrimary = 0, colorSecondary = 0;
			GetVehicleColours(vehicleId, ref colorPrimary, ref colorSecondary);
			int pearlescentColor = 0, wheelColor = 0;
			GetVehicleExtraColours(vehicleId, ref pearlescentColor, ref wheelColor);

			if (props.plate != null) SetVehicleNumberPlateText(vehicleId, props.plate);
			if (props.plateIndex != null) SetVehicleNumberPlateTextIndex(vehicleId, props.plateIndex.Value);
			if (props.bodyHealth != null)SetVehicleBodyHealth(vehicleId, props.bodyHealth.Value);
			if (props.engineHealth != null) SetVehicleEngineHealth(vehicleId, props.engineHealth.Value);
			if (props.fuelLevel != null) SetVehicleFuelLevel(vehicleId, props.fuelLevel.Value);
			if (props.dirtLevel != null) SetVehicleDirtLevel(vehicleId, props.dirtLevel.Value);
			if (props.color1 != null) SetVehicleColours(vehicleId, props.color1.Value, colorSecondary);
			if (props.color2 != null) SetVehicleColours(vehicleId, props.color1 ?? colorPrimary, props.color2.Value);
			if (props.pearlescentColor != null) SetVehicleExtraColours(vehicleId, props.pearlescentColor.Value, wheelColor);
			if (props.wheelColor != null) SetVehicleExtraColours(vehicleId, props.pearlescentColor ?? pearlescentColor, props.wheelColor.Value);
			if (props.wheels != null) SetVehicleWheelType(vehicleId, props.wheels.Value);
			if (props.windowTint != null) SetVehicleWindowTint(vehicleId, props.windowTint.Value);

			if (props.neonEnabled != null)
			{
				SetVehicleNeonLightEnabled(vehicleId, 0, props.neonEnabled[0]);
				SetVehicleNeonLightEnabled(vehicleId, 1, props.neonEnabled[1]);
				SetVehicleNeonLightEnabled(vehicleId, 2, props.neonEnabled[2]);
				SetVehicleNeonLightEnabled(vehicleId, 3, props.neonEnabled[3]);
			}

			if (props.extras != null)
			{
				for (int i = 0; i < 20; i++)
				{
					if (props.extras.ContainsKey(i.ToString()))
					{
						SetVehicleExtra(vehicleId, i, !props.extras[i.ToString()]);
					}
				}
			}

			if (props.neonColor != null) SetVehicleNeonLightsColour(vehicleId, props.neonColor[0], props.neonColor[1], props.neonColor[2]);
			if (props.xenonColor != null) SetVehicleXenonLightsColour(vehicleId, props.xenonColor.Value);
			if (props.modSmokeEnabled != null) ToggleVehicleMod(vehicleId, 20, true);
			if (props.tyreSmokeColor != null) SetVehicleTyreSmokeColor(vehicleId, props.tyreSmokeColor[0], props.tyreSmokeColor[1], props.tyreSmokeColor[2]);

			if (props.modSpoilers != null) SetVehicleMod(vehicleId, 0, props.modSpoilers.Value, false);
			if (props.modFrontBumper != null) SetVehicleMod(vehicleId, 1, props.modFrontBumper.Value, false);
			if (props.modRearBumper != null) SetVehicleMod(vehicleId, 2, props.modRearBumper.Value, false);
			if (props.modSideSkirt != null) SetVehicleMod(vehicleId, 3, props.modSideSkirt.Value, false);
			if (props.modExhaust != null) SetVehicleMod(vehicleId, 4, props.modExhaust.Value, false);
			if (props.modFrame != null) SetVehicleMod(vehicleId, 5, props.modFrame.Value, false);
			if (props.modGrille != null) SetVehicleMod(vehicleId, 6, props.modGrille.Value, false);
			if (props.modHood != null) SetVehicleMod(vehicleId, 7, props.modHood.Value, false);
			if (props.modFender != null) SetVehicleMod(vehicleId, 8, props.modFender.Value, false);
			if (props.modRightFender != null) SetVehicleMod(vehicleId, 9, props.modRightFender.Value, false);
			if (props.modRoof != null) SetVehicleMod(vehicleId, 10, props.modRoof.Value, false);
			if (props.modEngine != null) SetVehicleMod(vehicleId, 11, props.modEngine.Value, false);
			if (props.modBrakes != null) SetVehicleMod(vehicleId, 12, props.modBrakes.Value, false);
			if (props.modTransmission != null) SetVehicleMod(vehicleId, 13, props.modTransmission.Value, false);
			if (props.modHorns != null) SetVehicleMod(vehicleId, 14, props.modHorns.Value, false);
			if (props.modSuspension != null) SetVehicleMod(vehicleId, 15, props.modSuspension.Value, false);
			if (props.modArmor != null) SetVehicleMod(vehicleId, 16, props.modArmor.Value, false);
			if (props.modTurbo != null) ToggleVehicleMod(vehicleId, 18, props.modTurbo.Value);
			if (props.modXenon != null) ToggleVehicleMod(vehicleId, 22, props.modXenon.Value);
			if (props.modFrontWheels != null) SetVehicleMod(vehicleId, 23, props.modFrontWheels.Value, false);
			if (props.modBackWheels != null) SetVehicleMod(vehicleId, 24, props.modBackWheels.Value, false);
			if (props.modPlateHolder != null) SetVehicleMod(vehicleId, 25, props.modPlateHolder.Value, false);
			if (props.modVanityPlate != null) SetVehicleMod(vehicleId, 26, props.modVanityPlate.Value, false);
			if (props.modTrimA != null) SetVehicleMod(vehicleId, 27, props.modTrimA.Value, false);
			if (props.modOrnaments != null) SetVehicleMod(vehicleId, 28, props.modOrnaments.Value, false);
			if (props.modDashboard != null) SetVehicleMod(vehicleId, 29, props.modDashboard.Value, false);
			if (props.modDial != null) SetVehicleMod(vehicleId, 30, props.modDial.Value, false);
			if (props.modDoorSpeaker != null) SetVehicleMod(vehicleId, 31, props.modDoorSpeaker.Value, false);
			if (props.modSeats != null) SetVehicleMod(vehicleId, 32, props.modSeats.Value, false);
			if (props.modSteeringWheel != null) SetVehicleMod(vehicleId, 33, props.modSteeringWheel.Value, false);
			if (props.modShifterLeavers != null) SetVehicleMod(vehicleId, 34, props.modShifterLeavers.Value, false);
			if (props.modAPlate != null) SetVehicleMod(vehicleId, 35, props.modAPlate.Value, false);
			if (props.modSpeakers != null) SetVehicleMod(vehicleId, 36, props.modSpeakers.Value, false);
			if (props.modTrunk != null) SetVehicleMod(vehicleId, 37, props.modTrunk.Value, false);
			if (props.modHydrolic != null) SetVehicleMod(vehicleId, 38, props.modHydrolic.Value, false);
			if (props.modEngineBlock != null) SetVehicleMod(vehicleId, 39, props.modEngineBlock.Value, false);
			if (props.modAirFilter != null) SetVehicleMod(vehicleId, 40, props.modAirFilter.Value, false);
			if (props.modStruts != null) SetVehicleMod(vehicleId, 41, props.modStruts.Value, false);
			if (props.modArchCover != null) SetVehicleMod(vehicleId, 42, props.modArchCover.Value, false);
			if (props.modAerials != null) SetVehicleMod(vehicleId, 43, props.modAerials.Value, false);
			if (props.modTrimB != null) SetVehicleMod(vehicleId, 44, props.modTrimB.Value, false);
			if (props.modTank != null) SetVehicleMod(vehicleId, 45, props.modTank.Value, false);
			if (props.modWindows != null) SetVehicleMod(vehicleId, 46, props.modWindows.Value, false);

			if (props.modLivery != null)
			{
				SetVehicleMod(vehicleId, 48, props.modLivery.Value, false);
				SetVehicleLivery(vehicleId, props.modLivery.Value);
			}
		}

		#endregion

		#region string utils
		public static string[] StringToArray(string inputString)
		{
			return CitizenFX.Core.UI.Screen.StringToArray(inputString);
		}
		#endregion

		#region Draw text
		public static void DrawTextOnScreen(string text, float xPosition, float yPosition) =>
				DrawTextOnScreen(text, xPosition, yPosition, size: 0.48f);

		public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size) =>
				DrawTextOnScreen(text, xPosition, yPosition, size, CitizenFX.Core.UI.Alignment.Left);

		public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification) =>
				DrawTextOnScreen(text, xPosition, yPosition, size, justification, 6);

		public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font) =>
				DrawTextOnScreen(text, xPosition, yPosition, size, justification, font, false);

		public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font, bool disableTextOutline)
		{
			if (IsHudPreferenceSwitchedOn() && Hud.IsVisible && !IsPlayerSwitchInProgress() && IsScreenFadedIn() && !IsPauseMenuActive() && !IsFrontendFading() && !IsPauseMenuRestarting() && !IsHudHidden())
			{
				SetTextFont(font);
				SetTextScale(1.0f, size);
				if (justification == CitizenFX.Core.UI.Alignment.Right)
				{
					SetTextWrap(0f, xPosition);
				}
				SetTextJustification((int)justification);
				if (!disableTextOutline) { SetTextOutline(); }
				BeginTextCommandDisplayText("STRING");
				AddTextComponentSubstringPlayerName(text);
				EndTextCommandDisplayText(xPosition, yPosition);
			}
		}
		#endregion

		#region vMenu - GetUserInput
		public static async Task<string> GetUserInput() => await GetUserInput(null, null, 30);
		public static async Task<string> GetUserInput(int maxInputLength) => await GetUserInput(null, null, maxInputLength);
		public static async Task<string> GetUserInput(string windowTitle) => await GetUserInput(windowTitle, null, 30);
		public static async Task<string> GetUserInput(string windowTitle, int maxInputLength) => await GetUserInput(windowTitle, null, maxInputLength);
		public static async Task<string> GetUserInput(string windowTitle, string defaultText) => await GetUserInput(windowTitle, defaultText, 30);
		public static async Task<string> GetUserInput(string windowTitle, string defaultText, int maxInputLength)
		{
			// Create the window title string.
			var spacer = "\t";
			AddTextEntry($"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", $"{windowTitle ?? "Enter"}:{spacer}(MAX {maxInputLength} Characters)");

			// Display the input box.
			DisplayOnscreenKeyboard(1, $"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", "", defaultText ?? "", "", "", "", maxInputLength);
			await Delay(0);
			// Wait for a result.
			while (true)
			{
				int keyboardStatus = UpdateOnscreenKeyboard();

				switch (keyboardStatus)
				{
					case 3: // not displaying input field anymore somehow
					case 2: // cancelled
						return null;
					case 1: // finished editing
						return GetOnscreenKeyboardResult();
					default:
						await Delay(0);
						break;
				}
			}
		}
		#endregion
	}
}
