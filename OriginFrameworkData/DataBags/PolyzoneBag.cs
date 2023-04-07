using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
	public class PolyzoneBag
	{
		public PosBag Center { get; set; }
		public DimensionsBag Dimensions { get; set; }
		public string ActionName { get; set; }
		public Action OnAction { get; set; }
		public Func<bool> DisplayCondition { get; set; }
	}
}
