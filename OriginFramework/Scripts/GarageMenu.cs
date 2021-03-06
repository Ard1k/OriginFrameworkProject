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
		private static string MenuTitle { get; set; }
		private static List<GarageMenuButton> Buttons { get; set; } = new List<GarageMenuButton>();
		private static int SelectedIndex { get; set; } = 0;
		public static bool IsHidden { get; protected set; } = true;
		private static GarageBag CurrentGarage { get; set; } = null;
		private static dynamic ESX { get; set; } = null;
		private static int ActiveCamera { get; set; } = -1;
		private static int LocalVehicle { get; set; } = -1;
		private static int MaxIndexVisible = 14;
		private static int SelectedIndexVisible = 0;
		private static bool controlsLocked = false;
		private static int recoverVehiclePrice = 1000;

		private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
		{
			Error = (obj, args) =>
			{
				var context = args.ErrorContext;

				context.Handled = true;
			}
		};

		public GarageMenu()
		{
			EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
		}

		private async void OnClientResourceStart(string resourceName)
		{
			if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

			while (SettingsManager.Settings == null)
				await Delay(0);

			recoverVehiclePrice = SettingsManager.Settings.GarageRecoverPrice;

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
			AddButton(name, onSelected, false, null, null, null, null, -1);
		}
		public static void AddButton(string name, Action onSelected, int index)
		{
			AddButton(name, onSelected, false, null, null, null, null, index);
		}
		public static void AddButton(string name, Action onSelected, bool colorAsUnavailable, string nameRight, string extraLeft, string extraRight, Action onHover)
		{
			AddButton(name, onSelected, colorAsUnavailable, nameRight, extraLeft, extraRight, onHover, -1);
		}
		public static void AddButton(string name, Action onSelected, bool colorAsUnavailable, string nameRight, string extraLeft, string extraRight, Action onHover, int index)
		{			
			var btn = new GarageMenuButton();

			btn.NameRight = nameRight;
			btn.ExtraLeft = extraLeft;
			btn.ExtraRight = extraRight;
			btn.Name = name;
			btn.FuncOnSelected = onSelected;
			btn.FuncOnHover = onHover;
			btn.Active = false;
			btn.ColorAsUnavailable = colorAsUnavailable;

			if (index >= 0)
				Buttons.Insert(index, btn);
			else
				Buttons.Add(btn);
			
		}
		public void UpdateSelection()
		{
			if (controlsLocked)
				return;

			if (IsControlJustPressed(1, 173)) //key down
			{
				if (SelectedIndex < Buttons.Count - 1)
				{
					SelectedIndex++;
					if (SelectedIndexVisible < MaxIndexVisible)
						SelectedIndexVisible++;
				}
				else
				{
					SelectedIndex = 0;
					SelectedIndexVisible = 0;
				}

				Buttons[SelectedIndex].FuncOnHover?.Invoke();
				PlaySound(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false, 0, true);
			}
			else if (IsControlJustPressed(1, 27)) // key up
			{
				if (SelectedIndex > 0)
				{
					SelectedIndex--;
					if (SelectedIndexVisible > 0)
						SelectedIndexVisible--;
				}
				else
				{
					SelectedIndex = Buttons.Count - 1;
					SelectedIndexVisible = (Buttons.Count - 1) <= MaxIndexVisible ? Buttons.Count - 1 : MaxIndexVisible;
				}

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
			float yoffset = 0.25f;
			float ymaxoffset = 0.03f;

			int indexMin = SelectedIndex - SelectedIndexVisible;
			int indexMax = SelectedIndex + (MaxIndexVisible - SelectedIndexVisible);

			if (MenuTitle != null)
			{
				SetTextScale(0.34f, 0.34f);
				SetTextColour(0, 0, 0, 255);
				SetTextEntry("STRING");
				AddTextComponentString("~h~" + MenuTitle.ToUpper());
				DrawText(0.63f, (yoffset + (0.03f * (-2 + 0.01f)) - 0.012f));

				DrawRect(0.7f, yoffset + (0.03f * (-2 + 0.01f)), 0.15f, ymaxoffset - 0.002f, 255, 255, 255, 200);
			}

			if (indexMin > 0)
			{
				SetTextScale(0.34f, 0.34f);
				SetTextColour(255, 255, 255, 255);
				SetTextEntry("STRING");
				AddTextComponentString("↑↑↑");
				DrawText(0.63f, (yoffset + (0.03f * (-1 + 0.01f)) - 0.012f));

				DrawRect(0.7f, yoffset + (0.03f * (-1 + 0.01f)), 0.15f, ymaxoffset - 0.002f, 13, 11, 10, 233); //main
			}

			if (indexMax < Buttons.Count - 1)
			{
				SetTextScale(0.34f, 0.34f);
				SetTextColour(255, 255, 255, 255);
				SetTextEntry("STRING");
				AddTextComponentString("↓↓↓");
				DrawText(0.63f, (yoffset + (0.03f * (15 + 0.01f)) - 0.012f));

				DrawRect(0.7f, yoffset + (0.03f * (15 + 0.01f)), 0.15f, ymaxoffset - 0.002f, 13, 11, 10, 233); //main
			}

			for (int i = 0; i < Buttons.Count; i++)
			{
				if (i < indexMin)
					continue;
				if (i > indexMax)
					continue;

				int screen_w = 0;
				int screen_h = 0;
				GetScreenResolution(ref screen_w, ref screen_h);
				float moveText = 0f;

				var boxColor = new int[4];

				if (!Buttons[i].Active)
				{
					if (Buttons[i].ColorAsUnavailable)
						boxColor = new int[] { 60, 20, 20, 233 };
					else
						boxColor = new int[] { 13, 11, 10, 233 };
				}
				else
				{
					if (Buttons[i].ColorAsUnavailable)
						boxColor = new int[] { 120, 45, 45, 230 };
					else
						boxColor = new int[] { 45, 45, 45, 230 };
				}

				SetTextFont(4);

				if (Buttons[i].Name != null)
				{
					SetTextScale(0.34f, 0.34f);
					SetTextColour(255, 255, 255, 255);
					SetTextEntry("STRING");
					AddTextComponentString(Buttons[i].Name);
					DrawText(0.63f, (yoffset + (0.03f * (i - indexMin + 0.01f)) - 0.012f));
				}

				if (Buttons[i].NameRight != null)
				{
					SetTextFont(4);
					SetTextScale(0.26f, 0.26f);
					SetTextColour(255, 255, 255, 255);
					SetTextEntry("STRING");
					AddTextComponentString(Buttons[i].NameRight);
					DrawText(0.730f + moveText, (yoffset + (0.03f * (i - indexMin + 0.01f)) - 0.009f));
				}

				if (Buttons[i].ExtraLeft != null)
				{
					SetTextFont(4);
					SetTextScale(0.31f, 0.31f);
					SetTextColour(11, 11, 11, 255);
					SetTextEntry("STRING");
					AddTextComponentString(Buttons[i].ExtraLeft);
					DrawText(0.78f, (yoffset + (0.03f * (i - indexMin + 0.01f)) - 0.012f));
				}

				if (Buttons[i].ExtraRight != null)
				{
					SetTextFont(4);
					SetTextScale(0.31f, 0.31f);
					SetTextColour(11, 11, 11, 255);
					SetTextEntry("STRING");
					AddTextComponentString(Buttons[i].ExtraRight);
					DrawText(0.845f, (yoffset + (0.03f * (i - indexMin + 0.01f)) - 0.012f));
				}


				DrawRect(0.7f, yoffset + (0.03f * (i - indexMin + 0.01f)), 0.15f, ymaxoffset - 0.002f, boxColor[0], boxColor[1], boxColor[2], boxColor[3]); //main

				if (Buttons[i].ExtraLeft != null || Buttons[i].ExtraRight != null)
					DrawRect(0.832f, yoffset + (0.03f * (i - indexMin + 0.01f)), 0.11f, ymaxoffset - 0.002f, 255, 255, 255, 199); // extra
			}
		}

		public static void ClearMenu()
		{
			Buttons.Clear();
			SelectedIndex = 0;
			SelectedIndexVisible = 0;
		}

		public static void CloseMenu()
		{
			IsHidden = true;
			controlsLocked = false;
			ClearMenu();
			DeleteLocalVehicle();
			EnsureGarageCamera(null);
			RenderScriptCams(false, true, 750, true, false);
			PlaySound(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false, 0, true);

			var vehs = World.GetAllVehicles();

			if (vehs != null)
			{
				foreach (var veh in vehs)
				{
					if (!NetworkGetEntityIsNetworked(veh.Handle))
					{
						int vehId = veh.Handle;
						DeleteEntity(ref vehId);
					}
				}
			}
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
			MenuTitle = "garaz";
			ClearMenu();

			AddButton("Seznam vozidel", new Action(() => { ListVehicles(); }));
			if (!CurrentGarage.Id.StartsWith("Imp"))
				if (garage.Job == null || garage.Job == ESX.GetPlayerData()?.job?.name)
					AddButton("Znicene/zabavene", new Action(() => { ListOutOfGarageVehicles(); }));
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
			MenuTitle = "Vlastnena auta";
			ClearMenu();
			EnsureGarageCamera(CurrentGarage);
			var playerJob = ESX.GetPlayerData()?.job;

			string ret = await OfwFunctions.ServerAsyncCallbackToSync<string>("ofw_esxgarage:GetVehicles", CurrentGarage.Type, CurrentGarage.Id, (string)playerJob?.name);

			if (ret != null)
			{
				controlsLocked = true;

				var fetched = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(ret);
				foreach (var row in fetched)
				{
					if ((long)row["stored"] <= 0)
						continue;

					var vehicle = JsonConvert.DeserializeObject<VehiclePropertiesBag>((string)row["vehicle"], jsonSettings);
					float engine = 1000f;
					float fuel = 100f;

					if (vehicle.engineHealth != null)
						engine = (float)vehicle.engineHealth;
					if (vehicle.fuelLevel != null)
						fuel = (float)vehicle.fuelLevel;

					var displayName = GetDisplayNameFromVehicleModel((uint)vehicle.model);
					var plate = (string)row["plate"];
					bool correctGarage = (string)row["garage"] == CurrentGarage?.Id;
					bool correctJob = true;
					string ownerSocietyName = null;
					string owner = (string)row["owner"];
					var minJobGrade = Convert.ToInt32(row["minjobgrade"]);
					if (owner.StartsWith("society:"))
						ownerSocietyName = owner.Replace("society:", String.Empty);

					if (playerJob != null && (string)row["job"] != null && !CurrentGarage.Id.StartsWith("Imp"))
					{
						if (playerJob?.name != (string)row["job"])
							correctJob = false;
					}

					if (ownerSocietyName != null && playerJob?.grade < minJobGrade)
						correctJob = false;

					AddButton($"{plate} | {ownerSocietyName ?? "osobni"} | {displayName}", new Action(() => { VehicleMenu(vehicle, $"{plate} | {ownerSocietyName ?? "osobni"} | {displayName}", correctGarage, correctJob, ownerSocietyName, minJobGrade); }), !correctJob || !correctGarage, $"Garage: {(string)row["garage"]}", $"Motor: {engine}/1000", $"Palivo: {fuel}%", new Action(() => { SpawnLocalVehicle(vehicle); }));
				}

				Buttons = Buttons.OrderBy(b => b.ColorAsUnavailable).ToList();
				AddButton("Zpet", new Action(() => { DeleteLocalVehicle(); ShowGarageMenu(); }), 0);
				controlsLocked = false;
			}
			else
			{
				Notify.Alert("Nemas zadne auta!");
				ShowGarageMenu();
				return;
			}

			IsHidden = false;
		}

		private static async void ListOutOfGarageVehicles()
		{
			MenuTitle = "Auta mimo garaz";
			ClearMenu();
			EnsureGarageCamera(CurrentGarage);
			var playerJob = ESX.GetPlayerData()?.job;

			string ret = null;
			bool completed = false;
			Func<string, bool> CallbackFunction = (data) =>
			{
				ret = data;
				completed = true;
				return true;
			};

			BaseScript.TriggerServerEvent("ofw_esxgarage:GetVehicles", CurrentGarage.Type, CurrentGarage.Id, (string)playerJob?.name, CallbackFunction);

			while (!completed)
			{
				await Delay(0);
			}

			if (ret != null)
			{
				var fetched = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(ret);
				controlsLocked = true;
				foreach (var row in fetched)
				{
					if ((long)row["stored"] >= 1)
						continue;

					var plate = (string)row["plate"];
					var vehicle = JsonConvert.DeserializeObject<VehiclePropertiesBag>((string)row["vehicle"], jsonSettings);
					bool doesExistNow = await OfwFunctions.IsVehicleWithPlateOutOfGarageSpawned(plate, ESX);

					var displayName = GetDisplayNameFromVehicleModel((uint)vehicle.model);
					bool correctJob = true;
					string ownerSocietyName = null;
					string owner = (string)row["owner"];
					int minJobGrade = Convert.ToInt32(row["minjobgrade"]);
					if (owner.StartsWith("society:"))
						ownerSocietyName = owner.Replace("society:", String.Empty);

					if (playerJob != null && (string)row["job"] != null && !CurrentGarage.Id.StartsWith("Imp"))
					{
						if (playerJob?.name != (string)row["job"])
							correctJob = false;
					}

					if (ownerSocietyName != null && playerJob?.grade < minJobGrade)
						correctJob = false;

					string extraString = null;
					if (doesExistNow)
						extraString = "Vozidlo je nekde vyparkovane!";
					else if (!correctJob)
						extraString = "Nemas spravny job!";

					AddButton($"{plate} | {ownerSocietyName ?? "osobni"} | {displayName}", new Action(() => { VehicleOutOfGarageMenu(vehicle, $"{plate} | {ownerSocietyName ?? "osobni"} | {displayName} ", doesExistNow, correctJob); }), !correctJob || doesExistNow, null, extraString, null, new Action(() => { SpawnLocalVehicle(vehicle); }));
				}

				Buttons = Buttons.OrderBy(b => b.ColorAsUnavailable).ToList();
				AddButton("Zpet", new Action(() => { DeleteLocalVehicle(); ShowGarageMenu(); }), 0);
				controlsLocked = false;
			}
			else
			{
				Notify.Alert("Nemas zadne auta mimo garaz!");
				ShowGarageMenu();
				return;
			}

			IsHidden = false;
		}

		private static async void ManageVehicleRights(string menutitle, VehiclePropertiesBag vehicle, int currentMinGrade, string society)
		{
			MenuTitle = menutitle;
			ClearMenu();

			AddButton("Zpet", new Action(() => { ListVehicles(); }));
			AddButton("Soucasna minimalni pozice: " + currentMinGrade, null);
			AddButton($"Nastavit: bez omezeni", new Action(() => { SetVehicleJobGrade(vehicle, -1); ListVehicles(); }));
			string ret = await OfwFunctions.ServerAsyncCallbackToSync<string>("ofw_misc:GetJobGrades", society);

			if (ret != null)
			{
				var fetched = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(ret);
				foreach (var row in fetched)
				{
					var grade = Convert.ToInt32(row["grade"]);
					var label = (string)row["label"];

					AddButton($"Nastavit: {label} ({grade}+)", new Action(() => { SetVehicleJobGrade(vehicle, grade); ListVehicles(); }));
				}
			}

			IsHidden = false;
		}

		private static async void VehicleMenu(VehiclePropertiesBag vehicle, string menutitle, bool correctGarage, bool correctJob, string society, int currentMinGrade)
		{
			MenuTitle = menutitle;
			ClearMenu();
			
			if (!correctJob)
				AddButton("Nemas spravny job/pozici!", null);
			else if (!correctGarage)
				AddButton("Auto je v jine garazi!", null);
			else
				AddButton("Vyparkuj auto", new Action(() => { SpawnVehicle(vehicle); }));

			var playerJob = ESX.GetPlayerData()?.job;
			if (playerJob != null && playerJob.name != null && playerJob.name == society && (playerJob.grade_name == "boss" || playerJob.grade_name == "viceboss"))
				AddButton("Nastaveni firemniho opravneni", new Action(() => { ManageVehicleRights(menutitle, vehicle, currentMinGrade, society); }));

			AddButton("Zpet", new Action(() => { ListVehicles(); }));
		}

		private static async void VehicleOutOfGarageMenu(VehiclePropertiesBag vehicle, string menutitle, bool doesExistNow, bool correctJob)
		{
			MenuTitle = menutitle;
			ClearMenu();

			if (!correctJob)
				AddButton("Nemas spravny job/pozici!", null);
			else if (doesExistNow)
				AddButton("Vozidlo je nekde vyparkovane!", null);
			else
				AddButton($"Obnovit auto do teto garaze {recoverVehiclePrice}$", new Action(() => { RecoverVehicle(vehicle); }));

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

		private static async void SpawnLocalVehicle(VehiclePropertiesBag vehicleData)
		{
			if (vehicleData == null || CurrentGarage == null)
				return;

			controlsLocked = true;

			var spawnpoint = new Vector3(CurrentGarage.VehiclePosition.X, CurrentGarage.VehiclePosition.Y, CurrentGarage.VehiclePosition.Z);

			DeleteLocalVehicle();

			if (!IsModelValid((uint)vehicleData?.model))
			{
				controlsLocked = false;
				return;
			}

			WaitForModel((int)vehicleData.model);

			if (!ESX.Game.IsSpawnPointClear(spawnpoint, 3.0f))
			{
				Notify.Alert("Spawnpoint je blokovan");
				controlsLocked = false;
				return;
			}

			ESX.Game.SpawnLocalVehicle(vehicleData.model, spawnpoint, CurrentGarage.VehiclePosition.Heading, new Action<dynamic>((spawnedVeh) => {
				LocalVehicle = (int)spawnedVeh;

				SetVehicleProperties((int)spawnedVeh, vehicleData);

				SetModelAsNoLongerNeeded((uint)vehicleData.model);

				controlsLocked = false;
			}));
		}

		private static async void SetVehicleJobGrade(VehiclePropertiesBag vehicleData, int grade)
		{
			if (vehicleData == null)
				return;

			bool ret = await OfwFunctions.ServerAsyncCallbackToSync<bool>("ofw_esxgarage:SetVehicleMinJobGrade", vehicleData.plate, grade);
		}

		private static async void SpawnVehicle(VehiclePropertiesBag vehicleData)
		{
			if (vehicleData == null || CurrentGarage == null)
				return;

			var spawnpoint = new Vector3(CurrentGarage.VehiclePosition.X, CurrentGarage.VehiclePosition.Y, CurrentGarage.VehiclePosition.Z);

			if (!IsModelValid((uint)vehicleData?.model))
				return;

			DeleteLocalVehicle();

			if (!ESX.Game.IsSpawnPointClear(spawnpoint, 3.0f))
			{
				Notify.Alert("Spawnpoint je blokovan");
				return;
			}

			controlsLocked = true;

			int ret = -1;
			bool completed = false;
			Func<int, bool> CallbackFunction = (data) =>
			{
				ret = data;
				completed = true;
				return true;
			};

			BaseScript.TriggerServerEvent("ofw_veh:SpawnServerVehicleFromGarage", vehicleData.plate, new Vector3(CurrentGarage.VehiclePosition.X, CurrentGarage.VehiclePosition.Y, CurrentGarage.VehiclePosition.Z), CurrentGarage.VehiclePosition.Heading, CallbackFunction);

			while (!completed)
			{
				await Delay(0);

				DrawScreenText($"Cekani na spawn ...", 255, 255, 255, 150);
			}

			controlsLocked = false;
			if (ret == -1)
				return;

			CloseMenu();

			int frameCounter = 0;
			while (!NetworkDoesNetworkIdExist(ret) || !NetworkDoesEntityExistWithNetworkId(ret))
			{
				await Delay(100);
				frameCounter++;
				if (frameCounter > 100)
				{
					Notify.Error("Nepodarilo se identifikovat vozidlo vytvorene serverem!");
					return;
				}
			}

			int vehID = NetworkGetEntityFromNetworkId(ret);
			TaskWarpPedIntoVehicle(Game.PlayerPed.Handle, vehID, -1);
			await Delay(100);
			SetVehicleProperties(vehID, vehicleData);
		}

		private static async void RecoverVehicle(VehiclePropertiesBag vehicleData)
		{
			if (vehicleData == null || CurrentGarage == null)
				return;

			BaseScript.TriggerServerEvent("ofw_esxgarage:RecoverVehicle", vehicleData.plate, CurrentGarage.Id);

			CloseMenu();
		}

		private async static void SetVehicleProperties(int vehicle, VehiclePropertiesBag vehicleProps)
		{
			OfwFunctions.SetVehicleProperties(vehicle, vehicleProps);

			if (vehicleProps.windows != null)
			{
				for (int i = 0; i <= 13; i++)
				{
					if (vehicleProps.windows.Length > i && (bool)vehicleProps.windows[i] == false)
						SmashVehicleWindow(vehicle, i);
				}
			}

			if (vehicleProps.tyres != null)
			{
				for (int i = 0; i <= 7; i++)
				{
					if (vehicleProps.tyres.Length > i && (bool)vehicleProps.tyres[i] == false)
						SetVehicleTyreBurst(vehicle, i, true, 1000);
				}
			}
			
			if (vehicleProps.doors != null)
			{
				for (int i = 0; i <= 5; i++)
				{
					if (vehicleProps.doors.Length > i && (bool)vehicleProps.doors[i] == false)
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

			Console.WriteLine(JsonConvert.SerializeObject(vehicleProps));

			bool ret = false;
			bool completed = false;
			Func<bool, bool> CallbackFunction = (data) =>
			{
				ret = data;
				completed = true;
				return true;
			};

			BaseScript.TriggerServerEvent("ofw_esxgarage:CanParkVehicle", (string)vehicleProps.plate, CurrentGarage.Type, CallbackFunction);

			while (!completed)
			{
				await Delay(0);
			}

			if (!ret)
			{
				Notify.Error("Vozidlo nelze zaparkovat. Neni vlastnene nebo spatny typ garaze!");
				return;
			}

			TaskLeaveVehicle(Game.PlayerPed.Handle, veh, 0);

			while (IsPedInVehicle(Game.PlayerPed.Handle, veh, true))
				await Delay(0);
			await Delay(1000);

			Notify.Success("Vozidlo zaparkovano!");
			DeleteEntity(ref veh);

			TriggerServerEvent("ofw_esxgarage:SaveVehicle", CurrentGarage.Id, JsonConvert.SerializeObject(vehicleProps));
		}

		private static VehiclePropertiesBag GetVehicleProperties(int veh)
		{
			if (!DoesEntityExist(veh))
				return null;

			var vehicleProps = OfwFunctions.GetVehicleProperties(veh);

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
		public string ExtraLeft { get; set; }
		public string ExtraRight { get; set; }
		public bool ColorAsUnavailable { get; set; }
	}
}
