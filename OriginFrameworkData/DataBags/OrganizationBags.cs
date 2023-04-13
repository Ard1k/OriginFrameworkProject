using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
  public enum eOrganizationColor : int
  {
    None = 0,
    Red = 1,
    Blue = 2,
    Green = 3,
    Yellow = 4,
    Orange = 5,
    Purple = 6,
    Pink = 7,
    White = 8,
    Black = 9,
    Grey = 10,
  }

  public class OrganizationBag
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Tag { get; set; }
    public int Owner { get; set; }
    public eOrganizationColor OrganizationColor { get; set; }
    public OrganizationDataBag Data { get; set; }
    public List<OrganizationMemberBag> Members { get; set; } = new List<OrganizationMemberBag> { };

    public static OrganizationBag ParseFromSql(Dictionary<string, object> row)
    {
      var it = new OrganizationBag
      {
        Id = Convert.ToInt32(row["id"]),
        Name = Convert.ToString(row["name"]),
        Tag = Convert.ToString(row["tag"]),
        Owner = Convert.ToInt32(row["owner"]),
        OrganizationColor = (eOrganizationColor)Convert.ToInt32(row["color"]),
      };

      if (row.ContainsKey("data") && row["data"] != null && row["data"] != DBNull.Value)
        it.Data = JsonConvert.DeserializeObject<OrganizationDataBag>(Convert.ToString(row["data"]));

      return it;
    }
  }

  public class OrganizationDataBag
  {
    
  }

  public class OrganizationMemberBag
  {
    public int CharId { get; set; }
    public string CharName { get; set; }
  }

  public class OrganizationInviteBag
  {
    public int CharId { get; set; }
    public int OrganizationId { get; set; }
    public string InvitedByHandle { get; set; }
    public DateTime Created { get; set; }

    public OrganizationInviteBag(int charId, int organizationId, string invitedByHandle)
    {
      Created = DateTime.Now;
      CharId = charId;
      OrganizationId = organizationId;
      InvitedByHandle = invitedByHandle;
    }
  }
}
