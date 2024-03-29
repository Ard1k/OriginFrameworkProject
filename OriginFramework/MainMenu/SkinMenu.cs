﻿using CitizenFX.Core;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace OriginFramework.Menus
{
  public static class SkinMenu
  {
    public static NativeMenu GenerateMenu(string[] components, Dictionary<string, int> values)
    {
      if (values == null)
        values = new Dictionary<string, int>();

      Dictionary<string, MinMaxBag> minMax = new Dictionary<string, MinMaxBag>();
      bool isFemale = Game.PlayerPed.Model.Hash == GetHashKey("mp_f_freemode_01");

      foreach (var c in components)
      {
        if (!values.ContainsKey(c))
          values.Add(c, (isFemale && SkinManager.Components[c].DefaultFemale != null) ? SkinManager.Components[c].DefaultFemale.Value : SkinManager.Components[c].DefaultValue);
      }

      foreach (var c in components)
      {
        //musim si sice mimo pohlidat, ze tam hodnota parenta bude, ale kvuli poradi to projdu az kdyz mam vsechny hodnoty
        minMax.Add(c, new MinMaxBag(SkinManager.Components[c].MinValue, SkinManager.GetComponentMaxValue(c, SkinManager.Components[c].TextureOf != null ? values[SkinManager.Components[c].TextureOf] : 0)));
      }

      NativeMenu menu = new NativeMenu
      {
        MenuTitle = "Skin menu",
        Items = new List<NativeMenuItem>()
      };

      foreach (var c in components)
      {
        menu.Items.Add(new NativeMenuItem {
          Name = SkinManager.Components[c].Label,
          NameRight = $"←{values[c]}→",
          OnLeft = (item) =>
          {
            if (values[c] <= minMax[c].Min)
              return;

            values[c]--;

            if (SkinManager.Components[c].TextureFrom != null)
            {
              string txFrom = SkinManager.Components[c].TextureFrom;
              var mmBag = minMax[txFrom];
              mmBag.Max = SkinManager.GetComponentMaxValue(txFrom, values[c]);
              minMax[txFrom] = mmBag;

              if (values[txFrom] > minMax[txFrom].Max)
              {
                values[txFrom] = minMax[txFrom].Max;
              }
              if (values[txFrom] < minMax[txFrom].Min)
              {
                values[txFrom] = minMax[txFrom].Min;
              }
            }
            item.NameRight = $"←{values[c]}→";
            SkinManager.ApplySkin(values);
          },
          OnRight = (item) =>
          {
            if (values[c] >= minMax[c].Max)
              return;

            values[c]++;

            if (SkinManager.Components[c].TextureFrom != null)
            {
              string txFrom = SkinManager.Components[c].TextureFrom;
              var mmBag = minMax[txFrom];
              mmBag.Max = SkinManager.GetComponentMaxValue(txFrom, values[c]);
              minMax[txFrom] = mmBag;

              if (values[txFrom] > minMax[txFrom].Max)
              {
                values[txFrom] = minMax[txFrom].Max;
              }
              if (values[txFrom] < minMax[txFrom].Min)
              {
                values[txFrom] = minMax[txFrom].Min;
              }
            }
            item.NameRight = $"←{values[c]}→";
            SkinManager.ApplySkin(values);
          },
          OnRefresh = (item) =>
          {
            if (SkinManager.Components[c].TextureOf != null)
              item.NameRight = $"←{values[c]}→";
          }
        });
      }

      return menu;
    }
  }
}
