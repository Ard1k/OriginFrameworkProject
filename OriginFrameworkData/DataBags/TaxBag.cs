using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
	public struct TaxBag
	{
		public int Cars { get; set; }
		public int Clothes { get; set; }
		public int Weapons { get; set; }
		public int Food { get; set; }
		public int Medical { get; set; }
		public int Fuel { get; set; }

		public int Income { get; set; }

    public static TaxBag ParseFromSql(Dictionary<string, object> row)
    {
      var it = new TaxBag
      {
        Cars = Convert.ToInt32(row["cars"]),
        Clothes = Convert.ToInt32(row["clothes"]),
        Weapons = Convert.ToInt32(row["weapons"]),
        Food = Convert.ToInt32(row["food"]),
        Medical = Convert.ToInt32(row["medical"]),
        Fuel = Convert.ToInt32(row["fuel"]),

        Income = Convert.ToInt32(row["income"]),
      };

      return it;
    }

    public int GetPriceAfterTax(int price, eTaxType taxType)
    {
      switch (taxType)
      {
        case eTaxType.Income:
          return price + (price * Income / 100);
        case eTaxType.Cars:
          return price + (price * Cars / 100);
        case eTaxType.Clothes:
          return price + (price * Clothes / 100);
        case eTaxType.Weapons:
          return price + (price * Weapons / 100);
        case eTaxType.Food:
          return price + (price * Food / 100);
        case eTaxType.Medical:
          return price + (price * Medical / 100);
        case eTaxType.Fuel:
          return price + (price * Fuel / 100);
        default:
          {
            return price;
          }
      }
    }
  }

	public enum eTaxType : int
	{
    None = 0,

		Cars = 1,
    Clothes = 2,
    Weapons = 3,
		Food = 4,
		Medical = 5,
		Fuel = 6,

		Income = 7
	}
}
