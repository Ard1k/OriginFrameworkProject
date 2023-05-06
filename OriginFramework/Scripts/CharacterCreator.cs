using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using OriginFramework.Menus;
using OriginFramework.Scripts;
using OriginFrameworkData;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework
{
	public class CharacterCreator : BaseScript
	{
    public static bool IsInCharacterCreator { get; private set; } = false;
    private const string charCreatorMenuName = "characterCreatorMenu";
    private static CharacterBag NewCharacter = null;
    private static PosBag CharCreatorPedPos = new PosBag(400.05f, -1000.2399f, -99.9506f, 358.4357f);

    private int selectedModelIndex = 0;
    private static List<Tuple<string, string>> availablePedModels = new List<Tuple<string, string>>
    {
      new Tuple<string, string>("mp_m_freemode_01", "Muž"),
      new Tuple<string, string>("mp_f_freemode_01", "Žena"),
      //new Tuple<string, string>("a_c_retriever", "Peso more"),
      //new Tuple<string, string>("player_two", "Trevor"),
      //new Tuple<string, string>("player_one", "Misa"),
    };

    public CharacterCreator()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      if (!await InternalDependencyManager.CanStart(eScriptArea.CharacterCreator))
        return;

      Tick += OnTick;

      InternalDependencyManager.Started(eScriptArea.CharacterCreator);
    }

    private async Task OnTick()
		{
      if (IsInCharacterCreator)
      {
        if (!NativeMenuManager.IsMenuOpen(charCreatorMenuName))
          NativeMenuManager.EnsureMenuOpen(charCreatorMenuName, getCreatorMenu);

        //kamera
        SetGameplayCamRelativeHeading(180.0f);
        SetGameplayCamRelativePitch(0.0f, 1.0f);
        SetFollowPedCamViewMode(2);

        PedCam.StartCam(60f);
      }
		}

    private static void FreezePlayer(bool isFreeze)
    {
      SetPlayerControl(Game.Player.Handle, !isFreeze, 0);

      if (isFreeze)
      {
        if (IsEntityVisible(Game.PlayerPed.Handle))
          SetEntityVisible(Game.PlayerPed.Handle, false, false);

        SetEntityCollision(Game.PlayerPed.Handle, false, false);
        FreezeEntityPosition(Game.PlayerPed.Handle, true);
        SetPlayerInvincible(Game.Player.Handle, true);
      }
      else 
      {
        if (!IsEntityVisible(Game.PlayerPed.Handle))
          SetEntityVisible(Game.PlayerPed.Handle, true, false);

        SetEntityCollision(Game.PlayerPed.Handle, true, false);
        FreezeEntityPosition(Game.PlayerPed.Handle, false);
        SetPlayerInvincible(Game.Player.Handle, false);
      }
    }

    private NativeMenu getCreatorMenu()
    {
      if (NewCharacter == null)
      {
        selectedModelIndex = 0;
        NewCharacter = new CharacterBag(GetHashKey(availablePedModels[selectedModelIndex].Item1));
      }

      if (NewCharacter.Skin == null)
        NewCharacter.Skin = new Dictionary<string, int>();

      string[] values = SkinManager.AppearanceAll;

      Dictionary<string, MinMaxBag> minMax = new Dictionary<string, MinMaxBag>();
      bool isFemale = Game.PlayerPed.Model.Hash == GetHashKey("mp_f_freemode_01");

      foreach (var c in values)
      {
        if (!NewCharacter.Skin.ContainsKey(c))
          NewCharacter.Skin.Add(c, (isFemale && SkinManager.Components[c].DefaultFemale != null) ? SkinManager.Components[c].DefaultFemale.Value : SkinManager.Components[c].DefaultValue);
      }

      var menu = new NativeMenu
      {
        MenuTitle = "Character creator",
        Items = new List<NativeMenuItem>
          {
            new NativeMenuItem
            {
              Name = "Jméno",
              NameRight = NewCharacter.Name ?? "---",
              IsTextInput = true,
              TextInputMaxLength = 50,
              TextInputRequest = "Zadejte jméno postavy - 2 nebo 3 slova začínající velkým písmenem",
              OnTextInput = (item, input) =>
                {
                  NewCharacter.Name = input;
                  item.NameRight = NewCharacter.Name;
                }
            },
            new NativeMenuItem
            {
              Name = "Datum narození",
              NameRight = NewCharacter.Born.ToString("dd.MM.yyyy"),
              IsTextInput = true,
              TextInputMaxLength = 50,
              TextInputRequest = "Zadejte datum narození",
              OnTextInput = (item, input) =>
                {
                  DateTime dt;
                  if (DateTime.TryParseExact(input, "d.M.yyyy", CultureInfo.InvariantCulture , DateTimeStyles.None, out dt))
                  {
                    NewCharacter.Born = dt;
                    item.NameRight = NewCharacter.Born.ToString("dd.MM.yyyy");
                  }
                  else
                  {
                    Notify.Error("Neplatný formát");
                  }
                }
            },
            new NativeMenuItem
            {
              Name = "Pohlaví",
              NameRight = $"←{availablePedModels[selectedModelIndex].Item2 ?? "---"}→",
              OnLeft = (item) =>
              {
                if (selectedModelIndex <= 0)
                  return;

                selectedModelIndex--;
              },
              OnRight = (item) =>
              {
                if (selectedModelIndex >= availablePedModels.Count - 1)
                  return;

                selectedModelIndex++;
              },
              OnRefresh = (item) =>
              {
                item.NameRight = $"←{availablePedModels[selectedModelIndex].Item2 ?? "---"}→";
                NewCharacter.Model = GetHashKey(availablePedModels[selectedModelIndex].Item1);
                UpdateModel();
              }
            },
            new NativeMenuItem
            {
              Name = "Upravit vzhled",
              GetSubMenu = getAppearanceSubmenu
            },
            new NativeMenuItem { IsUnselectable = true },
            new NativeMenuItem
            {
              Name = "Odejít",
              OnSelected = (item) =>
              {
                NativeMenuManager.UnlockMenu(charCreatorMenuName);
                IsInCharacterCreator = false;
                PedCam.EndCam();
                CharacterCaretaker.Instance.LoginSpawn(true, true);
              }
            },
            new NativeMenuItem
            {
              Name = "Potvrdit a uložit",
              OnSelected = (item) =>
              {
                CreateCharacter();
              }
            }   
          }
      };

      return menu;
    }

    private static async void UpdateModel()
    {
      RequestModel((uint)NewCharacter.Model);

      while (!HasModelLoaded((uint)NewCharacter.Model))
      {
        RequestModel((uint)NewCharacter.Model);

        await Delay(0);
      }

      SetPlayerModel(Game.Player.Handle, (uint)NewCharacter.Model);
      SetPedDefaultComponentVariation(Game.PlayerPed.Handle);
      await Delay(0);
      if (NewCharacter.Model == GetHashKey("mp_m_freemode_01") || NewCharacter.Model == GetHashKey("mp_f_freemode_01"))
        SkinManager.SetDefaultSkin(SkinManager.ClothesAll);
      SetModelAsNoLongerNeeded((uint)NewCharacter.Model);
      SkinManager.ApplySkin(NewCharacter.Skin);
      PedCam.SetCamZoomAndOffset(2f, 0f, 15f);
      PedCam.ResetForward();
    }

    public static async void EnterCreator()
    {
      NewCharacter = null;
      while (NativeMenuManager.LockInMenu(charCreatorMenuName) == false)
        await Delay(0);

      DoScreenFadeOut(500);

      await Delay(500);

      IsInCharacterCreator = true;

      while (NewCharacter == null)
        await Delay(0); //Az se otevre menu, tak se inicializuje

      TriggerServerEvent("ofw_instance:TransferToPrivateInstance");

      UpdateModel();
      
      while (!IsScreenFadedOut())
        await Delay(0);

      FreezePlayer(true);

      RequestCollisionAtCoord(CharCreatorPedPos.X, CharCreatorPedPos.Y, CharCreatorPedPos.Z);

      int time = GetGameTimer();

      while (!HasCollisionLoadedAroundEntity(Game.PlayerPed.Handle) && (GetGameTimer() - time) < 5000)
        await Delay(0);

      SetEntityCoords(Game.PlayerPed.Handle, CharCreatorPedPos.X, CharCreatorPedPos.Y, CharCreatorPedPos.Z, false, false, false, false);
      NetworkResurrectLocalPlayer(CharCreatorPedPos.X, CharCreatorPedPos.Y, CharCreatorPedPos.Z, CharCreatorPedPos.Heading, true, true);
      SetEntityHeading(Game.PlayerPed.Handle, CharCreatorPedPos.Heading);

      ClearPedTasksImmediately(Game.PlayerPed.Handle);
      //RemoveAllPedWeapons(Game.PlayerPed.Handle, true);
      //ClearPlayerWantedLevel(Game.Player.Handle);

      FreezePlayer(false);
      FreezeEntityPosition(Game.PlayerPed.Handle, true);

      await Delay(1000);

      if (IsScreenFadedOut())
        DoScreenFadeIn(500);

      while (!IsScreenFadedIn())
        await Delay(0);
    }

    public static async void CreateCharacter()
    {
      var result = await Callbacks.ServerAsyncCallbackToSyncWithErrorMessage("ofw_login:CreateCharacter", JsonConvert.SerializeObject(NewCharacter));

      if (result == true)
      {
        NativeMenuManager.UnlockMenu(charCreatorMenuName);
        IsInCharacterCreator = false;
        PedCam.EndCam();
        await CharacterCaretaker.Instance.LoginSpawn(false);
        NativeMenuManager.CloseAndUnlockMenu(charCreatorMenuName);
        Login.ReturnToLogin();
      }
    }

    public static NativeMenu getAppearanceSubmenu()
    {
      if (NewCharacter.Skin == null)
        NewCharacter.Skin = new Dictionary<string, int>();

      string[] values = SkinManager.AppearanceAll;

      Dictionary<string, MinMaxBag> minMax = new Dictionary<string, MinMaxBag>();
      bool isFemale = Game.PlayerPed.Model.Hash == GetHashKey("mp_f_freemode_01");

      foreach (var c in values)
      {
        if (!NewCharacter.Skin.ContainsKey(c))
          NewCharacter.Skin.Add(c, (isFemale && SkinManager.Components[c].DefaultFemale != null) ? SkinManager.Components[c].DefaultFemale.Value : SkinManager.Components[c].DefaultValue);
      }

      foreach (var c in values)
      {
        //musim si sice mimo pohlidat, ze tam hodnota parenta bude, ale kvuli poradi to projdu az kdyz mam vsechny hodnoty
        minMax.Add(c, new MinMaxBag(SkinManager.Components[c].MinValue, SkinManager.GetComponentMaxValue(c, SkinManager.Components[c].TextureOf != null ? NewCharacter.Skin[SkinManager.Components[c].TextureOf] : 0)));
      }

      NativeMenu menu = new NativeMenu
      {
        MenuTitle = "Vzhled",
        Items = new List<NativeMenuItem>()
      };

      foreach (var c in values)
      {
        Debug.WriteLine($"Comp: {c}");
        menu.Items.Add(new NativeMenuItem
        {
          Name = SkinManager.Components[c].Label,
          NameRight = $"←{NewCharacter.Skin[c]}→",
          OnLeft = (item) =>
          {
            if (NewCharacter.Skin[c] <= minMax[c].Min)
              return;

            NewCharacter.Skin[c]--;

            if (SkinManager.Components[c].TextureFrom != null)
            {
              string txFrom = SkinManager.Components[c].TextureFrom;
              minMax[txFrom].Max = SkinManager.GetComponentMaxValue(txFrom, NewCharacter.Skin[c]);

              if (NewCharacter.Skin[txFrom] > minMax[txFrom].Max)
              {
                NewCharacter.Skin[txFrom] = minMax[txFrom].Max;
              }
              if (NewCharacter.Skin[txFrom] < minMax[txFrom].Min)
              {
                NewCharacter.Skin[txFrom] = minMax[txFrom].Min;
              }
            }
            item.NameRight = $"←{NewCharacter.Skin[c]}→";
            SkinManager.ApplySkin(NewCharacter.Skin);
          },
          OnRight = (item) =>
          {
            if (NewCharacter.Skin[c] >= minMax[c].Max)
              return;

            NewCharacter.Skin[c]++;

            if (SkinManager.Components[c].TextureFrom != null)
            {
              string txFrom = SkinManager.Components[c].TextureFrom;
              minMax[txFrom].Max = SkinManager.GetComponentMaxValue(txFrom, NewCharacter.Skin[c]);

              if (NewCharacter.Skin[txFrom] > minMax[txFrom].Max)
              {
                NewCharacter.Skin[txFrom] = minMax[txFrom].Max;
              }
              if (NewCharacter.Skin[txFrom] < minMax[txFrom].Min)
              {
                NewCharacter.Skin[txFrom] = minMax[txFrom].Min;
              }
            }
            item.NameRight = $"←{NewCharacter.Skin[c]}→";
            SkinManager.ApplySkin(NewCharacter.Skin);
          },
          OnRefresh = (item) =>
          {
            if (SkinManager.Components[c].TextureOf != null)
              item.NameRight = $"←{NewCharacter.Skin[c]}→";
          },
          OnHover = () =>
          {
            PedCam.SetCamZoomAndOffset(SkinManager.Components[c].ZoomOffset, SkinManager.Components[c].CamOffset, 60f);
          }
        });
      }

      menu.Items.Add(new NativeMenuItem { Name = "Zpět", IsBack = true });
      if (menu.Items[0].OnHover != null)
        menu.Items[0].OnHover();

      return menu;
    }
  }
}
