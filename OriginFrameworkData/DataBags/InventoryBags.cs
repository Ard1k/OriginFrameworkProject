using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
  public class InventoryBag
  {
    public string Place { get; set; }
    public int RowCount 
    { 
      get 
      {
        return PlaceToRowCount(Place);
      } 
    }
    public List<InventoryItemBag> Items { get; set; }

    public InventoryBag(string place)
    {
      Place = place;
      Items = new List<InventoryItemBag>();
    }

    public static int PlaceToRowCount(string place)
    {
      if (string.IsNullOrEmpty(place))
        return -1;
      if (place.StartsWith("char_"))
        return 5;
      if (place.StartsWith("world_"))
        return 5;
      if (place.StartsWith("trunk_"))
        return 5;
      if (place.StartsWith("glovebox_"))
        return 5;

      return 5;
    }

    public static string GetPlaceName(string place)
    {
      if (string.IsNullOrEmpty(place))
        return "---";
      if (place.StartsWith("char_"))
        return "Hráč";
      if (place.StartsWith("world_"))
        return $"Svět {place.Substring(6)}";
      if (place.StartsWith("trunk_"))
        return $"Kufr {place.Substring(6)}";
      if (place.StartsWith("glovebox_"))
        return $"Přihrádka {place.Substring(9)}";

      return "---";
    }

    public int GetEmptySlotCount()
    {
      int rows = PlaceToRowCount(Place);
      if (rows < 0)
        return rows;

      int itemCount = Items.Where(it => it.Y >= 0).Count();
      return (rows * 5) - itemCount;
    }

    public bool GetNextPossibleSlot(out int x, out int y, int item_id, out InventoryItemBag slot)
    {
      x = 0;
      y = 0;
      int rowCnt = RowCount;
      slot = null;

      if (item_id > 0)
      {
        slot = Items.Where(it => it.ItemId == item_id && it.Count < ItemsDefinitions.Items[item_id].StackSize && (it.Metadata == null || it.Metadata.Count() <= 0)).FirstOrDefault();
        if (slot != null)
          return true;
      }

      for (int j = 0; rowCnt < 0 || j < rowCnt; j++)
      {
        for (int i = 0; i < 5; i++)
        {
          if (!Items.Any(it => it.X == i && it.Y == j))
          {
            x = i;
            y = j;
            return true;
          }
        }
      }

      return false;
    }
  }

  public class InventoryItemBag
  {
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string Place { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Count { get; set; }
    public string[] Metadata { get; set; }
    [JsonIgnore]
    public bool IsDragged { get; set; }
    [JsonIgnore]
    public bool IsWaitingActionResult { get; set; }

    public static InventoryItemBag ParseFromSql(Dictionary<string, object> row)
    {
      var it = new InventoryItemBag
      {
        Id = Convert.ToInt32(row["id"]),
        ItemId = Convert.ToInt32(row["item_id"]),
        Place = Convert.ToString(row["place"]),
        X = Convert.ToInt32(row["x"]),
        Y = Convert.ToInt32(row["y"]),
        Count = Convert.ToInt32(row["count"]),
      };

      if (row.ContainsKey("metadata") && row["metadata"] != null)
        it.Metadata = Convert.ToString(row["pos"])?.Split('|');

      return it;
    }
  }

  public class ItemDefinition
  {
    public string Name { get; set; }
    //public string Code { get; set; }
    public string Texture { get; set; }
    public InventoryColor Color { get; set; }
    public int StackSize { get; set; } = 1;
    [JsonIgnore]
    public bool Stackable { get { return StackSize > 0; } }
    public eSpecialSlotType? SpecialSlotType { get; set; }
    public Dictionary<string, int> MaleSkin { get; set; }
    public Dictionary<string, int> FemaleSkin { get; set; }
  }

  public class InventoryColor
  {
    public string Label { get; set; }
    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }

    public InventoryColor(string label, int r, int g, int b)
    {
      Label = label;
      R = r;
      G = g;
      B = b;
    }
  }

  public enum eItemCarryType : int
  {
    Inventory = 0,
    Hands = 1,
    Forklift = 2
  }

  public enum eSpecialSlotType : int
  {
    Head = 0,
    Mask = 1,

    Glasses = 2,
    Necklace = 3,

    Earring = 4,
    Wrist = 5,

    Torso = 6,
    Legs = 7,

    Boots = 8,
    Gloves = 9,
  }

  public static class ItemsDefinitions
  {
    private static ItemDefinition[] _items = null;
    public static ItemDefinition[] Items
    {
      get
      {
        if (_items == null)
          InitializeItems();

        return _items;
      }
    }

    private static Dictionary<string, InventoryColor> _knownColors = null;
    public static Dictionary<string, InventoryColor> KnownColors
    {
      get
      {
        if (_knownColors == null)
          InitializeColors();
        return _knownColors;
      }
    }

    private static void InitializeColors()
    {
      _knownColors = new Dictionary<string, InventoryColor>();
      _knownColors.Add("red", new InventoryColor("Červená", 255, 0, 0));
      _knownColors.Add("green", new InventoryColor("Zelená", 0, 255, 0));
      _knownColors.Add("blue", new InventoryColor("Modrá", 0, 0, 255));
      _knownColors.Add("white", new InventoryColor("Bílá", 255, 255, 255));
      _knownColors.Add("black", new InventoryColor("Černá", 0, 0, 0));
    }

    private static void InitializeItems()
    {
      _items = new ItemDefinition[2000];

      #region itemy
      _items[1] = new ItemDefinition
      {
        Name = "Autodíly",
        //Code = "ITEM_COMPONENT",
        Texture = "item_component",
        StackSize = 500,
      };
      #endregion

      #region hadry
      _items[1000] = new ItemDefinition
      {
        Name = "Kšiltovka",
        //Code = "CLOTHING_CAP1",
        Texture = "cap1",
        SpecialSlotType = eSpecialSlotType.Head,
        Color = KnownColors["green"],
        MaleSkin = new Dictionary<string, int> 
        { 
          { "helmet_1", 10 },
          { "helmet_2", 5 }
        }
      };
      _items[1001] = new ItemDefinition
      {
        Name = "Mikča",
        //Code = "CLOTHING_CAP1",
        Texture = "hoodie1",
        SpecialSlotType = eSpecialSlotType.Torso,
        Color = KnownColors["red"],
        MaleSkin = new Dictionary<string, int>
        {
          { "torso_1", 113 },
          { "torso_2", 0 },
          { "arms_1", 6 },
          { "arms_2", 0 }
        }
      };
      #endregion
    }
  }
}
