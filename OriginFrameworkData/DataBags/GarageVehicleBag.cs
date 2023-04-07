using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
  public class GarageVehicleBag
  {
    public int Id { get; set; }
    public string Model { get; set; }
    public string Plate { get; set; }
    public string Place { get; set; }
    public VehiclePropertiesBag Properties { get; set; }
    public int OwnerChar { get; set; }
    public int OwnerOrganization { get; set; }
    public bool IsOut { get; set; }

    public static GarageVehicleBag ParseFromSql(Dictionary<string, object> row)
    {
      var it = new GarageVehicleBag
      {
        Id = Convert.ToInt32(row["id"]),
        Model = Convert.ToString(row["model"]),
        Plate = Convert.ToString(row["plate"]),
        Place = Convert.ToString(row["place"]),
        Properties = JsonConvert.DeserializeObject<VehiclePropertiesBag>(Convert.ToString(row["properties"])),
        OwnerChar = Convert.ToInt32(row["owner_char"]),
        OwnerOrganization = Convert.ToInt32(row["owner_organization"]),
      };

      return it;
    }
  }
}
