using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
  public class InventoryBag
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public int RowCount { get; set; }
    public List<InventoryItemBag> Items { get; set; }
  }

  public class InventoryItemBag
  {
    public int Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int? XSpan { get; set; }
    public int? YSpan { get; set; }
    public string Name { get; set; }
    public int[] Color { get; set; }
  }
}
