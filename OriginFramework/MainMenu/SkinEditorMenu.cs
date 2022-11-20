using CitizenFX.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework.Menus
{
  public static class SkinEditorMenu
  {
    private class MinMaxBag
    {
      public int Min { get; set; }
      public int Max { get; set; }

      public MinMaxBag(int min, int max)
      {
        Min = min;
        Max = max;
      }
    }

    public static async void EnsurePedModel(string modelRequest, Dictionary<string, int> values)
    {
      int requestedHash = GetHashKey(modelRequest);
      if (Game.PlayerPed.Model.Hash == requestedHash)
        return;

      uint model = (uint)requestedHash;

      RequestModel(model);
      while (!HasModelLoaded(model))
      {
        RequestModel(model);

        await BaseScript.Delay(0);
      }

      SetPlayerModel(Game.Player.Handle, model);
      SetPedDefaultComponentVariation(Game.PlayerPed.Handle);

      SkinManager.SetDefaultSkin(SkinManager.ClothesAll);
      SkinManager.ApplySkin(values);
      SetModelAsNoLongerNeeded(model);
    }

    public static NativeMenu GenerateMenu(string[] components, Dictionary<string, int> valuesMale, Dictionary<string, int> valuesFemale)
    {
      if (valuesMale == null)
        valuesMale = new Dictionary<string, int>();

      if (valuesFemale == null)
        valuesFemale = new Dictionary<string, int>();

      Dictionary<string, MinMaxBag> minMaxMale = new Dictionary<string, MinMaxBag>();
      Dictionary<string, MinMaxBag> minMaxFemale = new Dictionary<string, MinMaxBag>();

      foreach (var c in components)
      {
        if (!valuesMale.ContainsKey(c))
          valuesMale.Add(c, SkinManager.Components[c].DefaultValue);

        if (!valuesFemale.ContainsKey(c))
          valuesFemale.Add(c, (SkinManager.Components[c].DefaultFemale != null) ? SkinManager.Components[c].DefaultFemale.Value : SkinManager.Components[c].DefaultValue);
      }

      foreach (var c in components)
      {
        //musim si sice mimo pohlidat, ze tam hodnota parenta bude, ale kvuli poradi to projdu az kdyz mam vsechny hodnoty
        minMaxMale.Add(c, new MinMaxBag(SkinManager.Components[c].MinValue, SkinManager.GetComponentMaxValue(c, SkinManager.Components[c].TextureOf != null ? valuesMale[SkinManager.Components[c].TextureOf] : 0)));
        minMaxFemale.Add(c, new MinMaxBag(SkinManager.Components[c].MinValue, SkinManager.GetComponentMaxValue(c, SkinManager.Components[c].TextureOf != null ? valuesFemale[SkinManager.Components[c].TextureOf] : 0)));
      }

      NativeMenu menu = new NativeMenu
      {
        MenuTitle = "Skin editor menu",
        Items = new List<NativeMenuItem>()
      };

      foreach (var c in components)
      {
        menu.Items.Add(new NativeMenuItem {
          Name = SkinManager.Components[c].Label,
          NameRight = $"←{valuesMale[c]}→",
          OnLeft = (item) =>
          {
            if (valuesMale[c] <= minMaxMale[c].Min)
              return;

            valuesMale[c]--;

            if (SkinManager.Components[c].TextureFrom != null)
            {
              string txFrom = SkinManager.Components[c].TextureFrom;
              minMaxMale[txFrom].Max = SkinManager.GetComponentMaxValue(txFrom, valuesMale[c]);

              if (valuesMale[txFrom] > minMaxMale[txFrom].Max)
              {
                valuesMale[txFrom] = minMaxMale[txFrom].Max;
              }
              if (valuesMale[txFrom] < minMaxMale[txFrom].Min)
              {
                valuesMale[txFrom] = minMaxMale[txFrom].Min;
              }
            }
            item.NameRight = $"←{valuesMale[c]}→";
            SkinManager.ApplySkin(valuesMale);
          },
          OnRight = (item) =>
          {
            if (valuesMale[c] >= minMaxMale[c].Max)
              return;

            valuesMale[c]++;

            if (SkinManager.Components[c].TextureFrom != null)
            {
              string txFrom = SkinManager.Components[c].TextureFrom;
              minMaxMale[txFrom].Max = SkinManager.GetComponentMaxValue(txFrom, valuesMale[c]);

              if (valuesMale[txFrom] > minMaxMale[txFrom].Max)
              {
                valuesMale[txFrom] = minMaxMale[txFrom].Max;
              }
              if (valuesMale[txFrom] < minMaxMale[txFrom].Min)
              {
                valuesMale[txFrom] = minMaxMale[txFrom].Min;
              }
            }
            item.NameRight = $"←{valuesMale[c]}→";
            SkinManager.ApplySkin(valuesMale);
          },
          OnRefresh = (item) =>
          {
            if (SkinManager.Components[c].TextureOf != null)
              item.NameRight = $"←{valuesMale[c]}→";
          },
          OnHover = () => { EnsurePedModel("mp_m_freemode_01", valuesMale); },
        });
      }

      menu.Items.Add(new NativeMenuItem { IsUnselectable = true });

      foreach (var c in components)
      {
        menu.Items.Add(new NativeMenuItem
        {
          Name = SkinManager.Components[c].Label,
          NameRight = $"←{valuesFemale[c]}→",
          OnLeft = (item) =>
          {
            if (valuesFemale[c] <= minMaxFemale[c].Min)
              return;

            valuesFemale[c]--;

            if (SkinManager.Components[c].TextureFrom != null)
            {
              string txFrom = SkinManager.Components[c].TextureFrom;
              minMaxFemale[txFrom].Max = SkinManager.GetComponentMaxValue(txFrom, valuesFemale[c]);

              if (valuesFemale[txFrom] > minMaxFemale[txFrom].Max)
              {
                valuesFemale[txFrom] = minMaxFemale[txFrom].Max;
              }
              if (valuesFemale[txFrom] < minMaxFemale[txFrom].Min)
              {
                valuesFemale[txFrom] = minMaxFemale[txFrom].Min;
              }
            }
            item.NameRight = $"←{valuesFemale[c]}→";
            SkinManager.ApplySkin(valuesFemale);
          },
          OnRight = (item) =>
          {
            if (valuesFemale[c] >= minMaxFemale[c].Max)
              return;

            valuesFemale[c]++;

            if (SkinManager.Components[c].TextureFrom != null)
            {
              string txFrom = SkinManager.Components[c].TextureFrom;
              minMaxFemale[txFrom].Max = SkinManager.GetComponentMaxValue(txFrom, valuesFemale[c]);

              if (valuesFemale[txFrom] > minMaxFemale[txFrom].Max)
              {
                valuesFemale[txFrom] = minMaxFemale[txFrom].Max;
              }
              if (valuesFemale[txFrom] < minMaxFemale[txFrom].Min)
              {
                valuesFemale[txFrom] = minMaxFemale[txFrom].Min;
              }
            }
            item.NameRight = $"←{valuesFemale[c]}→";
            SkinManager.ApplySkin(valuesFemale);
          },
          OnRefresh = (item) =>
          {
            if (SkinManager.Components[c].TextureOf != null)
              item.NameRight = $"←{valuesFemale[c]}→";
          },
          OnHover = () => { EnsurePedModel("mp_f_freemode_01", valuesFemale); },
        });
      }

      menu.Items.Add(new NativeMenuItem { IsUnselectable = true });
      menu.Items.Add(new NativeMenuItem 
      {
        Name = "Kopírovat do schránky",
        OnSelected = (item) =>
        {
          var sb = new StringBuilder();
          sb.AppendLine("MaleSkin = new Dictionary<string, int>\r\n        {");
          foreach (var v in valuesMale)
            sb.AppendLine($"          {{ \"{v.Key}\", {v.Value} }},");
          sb.AppendLine("        },");

          sb.AppendLine("FemaleSkin = new Dictionary<string, int>\r\n        {");
          foreach (var v in valuesFemale)
            sb.AppendLine($"          {{ \"{v.Key}\", {v.Value} }},");
          sb.AppendLine("        },");

          Misc.CopyStringToClipboard(sb.ToString());
        }
      });

      return menu;
    }
  }
}
