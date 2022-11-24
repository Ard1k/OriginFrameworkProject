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
        return 0;
      if (place.StartsWith("char_"))
        return 5;
      if (place.StartsWith("world_"))
        return 10;
      if (place.StartsWith("trunk_"))
      {
        var splits = place.Split('_');
        int category = 0;
        if (splits.Length < 3 || !Int32.TryParse(splits[2], out category))
          return 0;
        switch (category)
        {
          default: return 3;
        }
      }
      if (place.StartsWith("glovebox_"))
        return 1;

      return 0;
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
      {
        var splits = place.Split('_');
        return $"Kufr {splits[1]}";
      }
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

    public bool HasEnoughItems(int item_id, int count)
    {
      var correctItems = Items?.Where(it => it.ItemId == item_id)?.ToList();

      if (correctItems == null || correctItems.Count <= 0)
        return false;

      int inventoryCount = correctItems.Sum(it => it.Count);

      return count <= inventoryCount;
    }

    public InventoryItemBag GetNextItemOfType(int item_id)
    {
      return Items.Where(it => it.ItemId == item_id).FirstOrDefault();
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
    public int ItemId { get; set; }
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

    public ItemDefinition GetInstanceCopy()
    {
      return new ItemDefinition
      {
        ItemId = this.ItemId,
        Name = this.Name,
        Texture = this.Texture,
        Color = this.Color,
        StackSize = this.StackSize,
        SpecialSlotType = this.SpecialSlotType,
        MaleSkin = this.MaleSkin,
        FemaleSkin = this.FemaleSkin,
      };
    }
  }

  public enum eInventoryColor : int
  {
    None = 0,
    Bílá = 1,
    Červená = 2,
    Zelená = 3,
    Modrá = 4,
    Oranžová = 5,
    Žlutá = 6,
    Fialová = 7,
    Černá = 8,
    Růžová = 9,
  }

  public class InventoryColor
  {
    [JsonIgnore]
    public string Label 
    { 
      get 
      {
        if (ColorEnum == 0)
          return null;
        return ColorEnum.ToString();
      } 
    }
    public eInventoryColor ColorEnum { get; set; }
    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }

    public InventoryColor(eInventoryColor color, int r, int g, int b)
    {
      ColorEnum = color;
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

    public static List<string> KnownClothesIcons { get; set; } = new List<string> 
    {
      //headky
      "cap1",

      //masky
      "mask1",

      //bryle
      "glasses1",

      //necklace
      "necklace1",

      //earring
      "earrings1",

      //wrist
      "watch1",

      //torso
      "hoodie1",
      "shirt1",

      //legs
      "shorts1",
      "trousers1",

      //Boots
      "sneakers1",
      "boots1"
    };

    private static void InitializeColors()
    {
      _knownColors = new Dictionary<string, InventoryColor>();
      _knownColors.Add("red", new InventoryColor(eInventoryColor.Červená, 255, 0, 0));
      _knownColors.Add("green", new InventoryColor(eInventoryColor.Zelená, 0, 255, 0));
      _knownColors.Add("blue", new InventoryColor(eInventoryColor.Modrá, 0, 0, 255));
      _knownColors.Add("white", new InventoryColor(eInventoryColor.Bílá, 255, 255, 255));
      _knownColors.Add("black", new InventoryColor(eInventoryColor.Černá, 55, 55, 55));
    }

    private static void InitializeItems()
    {
      _items = new ItemDefinition[2000];

      #region itemy
      _items[1] = new ItemDefinition
      {
        ItemId = 1,
        Name = "Autodíly",
        //Code = "ITEM_COMPONENT",
        Texture = "item_component",
        StackSize = 500,
      };
      #endregion

      #region hadry
      _items[1000] = new ItemDefinition
      {
        ItemId = 1000,
        Name = "Kužel hlupák",
        Texture = "cap1",
        SpecialSlotType = eSpecialSlotType.Head,
        Color = KnownColors["white"],
        MaleSkin = new Dictionary<string, int>
        {
          { "helmet_1", 1 },
          { "helmet_2", 0 },
        },
        FemaleSkin = new Dictionary<string, int>
        {
          { "helmet_1", 1 },
          { "helmet_2", 0 },
        },
      };
      _items[1001] = new ItemDefinition
      {
        ItemId = 1001,
        Name = "Kulich",
        Texture = "cap1",
        SpecialSlotType = eSpecialSlotType.Head,
        Color = KnownColors["red"],
        MaleSkin = new Dictionary<string, int>
        {
          { "helmet_1", 2 },
          { "helmet_2", 7 },
        },
        FemaleSkin = new Dictionary<string, int>
        {
          { "helmet_1", 29 },
          { "helmet_2", 2 },
        },
      };
      _items[1002] = new ItemDefinition
      {
        ItemId = 1002,
        Name = "Maska vraha",
        Texture = "mask1",
        SpecialSlotType = eSpecialSlotType.Mask,
        Color = KnownColors["blue"],
        MaleSkin = new Dictionary<string, int>
        {
          { "mask_1", 14 },
          { "mask_2", 0 },
        },
        FemaleSkin = new Dictionary<string, int>
        {
          { "mask_1", 14 },
          { "mask_2", 0 },
        },
      };
      _items[1003] = new ItemDefinition
      {
        ItemId = 1003,
        Name = "Maska býka",
        Texture = "mask1",
        SpecialSlotType = eSpecialSlotType.Mask,
        Color = KnownColors["black"],
        MaleSkin = new Dictionary<string, int>
        {
          { "mask_1", 23 },
          { "mask_2", 0 },
        },
        FemaleSkin = new Dictionary<string, int>
        {
          { "mask_1", 23 },
          { "mask_2", 0 },
        },
      };
      _items[1004] = new ItemDefinition
      {
        ItemId = 1004,
        Name = "Zelený brejle",
        Texture = "glasses1",
        SpecialSlotType = eSpecialSlotType.Glasses,
        Color = KnownColors["green"],
        MaleSkin = new Dictionary<string, int>
        {
          { "glasses_1", 19 },
          { "glasses_2", 3 },
        },
      };
      _items[1005] = new ItemDefinition
      {
        ItemId = 1005,
        Name = "Lyžařský brejle",
        Texture = "glasses1",
        SpecialSlotType = eSpecialSlotType.Glasses,
        Color = KnownColors["blue"],
        MaleSkin = new Dictionary<string, int>
        {
          { "glasses_1", 25 },
          { "glasses_2", 0 },
        },
        FemaleSkin = new Dictionary<string, int>
        {
          { "glasses_1", 27 },
          { "glasses_2", 0 },
        },
      };
      _items[1006] = new ItemDefinition
      {
        ItemId = 1006,
        Name = "Stříbrný řetízek",
        Texture = "necklace1",
        SpecialSlotType = eSpecialSlotType.Necklace,
        Color = KnownColors["white"],
        MaleSkin = new Dictionary<string, int>
        {
          { "chain_1", 17 },
          { "chain_2", 0 },
        },
        FemaleSkin = new Dictionary<string, int>
        {
          { "chain_1", 56 },
          { "chain_2", 1 },
        },
      };
      _items[1007] = new ItemDefinition
      {
        ItemId = 1007,
        Name = "Náhrdelník",
        Texture = "necklace1",
        SpecialSlotType = eSpecialSlotType.Necklace,
        Color = KnownColors["white"],
        FemaleSkin = new Dictionary<string, int>
        {
          { "chain_1", 7 },
          { "chain_2", 1 },
        },
      };
      _items[1008] = new ItemDefinition
      {
        ItemId = 1008,
        Name = "Handsfree",
        Texture = "earrings1",
        SpecialSlotType = eSpecialSlotType.Earring,
        Color = KnownColors["black"],
        MaleSkin = new Dictionary<string, int>
        {
          { "ears_1", 0 },
          { "ears_2", 0 },
        },
        FemaleSkin = new Dictionary<string, int>
        {
          { "ears_1", 0 },
          { "ears_2", 0 },
        },
      };
      _items[1009] = new ItemDefinition
      {
        ItemId = 1009,
        Name = "Hodinky a náramek",
        Texture = "watch1",
        SpecialSlotType = eSpecialSlotType.Wrist,
        Color = KnownColors["black"],
        MaleSkin = new Dictionary<string, int>
        {
          { "watches_1", 13 },
          { "watches_2", 2 },
          { "bracelets_1", 1 },
          { "bracelets_2", 0 },
        },
        FemaleSkin = new Dictionary<string, int>
        {
          { "watches_1", 10 },
          { "watches_2", 1 },
          { "bracelets_1", 8 },
          { "bracelets_2", 0 },
        },

      };
      _items[1010] = new ItemDefinition
      {
        ItemId = 1010,
        Name = "Obyč triko",
        Texture = "shirt1",
        SpecialSlotType = eSpecialSlotType.Torso,
        Color = KnownColors["red"],
        MaleSkin = new Dictionary<string, int>
        {
          { "tshirt_1", 15 },
          { "tshirt_2", 0 },
          { "torso_1", 16 },
          { "torso_2", 2 },
          { "arms_1", 0 },
          { "arms_2", 0 },
          { "decals_1", 0 },
          { "decals_2", 0 },
          { "bproof_1", 0 },
          { "bproof_2", 0 },
        },
        FemaleSkin = new Dictionary<string, int>
        {
          { "tshirt_1", 14 },
          { "tshirt_2", 0 },
          { "torso_1", 30 },
          { "torso_2", 2 },
          { "arms_1", 2 },
          { "arms_2", 0 },
          { "decals_1", 0 },
          { "decals_2", 0 },
          { "bproof_1", 0 },
          { "bproof_2", 0 },
        },
      };
      _items[1011] = new ItemDefinition
      {
        ItemId = 1011,
        Name = "Mikina",
        Texture = "hoodie1",
        SpecialSlotType = eSpecialSlotType.Torso,
        Color = KnownColors["blue"],
        MaleSkin = new Dictionary<string, int>
        {
          { "tshirt_1", 15 },
          { "tshirt_2", 0 },
          { "torso_1", 113 },
          { "torso_2", 2 },
          { "arms_1", 6 },
          { "arms_2", 0 },
          { "decals_1", 0 },
          { "decals_2", 0 },
          { "bproof_1", 0 },
          { "bproof_2", 0 },
        },
        FemaleSkin = new Dictionary<string, int>
        {
          { "tshirt_1", 14 },
          { "tshirt_2", 0 },
          { "torso_1", 106 },
          { "torso_2", 2 },
          { "arms_1", 6 },
          { "arms_2", 0 },
          { "decals_1", 0 },
          { "decals_2", 0 },
          { "bproof_1", 0 },
          { "bproof_2", 0 },
        },
      };
      _items[1012] = new ItemDefinition
      {
        ItemId = 1012,
        Name = "Tepláky",
        Texture = "trousers1",
        SpecialSlotType = eSpecialSlotType.Legs,
        Color = KnownColors["black"],
        MaleSkin = new Dictionary<string, int>
        {
          { "pants_1", 55 },
          { "pants_2", 0 },
        },
        FemaleSkin = new Dictionary<string, int>
        {
          { "pants_1", 58 },
          { "pants_2", 0 },
        },
      };
      _items[1013] = new ItemDefinition
      {
        ItemId = 1013,
        Name = "Sneakersky",
        Texture = "sneakers1",
        SpecialSlotType = eSpecialSlotType.Boots,
        Color = KnownColors["red"],
        MaleSkin = new Dictionary<string, int>
        {
          { "shoes_1", 77 },
          { "shoes_2", 20 },
        },
        FemaleSkin = new Dictionary<string, int>
        {
          { "shoes_1", 81 },
          { "shoes_2", 20 },
        },
      };
      #endregion
    }
  }
}
