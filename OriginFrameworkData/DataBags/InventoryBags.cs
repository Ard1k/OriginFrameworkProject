using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
  public class InventoryBag
  {
    public string Place { get; set; }
    private int? _rowCountNormal = null;
    private int? _rowCountHands = null;
    private int? _rowCountForklift = null;
    private int? _columnCountNormal = null;
    [JsonIgnore]
    public int RowCount 
    { 
      get 
      {
        if (_rowCountNormal == null)
         _rowCountNormal = PlaceToRowCount(Place, eItemCarryType.Inventory);
        
        return _rowCountNormal.Value;
      } 
    }
    [JsonIgnore]
    public int RowCountHands
    {
      get
      {
        if (_rowCountHands == null)
          _rowCountHands = PlaceToRowCount(Place, eItemCarryType.Hands);

        return _rowCountHands.Value;
      }
    }
    [JsonIgnore]
    public int RowCountForklift
    {
      get
      {
        if (_rowCountForklift == null)
          _rowCountForklift = PlaceToRowCount(Place, eItemCarryType.Forklift);

        return _rowCountForklift.Value;
      }
    }
    [JsonIgnore]
    public int ColumnCount
    {
      get
      {
        if (_columnCountNormal == null)
          _columnCountNormal = PlaceToColumnCount(Place, eItemCarryType.Inventory);

        return _columnCountNormal.Value;
      }
    }
    public List<InventoryItemBag> Items { get; set; } = new List<InventoryItemBag>();

    [JsonIgnore]
    public int ScrollOffset { get; set; } = 0;

    public InventoryBag(string place)
    {
      Place = place;
    }

    public bool IsDisableMoveControls()
    {
      if (string.IsNullOrEmpty(Place) || Place.StartsWith("glovebox"))
        return false;

      return true;
    }

    public static int PlaceToRowCount(string place, eItemCarryType carryType)
    {
      if (string.IsNullOrEmpty(place))
        return 0;

      if (carryType != eItemCarryType.Inventory && !place.StartsWith("trunk_") && !place.StartsWith("fork_"))
        return 0;

      if (place.StartsWith("char_"))
        return 5;
      if (place.StartsWith("fork_"))
        return 1;
      if (place.StartsWith("world_"))
        return 10;
      if (place.StartsWith("trunk_"))
      {
        var splits = place.Split('_');
        int category = 0;
        int model = 0;
        if (splits.Length < 4 || !Int32.TryParse(splits[2], out category))
          return 0;
        if (splits.Length < 4 || !Int32.TryParse(splits[3], out model))
          return 0;
        return VehicleTrunkSize.GetTrunkRowcount(category, model, carryType);
      }
      if (place.StartsWith("glovebox_"))
        return 1;

      return 0;
    }

    public static int PlaceToColumnCount(string place, eItemCarryType carryType)
    {
      if (string.IsNullOrEmpty(place))
        return 0;

      if (place.StartsWith("fork_"))
        return 1;

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
      {
        var splits = place.Split('_');
        return $"Kufr {splits[1]}";
      }
      if (place.StartsWith("glovebox_"))
        return $"Přihrádka {place.Substring(9)}";
      if (place.StartsWith("fork_"))
        return $"Ještěrka {place.Substring(5)}";

      return "---";
    }

    //public int GetEmptySlotCount()
    //{
    //  int rows = PlaceToRowCount(Place);
    //  if (rows < 0)
    //    return rows;

    //  int itemCount = Items.Where(it => it.Y >= 0).Count();
    //  return (rows * 5) - itemCount;
    //}

    public bool GetNextPossibleSlot(out int x, out int y, int item_id, out InventoryItemBag slot)
    {
      x = 0;
      y = 0;
      int rowCnt = RowCount;
      int colCnt = ColumnCount;
      slot = null;

      if (item_id > 0)
      {
        slot = Items.Where(it => it.ItemId == item_id && (it.Metadata == null || it.Metadata.Length == 0) && it.Count < ItemsDefinitions.Items[item_id].StackSize && (it.Metadata == null || it.Metadata.Count() <= 0)).FirstOrDefault();
        if (slot != null)
          return true;
      }

      for (int j = 0; rowCnt < 0 || j < rowCnt; j++)
      {
        for (int i = 0; i < colCnt; i++)
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
    public string RelatedTo { get; set; } //napr. pro SPZky... potrebuju neco k cemu se rychle bez parsovani dostanu 
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
        it.Metadata = Convert.ToString(row["metadata"])?.Split('|')?.Where(m => 
          m != null &&
          !m.StartsWith("_charid") &&
          !m.StartsWith("_model") &&
          !m.StartsWith("_skin")
          )?.ToArray();

      if (row.ContainsKey("related_to") && row["related_to"] != null)
        it.RelatedTo = Convert.ToString(row["related_to"]);

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
    public eUsableType UsableType { get; set; }
    public int WeaponHash { get; set; }
    public int AmmoItemId { get; set; }
    public AnimationBag UseAnim { get; set; }
    public AnimationBag PutAwayAnim { get; set; }
    public bool IsMoney { get; set; }
    public eItemCarryType CarryType { get; set; }
    public InventoryCarryInfo CarryInfo { get; set; }

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
        UsableType = this.UsableType,
        WeaponHash = this.WeaponHash,
        AmmoItemId = this.AmmoItemId,
        UseAnim = this.UseAnim?.GetInstanceCopy(),
        PutAwayAnim = this.PutAwayAnim?.GetInstanceCopy(),
        IsMoney = this.IsMoney,
        CarryType = this.CarryType,
        CarryInfo = this.CarryInfo,
      };
    }

    public string FormatAmount(int amount, bool addSign = false)
    {
      if (IsMoney)
        return ((decimal)amount / 100).ToString("#,0.##").Replace(',', ' ') + "$";

      return ((amount >= 0 && addSign) ? "+" : String.Empty) + amount.ToString();
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

  public class InventoryCarryInfo
  {
    public string PropName { get; set; }
    public string CarryAnim { get; set; }
    public int PedBoneId { get; set; }
    public string EntityBoneName { get; set; } //forklift only
    public float XPos { get; set; }
    public float YPos { get; set; }
    public float ZPos { get; set; }
    public float XRot { get; set; }
    public float YRot { get; set; }
    public float ZRot { get; set; }
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

  public enum eUsableType : int
  {
    None = 0,
    Weapon = 1,
    IdentityCard = 2,
    CarKey = 3
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
        Texture = "item_component",
        StackSize = 500,
      };

      _items[2] = new ItemDefinition
      {
        ItemId = 2,
        Name = "Náboje malé ráže",
        Texture = "ammo_pistol",
        StackSize = 30,
      };
      _items[3] = new ItemDefinition
      {
        ItemId = 3,
        Name = "Náboje velké ráže",
        Texture = "ammo_rifle",
        StackSize = 50,
      };
      _items[4] = new ItemDefinition
      {
        ItemId = 4,
        Name = "Náboje do brokovnic",
        Texture = "ammo_shotgun",
        StackSize = 20,
      };

      _items[10] = new ItemDefinition
      {
        ItemId = 10,
        Name = "Combat pistol",
        Texture = "gun",
        StackSize = 1,
        AmmoItemId = 2,
        UsableType = eUsableType.Weapon,
        WeaponHash = 1593441988,
        UseAnim = new AnimationBag 
        {
          AnimDict = "reaction@intimidation@1h",
          AnimName = "intro",
          Speed = 4f,
          IsAllowRotation = true,
          IsUpperBodyOnly = true,
          Time = 2000
        },
        PutAwayAnim = new AnimationBag
        {
          AnimDict = "reaction@intimidation@1h",
          AnimName = "outro",
          Speed = 8f,
          IsAllowRotation = true,
          IsUpperBodyOnly = true,
          Time = 1400
        }
      };
      _items[11] = new ItemDefinition
      {
        ItemId = 11,
        Name = "Heavy pistol",
        Texture = "gun",
        StackSize = 1,
        AmmoItemId = 2,
        UsableType = eUsableType.Weapon,
        WeaponHash = -771403250,
        UseAnim = new AnimationBag
        {
          AnimDict = "reaction@intimidation@1h",
          AnimName = "intro",
          Speed = 4f,
          IsAllowRotation = true,
          IsUpperBodyOnly = true,
          Time = 2000
        },
        PutAwayAnim = new AnimationBag
        {
          AnimDict = "reaction@intimidation@1h",
          AnimName = "outro",
          Speed = 8f,
          IsAllowRotation = true,
          IsUpperBodyOnly = true,
          Time = 1400
        }
      };
      _items[12] = new ItemDefinition
      {
        ItemId = 12,
        Name = "UZI",
        Texture = "micro_smg",
        StackSize = 1,
        AmmoItemId = 2,
        UsableType = eUsableType.Weapon,
        WeaponHash = 324215364,
        UseAnim = new AnimationBag
        {
          AnimDict = "reaction@intimidation@1h",
          AnimName = "intro",
          Speed = 4f,
          IsAllowRotation = true,
          IsUpperBodyOnly = true,
          Time = 2000
        },
        PutAwayAnim = new AnimationBag
        {
          AnimDict = "reaction@intimidation@1h",
          AnimName = "outro",
          Speed = 8f,
          IsAllowRotation = true,
          IsUpperBodyOnly = true,
          Time = 1400
        }
      };
      _items[13] = new ItemDefinition
      {
        ItemId = 13,
        Name = "SMG",
        Texture = "submachine",
        StackSize = 1,
        AmmoItemId = 2,
        UsableType = eUsableType.Weapon,
        WeaponHash = 736523883,
        UseAnim = new AnimationBag
        {
          AnimDict = "reaction@intimidation@1h",
          AnimName = "intro",
          Speed = 4f,
          IsAllowRotation = true,
          IsUpperBodyOnly = true,
          Time = 2000
        },
        PutAwayAnim = new AnimationBag
        {
          AnimDict = "reaction@intimidation@1h",
          AnimName = "outro",
          Speed = 8f,
          IsAllowRotation = true,
          IsUpperBodyOnly = true,
          Time = 1400
        }
      };
      _items[14] = new ItemDefinition
      {
        ItemId = 14,
        Name = "Special Carbine",
        Texture = "assault_rifle",
        StackSize = 1,
        AmmoItemId = 3,
        UsableType = eUsableType.Weapon,
        WeaponHash = -1063057011,
        UseAnim = new AnimationBag
        {
          AnimDict = "combat@combat_reactions@rifle_react_messy",
          AnimName = "0",
          Speed = 8f,
          IsAllowRotation = true,
          IsUpperBodyOnly = true,
          Time = 1400
        },
        PutAwayAnim = new AnimationBag
        {
          AnimDict = "combat@combat_reactions@rifle_react_messy",
          AnimName = "0",
          Speed = 8f,
          IsAllowRotation = true,
          IsUpperBodyOnly = true,
          Time = 1400
        }
      };
      _items[15] = new ItemDefinition
      {
        ItemId = 15,
        Name = "Brokovnice",
        Texture = "shotgun",
        StackSize = 1,
        AmmoItemId = 4,
        UsableType = eUsableType.Weapon,
        WeaponHash = 2017895192,
        UseAnim = new AnimationBag
        {
          AnimDict = "combat@combat_reactions@rifle_react_messy",
          AnimName = "0",
          Speed = 8f,
          IsAllowRotation = true,
          IsUpperBodyOnly = true,
          Time = 1400
        },
        PutAwayAnim = new AnimationBag
        {
          AnimDict = "combat@combat_reactions@rifle_react_messy",
          AnimName = "0",
          Speed = 8f,
          IsAllowRotation = true,
          IsUpperBodyOnly = true,
          Time = 1400
        }
      };
      _items[16] = new ItemDefinition
      {
        ItemId = 16,
        Name = "AK-47",
        Texture = "rifle",
        StackSize = 1,
        AmmoItemId = 3,
        UsableType = eUsableType.Weapon,
        WeaponHash = -1074790547,
        UseAnim = new AnimationBag
        {
          AnimDict = "combat@combat_reactions@rifle_react_messy",
          AnimName = "0",
          Speed = 8f,
          IsAllowRotation = true,
          IsUpperBodyOnly = true,
          Time = 1400
        },
        PutAwayAnim = new AnimationBag
        {
          AnimDict = "combat@combat_reactions@rifle_react_messy",
          AnimName = "0",
          Speed = 8f,
          IsAllowRotation = true,
          IsUpperBodyOnly = true,
          Time = 1400
        }
      };
      _items[17] = new ItemDefinition
      {
        ItemId = 17,
        Name = "Peníze",
        Texture = "money1",
        StackSize = 100000,
        IsMoney = true
      };
      _items[18] = new ItemDefinition
      {
        ItemId = 18,
        Name = "Krabice do ruky",
        Texture = "crate",
        StackSize = 1,
        Color = new InventoryColor(0, 0, 0, 150),
        CarryType = eItemCarryType.Hands,
        CarryInfo = new InventoryCarryInfo
        {
          PropName = "ng_proc_box_01a",
          CarryAnim = "anim@heists@box_carry@",
          PedBoneId = 60309,
          XPos = 0.135f,
          YPos = -0.1f,
          ZPos = 0.22f,
          XRot = -125.0f,
          YRot = 100.0f,
          ZRot = 0.0f
        }
      };
      _items[19] = new ItemDefinition
      {
        ItemId = 19,
        Name = "Krabice do na ještěrku",
        Texture = "crate",
        StackSize = 1,
        Color = new InventoryColor(0, 150, 0, 0),
        CarryType = eItemCarryType.Forklift,
        CarryInfo = new InventoryCarryInfo
        {
          PropName = "prop_box_wood07a",
          //CarryAnim = "anim@heists@box_carry@",
          //PedBoneId = 60309,
          EntityBoneName = "forks_attach",
          XPos = 0.0f,
          YPos = 0.0f,
          ZPos = 0.0f,
          XRot = 0.0f,
          YRot = 0.0f,
          ZRot = 0.0f
        }
      };
      _items[20] = new ItemDefinition
      {
        ItemId = 20,
        Name = "Řidičský průkaz",
        Texture = "item_card_driver",
        StackSize = 1,
        Color = new InventoryColor(0, 255, 255, 255),
        UsableType = eUsableType.IdentityCard
      };
      _items[21] = new ItemDefinition
      {
        ItemId = 21,
        Name = "Průkaz policisty",
        Texture = "item_card_police",
        StackSize = 1,
        Color = new InventoryColor(0, 255, 255, 255),
        UsableType = eUsableType.IdentityCard
      };
      _items[22] = new ItemDefinition
      {
        ItemId = 22,
        Name = "Zbrojní průkaz",
        Texture = "item_card_weapon",
        StackSize = 1,
        Color = new InventoryColor(0, 255, 255, 255),
        UsableType = eUsableType.IdentityCard
      };
      _items[23] = new ItemDefinition
      {
        ItemId = 23,
        Name = "Klíče",
        Texture = "carkey_basic",
        StackSize = 1,
        Color = new InventoryColor(0, 255, 255, 255),
        UsableType = eUsableType.CarKey
      };
      #endregion

      #region hadry default
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
