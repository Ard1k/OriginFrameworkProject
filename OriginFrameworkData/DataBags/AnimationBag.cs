using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginFrameworkData.DataBags
{
  public class AnimationBag
  {
    public string AnimDict { get; set; }
    public string AnimName { get; set; }
    public float? Speed { get; set; }
    public int Time { get; set; }
    public bool IsUpperBodyOnly { get; set; }
    public bool IsAllowRotation { get; set; }
    public string SoundName { get; set; }

    public AnimationBag GetInstanceCopy()
    {
      return new AnimationBag
      {
        AnimDict = this.AnimDict,
        AnimName = this.AnimName,
        Speed = this.Speed,  
        Time = this.Time,
        IsAllowRotation = this.IsAllowRotation,
        IsUpperBodyOnly = this.IsUpperBodyOnly,
      };
    }
  }
}
