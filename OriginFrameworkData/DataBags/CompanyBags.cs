using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
	public class PlayerCompanyBag
	{
		public string CompanyName { get; set; }
		public string CompanyCode { get; set; }
		public bool IsCompanyManager { get; set; }
		public bool IsCompanyOwner { get; set; }
	}
}
