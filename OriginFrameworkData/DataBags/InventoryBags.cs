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

      return 5;
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
    public string Code { get; set; }
    public string Texture { get; set; }
    public int StackSize { get; set; }
    [JsonIgnore]
    public bool Stackable { get { return StackSize > 0; } }
    public eItemClothingSlot ClothingSlot { get; set; }
  }

  public enum eItemCarryType : int
  {
    Inventory = 0,
    Hands = 1,
    Forklift = 2
  }

  public enum eItemClothingSlot : int
  {
    None = 0,
    Head = 1,
    Glasses = 2,
    Earring = 3,
    Necklace = 4,
    Mask = 5,
    Torso = 6,
    Legs = 7,
    Boots = 8
  }

  public static class ItemsDefinitions
  {
    private static ItemDefinition[] _items = null;
    public static ItemDefinition[] Items
    {
      get
      {
        if (_items == null)
          Initialize();

        return _items;
      }
    }

    private static void Initialize()
    {
      _items = new ItemDefinition[1000];

      _items[1] = new ItemDefinition
      {
        Name = "Autodíly",
        Code = "ITEM_COMPONENT",
        Texture = "item_component",
        StackSize = 500,
      };
    }
  }
}
