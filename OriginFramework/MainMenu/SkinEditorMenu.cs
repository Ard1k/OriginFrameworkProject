using CitizenFX.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OriginFrameworkData.DataBags;
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

    public static NativeMenu GenerateMenu(string[] components)
    {
      if (SkinEditor.ItemDefinition.MaleSkin == null)
        SkinEditor.ItemDefinition.MaleSkin = new Dictionary<string, int>();

      if (SkinEditor.ItemDefinition.FemaleSkin == null)
        SkinEditor.ItemDefinition.FemaleSkin = new Dictionary<string, int>();

      if (SkinEditor.ItemDefinition.Color == null)
        SkinEditor.ItemDefinition.Color = new InventoryColor(0, 255, 255, 255);

      Dictionary<string, MinMaxBag> minMaxMale = new Dictionary<string, MinMaxBag>();
      Dictionary<string, MinMaxBag> minMaxFemale = new Dictionary<string, MinMaxBag>();

      foreach (var c in components)
      {
        if (!SkinEditor.ItemDefinition.MaleSkin.ContainsKey(c))
          SkinEditor.ItemDefinition.MaleSkin.Add(c, SkinManager.Components[c].DefaultValue);

        if (!SkinEditor.ItemDefinition.FemaleSkin.ContainsKey(c))
          SkinEditor.ItemDefinition.FemaleSkin.Add(c, (SkinManager.Components[c].DefaultFemale != null) ? SkinManager.Components[c].DefaultFemale.Value : SkinManager.Components[c].DefaultValue);
      }

      foreach (var c in components)
      {
        //musim si sice mimo pohlidat, ze tam hodnota parenta bude, ale kvuli poradi to projdu az kdyz mam vsechny hodnoty
        minMaxMale.Add(c, new MinMaxBag(SkinManager.Components[c].MinValue, SkinManager.GetComponentMaxValue(c, SkinManager.Components[c].TextureOf != null ? SkinEditor.ItemDefinition.MaleSkin[SkinManager.Components[c].TextureOf] : 0)));
        minMaxFemale.Add(c, new MinMaxBag(SkinManager.Components[c].MinValue, SkinManager.GetComponentMaxValue(c, SkinManager.Components[c].TextureOf != null ? SkinEditor.ItemDefinition.MaleSkin[SkinManager.Components[c].TextureOf] : 0)));
      }

      NativeMenu menu = new NativeMenu
      {
        MenuTitle = "Skin editor menu",
        Items = new List<NativeMenuItem>()
      };

      menu.Items.Add(new NativeMenuItem
      {
        Name = "Jméno itemu",
        NameRight = SkinEditor.ItemDefinition.Name,
        IsTextInput = true,
        TextInputMaxLength = 40,
        TextInputPrefill = SkinEditor.ItemDefinition.Name,
        TextInputRequest = "Zadejte jméno itemu",
        OnTextInput = (item, input) => { item.NameRight = input; SkinEditor.ItemDefinition.Name = input; }
      });

      menu.Items.Add(new NativeMenuItem
      {
        Name = "Textura",
        NameRight = SkinEditor.ItemDefinition.Texture ?? ItemsDefinitions.KnownClothesIcons.First(),
        OnLeft = (item) =>
        {
          int index = ItemsDefinitions.KnownClothesIcons.IndexOf(item.NameRight);
          if (index > 0)
          {
            item.NameRight = ItemsDefinitions.KnownClothesIcons[index - 1];
            SkinEditor.ItemDefinition.Texture = ItemsDefinitions.KnownClothesIcons[index - 1];
          }
        },
        OnRight = (item) =>
        {
          int index = ItemsDefinitions.KnownClothesIcons.IndexOf(item.NameRight);
          if (index < ItemsDefinitions.KnownClothesIcons.Count - 1)
          {
            item.NameRight = ItemsDefinitions.KnownClothesIcons[index + 1];
            SkinEditor.ItemDefinition.Texture = ItemsDefinitions.KnownClothesIcons[index + 1];
          }
        }
      });

      menu.Items.Add(new NativeMenuItem {
        Name = "Barva (kategorie)",
        NameRight = SkinEditor.ItemDefinition.Color.Label,
        OnLeft = (item) =>
        {
          int index = (int)SkinEditor.ItemDefinition.Color.ColorEnum;
          if (index > 0)
          {
            SkinEditor.ItemDefinition.Color.ColorEnum = (eInventoryColor)(index - 1);
            item.NameRight = SkinEditor.ItemDefinition.Color.Label;
          }
        },
        OnRight = (item) =>
        {
          int index = (int)SkinEditor.ItemDefinition.Color.ColorEnum;
          if (Enum.IsDefined(typeof(eInventoryColor), index + 1))
          {
            SkinEditor.ItemDefinition.Color.ColorEnum = (eInventoryColor)(index + 1);
            item.NameRight = SkinEditor.ItemDefinition.Color.Label;
          }
        }
      });

      menu.Items.Add(new NativeMenuItem
      {
        Name = "   Red",
        NameRight = SkinEditor.ItemDefinition.Color.R.ToString(),
        IsTextInput = true,
        TextInputMaxLength = 3,
        TextInputRequest = "Red",
        OnTextInput = (item, input) => 
        { 
          int val;
          if (Int32.TryParse(input, out val) && val >= 0 && val <= 255)
            SkinEditor.ItemDefinition.Color.R = val;
          else
            SkinEditor.ItemDefinition.Color.R = 255;

          item.NameRight = SkinEditor.ItemDefinition?.Color?.R.ToString();
        },
        OnLeft = (item) => 
        {
          if (SkinEditor.ItemDefinition.Color.R > 0)
          {
            SkinEditor.ItemDefinition.Color.R--;
            item.NameRight = SkinEditor.ItemDefinition.Color.R.ToString();
          }
        },
        OnRight = (item) =>
        {
          if (SkinEditor.ItemDefinition.Color.R < 255)
          {
            SkinEditor.ItemDefinition.Color.R++;
            item.NameRight = SkinEditor.ItemDefinition.Color.R.ToString();
          }
        }
      });

      menu.Items.Add(new NativeMenuItem
      {
        Name = "   Green",
        NameRight = SkinEditor.ItemDefinition.Color.G.ToString(),
        IsTextInput = true,
        TextInputMaxLength = 3,
        TextInputRequest = "Green",
        OnTextInput = (item, input) =>
        {
          int val;
          if (Int32.TryParse(input, out val) && val >= 0 && val <= 255)
            SkinEditor.ItemDefinition.Color.G = val;
          else
            SkinEditor.ItemDefinition.Color.G = 255;

          item.NameRight = SkinEditor.ItemDefinition?.Color?.G.ToString();
        },
        OnLeft = (item) =>
        {
          if (SkinEditor.ItemDefinition.Color.G > 0)
          {
            SkinEditor.ItemDefinition.Color.G--;
            item.NameRight = SkinEditor.ItemDefinition.Color.G.ToString();
          }
        },
        OnRight = (item) =>
        {
          if (SkinEditor.ItemDefinition.Color.G < 255)
          {
            SkinEditor.ItemDefinition.Color.G++;
            item.NameRight = SkinEditor.ItemDefinition.Color.G.ToString();
          }
        }
      });

      menu.Items.Add(new NativeMenuItem
      {
        Name = "   Blue",
        NameRight = SkinEditor.ItemDefinition.Color.B.ToString(),
        IsTextInput = true,
        TextInputMaxLength = 3,
        TextInputRequest = "Blue",
        OnTextInput = (item, input) =>
        {
          int val;
          if (Int32.TryParse(input, out val) && val >= 0 && val <= 255)
            SkinEditor.ItemDefinition.Color.B = val;
          else
            SkinEditor.ItemDefinition.Color.B = 255;

          item.NameRight = SkinEditor.ItemDefinition?.Color?.B.ToString();
        },
        OnLeft = (item) =>
        {
          if (SkinEditor.ItemDefinition.Color.B > 0)
          {
            SkinEditor.ItemDefinition.Color.B--;
            item.NameRight = SkinEditor.ItemDefinition.Color.B.ToString();
          }
        },
        OnRight = (item) =>
        {
          if (SkinEditor.ItemDefinition.Color.B < 255)
          {
            SkinEditor.ItemDefinition.Color.B++;
            item.NameRight = SkinEditor.ItemDefinition.Color.B.ToString();
          }
        }
      });

      menu.Items.Add(new NativeMenuItem { IsUnselectable = true });

      foreach (var c in components)
      {
        menu.Items.Add(new NativeMenuItem {
          Name = SkinManager.Components[c].Label,
          NameRight = $"←{SkinEditor.ItemDefinition.MaleSkin[c]}→",
          OnLeft = (item) =>
          {
            if (SkinEditor.ItemDefinition.MaleSkin[c] <= minMaxMale[c].Min)
              return;

            SkinEditor.ItemDefinition.MaleSkin[c]--;

            if (SkinManager.Components[c].TextureFrom != null)
            {
              string txFrom = SkinManager.Components[c].TextureFrom;
              minMaxMale[txFrom].Max = SkinManager.GetComponentMaxValue(txFrom, SkinEditor.ItemDefinition.MaleSkin[c]);

              if (SkinEditor.ItemDefinition.MaleSkin[txFrom] > minMaxMale[txFrom].Max)
              {
                SkinEditor.ItemDefinition.MaleSkin[txFrom] = minMaxMale[txFrom].Max;
              }
              if (SkinEditor.ItemDefinition.MaleSkin[txFrom] < minMaxMale[txFrom].Min)
              {
                SkinEditor.ItemDefinition.MaleSkin[txFrom] = minMaxMale[txFrom].Min;
              }
            }
            item.NameRight = $"←{SkinEditor.ItemDefinition.MaleSkin[c]}→";
            SkinManager.ApplySkin(SkinEditor.ItemDefinition.MaleSkin);
          },
          OnRight = (item) =>
          {
            if (SkinEditor.ItemDefinition.MaleSkin[c] >= minMaxMale[c].Max)
              return;

            SkinEditor.ItemDefinition.MaleSkin[c]++;

            if (SkinManager.Components[c].TextureFrom != null)
            {
              string txFrom = SkinManager.Components[c].TextureFrom;
              minMaxMale[txFrom].Max = SkinManager.GetComponentMaxValue(txFrom, SkinEditor.ItemDefinition.MaleSkin[c]);

              if (SkinEditor.ItemDefinition.MaleSkin[txFrom] > minMaxMale[txFrom].Max)
              {
                SkinEditor.ItemDefinition.MaleSkin[txFrom] = minMaxMale[txFrom].Max;
              }
              if (SkinEditor.ItemDefinition.MaleSkin[txFrom] < minMaxMale[txFrom].Min)
              {
                SkinEditor.ItemDefinition.MaleSkin[txFrom] = minMaxMale[txFrom].Min;
              }
            }
            item.NameRight = $"←{SkinEditor.ItemDefinition.MaleSkin[c]}→";
            SkinManager.ApplySkin(SkinEditor.ItemDefinition.MaleSkin);
          },
          OnRefresh = (item) =>
          {
            if (SkinManager.Components[c].TextureOf != null)
              item.NameRight = $"←{SkinEditor.ItemDefinition.MaleSkin[c]}→";
          },
          OnHover = () => { SkinEditor.CurrentPedModel = "mp_m_freemode_01"; },
        });
      }

      menu.Items.Add(new NativeMenuItem { IsUnselectable = true });

      foreach (var c in components)
      {
        menu.Items.Add(new NativeMenuItem
        {
          Name = SkinManager.Components[c].Label,
          NameRight = $"←{SkinEditor.ItemDefinition.FemaleSkin[c]}→",
          OnLeft = (item) =>
          {
            if (SkinEditor.ItemDefinition.FemaleSkin[c] <= minMaxFemale[c].Min)
              return;

            SkinEditor.ItemDefinition.FemaleSkin[c]--;

            if (SkinManager.Components[c].TextureFrom != null)
            {
              string txFrom = SkinManager.Components[c].TextureFrom;
              minMaxFemale[txFrom].Max = SkinManager.GetComponentMaxValue(txFrom, SkinEditor.ItemDefinition.FemaleSkin[c]);

              if (SkinEditor.ItemDefinition.FemaleSkin[txFrom] > minMaxFemale[txFrom].Max)
              {
                SkinEditor.ItemDefinition.FemaleSkin[txFrom] = minMaxFemale[txFrom].Max;
              }
              if (SkinEditor.ItemDefinition.FemaleSkin[txFrom] < minMaxFemale[txFrom].Min)
              {
                SkinEditor.ItemDefinition.FemaleSkin[txFrom] = minMaxFemale[txFrom].Min;
              }
            }
            item.NameRight = $"←{SkinEditor.ItemDefinition.FemaleSkin[c]}→";
            SkinManager.ApplySkin(SkinEditor.ItemDefinition.FemaleSkin);
          },
          OnRight = (item) =>
          {
            if (SkinEditor.ItemDefinition.FemaleSkin[c] >= minMaxFemale[c].Max)
              return;

            SkinEditor.ItemDefinition.FemaleSkin[c]++;

            if (SkinManager.Components[c].TextureFrom != null)
            {
              string txFrom = SkinManager.Components[c].TextureFrom;
              minMaxFemale[txFrom].Max = SkinManager.GetComponentMaxValue(txFrom, SkinEditor.ItemDefinition.FemaleSkin[c]);

              if (SkinEditor.ItemDefinition.FemaleSkin[txFrom] > minMaxFemale[txFrom].Max)
              {
                SkinEditor.ItemDefinition.FemaleSkin[txFrom] = minMaxFemale[txFrom].Max;
              }
              if (SkinEditor.ItemDefinition.FemaleSkin[txFrom] < minMaxFemale[txFrom].Min)
              {
                SkinEditor.ItemDefinition.FemaleSkin[txFrom] = minMaxFemale[txFrom].Min;
              }
            }
            item.NameRight = $"←{SkinEditor.ItemDefinition.FemaleSkin[c]}→";
            SkinManager.ApplySkin(SkinEditor.ItemDefinition.FemaleSkin);
          },
          OnRefresh = (item) =>
          {
            if (SkinManager.Components[c].TextureOf != null)
              item.NameRight = $"←{SkinEditor.ItemDefinition.FemaleSkin[c]}→";
          },
          OnHover = () => { SkinEditor.CurrentPedModel = "mp_f_freemode_01"; },
        });
      }

      menu.Items.Add(new NativeMenuItem { IsUnselectable = true });
      menu.Items.Add(new NativeMenuItem 
      {
        Name = "Kopírovat do schránky",
        OnSelected = (item) =>
        {
          Misc.CopyStringToClipboard(JsonConvert.SerializeObject(SkinEditor.ItemDefinition));
        }
      });
      menu.Items.Add(new NativeMenuItem
      {
        Name = "Uložit pouze pro ženy",
        OnSelected = (item) =>
        {
          SkinEditor.SendItemToServer(false, true);
        }
      });
      menu.Items.Add(new NativeMenuItem
      {
        Name = "Uložit pouze pro muže",
        OnSelected = (item) =>
        {
          SkinEditor.SendItemToServer(true, false);
        }
      });
      menu.Items.Add(new NativeMenuItem
      {
        Name = "Uložit pro obě pohlaví",
        OnSelected = (item) =>
        {
          SkinEditor.SendItemToServer(true, true);
        }
      });
      menu.Items.Add(new NativeMenuItem
      {
        Name = "Zpět",
        OnSelected = (item) =>
        {
          SkinEditor.EnterEditor();
        },
        IsClose = true,
      });

      return menu;
    }
  }
}
