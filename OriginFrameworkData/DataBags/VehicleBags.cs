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
    public int ParentTuning { get; set; }
    public int Price { get; set; }
    public int PriceItemId { get; set; }
    public string Name { get; set; }
    public bool IsToggle { get; set; }
    public bool IsDisabled { get; set; }

    public static Dictionary<int, VehTuningTypeDefinition> Defined { get; set; } = new Dictionary<int, VehTuningTypeDefinition>
    {
      { 0, new VehTuningTypeDefinition { TuningType = 0, ParentTuning = -1, Name = "Spoilery", Price = 0, PriceItemId = 0 } },
      { 1, new VehTuningTypeDefinition { TuningType = 1, ParentTuning = -1, Name = "Přední nárazník", Price = 0, PriceItemId = 0 } },
      { 2, new VehTuningTypeDefinition { TuningType = 2, ParentTuning = -1, Name = "Zadní nárazník", Price = 0, PriceItemId = 0 } },
      { 3, new VehTuningTypeDefinition { TuningType = 3, ParentTuning = -1, Name = "Bočnice", Price = 0, PriceItemId = 0 } },
      { 4, new VehTuningTypeDefinition { TuningType = 4, ParentTuning = -1, Name = "Výfuk", Price = 0, PriceItemId = 0 } },
      { 5, new VehTuningTypeDefinition { TuningType = 5, ParentTuning = -1, Name = "Klec", Price = 0, PriceItemId = 0 } },
      { 6, new VehTuningTypeDefinition { TuningType = 6, ParentTuning = -1, Name = "Přední maska", Price = 0, PriceItemId = 0 } },
      { 7, new VehTuningTypeDefinition { TuningType = 7, ParentTuning = -1, Name = "Kapota", Price = 0, PriceItemId = 0 } },
      { 8, new VehTuningTypeDefinition { TuningType = 8, ParentTuning = -1, Name = "Doplňky 1", Price = 0, PriceItemId = 0 } },
      { 9, new VehTuningTypeDefinition { TuningType = 9, ParentTuning = -1, Name = "Doplňky 2", Price = 0, PriceItemId = 0 } },
      { 10, new VehTuningTypeDefinition { TuningType = 10, ParentTuning = -1, Name = "Střecha", Price = 0, PriceItemId = 0 } },
      { 11, new VehTuningTypeDefinition { TuningType = 11, ParentTuning = -1, Name = "Motor", Price = 0, PriceItemId = 0 } },
      { 12, new VehTuningTypeDefinition { TuningType = 12, ParentTuning = -1, Name = "Brzdy", Price = 0, PriceItemId = 0 } },
      { 13, new VehTuningTypeDefinition { TuningType = 13, ParentTuning = -1, Name = "Převodovka", Price = 0, PriceItemId = 0 } },
      { 14, new VehTuningTypeDefinition { TuningType = 14, ParentTuning = -1, Name = "Klakson", Price = 0, PriceItemId = 0 } },
      { 15, new VehTuningTypeDefinition { TuningType = 15, ParentTuning = -1, Name = "Podvozek", Price = 0, PriceItemId = 0 } },
      { 16, new VehTuningTypeDefinition { TuningType = 16, ParentTuning = -1, Name = "Pancíř", Price = 0, PriceItemId = 0, IsDisabled = true } },
      { 17, new VehTuningTypeDefinition { TuningType = 17, ParentTuning = -1, Name = "Nitro", Price = 0, PriceItemId = 0, IsDisabled = true, IsToggle = true } },
      { 18, new VehTuningTypeDefinition { TuningType = 18, ParentTuning = -1, Name = "Turbo", Price = 0, PriceItemId = 0, IsToggle = true} },
      { 19, new VehTuningTypeDefinition { TuningType = 19, ParentTuning = -1, Name = "Subwoofer", Price = 0, PriceItemId = 0, IsDisabled = true } },
      { 20, new VehTuningTypeDefinition { TuningType = 20, ParentTuning = -1, Name = "Pneu pigment", Price = 0, PriceItemId = 0 } },
      { 21, new VehTuningTypeDefinition { TuningType = 21, ParentTuning = -1, Name = "Hydraulika", Price = 0, PriceItemId = 0 } },
      { 22, new VehTuningTypeDefinition { TuningType = 22, ParentTuning = -1, Name = "Xenony", Price = 0, PriceItemId = 0, IsToggle = true } },
      { 23, new VehTuningTypeDefinition { TuningType = 23, ParentTuning = -1, Name = "Kola", Price = 0, PriceItemId = 0 } },
      { 24, new VehTuningTypeDefinition { TuningType = 24, ParentTuning = -1, Name = "Zadní kolo", Price = 0, PriceItemId = 0 } },
      { 25, new VehTuningTypeDefinition { TuningType = 25, ParentTuning = -1, Name = "SPZ rámeček", Price = 0, PriceItemId = 0 } },
      { 26, new VehTuningTypeDefinition { TuningType = 26, ParentTuning = -1, Name = "Custom SPZ", Price = 0, PriceItemId = 0 } },
      { 27, new VehTuningTypeDefinition { TuningType = 27, ParentTuning = -1, Name = "Zdobení 1", Price = 0, PriceItemId = 0 } },
      { 28, new VehTuningTypeDefinition { TuningType = 28, ParentTuning = -1, Name = "Ornamenty", Price = 0, PriceItemId = 0 } },
      { 29, new VehTuningTypeDefinition { TuningType = 29, ParentTuning = -1, Name = "Palubní deska", Price = 0, PriceItemId = 0 } },
      { 30, new VehTuningTypeDefinition { TuningType = 30, ParentTuning = -1, Name = "Budíky", Price = 0, PriceItemId = 0 } },
      { 31, new VehTuningTypeDefinition { TuningType = 31, ParentTuning = -1, Name = "Repráky ve dveřích", Price = 0, PriceItemId = 0 } },
      { 32, new VehTuningTypeDefinition { TuningType = 32, ParentTuning = -1, Name = "Sedačky", Price = 0, PriceItemId = 0 } },
      { 33, new VehTuningTypeDefinition { TuningType = 33, ParentTuning = -1, Name = "Volant", Price = 0, PriceItemId = 0 } },
      { 34, new VehTuningTypeDefinition { TuningType = 34, ParentTuning = -1, Name = "Řadička", Price = 0, PriceItemId = 0 } },
      { 35, new VehTuningTypeDefinition { TuningType = 35, ParentTuning = -1, Name = "Plaketa", Price = 0, PriceItemId = 0 } },
      { 36, new VehTuningTypeDefinition { TuningType = 36, ParentTuning = -1, Name = "Stereo", Price = 0, PriceItemId = 0 } },
      { 37, new VehTuningTypeDefinition { TuningType = 37, ParentTuning = -1, Name = "Kufr", Price = 0, PriceItemId = 0 } },
      { 38, new VehTuningTypeDefinition { TuningType = 38, ParentTuning = -1, Name = "Hydraulika 2", Price = 0, PriceItemId = 0 } },
      { 39, new VehTuningTypeDefinition { TuningType = 39, ParentTuning = -1, Name = "Blok motoru", Price = 0, PriceItemId = 0 } },
      { 40, new VehTuningTypeDefinition { TuningType = 40, ParentTuning = -1, Name = "Vzduchový filtr", Price = 0, PriceItemId = 0 } },
      { 41, new VehTuningTypeDefinition { TuningType = 41, ParentTuning = -1, Name = "Vzpěry", Price = 0, PriceItemId = 0 } },
      { 42, new VehTuningTypeDefinition { TuningType = 42, ParentTuning = -1, Name = "Kryty", Price = 0, PriceItemId = 0 } },
      { 43, new VehTuningTypeDefinition { TuningType = 43, ParentTuning = -1, Name = "Antény", Price = 0, PriceItemId = 0 } },
      { 44, new VehTuningTypeDefinition { TuningType = 44, ParentTuning = -1, Name = "Zdobení 2", Price = 0, PriceItemId = 0 } },
      { 45, new VehTuningTypeDefinition { TuningType = 45, ParentTuning = -1, Name = "Nádrž", Price = 0, PriceItemId = 0 } },
      { 46, new VehTuningTypeDefinition { TuningType = 46, ParentTuning = -1, Name = "Levé dveře", Price = 0, PriceItemId = 0 } },
      { 47, new VehTuningTypeDefinition { TuningType = 47, ParentTuning = -1, Name = "Pravé dveře", Price = 0, PriceItemId = 0 } },
      { 48, new VehTuningTypeDefinition { TuningType = 48, ParentTuning = -1, Name = "Polepy", Price = 0, PriceItemId = 0 } },
      { 49, new VehTuningTypeDefinition { TuningType = 49, ParentTuning = -1, Name = "Lightbar", Price = 0, PriceItemId = 0 } },
    };
  }

  [AttributeUsage(AttributeTargets.Property)]
  public class VehTuningTypeAttribute : Attribute
  {
    public int TuningType { get; set; }

    public VehTuningTypeAttribute(int tuningType)
    {
      TuningType = tuningType;
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
