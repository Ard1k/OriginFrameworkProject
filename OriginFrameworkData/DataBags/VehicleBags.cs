﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace OriginFrameworkData.DataBags
{
  public class VehTuningTypeDefinition
  {
    public int TuningType { get; set; }
    public int Price { get; set; }
    public int PriceItemId { get; set; }
    public string Name { get; set; }
    public bool IsToggle { get; set; }
    public bool IsDisabled { get; set; }

    public static Dictionary<int, VehTuningTypeDefinition> Defined { get; set; } = new Dictionary<int, VehTuningTypeDefinition>
    {
      { 0, new VehTuningTypeDefinition { TuningType = 0, Name = "Spoilery", Price = 100, PriceItemId = 1 } },
      { 1, new VehTuningTypeDefinition { TuningType = 1, Name = "Přední nárazník", Price = 100, PriceItemId = 1 } },
      { 2, new VehTuningTypeDefinition { TuningType = 2, Name = "Zadní nárazník", Price = 100, PriceItemId = 1 } },
      { 3, new VehTuningTypeDefinition { TuningType = 3, Name = "Bočnice", Price = 100, PriceItemId = 1 } },
      { 4, new VehTuningTypeDefinition { TuningType = 4, Name = "Výfuk", Price = 100, PriceItemId = 1 } },
      { 5, new VehTuningTypeDefinition { TuningType = 5, Name = "Klec", Price = 100, PriceItemId = 1 } },
      { 6, new VehTuningTypeDefinition { TuningType = 6, Name = "Přední maska", Price = 100, PriceItemId = 1 } },
      { 7, new VehTuningTypeDefinition { TuningType = 7, Name = "Kapota", Price = 100, PriceItemId = 1 } },
      { 8, new VehTuningTypeDefinition { TuningType = 8, Name = "Doplňky 1", Price = 100, PriceItemId = 1 } },
      { 9, new VehTuningTypeDefinition { TuningType = 9, Name = "Doplňky 2", Price = 100, PriceItemId = 1 } },
      { 10, new VehTuningTypeDefinition { TuningType = 10, Name = "Střecha", Price = 100, PriceItemId = 1 } },
      { 11, new VehTuningTypeDefinition { TuningType = 11, Name = "Motor", Price = 100, PriceItemId = 1 } },
      { 12, new VehTuningTypeDefinition { TuningType = 12, Name = "Brzdy", Price = 100, PriceItemId = 1 } },
      { 13, new VehTuningTypeDefinition { TuningType = 13, Name = "Převodovka", Price = 100, PriceItemId = 1 } },
      { 14, new VehTuningTypeDefinition { TuningType = 14, Name = "Klakson", Price = 100, PriceItemId = 1 } },
      { 15, new VehTuningTypeDefinition { TuningType = 15, Name = "Podvozek", Price = 100, PriceItemId = 1 } },
      { 16, new VehTuningTypeDefinition { TuningType = 16, Name = "Pancíř", Price = 100, PriceItemId = 1, IsDisabled = true } },
      { 17, new VehTuningTypeDefinition { TuningType = 17, Name = "Nitro", Price = 100, PriceItemId = 1, IsDisabled = true, IsToggle = true } },
      { 18, new VehTuningTypeDefinition { TuningType = 18, Name = "Turbo", Price = 100, PriceItemId = 1, IsToggle = true} },
      { 19, new VehTuningTypeDefinition { TuningType = 19, Name = "Subwoofer", Price = 100, PriceItemId = 1, IsDisabled = true } },
      { 20, new VehTuningTypeDefinition { TuningType = 20, Name = "Pneu pigment", Price = 100, PriceItemId = 1 } },
      { 21, new VehTuningTypeDefinition { TuningType = 21, Name = "Hydraulika", Price = 100, PriceItemId = 1 } },
      { 22, new VehTuningTypeDefinition { TuningType = 22, Name = "Xenony", Price = 100, PriceItemId = 1, IsToggle = true } },
      { 23, new VehTuningTypeDefinition { TuningType = 23, Name = "Kola", Price = 100, PriceItemId = 1 } },
      { 24, new VehTuningTypeDefinition { TuningType = 24, Name = "Zadní kolo", Price = 100, PriceItemId = 1 } },
      { 25, new VehTuningTypeDefinition { TuningType = 25, Name = "SPZ rámeček", Price = 100, PriceItemId = 1 } },
      { 26, new VehTuningTypeDefinition { TuningType = 26, Name = "Custom SPZ", Price = 100, PriceItemId = 1 } },
      { 27, new VehTuningTypeDefinition { TuningType = 27, Name = "Zdobení 1", Price = 100, PriceItemId = 1 } },
      { 28, new VehTuningTypeDefinition { TuningType = 28, Name = "Ornamenty", Price = 100, PriceItemId = 1 } },
      { 29, new VehTuningTypeDefinition { TuningType = 29, Name = "Palubní deska", Price = 100, PriceItemId = 1 } },
      { 30, new VehTuningTypeDefinition { TuningType = 30, Name = "Budíky", Price = 100, PriceItemId = 1 } },
      { 31, new VehTuningTypeDefinition { TuningType = 31, Name = "Repráky ve dveřích", Price = 100, PriceItemId = 1 } },
      { 32, new VehTuningTypeDefinition { TuningType = 32, Name = "Sedačky", Price = 100, PriceItemId = 1 } },
      { 33, new VehTuningTypeDefinition { TuningType = 33, Name = "Volant", Price = 100, PriceItemId = 1 } },
      { 34, new VehTuningTypeDefinition { TuningType = 34, Name = "Řadička", Price = 100, PriceItemId = 1 } },
      { 35, new VehTuningTypeDefinition { TuningType = 35, Name = "Plaketa", Price = 100, PriceItemId = 1 } },
      { 36, new VehTuningTypeDefinition { TuningType = 36, Name = "Stereo", Price = 100, PriceItemId = 1 } },
      { 37, new VehTuningTypeDefinition { TuningType = 37, Name = "Kufr", Price = 100, PriceItemId = 1 } },
      { 38, new VehTuningTypeDefinition { TuningType = 38, Name = "Hydraulika 2", Price = 100, PriceItemId = 1 } },
      { 39, new VehTuningTypeDefinition { TuningType = 39, Name = "Blok motoru", Price = 100, PriceItemId = 1 } },
      { 40, new VehTuningTypeDefinition { TuningType = 40, Name = "Vzduchový filtr", Price = 100, PriceItemId = 1 } },
      { 41, new VehTuningTypeDefinition { TuningType = 41, Name = "Vzpěry", Price = 100, PriceItemId = 1 } },
      { 42, new VehTuningTypeDefinition { TuningType = 42, Name = "Kryty", Price = 100, PriceItemId = 1 } },
      { 43, new VehTuningTypeDefinition { TuningType = 43, Name = "Antény", Price = 100, PriceItemId = 1 } },
      { 44, new VehTuningTypeDefinition { TuningType = 44, Name = "Zdobení 2", Price = 100, PriceItemId = 1 } },
      { 45, new VehTuningTypeDefinition { TuningType = 45, Name = "Nádrž", Price = 100, PriceItemId = 1 } },
      { 46, new VehTuningTypeDefinition { TuningType = 46, Name = "Levé dveře", Price = 100, PriceItemId = 1 } },
      { 47, new VehTuningTypeDefinition { TuningType = 47, Name = "Pravé dveře", Price = 100, PriceItemId = 1 } },
      { 48, new VehTuningTypeDefinition { TuningType = 48, Name = "Polepy", Price = 100, PriceItemId = 1 } },
      { 49, new VehTuningTypeDefinition { TuningType = 49, Name = "Lightbar", Price = 100, PriceItemId = 1 } },
    };

    public static Dictionary<string, VehTuningTypeDefinition> DefinedSpecial { get; set; } = new Dictionary<string, VehTuningTypeDefinition>
    {
      { "color0", new VehTuningTypeDefinition {Name = "Primární barva laku", Price = 100, PriceItemId = 1 } },
      { "color1", new VehTuningTypeDefinition {Name = "Sekundární barva laku", Price = 100, PriceItemId = 1 } },
      { "color2", new VehTuningTypeDefinition {Name = "Perleť", Price = 100, PriceItemId = 1 } },
      { "colorw", new VehTuningTypeDefinition {Name = "Barva kol", Price = 100, PriceItemId = 1 } },
      { "customtires", new VehTuningTypeDefinition {Name = "Polepy na kola", Price = 100, PriceItemId = 1, IsToggle = true } },
    };

    public int ComputeUpgradePrice(int model, int selectedIndex)
    {
      double price = Price;

      return (int)Math.Floor(price);
    }
  }

  public class VehiclePropertiesBag
  {
    public int model { get; set; }
    public string plate { get; set; }
    public int? plateIndex { get; set; }
    public float? bodyHealth { get; set; }
    public float? engineHealth { get; set; }
    public float? fuelLevel { get; set; }
    public float? dirtLevel { get; set; }
    public int? color1 { get; set; }
    public int? color2 { get; set; }
    public int? pearlescentColor { get; set; }
    public int? wheelColor { get; set; }
    public int? wheels { get; set; }
    public bool? customTires { get; set; }
    public int? windowTint { get; set; }
    public int? xenonColor { get; set; }
    public List<bool> neonEnabled { get; set; }
    public List<int> neonColor { get; set; }
    public Dictionary<string, bool> extras { get; set; }
    public List<int> tyreSmokeColor { get; set; }

    public object[] tunings = new object[50];

    public bool[] windows { get; set; }
    public bool[] tyres { get; set; }
    public bool[] doors { get; set; }

    public void Substract(VehiclePropertiesBag subVP)
    {
      if (subVP == null)
        return;

      if (subVP.color1 != null) color1 = null;
      if (subVP.color2 != null) color2 = null;
      if (subVP.pearlescentColor != null) pearlescentColor = null;
      if (subVP.wheelColor != null) wheelColor = null;
      if (subVP.wheels != null) wheels = null;
      if (subVP.customTires != null) customTires = null;
      if (subVP.windowTint != null) windowTint = null;
      if (subVP.xenonColor != null) xenonColor = null;
      if (subVP.neonEnabled != null) neonEnabled = null;
      if (subVP.neonColor != null) neonColor = null;
      if (subVP.extras != null) extras = null;
      if (subVP.tyreSmokeColor != null) tyreSmokeColor = null;
      if (subVP.tunings != null && tunings != null)
      {
        for (int i = 0; i < subVP.tunings.Length; i++)
        {
          if (subVP.tunings[i] != null && tunings.Length >= i + 1)
            tunings[i] = null;
        }
      }
    }

    public void FillNulls(VehiclePropertiesBag subVP)
    {
      if (subVP == null)
        return;

      if (color1 == null) color1 = subVP.color1;
      if (color2 == null) color2 = subVP.color2;
      if (pearlescentColor == null) pearlescentColor = subVP.pearlescentColor;
      if (wheelColor == null) wheelColor = subVP.wheelColor;
      if (wheels == null) wheels = subVP.wheels;
      if (customTires == null) customTires = subVP.customTires;
      if (windowTint == null) windowTint = subVP.windowTint;
      if (xenonColor == null) xenonColor = subVP.xenonColor;
      if (neonEnabled == null) neonEnabled = subVP.neonEnabled;
      if (neonColor == null) neonColor = subVP.neonColor;
      if (extras == null) extras = subVP.extras;
      if (tyreSmokeColor == null) tyreSmokeColor = subVP.tyreSmokeColor;
      if (subVP.tunings != null && tunings != null)
      {
        for (int i = 0; i < subVP.tunings.Length; i++)
        {
          if (tunings.Length >= i + 1 && tunings[i] == null)
            tunings[i] = subVP.tunings[i];
        }
      }
    }

    public void Add(VehiclePropertiesBag subVP)
    {
      if (subVP == null)
        return;

      if (subVP.color1 != null) color1 = subVP.color1;
      if (subVP.color2 != null) color2 = subVP.color2;
      if (subVP.pearlescentColor != null) pearlescentColor = subVP.pearlescentColor;
      if (subVP.wheelColor != null) wheelColor = subVP.wheelColor;
      if (subVP.wheels != null) wheels = subVP.wheels;
      if (subVP.customTires != null) customTires = subVP.customTires;
      if (subVP.windowTint != null) windowTint = subVP.windowTint;
      if (subVP.xenonColor != null) xenonColor = subVP.xenonColor;
      if (subVP.neonEnabled != null) neonEnabled = subVP.neonEnabled;
      if (subVP.neonColor != null) neonColor = subVP.neonColor;
      if (subVP.extras != null) extras = subVP.extras;
      if (subVP.tyreSmokeColor != null) tyreSmokeColor = subVP.tyreSmokeColor;
      if (subVP.tunings != null && tunings != subVP.tunings)
      {
        for (int i = 0; i < subVP.tunings.Length; i++)
        {
          if (subVP.tunings[i] != null && tunings.Length >= i + 1)
            tunings[i] = subVP.tunings[i];
        }
      }
    }

    public Dictionary<int, int> ComputeUpgradePrice()
    {
      var prices = new Dictionary<int, int>();
      
      if (color1 != null)
      {
        var def = VehTuningTypeDefinition.DefinedSpecial["color0"];
        var price = def.ComputeUpgradePrice(model, color1.Value);
        if (prices.ContainsKey(def.PriceItemId))
          prices[def.PriceItemId] += price;
        else
          prices.Add(def.PriceItemId, price);
      }
      if (color2 != null)
      {
        var def = VehTuningTypeDefinition.DefinedSpecial["color1"];
        var price = def.ComputeUpgradePrice(model, color2.Value);
        if (prices.ContainsKey(def.PriceItemId))
          prices[def.PriceItemId] += price;
        else
          prices.Add(def.PriceItemId, price);
      }
      if (pearlescentColor != null) 
      {
        var def = VehTuningTypeDefinition.DefinedSpecial["color2"];
        var price = def.ComputeUpgradePrice(model, pearlescentColor.Value);
        if (prices.ContainsKey(def.PriceItemId))
          prices[def.PriceItemId] += price;
        else
          prices.Add(def.PriceItemId, price);
      }
      if (wheelColor != null) 
      {
        var def = VehTuningTypeDefinition.DefinedSpecial["colorw"];
        var price = def.ComputeUpgradePrice(model, wheelColor.Value);
        if (prices.ContainsKey(def.PriceItemId))
          prices[def.PriceItemId] += price;
        else
          prices.Add(def.PriceItemId, price);
      }
      if (customTires != null)
      {
        var def = VehTuningTypeDefinition.DefinedSpecial["customtires"];
        var price = def.ComputeUpgradePrice(model, customTires.Value ? 1 : 0);
        if (prices.ContainsKey(def.PriceItemId))
          prices[def.PriceItemId] += price;
        else
          prices.Add(def.PriceItemId, price);
      }

      if (tunings != null)
      {
        for (int i = 0; i < tunings.Length; i++)
        {
          if (tunings[i] != null)
          {
            var def = VehTuningTypeDefinition.Defined[i];
            var price = def.ComputeUpgradePrice(model, def.IsToggle ? 0 : Convert.ToInt32(tunings[i]));
            if (prices.ContainsKey(def.PriceItemId))
              prices[def.PriceItemId] += price;
            else
              prices.Add(def.PriceItemId, price);
          }
        }
      }

      return prices;
    }

    public static VehiclePropertiesBag InitializeNew(int model, string plate)
    {
      var prop = new VehiclePropertiesBag();
      prop.model = model;
      prop.plate = plate;
      return prop;
    }
  }

  public class VehColor
  {
    public int ColorIndex { get; set; }
    public string ColorName { get; set; }
    public eVehColorCategory Category { get; set; }
    public bool IsUnused { get; set; }

    public static VehColor[] Defined = new VehColor[] {
      new VehColor { Category = eVehColorCategory.Černá, ColorIndex = 0, ColorName = "Černá", IsUnused = true },
      new VehColor { Category = eVehColorCategory.Černá, ColorIndex = 1, ColorName = "Grafitově černá" },
      new VehColor { Category = eVehColorCategory.Černá, ColorIndex = 2, ColorName = "Ocelově černá" },
      new VehColor { Category = eVehColorCategory.Šedá, ColorIndex = 3, ColorName = "Ocelová šeď" },
      new VehColor { Category = eVehColorCategory.Šedá, ColorIndex = 4, ColorName = "Stříbřitá šedá" },
      new VehColor { Category = eVehColorCategory.Šedá, ColorIndex = 5, ColorName = "Broušená stříbrná" },
      new VehColor { Category = eVehColorCategory.Šedá, ColorIndex = 6, ColorName = "Světle ocelová šedá" },
      new VehColor { Category = eVehColorCategory.Šedá, ColorIndex = 7, ColorName = "Šeď temné oblohy" },
      new VehColor { Category = eVehColorCategory.Šedá, ColorIndex = 8, ColorName = "Kamenná šedá" },
      new VehColor { Category = eVehColorCategory.Černá, ColorIndex = 9, ColorName = "Půlnoční černá" },
      new VehColor { Category = eVehColorCategory.Šedá, ColorIndex = 10, ColorName = "Temná ocelová šeď" },
      new VehColor { Category = eVehColorCategory.Černá, ColorIndex = 11, ColorName = "Antracitová" },
      new VehColor { Category = eVehColorCategory.Černá, ColorIndex = 12, ColorName = "Jet-black černá" },
      new VehColor { Category = eVehColorCategory.Černá, ColorIndex = 13, ColorName = "Matná černá" },
      new VehColor { Category = eVehColorCategory.Šedá, ColorIndex = 14, ColorName = "Matná ocelově šedá" },
      new VehColor { Category = eVehColorCategory.Černá, ColorIndex = 15, ColorName = "", IsUnused = true },
      new VehColor { Category = eVehColorCategory.Černá, ColorIndex = 16, ColorName = "Leštěná černá" },
      new VehColor { Category = eVehColorCategory.Šedá, ColorIndex = 17, ColorName = "", IsUnused = true },
      new VehColor { Category = eVehColorCategory.Šedá, ColorIndex = 18, ColorName = "Šedá No.1" },
      new VehColor { Category = eVehColorCategory.Šedá, ColorIndex = 19, ColorName = "Šedá No.2" },
      new VehColor { Category = eVehColorCategory.Šedá, ColorIndex = 20, ColorName = "Šedá No.3" },
      new VehColor { Category = eVehColorCategory.Černá, ColorIndex = 21, ColorName = "Polomatná černá" },
      new VehColor { Category = eVehColorCategory.Šedá, ColorIndex = 22, ColorName = "Tmavě šedá" },
      new VehColor { Category = eVehColorCategory.Šedá, ColorIndex = 23, ColorName = "Světle šedá" },
      new VehColor { Category = eVehColorCategory.Bílá, ColorIndex = 24, ColorName = "Bílá" },
      new VehColor { Category = eVehColorCategory.Bílá, ColorIndex = 25, ColorName = "Tlumená bílá" },
      new VehColor { Category = eVehColorCategory.Šedá, ColorIndex = 26, ColorName = "Šedá obloha" },
      new VehColor { Category = eVehColorCategory.Červená, ColorIndex = 27, ColorName = "Červená" },
      new VehColor { Category = eVehColorCategory.Červená, ColorIndex = 28, ColorName = "Torino červená" },
      new VehColor { Category = eVehColorCategory.Červená, ColorIndex = 29, ColorName = "Formula červená" },
      new VehColor { Category = eVehColorCategory.Červená, ColorIndex = 30, ColorName = "Žhnoucí červená" },
      new VehColor { Category = eVehColorCategory.Červená, ColorIndex = 31, ColorName = "Elegantní červená" },
      new VehColor { Category = eVehColorCategory.Červená, ColorIndex = 32, ColorName = "Garnátová" },
      new VehColor { Category = eVehColorCategory.Červená, ColorIndex = 33, ColorName = "Zapadající slunce" },
      new VehColor { Category = eVehColorCategory.Červená, ColorIndex = 34, ColorName = "Cabernet" },
      new VehColor { Category = eVehColorCategory.Červená, ColorIndex = 35, ColorName = "Lízátková červená" },
      new VehColor { Category = eVehColorCategory.Oranžová, ColorIndex = 36, ColorName = "Sunrise oranžová" },
      new VehColor { Category = eVehColorCategory.Oranžová, ColorIndex = 37, ColorName = "Tlumená oranžová" },
      new VehColor { Category = eVehColorCategory.Oranžová, ColorIndex = 38, ColorName = "Oranžová" },
      new VehColor { Category = eVehColorCategory.Červená, ColorIndex = 39, ColorName = "Matná červená" },
      new VehColor { Category = eVehColorCategory.Červená, ColorIndex = 40, ColorName = "Matná tmavě červená" },
      new VehColor { Category = eVehColorCategory.Oranžová, ColorIndex = 41, ColorName = "Matná oranžová" },
      new VehColor { Category = eVehColorCategory.Žlutá, ColorIndex = 42, ColorName = "Matná žlutá" },
      new VehColor { Category = eVehColorCategory.Červená, ColorIndex = 43, ColorName = "Tmavě červená" },
      new VehColor { Category = eVehColorCategory.Červená, ColorIndex = 44, ColorName = "Stylová červená" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 45, ColorName = "Mahagonově hnědá" },
      new VehColor { Category = eVehColorCategory.Červená, ColorIndex = 46, ColorName = "", IsUnused = true },
      new VehColor { Category = eVehColorCategory.Oranžová, ColorIndex = 47, ColorName = "", IsUnused = true },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 48, ColorName = "Tmavý mahagon" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 49, ColorName = "Tmavě zelená" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 50, ColorName = "Závodní zelená" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 51, ColorName = "Oceánově zelená" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 52, ColorName = "Olivově zelená" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 53, ColorName = "Světle zelená" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 54, ColorName = "Palivově zelená" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 55, ColorName = "Matná limetkově zelená" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 56, ColorName = "", IsUnused = true },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 57, ColorName = "Plná zelená" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 58, ColorName = "Tlumená zelená" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 59, ColorName = "Švětlá khaki" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 60, ColorName = "Švětlá mint" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 61, ColorName = "Galaktická modrá" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 62, ColorName = "Tmavě modrá" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 63, ColorName = "Saxon modrá" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 64, ColorName = "Modrá" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 65, ColorName = "Mariňácká modrá" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 66, ColorName = "Přístavní modř" },
      new VehColor { Category = eVehColorCategory.Bílá, ColorIndex = 67, ColorName = "Diamantově bílá" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 68, ColorName = "Surfová modř" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 69, ColorName = "Námořní modrá" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 70, ColorName = "Ultra modrá" },
      new VehColor { Category = eVehColorCategory.Fialová, ColorIndex = 71, ColorName = "Schafter fialová" },
      new VehColor { Category = eVehColorCategory.Fialová, ColorIndex = 72, ColorName = "Tmavě fialová" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 73, ColorName = "Závodní modrá" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 74, ColorName = "Světle modrá" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 75, ColorName = "Elegantní modrá" },
      new VehColor { Category = eVehColorCategory.Fialová, ColorIndex = 76, ColorName = "Hluboká fialová" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 77, ColorName = "Nebesky modrá" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 78, ColorName = "Aquaristic modrá" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 79, ColorName = "Sytě modrá" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 80, ColorName = "Moderní modrá" },
      new VehColor { Category = eVehColorCategory.Fialová, ColorIndex = 81, ColorName = "Výrazná fialová" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 82, ColorName = "Matná tmavě modrá" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 83, ColorName = "Matná modrá" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 84, ColorName = "Matná půlnoční modrá" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 85, ColorName = "", IsUnused = true },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 86, ColorName = "", IsUnused = true },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 87, ColorName = "Světle azurová" },
      new VehColor { Category = eVehColorCategory.Žlutá, ColorIndex = 88, ColorName = "Žlutá" },
      new VehColor { Category = eVehColorCategory.Žlutá, ColorIndex = 89, ColorName = "Závodní žlutá" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 90, ColorName = "Bronzově hnědá" },
      new VehColor { Category = eVehColorCategory.Žlutá, ColorIndex = 91, ColorName = "Pastelově žlutá" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 92, ColorName = "Limetkově zelená" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 93, ColorName = "", IsUnused = true },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 94, ColorName = "Feltzer hnědá" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 95, ColorName = "Světle hnědá" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 96, ColorName = "Čokoládově hnědá" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 97, ColorName = "Javorová hněď" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 98, ColorName = "Sedlová hnědá" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 99, ColorName = "Slámově hnědá" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 100, ColorName = "Mechová zeleň" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 101, ColorName = "Hnědý bizon" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 102, ColorName = "__Woodbeech brown", IsUnused = true },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 103, ColorName = "Buková hnědá" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 104, ColorName = "Sienna hnědá" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 105, ColorName = "Písečně hnědá", IsUnused = true },
      new VehColor { Category = eVehColorCategory.Bílá, ColorIndex = 106, ColorName = "Krémově bílá" },
      new VehColor { Category = eVehColorCategory.Bílá, ColorIndex = 107, ColorName = "Světle krémová bílá" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 108, ColorName = "Pravá hnědá" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 109, ColorName = "Kávově hnědá" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 110, ColorName = "", IsUnused = true },
      new VehColor { Category = eVehColorCategory.Bílá, ColorIndex = 111, ColorName = "Matná bílá" },
      new VehColor { Category = eVehColorCategory.Bílá, ColorIndex = 112, ColorName = "Ledově bílá" },
      new VehColor { Category = eVehColorCategory.Šedá, ColorIndex = 113, ColorName = "Zašlá šedá" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 114, ColorName = "Matná hnědá" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 115, ColorName = "", IsUnused = true },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 116, ColorName = "", IsUnused = true },
      new VehColor { Category = eVehColorCategory.Kovová, ColorIndex = 117, ColorName = "Broušená ocel" },
      new VehColor { Category = eVehColorCategory.Kovová, ColorIndex = 118, ColorName = "Broušená černá ocel" },
      new VehColor { Category = eVehColorCategory.Kovová, ColorIndex = 119, ColorName = "Broušený hliník" },
      new VehColor { Category = eVehColorCategory.Kovová, ColorIndex = 120, ColorName = "Chrom" },
      new VehColor { Category = eVehColorCategory.Bílá, ColorIndex = 121, ColorName = "Bílá standard" },
      new VehColor { Category = eVehColorCategory.Bílá, ColorIndex = 122, ColorName = "La Crema bílá" },
      new VehColor { Category = eVehColorCategory.Oranžová, ColorIndex = 123, ColorName = "Mandarinkově oranžová" },
      new VehColor { Category = eVehColorCategory.Oranžová, ColorIndex = 124, ColorName = "Pomerančově oranžová" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 125, ColorName = "Pastelová švětle zelená" },
      new VehColor { Category = eVehColorCategory.Žlutá, ColorIndex = 126, ColorName = "Pravá žlutá" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 127, ColorName = "Azurová" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 128, ColorName = "Matná khaki" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 129, ColorName = "", IsUnused = true },
      new VehColor { Category = eVehColorCategory.Oranžová, ColorIndex = 130, ColorName = "Melounově oranžová" },
      new VehColor { Category = eVehColorCategory.Bílá, ColorIndex = 131, ColorName = "Polární bílá" },
      new VehColor { Category = eVehColorCategory.Bílá, ColorIndex = 132, ColorName = "Super bílá" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 133, ColorName = "Armádní zelená" },
      new VehColor { Category = eVehColorCategory.Bílá, ColorIndex = 134, ColorName = "Ultra bílá" },
      new VehColor { Category = eVehColorCategory.Růžová, ColorIndex = 135, ColorName = "Hot růžová" },
      new VehColor { Category = eVehColorCategory.Růžová, ColorIndex = 136, ColorName = "Lososová" },
      new VehColor { Category = eVehColorCategory.Růžová, ColorIndex = 137, ColorName = "Barbie růžová" },
      new VehColor { Category = eVehColorCategory.Oranžová, ColorIndex = 138, ColorName = "Výrazná oranžová" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 139, ColorName = "Pastelově zelená" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 140, ColorName = "Pravá azurová" },
      new VehColor { Category = eVehColorCategory.Fialová, ColorIndex = 141, ColorName = "Půlnoční tmavá fialová" },
      new VehColor { Category = eVehColorCategory.Fialová, ColorIndex = 142, ColorName = "Půlnoční fialová" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 143, ColorName = "Vínově hnědá" },
      new VehColor { Category = eVehColorCategory.Šedá, ColorIndex = 144, ColorName = "Šedá No.4" },
      new VehColor { Category = eVehColorCategory.Fialová, ColorIndex = 145, ColorName = "Zářivě fialová" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 146, ColorName = "Ultra-deep modrá" },
      new VehColor { Category = eVehColorCategory.Černá, ColorIndex = 147, ColorName = "Karbonová černá" },
      new VehColor { Category = eVehColorCategory.Fialová, ColorIndex = 148, ColorName = "Matná zářivě fialová" },
      new VehColor { Category = eVehColorCategory.Fialová, ColorIndex = 149, ColorName = "Matná půlnoční fialová" },
      new VehColor { Category = eVehColorCategory.Červená, ColorIndex = 150, ColorName = "Lávově červená" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 151, ColorName = "Matná lesní zelená" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 152, ColorName = "Matná olivová" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 153, ColorName = "Matná zeminová" },
      new VehColor { Category = eVehColorCategory.Hnědá, ColorIndex = 154, ColorName = "Matná pouštní" },
      new VehColor { Category = eVehColorCategory.Zelená, ColorIndex = 155, ColorName = "Matná keříková zelená" },
      new VehColor { Category = eVehColorCategory.Šedá, ColorIndex = 156, ColorName = "Šedá No.5" },
      new VehColor { Category = eVehColorCategory.Modrá, ColorIndex = 157, ColorName = "", IsUnused = true },
      new VehColor { Category = eVehColorCategory.Kovová, ColorIndex = 158, ColorName = "Čisté zlato" },
      new VehColor { Category = eVehColorCategory.Kovová, ColorIndex = 159, ColorName = "Broušené zlato" },
      new VehColor { Category = eVehColorCategory.Žlutá, ColorIndex = 160, ColorName = "Světle žlutá" },
      //
    };
  }

  public enum eVehColorCategory : int
  {
    Neznámá = 0,
    Bílá = 1,
    Černá = 2,
    Šedá = 3,
    Červená = 4,
    Modrá = 5,
    Zelená = 6,
    Žlutá = 7,
    Oranžová = 8,
    Fialová = 9,
    Růžová = 10,
    Hnědá = 11,
    Kovová = 12
  }

}