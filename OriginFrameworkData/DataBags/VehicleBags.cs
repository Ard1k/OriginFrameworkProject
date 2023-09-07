using Newtonsoft.Json;
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

  public class VehicleDamageBag
  {
    public float? bodyHealth { get; set; }
    public float? engineHealth { get; set; }
    public float? fuelLevel { get; set; }
    public float? dirtLevel { get; set; }

    public bool[] doorsMissing { get; set; }
    public bool[] tireBurst { get; set; }
    public bool[] windowsBroken { get; set; }
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

  public enum VehicleOriginClass : int
  {
    Uncategorized = 0,
    E = 1,
    D = 2,
    C = 3,
    B = 4,
    A = 5,
    S = 6
  }

  public class VehicleInformation
  {
    public string Model { get; set; }
    public int KeyItemId { get; set; }
    public bool IsForVendors { get; set; }
    public bool IsElectric { get; set; }
    public int Value { get; set; }
    public string LockSound { get; set; }
    public VehicleOriginClass Class { get; set; }
    public string Brand { get; set; }
    public string BrandModel { get; set; }
  }

  public class DefinedVehicles
  {
    public static Dictionary<int, VehicleInformation> KnownVehiclesByHash { get; set; } = null;

    public static Dictionary<string, VehicleInformation> KnownVehicles = new Dictionary<string, VehicleInformation>
    {
      ["polraiden"] = new VehicleInformation
      {
        Model = "polraiden",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = true,
        LockSound = "manual_1",
        Value = 825000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["buffalo2"] = new VehicleInformation
      {
        Model = "buffalo2",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 385000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["gauntlet5"] = new VehicleInformation
      {
        Model = "gauntlet5",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_7",
        Value = 517400,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["buffalo"] = new VehicleInformation
      {
        Model = "buffalo",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 330000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["neon"] = new VehicleInformation
      {
        Model = "neon",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = true,
        LockSound = "manual_4",
        Value = 781200,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["mamba"] = new VehicleInformation
      {
        Model = "mamba",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 1650000,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["subwrx"] = new VehicleInformation
      {
        Model = "subwrx",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 167400,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["faggio2"] = new VehicleInformation
      {
        Model = "faggio2",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 5430,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["pariah"] = new VehicleInformation
      {
        Model = "pariah",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_13",
        Value = 305100,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["tornado2"] = new VehicleInformation
      {
        Model = "tornado2",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 67400,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["police3"] = new VehicleInformation
      {
        Model = "police3",
        KeyItemId = 24,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 550000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["manchez"] = new VehicleInformation
      {
        Model = "manchez",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 45837,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["cyclone"] = new VehicleInformation
      {
        Model = "cyclone",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = true,
        LockSound = "manual_10",
        Value = 1821300,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["huntley"] = new VehicleInformation
      {
        Model = "huntley",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 412500,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["sanchez"] = new VehicleInformation
      {
        Model = "sanchez",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 45837,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["peyote"] = new VehicleInformation
      {
        Model = "peyote",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_7",
        Value = 412500,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["chino2"] = new VehicleInformation
      {
        Model = "chino2",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_3",
        Value = 330000,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["stafford"] = new VehicleInformation
      {
        Model = "stafford",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 407400,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["boss429"] = new VehicleInformation
      {
        Model = "boss429",
        KeyItemId = 24,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 1160100,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["feltzer2"] = new VehicleInformation
      {
        Model = "feltzer2",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 605000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["raiden"] = new VehicleInformation
      {
        Model = "raiden",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = true,
        LockSound = "manual_12",
        Value = 382500,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["patriot"] = new VehicleInformation
      {
        Model = "patriot",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_4",
        Value = 231200,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["deviant"] = new VehicleInformation
      {
        Model = "deviant",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 236300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["pfister811"] = new VehicleInformation
      {
        Model = "pfister811",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 1122400,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["rs7"] = new VehicleInformation
      {
        Model = "rs7",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 536300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["issi3"] = new VehicleInformation
      {
        Model = "issi3",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 18600,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["cheetah"] = new VehicleInformation
      {
        Model = "cheetah",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 1610100,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["rocoto"] = new VehicleInformation
      {
        Model = "rocoto",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_6",
        Value = 142600,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["freecrawler"] = new VehicleInformation
      {
        Model = "freecrawler",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 1517600,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["superd"] = new VehicleInformation
      {
        Model = "superd",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_7",
        Value = 412500,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["verlierer2"] = new VehicleInformation
      {
        Model = "verlierer2",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 1452400,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["vstr"] = new VehicleInformation
      {
        Model = "vstr",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 398800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["dilettante"] = new VehicleInformation
      {
        Model = "dilettante",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = true,
        LockSound = "manual_14",
        Value = 37600,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["buccaneer"] = new VehicleInformation
      {
        Model = "buccaneer",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 192500,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["infernus2"] = new VehicleInformation
      {
        Model = "infernus2",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_7",
        Value = 478700,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["mesa"] = new VehicleInformation
      {
        Model = "mesa",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 121300,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["gt500"] = new VehicleInformation
      {
        Model = "gt500",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 343800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["novak"] = new VehicleInformation
      {
        Model = "novak",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 343800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["nero"] = new VehicleInformation
      {
        Model = "nero",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_6",
        Value = 1452400,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["pcj"] = new VehicleInformation
      {
        Model = "pcj",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_4",
        Value = 50430,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["hexer"] = new VehicleInformation
      {
        Model = "hexer",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_4",
        Value = 45837,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["specter"] = new VehicleInformation
      {
        Model = "specter",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_4",
        Value = 616200,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["blade"] = new VehicleInformation
      {
        Model = "blade",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_4",
        Value = 72600,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["pscoutpol"] = new VehicleInformation
      {
        Model = "pscoutpol",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 984900,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["ruiner"] = new VehicleInformation
      {
        Model = "ruiner",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 206300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["amggt"] = new VehicleInformation
      {
        Model = "amggt",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_13",
        Value = 857600,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["taipan"] = new VehicleInformation
      {
        Model = "taipan",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_3",
        Value = 770000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["pony"] = new VehicleInformation
      {
        Model = "pony",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 81400,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["police2"] = new VehicleInformation
      {
        Model = "police2",
        KeyItemId = 24,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 550000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["gauntlet3"] = new VehicleInformation
      {
        Model = "gauntlet3",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 156400,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["jester3"] = new VehicleInformation
      {
        Model = "jester3",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 261300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["ingot"] = new VehicleInformation
      {
        Model = "ingot",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 22600,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["911r"] = new VehicleInformation
      {
        Model = "911r",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 1028700,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["thrax"] = new VehicleInformation
      {
        Model = "thrax",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 1650000,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["feltzer3"] = new VehicleInformation
      {
        Model = "feltzer3",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_13",
        Value = 726200,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["primo2"] = new VehicleInformation
      {
        Model = "primo2",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_6",
        Value = 316300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["tornado3"] = new VehicleInformation
      {
        Model = "tornado3",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 21300,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["emperor2"] = new VehicleInformation
      {
        Model = "emperor2",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_6",
        Value = 22500,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["sultanrs"] = new VehicleInformation
      {
        Model = "sultanrs",
        KeyItemId = 24,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 288800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["evo9mr"] = new VehicleInformation
      {
        Model = "evo9mr",
        KeyItemId = 24,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 162600,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["zion"] = new VehicleInformation
      {
        Model = "zion",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 362600,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["baller4"] = new VehicleInformation
      {
        Model = "baller4",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 742500,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["rebel"] = new VehicleInformation
      {
        Model = "rebel",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_4",
        Value = 81400,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["stryder"] = new VehicleInformation
      {
        Model = "stryder",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 31663,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["bjxl"] = new VehicleInformation
      {
        Model = "bjxl",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 47600,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["italigto"] = new VehicleInformation
      {
        Model = "italigto",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 742500,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["regalia"] = new VehicleInformation
      {
        Model = "regalia",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 1375000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["banshee"] = new VehicleInformation
      {
        Model = "banshee",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_6",
        Value = 440000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["jackal"] = new VehicleInformation
      {
        Model = "jackal",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 159900,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["tornado4"] = new VehicleInformation
      {
        Model = "tornado4",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 16300,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["lp700r"] = new VehicleInformation
      {
        Model = "lp700r",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_3",
        Value = 3167600,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["schafter4"] = new VehicleInformation
      {
        Model = "schafter4",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 440000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["f620"] = new VehicleInformation
      {
        Model = "f620",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 687500,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["hakuchou2"] = new VehicleInformation
      {
        Model = "hakuchou2",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 440000,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["coquette3"] = new VehicleInformation
      {
        Model = "coquette3",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_4",
        Value = 605000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["eclipse"] = new VehicleInformation
      {
        Model = "eclipse",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_6",
        Value = 467500,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["stinger"] = new VehicleInformation
      {
        Model = "stinger",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 412500,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["double"] = new VehicleInformation
      {
        Model = "double",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 165000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["emperor"] = new VehicleInformation
      {
        Model = "emperor",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 31300,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["turismor"] = new VehicleInformation
      {
        Model = "turismor",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 1808700,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["speedo"] = new VehicleInformation
      {
        Model = "speedo",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 108600,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["fd"] = new VehicleInformation
      {
        Model = "fd",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 145000,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["asea"] = new VehicleInformation
      {
        Model = "asea",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 37600,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["srt10"] = new VehicleInformation
      {
        Model = "srt10",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 857600,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["guardian"] = new VehicleInformation
      {
        Model = "guardian",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_7",
        Value = 330000,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["krieger"] = new VehicleInformation
      {
        Model = "krieger",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 1386200,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["furoregt"] = new VehicleInformation
      {
        Model = "furoregt",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 522500,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["hermes"] = new VehicleInformation
      {
        Model = "hermes",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 371300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["cls63s"] = new VehicleInformation
      {
        Model = "cls63s",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 880000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["rancherxl"] = new VehicleInformation
      {
        Model = "rancherxl",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 68700,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["faggio"] = new VehicleInformation
      {
        Model = "faggio",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_3",
        Value = 5463,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["comet2"] = new VehicleInformation
      {
        Model = "comet2",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_13",
        Value = 775100,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["stalion"] = new VehicleInformation
      {
        Model = "stalion",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 291300,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["voltic"] = new VehicleInformation
      {
        Model = "voltic",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = true,
        LockSound = "manual_5",
        Value = 357500,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["kanjo"] = new VehicleInformation
      {
        Model = "kanjo",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_13",
        Value = 43700,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["benzsl63"] = new VehicleInformation
      {
        Model = "benzsl63",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_7",
        Value = 550000,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["locust"] = new VehicleInformation
      {
        Model = "locust",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 764900,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["kamacho"] = new VehicleInformation
      {
        Model = "kamacho",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_3",
        Value = 379900,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["lynx"] = new VehicleInformation
      {
        Model = "lynx",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 715000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["paragon"] = new VehicleInformation
      {
        Model = "paragon",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 715000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["intruder"] = new VehicleInformation
      {
        Model = "intruder",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_13",
        Value = 36300,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["vacca"] = new VehicleInformation
      {
        Model = "vacca",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 1122400,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["xa21"] = new VehicleInformation
      {
        Model = "xa21",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 742500,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["evo6"] = new VehicleInformation
      {
        Model = "evo6",
        KeyItemId = 24,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 153800,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["banshee2"] = new VehicleInformation
      {
        Model = "banshee2",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 550000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["fugitive"] = new VehicleInformation
      {
        Model = "fugitive",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 357500,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["stanier"] = new VehicleInformation
      {
        Model = "stanier",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 31300,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["baller3"] = new VehicleInformation
      {
        Model = "baller3",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 715000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["vagner"] = new VehicleInformation
      {
        Model = "vagner",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 1782400,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["gauntlet"] = new VehicleInformation
      {
        Model = "gauntlet",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 250100,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["tailgater"] = new VehicleInformation
      {
        Model = "tailgater",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 217500,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["vader"] = new VehicleInformation
      {
        Model = "vader",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 26663,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["massacro"] = new VehicleInformation
      {
        Model = "massacro",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 605000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["oracle2"] = new VehicleInformation
      {
        Model = "oracle2",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 250100,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["xls"] = new VehicleInformation
      {
        Model = "xls",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 316300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["yosemite2"] = new VehicleInformation
      {
        Model = "yosemite2",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 343800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["infernus"] = new VehicleInformation
      {
        Model = "infernus",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 857600,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["ellie"] = new VehicleInformation
      {
        Model = "ellie",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 313700,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["coquette"] = new VehicleInformation
      {
        Model = "coquette",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_7",
        Value = 346300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["demon"] = new VehicleInformation
      {
        Model = "demon",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 621300,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["felon"] = new VehicleInformation
      {
        Model = "felon",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 258700,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["hotknife"] = new VehicleInformation
      {
        Model = "hotknife",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 687500,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["seven70"] = new VehicleInformation
      {
        Model = "seven70",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 660000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["avarus"] = new VehicleInformation
      {
        Model = "avarus",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 45837,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["rapidgt3"] = new VehicleInformation
      {
        Model = "rapidgt3",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 305100,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["flashgt"] = new VehicleInformation
      {
        Model = "flashgt",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 764900,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["furia"] = new VehicleInformation
      {
        Model = "furia",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_6",
        Value = 1847600,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["riata"] = new VehicleInformation
      {
        Model = "riata",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_7",
        Value = 75000,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["bee"] = new VehicleInformation
      {
        Model = "bee",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_7",
        Value = 508800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["faction3"] = new VehicleInformation
      {
        Model = "faction3",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 266400,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["surano"] = new VehicleInformation
      {
        Model = "surano",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 398800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["impaler"] = new VehicleInformation
      {
        Model = "impaler",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 96400,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["toros"] = new VehicleInformation
      {
        Model = "toros",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 764900,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["alpha"] = new VehicleInformation
      {
        Model = "alpha",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 440000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["schafter3"] = new VehicleInformation
      {
        Model = "schafter3",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_13",
        Value = 506200,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["carbonizzare"] = new VehicleInformation
      {
        Model = "carbonizzare",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 467500,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["exemplar"] = new VehicleInformation
      {
        Model = "exemplar",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 385000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["manana"] = new VehicleInformation
      {
        Model = "manana",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 101200,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["c6z06"] = new VehicleInformation
      {
        Model = "c6z06",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_3",
        Value = 962500,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["tezeract"] = new VehicleInformation
      {
        Model = "tezeract",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = true,
        LockSound = "manual_9",
        Value = 1874900,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["imorgon"] = new VehicleInformation
      {
        Model = "imorgon",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = true,
        LockSound = "manual_2",
        Value = 1676300,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["komoda"] = new VehicleInformation
      {
        Model = "komoda",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_13",
        Value = 632500,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["sc1"] = new VehicleInformation
      {
        Model = "sc1",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 605000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["penetrator"] = new VehicleInformation
      {
        Model = "penetrator",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 627400,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["hustler"] = new VehicleInformation
      {
        Model = "hustler",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_6",
        Value = 132400,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["burrito3"] = new VehicleInformation
      {
        Model = "burrito3",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_4",
        Value = 44900,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["fmj"] = new VehicleInformation
      {
        Model = "fmj",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_6",
        Value = 1610100,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["bestiagts"] = new VehicleInformation
      {
        Model = "bestiagts",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 726200,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["tornado5"] = new VehicleInformation
      {
        Model = "tornado5",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_4",
        Value = 316300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["x5e53"] = new VehicleInformation
      {
        Model = "x5e53",
        KeyItemId = 24,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 577500,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["zentorno"] = new VehicleInformation
      {
        Model = "zentorno",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 1517600,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["stingergt"] = new VehicleInformation
      {
        Model = "stingergt",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 440000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["r8v10"] = new VehicleInformation
      {
        Model = "r8v10",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 990000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["specter2"] = new VehicleInformation
      {
        Model = "specter2",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 495000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["asbo"] = new VehicleInformation
      {
        Model = "asbo",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 21300,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["prevolter"] = new VehicleInformation
      {
        Model = "prevolter",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 577500,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["fbi2"] = new VehicleInformation
      {
        Model = "fbi2",
        KeyItemId = 24,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 550000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["dominator3"] = new VehicleInformation
      {
        Model = "dominator3",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_3",
        Value = 781200,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["moonbeam2"] = new VehicleInformation
      {
        Model = "moonbeam2",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 87600,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["dominator2"] = new VehicleInformation
      {
        Model = "dominator2",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 831300,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["asterope"] = new VehicleInformation
      {
        Model = "asterope",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 47600,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["voodoo"] = new VehicleInformation
      {
        Model = "voodoo",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 56300,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["windsor"] = new VehicleInformation
      {
        Model = "windsor",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 742500,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["zorrusso"] = new VehicleInformation
      {
        Model = "zorrusso",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 1505000,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["fbi"] = new VehicleInformation
      {
        Model = "fbi",
        KeyItemId = 24,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 550000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["windsor2"] = new VehicleInformation
      {
        Model = "windsor2",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_7",
        Value = 797500,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["tyrant"] = new VehicleInformation
      {
        Model = "tyrant",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_13",
        Value = 770000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["rebla"] = new VehicleInformation
      {
        Model = "rebla",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 346300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["nightblade"] = new VehicleInformation
      {
        Model = "nightblade",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 41200,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["schlagen"] = new VehicleInformation
      {
        Model = "schlagen",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 715000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["skyline"] = new VehicleInformation
      {
        Model = "skyline",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 508800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["bison"] = new VehicleInformation
      {
        Model = "bison",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_3",
        Value = 111400,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["felon2"] = new VehicleInformation
      {
        Model = "felon2",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 269900,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["zion3"] = new VehicleInformation
      {
        Model = "zion3",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 101200,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["bmci"] = new VehicleInformation
      {
        Model = "bmci",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 467500,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["daemon"] = new VehicleInformation
      {
        Model = "daemon",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 25000,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["granger"] = new VehicleInformation
      {
        Model = "granger",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 167600,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["hellion"] = new VehicleInformation
      {
        Model = "hellion",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 76300,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["picador"] = new VehicleInformation
      {
        Model = "picador",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 283700,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["autarch"] = new VehicleInformation
      {
        Model = "autarch",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 1782400,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["c5rs6"] = new VehicleInformation
      {
        Model = "c5rs6",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 167400,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["issi7"] = new VehicleInformation
      {
        Model = "issi7",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_13",
        Value = 132400,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["schwarzer"] = new VehicleInformation
      {
        Model = "schwarzer",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 660000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["serrano"] = new VehicleInformation
      {
        Model = "serrano",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_7",
        Value = 57400,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["lectro"] = new VehicleInformation
      {
        Model = "lectro",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_3",
        Value = 27137,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["warrener"] = new VehicleInformation
      {
        Model = "warrener",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 70100,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["nemesis"] = new VehicleInformation
      {
        Model = "nemesis",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_3",
        Value = 50430,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["clique"] = new VehicleInformation
      {
        Model = "clique",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 125000,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["schafter2"] = new VehicleInformation
      {
        Model = "schafter2",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 343800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["sadler"] = new VehicleInformation
      {
        Model = "sadler",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 128700,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["radi"] = new VehicleInformation
      {
        Model = "radi",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 86200,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["voodoo2"] = new VehicleInformation
      {
        Model = "voodoo2",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 60000,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["rmodx6"] = new VehicleInformation
      {
        Model = "rmodx6",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 1017500,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["bmwe6"] = new VehicleInformation
      {
        Model = "bmwe6",
        KeyItemId = 24,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 830100,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["taxi"] = new VehicleInformation
      {
        Model = "taxi",
        KeyItemId = 24,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_7",
        Value = 75000,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["jester"] = new VehicleInformation
      {
        Model = "jester",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_6",
        Value = 726200,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["sentinel3"] = new VehicleInformation
      {
        Model = "sentinel3",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 108600,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["sentinel2"] = new VehicleInformation
      {
        Model = "sentinel2",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 233800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["outlaw"] = new VehicleInformation
      {
        Model = "outlaw",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_3",
        Value = 115100,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["bfinjection"] = new VehicleInformation
      {
        Model = "bfinjection",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 36200,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["flatbed"] = new VehicleInformation
      {
        Model = "flatbed",
        KeyItemId = 24,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 130000,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["trhawk"] = new VehicleInformation
      {
        Model = "trhawk",
        KeyItemId = 24,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 967600,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["casco"] = new VehicleInformation
      {
        Model = "casco",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 990000,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["bentayga17"] = new VehicleInformation
      {
        Model = "bentayga17",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 907500,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["s600w220"] = new VehicleInformation
      {
        Model = "s600w220",
        KeyItemId = 24,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 508800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["zombiea"] = new VehicleInformation
      {
        Model = "zombiea",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 55870,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["rapidgt2"] = new VehicleInformation
      {
        Model = "rapidgt2",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 330000,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["vigero"] = new VehicleInformation
      {
        Model = "vigero",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 121300,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["baller2"] = new VehicleInformation
      {
        Model = "baller2",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 660000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["michelli"] = new VehicleInformation
      {
        Model = "michelli",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 76100,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["fq2"] = new VehicleInformation
      {
        Model = "fq2",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 371300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["futo"] = new VehicleInformation
      {
        Model = "futo",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 126300,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["ruffian"] = new VehicleInformation
      {
        Model = "ruffian",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 68767,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["surge"] = new VehicleInformation
      {
        Model = "surge",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = true,
        LockSound = "manual_1",
        Value = 126300,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["regina"] = new VehicleInformation
      {
        Model = "regina",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 28700,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["khamelion"] = new VehicleInformation
      {
        Model = "khamelion",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = true,
        LockSound = "manual_1",
        Value = 373800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["landstalker"] = new VehicleInformation
      {
        Model = "landstalker",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 214900,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["sugoi"] = new VehicleInformation
      {
        Model = "sugoi",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_7",
        Value = 236300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["cavalcade"] = new VehicleInformation
      {
        Model = "cavalcade",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 115100,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["vortex"] = new VehicleInformation
      {
        Model = "vortex",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 55870,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["enduro"] = new VehicleInformation
      {
        Model = "enduro",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 15033,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["gp1"] = new VehicleInformation
      {
        Model = "gp1",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_6",
        Value = 976400,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["vamos"] = new VehicleInformation
      {
        Model = "vamos",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 88800,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["tampa"] = new VehicleInformation
      {
        Model = "tampa",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 238900,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["moonbeam"] = new VehicleInformation
      {
        Model = "moonbeam",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_4",
        Value = 111400,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["bati"] = new VehicleInformation
      {
        Model = "bati",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_7",
        Value = 114603,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["rs5"] = new VehicleInformation
      {
        Model = "rs5",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 508800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["dukes"] = new VehicleInformation
      {
        Model = "dukes",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 440000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["rhapsody"] = new VehicleInformation
      {
        Model = "rhapsody",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 46200,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["tornado"] = new VehicleInformation
      {
        Model = "tornado",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 63800,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["buccaneer2"] = new VehicleInformation
      {
        Model = "buccaneer2",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_3",
        Value = 371300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["premier"] = new VehicleInformation
      {
        Model = "premier",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 57400,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["seminole"] = new VehicleInformation
      {
        Model = "seminole",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 117600,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["rebel2"] = new VehicleInformation
      {
        Model = "rebel2",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 151300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["gresley"] = new VehicleInformation
      {
        Model = "gresley",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 178800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["m5"] = new VehicleInformation
      {
        Model = "m5",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_4",
        Value = 171300,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["surfer"] = new VehicleInformation
      {
        Model = "surfer",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 46300,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["4881"] = new VehicleInformation
      {
        Model = "4881",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 3102400,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["fagaloa"] = new VehicleInformation
      {
        Model = "fagaloa",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 43800,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["faction2"] = new VehicleInformation
      {
        Model = "faction2",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_6",
        Value = 338700,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["kuruma"] = new VehicleInformation
      {
        Model = "kuruma",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_13",
        Value = 495000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["faction"] = new VehicleInformation
      {
        Model = "faction",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 132400,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["wolfsbane"] = new VehicleInformation
      {
        Model = "wolfsbane",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 59603,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["gargoyle"] = new VehicleInformation
      {
        Model = "gargoyle",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_13",
        Value = 48733,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["viseris"] = new VehicleInformation
      {
        Model = "viseris",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 316300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["police"] = new VehicleInformation
      {
        Model = "police",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 550000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["neo"] = new VehicleInformation
      {
        Model = "neo",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = true,
        LockSound = "manual_13",
        Value = 373800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["g65"] = new VehicleInformation
      {
        Model = "g65",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 902400,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["caracara2"] = new VehicleInformation
      {
        Model = "caracara2",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_3",
        Value = 343800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["f82"] = new VehicleInformation
      {
        Model = "f82",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 453800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["sandking2"] = new VehicleInformation
      {
        Model = "sandking2",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_6",
        Value = 176200,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["chino"] = new VehicleInformation
      {
        Model = "chino",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 151300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["visione"] = new VehicleInformation
      {
        Model = "visione",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 1847600,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["sandking"] = new VehicleInformation
      {
        Model = "sandking",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_6",
        Value = 197600,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["sultan2"] = new VehicleInformation
      {
        Model = "sultan2",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 47600,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["italigtb"] = new VehicleInformation
      {
        Model = "italigtb",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 1398800,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["tempesta"] = new VehicleInformation
      {
        Model = "tempesta",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 1517600,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["yosemite"] = new VehicleInformation
      {
        Model = "yosemite",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_13",
        Value = 44900,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["cogcabrio"] = new VehicleInformation
      {
        Model = "cogcabrio",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 247500,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["ninef"] = new VehicleInformation
      {
        Model = "ninef",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 385000,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["ninef2"] = new VehicleInformation
      {
        Model = "ninef2",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_4",
        Value = 398800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["sentinel"] = new VehicleInformation
      {
        Model = "sentinel",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 242400,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["oracle"] = new VehicleInformation
      {
        Model = "oracle",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_3",
        Value = 176200,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["fusilade"] = new VehicleInformation
      {
        Model = "fusilade",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 148700,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["retinue2"] = new VehicleInformation
      {
        Model = "retinue2",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_13",
        Value = 114900,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["reaper"] = new VehicleInformation
      {
        Model = "reaper",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 808700,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["peyote2"] = new VehicleInformation
      {
        Model = "peyote2",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 236300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["prairie"] = new VehicleInformation
      {
        Model = "prairie",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 159900,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["dynasty"] = new VehicleInformation
      {
        Model = "dynasty",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 27600,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["esskey"] = new VehicleInformation
      {
        Model = "esskey",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 35430,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["police4"] = new VehicleInformation
      {
        Model = "police4",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_13",
        Value = 550000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["jugular"] = new VehicleInformation
      {
        Model = "jugular",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_4",
        Value = 352400,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["swinger"] = new VehicleInformation
      {
        Model = "swinger",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_3",
        Value = 426300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["w210"] = new VehicleInformation
      {
        Model = "w210",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 160000,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["drafter"] = new VehicleInformation
      {
        Model = "drafter",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 302500,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["rumpo"] = new VehicleInformation
      {
        Model = "rumpo",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_4",
        Value = 86200,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["carbonrs"] = new VehicleInformation
      {
        Model = "carbonrs",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 62467,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["penumbra"] = new VehicleInformation
      {
        Model = "penumbra",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 371300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["faggio3"] = new VehicleInformation
      {
        Model = "faggio3",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_7",
        Value = 7103,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["sabregt2"] = new VehicleInformation
      {
        Model = "sabregt2",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 272500,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["nero2"] = new VehicleInformation
      {
        Model = "nero2",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 1122400,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["emerus"] = new VehicleInformation
      {
        Model = "emerus",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 1267400,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["ztype"] = new VehicleInformation
      {
        Model = "ztype",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 1755100,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["everon"] = new VehicleInformation
      {
        Model = "everon",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 269900,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["bf400"] = new VehicleInformation
      {
        Model = "bf400",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 72500,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["gauntlet4"] = new VehicleInformation
      {
        Model = "gauntlet4",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 343800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["baller"] = new VehicleInformation
      {
        Model = "baller",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_3",
        Value = 137500,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["panto"] = new VehicleInformation
      {
        Model = "panto",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_7",
        Value = 18600,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["cognoscenti"] = new VehicleInformation
      {
        Model = "cognoscenti",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_13",
        Value = 412500,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["streiter"] = new VehicleInformation
      {
        Model = "streiter",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 316300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["20f250"] = new VehicleInformation
      {
        Model = "20f250",
        KeyItemId = 24,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_7",
        Value = 481300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["sultan"] = new VehicleInformation
      {
        Model = "sultan",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 167600,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["cog55"] = new VehicleInformation
      {
        Model = "cog55",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 390100,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["gtr"] = new VehicleInformation
      {
        Model = "gtr",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_6",
        Value = 836200,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["revolter"] = new VehicleInformation
      {
        Model = "revolter",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 371300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["habanero"] = new VehicleInformation
      {
        Model = "habanero",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_13",
        Value = 47600,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["runner"] = new VehicleInformation
      {
        Model = "runner",
        KeyItemId = 24,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 508800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["69charger"] = new VehicleInformation
      {
        Model = "69charger",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 525100,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["rapidgt"] = new VehicleInformation
      {
        Model = "rapidgt",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 316300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["370z"] = new VehicleInformation
      {
        Model = "370z",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 1237500,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["pgto"] = new VehicleInformation
      {
        Model = "pgto",
        KeyItemId = 24,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_6",
        Value = 162600,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["sabregt"] = new VehicleInformation
      {
        Model = "sabregt",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 220000,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["blista2"] = new VehicleInformation
      {
        Model = "blista2",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 33700,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["fcr"] = new VehicleInformation
      {
        Model = "fcr",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 62467,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["gb200"] = new VehicleInformation
      {
        Model = "gb200",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 176200,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["savestra"] = new VehicleInformation
      {
        Model = "savestra",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 151300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["elegy"] = new VehicleInformation
      {
        Model = "elegy",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_4",
        Value = 162500,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["elegy2"] = new VehicleInformation
      {
        Model = "elegy2",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 371300,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["washington"] = new VehicleInformation
      {
        Model = "washington",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 233800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["cheburek"] = new VehicleInformation
      {
        Model = "cheburek",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 16300,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["rumpo3"] = new VehicleInformation
      {
        Model = "rumpo3",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 233800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["blista"] = new VehicleInformation
      {
        Model = "blista",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 40000,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["bodhi2"] = new VehicleInformation
      {
        Model = "bodhi2",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 44900,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["youga"] = new VehicleInformation
      {
        Model = "youga",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_13",
        Value = 39900,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["zion2"] = new VehicleInformation
      {
        Model = "zion2",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 417600,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["primo"] = new VehicleInformation
      {
        Model = "primo",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_6",
        Value = 41200,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["adder"] = new VehicleInformation
      {
        Model = "adder",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 1517600,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["entityxf"] = new VehicleInformation
      {
        Model = "entityxf",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 703800,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["nightshade"] = new VehicleInformation
      {
        Model = "nightshade",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 1187600,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["italigtb2"] = new VehicleInformation
      {
        Model = "italigtb2",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_6",
        Value = 831300,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["entity2"] = new VehicleInformation
      {
        Model = "entity2",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_7",
        Value = 1782400,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["deveste"] = new VehicleInformation
      {
        Model = "deveste",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 1940100,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["torero"] = new VehicleInformation
      {
        Model = "torero",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_14",
        Value = 1122400,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["osiris"] = new VehicleInformation
      {
        Model = "osiris",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_1",
        Value = 1293700,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["monroe"] = new VehicleInformation
      {
        Model = "monroe",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_8",
        Value = 1241200,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["defiler"] = new VehicleInformation
      {
        Model = "defiler",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 73337,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["t20"] = new VehicleInformation
      {
        Model = "t20",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 1544900,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["cheetah2"] = new VehicleInformation
      {
        Model = "cheetah2",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_3",
        Value = 1544900,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["turismo2"] = new VehicleInformation
      {
        Model = "turismo2",
        KeyItemId = 25,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_12",
        Value = 1003600,
        Class = VehicleOriginClass.A,
        Brand = null,
        BrandModel = null,
      },
      ["diablous"] = new VehicleInformation
      {
        Model = "diablous",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 42963,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["issi2"] = new VehicleInformation
      {
        Model = "issi2",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_9",
        Value = 104900,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
      ["glendale"] = new VehicleInformation
      {
        Model = "glendale",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_6",
        Value = 31300,
        Class = VehicleOriginClass.E,
        Brand = null,
        BrandModel = null,
      },
      ["comet3"] = new VehicleInformation
      {
        Model = "comet3",
        KeyItemId = 23,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_2",
        Value = 286200,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["tulip"] = new VehicleInformation
      {
        Model = "tulip",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_10",
        Value = 233800,
        Class = VehicleOriginClass.C,
        Brand = null,
        BrandModel = null,
      },
      ["z190"] = new VehicleInformation
      {
        Model = "z190",
        KeyItemId = 24,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_5",
        Value = 440000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["polcoquette"] = new VehicleInformation
      {
        Model = "polcoquette",
        KeyItemId = 24,
        IsForVendors = false,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 593800,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["phoenix"] = new VehicleInformation
      {
        Model = "phoenix",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_11",
        Value = 440000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["brawler"] = new VehicleInformation
      {
        Model = "brawler",
        KeyItemId = 25,
        IsForVendors = true,
        IsElectric = true,
        LockSound = "manual_3",
        Value = 605000,
        Class = VehicleOriginClass.B,
        Brand = null,
        BrandModel = null,
      },
      ["vagrant"] = new VehicleInformation
      {
        Model = "vagrant",
        KeyItemId = 23,
        IsForVendors = true,
        IsElectric = false,
        LockSound = "manual_13",
        Value = 122500,
        Class = VehicleOriginClass.D,
        Brand = null,
        BrandModel = null,
      },
    };
  }
}
