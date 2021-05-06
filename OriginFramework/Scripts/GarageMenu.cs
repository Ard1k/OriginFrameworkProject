using CitizenFX.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
	public class GarageMenu : BaseScript
	{
		static string MenuTitle { get; set; }
		static List<GarageMenuButton> Buttons { get; set; } = new List<GarageMenuButton>();
		static int SelectedIndex { get; set; } = 0;
		public static bool IsHidden { get; protected set; } = true;
		private static GarageBag CurrentGarage { get; set; } = null;
		public static dynamic ESX { get; set; } = null;
		public static int ActiveCamera { get; set; } = -1;
		public static int LocalVehicle { get; set; } = -1;

		public GarageMenu()
		{
			EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
		}

		private async void OnClientResourceStart(string resourceName)
		{
			if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

			while (SettingsManager.Settings == null)
				await Delay(0);


			while (ESX == null)
			{
				TriggerEvent("esx:getSharedObject", new object[] { new Action<dynamic>(esx => { ESX = esx; }) });
				await Delay(0);
			}

			Tick += OnTick;
		}

		#region menu functions

		public static void AddButton(string name, Action onSelected)
		{
			AddButton(name, onSelected, null, null, null, null);
		}
		public static void AddButton(string name, Action onSelected, string nameRight, string extraLeft, string extraRight, Action onHover)
		{
			var yoffset = 0.25f;
			var xoffset = 0.3f;
			var xmin = 0.0f;
			var xmax = 0.15f;
			var ymin = 0.03f;
			var ymax = 0.03f;

			var btn = new GarageMenuButton();

			btn.NameRight = nameRight;
			btn.ExtraLeft = extraLeft;
			btn.ExtraRight = extraRight;
			btn.Name = name;
			btn.FuncOnSelected = onSelected;
			btn.FuncOnHover = onHover;
			btn.Active = false;
			btn.Xmin = xmin;
			btn.Ymin = ymin * (Buttons.Count + 0.01f) + yoffset;
			btn.Xmax = xmax;
			btn.Ymax = ymax;

			Buttons.Add(btn);
		}
		public void UpdateSelection()
		{
			if (IsControlJustPressed(1, 173)) //key down
			{
				if (SelectedIndex < Buttons.Count - 1)
					SelectedIndex++;
				else
					SelectedIndex = 0;

				Buttons[SelectedIndex].FuncOnHover?.Invoke();
				PlaySound(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false, 0, true);
			}
			else if (IsControlJustPressed(1, 27)) // key up
			{
				if (SelectedIndex > 0)
					SelectedIndex--;
				else
					SelectedIndex = Buttons.Count - 1;

				Buttons[SelectedIndex].FuncOnHover?.Invoke();
				PlaySound(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false, 0, true);
			}
			else if (IsControlJustPressed(1, 38) || IsControlJustPressed(1, 18))
			{
				Buttons[SelectedIndex].FuncOnSelected?.Invoke();
				PlaySound(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false, 0, true);
			}

			for (int i = 0; i < Buttons.Count; i++)
			{
				if (i == SelectedIndex)
					Buttons[i].Active = true;
				else
					Buttons[i].Active = false;
			}
		}

		public void RenderGUI()
		{
			if (!IsHidden)
			{
				RenderButtons();
				UpdateSelection();
			}
		}

		public void RenderButtons()
		{
			float yoffset = 0.5f;
			float xoffset = 0f;

			foreach (var btn in Buttons)
			{
				int screen_w = 0;
				int screen_h = 0;
				GetScreenResolution(ref screen_w, ref screen_h);
				float moveText = 0f;

				var boxColor = new int[] { 13, 11, 10, 233 };
				//if (btn.Extra == "In")
				//	boxColor = new int[] { 44, 88, 44, 230 };
				//else if (btn.Extra == "Out")
				//	moveText = -0.025f;

				if (btn.Active)
					boxColor = new int[] { 45, 45, 45, 230 };

				SetTextFont(4);

				if (btn.Name != null)
				{
					SetTextScale(0.34f, 0.34f);
					SetTextColour(255, 255, 255, 255);
					SetTextEntry("STRING");
					AddTextComponentString(btn.Name);
					DrawText(0.63f, (btn.Ymin - 0.012f));
				}

				if (btn.NameRight != null)
				{
					SetTextFont(4);
					SetTextScale(0.26f, 0.26f);
					SetTextColour(255, 255, 255, 255);
					SetTextEntry("STRING");
					AddTextComponentString(btn.NameRight);
					DrawText(0.730f + moveText, (btn.Ymin - 0.009f));
				}

				if (btn.ExtraLeft != null)
				{
					SetTextFont(4);
					SetTextScale(0.31f, 0.31f);
					SetTextColour(11, 11, 11, 255);
					SetTextEntry("STRING");
					AddTextComponentString(btn.ExtraLeft);
					DrawText(0.78f, (btn.Ymin - 0.012f));
				}

				if (btn.ExtraRight != null)
				{
					SetTextFont(4);
					SetTextScale(0.31f, 0.31f);
					SetTextColour(11, 11, 11, 255);
					SetTextEntry("STRING");
					AddTextComponentString(btn.ExtraRight);
					DrawText(0.845f, (btn.Ymin - 0.012f));
				}


				DrawRect(0.7f, btn.Ymin, 0.15f, btn.Ymax - 0.002f, boxColor[0], boxColor[1], boxColor[2], boxColor[3]); //main

				if (btn.ExtraLeft != null || btn.ExtraRight != null)
					DrawRect(0.832f, btn.Ymin, 0.11f, btn.Ymax - 0.002f, 255, 255, 255, 199); // extra
			}
		}

		public static void ClearMenu()
		{
			Buttons.Clear();
			SelectedIndex = 0;
		}

		public static void CloseMenu()
		{
			IsHidden = true;
			ClearMenu();
			DeleteLocalVehicle();
			EnsureGarageCamera(null);
			RenderScriptCams(false, true, 750, true, false);
			PlaySound(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false, 0, true);
		}

		#endregion

		#region show menu functions
		public static void ShowGarageMenu()
		{
			ShowGarageMenu(CurrentGarage);
		}
		public static void ShowGarageMenu(GarageBag garage)
		{
			CurrentGarage = garage;
			MenuTitle = "Garage";
			ClearMenu();

			AddButton("Seznam vozidel", new Action(() => { ListVehicles(); }));
			AddButton("Znicene/zabavene", new Action(() => { CloseMenu(); }));
			AddButton("Zavrit", new Action(() => { CloseMenu(); }));

			IsHidden = false;
		}

		#endregion

		#region onTick events
		public async Task OnTick()
		{
			if (!IsHidden && IsControlJustPressed(1, 177))
			{
				CloseMenu();
			}

			RenderGUI();
		}
		#endregion

		#region menu onclick functions
		private static async void ListVehicles()
		{
			MenuTitle = "Moje auta";
			ClearMenu();
			EnsureGarageCamera(CurrentGarage);
			AddButton("Zpet", new Action(() => { DeleteLocalVehicle(); ShowGarageMenu(); }));

			string ret = null;
			bool completed = false;
			Func<string, bool> CallbackFunction = (data) =>
			{
				ret = data;
				completed = true;
				return true;
			};

			BaseScript.TriggerServerEvent("ofw_esxgarage:GetVehicles", CallbackFunction);

			while (!completed)
			{
				await Delay(0);
			}

			if (ret != null)
			{
				var fetched = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(ret);
				foreach (var row in fetched)
				{
					dynamic vehicle = await OfwFunctions.DeserializeToExpando((string)row["vehicle"]);
					float engine = 1000f;
					float fuel = 100f;

					if (((IDictionary<String, object>)vehicle).ContainsKey("engineHealth"))
						engine = (float)vehicle.engineHealth;
					if (((IDictionary<String, object>)vehicle).ContainsKey("fuelLevel"))
						fuel = (float)vehicle.fuelLevel;

					var displayName = GetDisplayNameFromVehicleModel((uint)vehicle.model);
					var plate = (string)row["plate"];
					AddButton($"{plate} | {displayName}", new Action(() => { VehicleMenu(vehicle); }), $"Garage: {(string)row["garage"]}", $"Motor: {engine}/1000", $"Palivo: {fuel}%", new Action(() => { SpawnLocalVehicle(vehicle); }));
				}
			}
			else
			{
				Notify.Alert("Nemas zadne auta!");
				ShowGarageMenu();
				return;
			}

			IsHidden = false;
		}

		private static async void VehicleMenu(dynamic vehicle)
		{
			MenuTitle = "Moje auta";
			ClearMenu();
			AddButton("Vyparkuj auto", null);
			AddButton("Zpet", new Action(() => { ListVehicles(); }));
		}
		#endregion

		#region private methods
		private static void EnsureGarageCamera(GarageBag garage)
		{
			if (ActiveCamera > 0)
			{
				DestroyCam(ActiveCamera, false);
				ActiveCamera = -1;
			}

			if (garage != null && garage.Camera != null)
			{
				ActiveCamera = CreateCam("DEFAULT_SCRIPTED_CAMERA", true);
				SetCamCoord(ActiveCamera, garage.Camera.X, garage.Camera.Y, garage.Camera.Z);
				SetCamRot(ActiveCamera, garage.Camera.RotationX, garage.Camera.RotationY, garage.Camera.RotationZ, 2);
				SetCamActive(ActiveCamera, true);

				RenderScriptCams(true, true, 750, true, true);
			}
		}

		private static async void SpawnLocalVehicle(dynamic vehicleData)
		{
			if (vehicleData == null || CurrentGarage == null)
				return;

			var spawnpoint = new Vector3(CurrentGarage.VehiclePosition.X, CurrentGarage.VehiclePosition.Y, CurrentGarage.VehiclePosition.Z);

			DeleteLocalVehicle();

			if (!IsModelValid((uint)vehicleData?.model))
				return;

			WaitForModel((int)vehicleData.model);

			if (!ESX.Game.IsSpawnPointClear(spawnpoint, 3.0f))
			{
				Notify.Alert("Spawnpoint je blokovan");
				return;
			}

			ESX.Game.SpawnLocalVehicle(vehicleData.model, spawnpoint, CurrentGarage.VehiclePosition.Heading, new Action<dynamic>((spawnedVeh) => {
				LocalVehicle = (int)spawnedVeh;

				SetVehicleProperties((int)spawnedVeh, vehicleData);

				SetModelAsNoLongerNeeded((uint)vehicleData.model);
			}));
		}

		private async static void SetVehicleProperties(int vehicle, dynamic vehicleProps)
		{
			ESX.Game.SetVehicleProperties(vehicle, vehicleProps);

			if (((IDictionary<String, object>)vehicleProps).ContainsKey("engineHealth"))
				SetVehicleEngineHealth(vehicle, (float)vehicleProps?.engineHealth);
			if (((IDictionary<String, object>)vehicleProps).ContainsKey("fuelLevel"))
			{
				SetVehicleFuelLevel(vehicle, (float)vehicleProps?.fuelLevel);
			}

			

			if (((IDictionary<String, object>)vehicleProps).ContainsKey("windows"))
			{
				for (int i = 0; i <= 13; i++)
				{
					if (vehicleProps.windows.Count > i && (bool)vehicleProps.windows[i] == false)
						SmashVehicleWindow(vehicle, i);
				}
			}

			if (((IDictionary<String, object>)vehicleProps).ContainsKey("tyres"))
			{
				for (int i = 0; i <= 7; i++)
				{
					if (vehicleProps.tyres.Count > i && (bool)vehicleProps.tyres[i] == false)
						SetVehicleTyreBurst(vehicle, i, true, 1000);
				}
			}
			
			if (((IDictionary<String, object>)vehicleProps).ContainsKey("doors"))
			{
				for (int i = 0; i <= 5; i++)
				{
					if (vehicleProps.doors.Count > i && (bool)vehicleProps.doors[i] == false)
						SetVehicleDoorBroken(vehicle, i, true);
				}
			}
		}

		private static void DeleteLocalVehicle()
		{
			if (LocalVehicle <= 0)
				return;

			int lv = LocalVehicle;
			DeleteEntity(ref lv);
			LocalVehicle = -1;
		}

		private static async void WaitForModel(int modelHash)
		{
			if (!IsModelValid((uint)modelHash))
			{
				Notify.Error("Model neexistuje, prosim napis report");
				return;
			}

			if (!HasModelLoaded((uint)modelHash))
				RequestModel((uint)modelHash);

			while (!HasModelLoaded((uint)modelHash))
			{
				await Delay(0);

				DrawScreenText($"Cekani na model {GetDisplayNameFromVehicleModel((uint)modelHash) ?? "Neznamy"}...", 255, 255, 255, 150);
			}
		}

		private static void DrawScreenText(string text, int red, int green, int blue, int alpha)
		{
			SetTextFont(4);
			SetTextScale(0.0f, 0.5f);
			SetTextColour(red, green, blue, alpha);
			SetTextDropshadow(0, 0, 0, 0, 255);
			SetTextEdge(1, 0, 0, 0, 255);
			SetTextDropShadow();
			SetTextOutline();
			SetTextCentre(true);
			BeginTextCommandDisplayText("STRING");
			AddTextComponentSubstringPlayerName(text);
			EndTextCommandDisplayText(0.5f, 0.5f);
		}

		public static async void SaveInGarage(GarageBag garage)
		{
			if (garage == null)
				return;

			CurrentGarage = garage;

			var veh = GetVehiclePedIsUsing(Game.PlayerPed.Handle);
			if (veh <= 0)
				return;

			var vehicleProps = GetVehicleProperties(veh);

			bool ret = false;
			bool completed = false;
			Func<bool, bool> CallbackFunction = (data) =>
			{
				ret = data;
				completed = true;
				return true;
			};

			BaseScript.TriggerServerEvent("ofw_esxgarage:IsVehicleOwned", (string)vehicleProps.plate, CallbackFunction);

			while (!completed)
			{
				await Delay(0);
			}

			if (!ret)
			{
				Notify.Error("Nelze zaparkovat NPC vozidlo!");
				return;
			}

			TaskLeaveVehicle(Game.PlayerPed.Handle, veh, 0);

			while (IsPedInVehicle(Game.PlayerPed.Handle, veh, true))
				await Delay(0);
			await Delay(1000);

			Notify.Success("Vozidlo zaparkovano!");
			DeleteEntity(ref veh);

			TriggerServerEvent("ofw_esxgarage:SaveVehicle", CurrentGarage.Id, vehicleProps);
		}

		private static dynamic GetVehicleProperties(int veh)
		{
			if (!DoesEntityExist(veh))
				return null;

			var vehicleProps = ESX.Game.GetVehicleProperties(veh);

			var windows = new bool[14];
			for (int i = 0; i <= 13; i++)
				windows[i] = IsVehicleWindowIntact(veh, i);

			var tyres = new bool[6];
			for (int i = 0; i <= 5; i++)
				tyres[i] = !IsVehicleTyreBurst(veh, i, false);

			var doors = new bool[6];
			for (int i = 0; i <= 5; i++)
				doors[i] = !IsVehicleDoorDamaged(veh, i);

			vehicleProps.windows = windows;
			vehicleProps.tyres = tyres;
			vehicleProps.doors = doors;

			return vehicleProps;
		}
		#endregion
	}


	public class GarageMenuButton
	{
		public string Name { get; set; }
		public string NameRight { get; set; }
		public Action FuncOnHover { get; set; }
		public Action FuncOnSelected { get; set; }
		public bool Active { get; set; }
		public float Xmin { get; set; }
		public float Xmax { get; set; }
		public float Ymin { get; set; }
		public float Ymax { get; set; }
		public string ExtraLeft { get; set; }
		public string ExtraRight { get; set; }
	}
}
