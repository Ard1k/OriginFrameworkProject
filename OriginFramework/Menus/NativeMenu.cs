using CitizenFX.Core;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using OriginFrameworkData;

namespace OriginFramework.Menus
{
  public class NativeMenu
  {
    public int SelectedIndex { get; set; } = 0;
    public int SelectedIndexVisible { get; set; } = 0;
    public int MaxMenuItemsVisible { get; set; } = 15;
    public List<NativeMenuItem> Items { get; set; }
    public NativeMenu PreviousMenu { get; set; }
    public NativeMenuSprite Sprite { get; set; }
    public string MenuTitle { get; set; }

    public NativeMenu()
    {
      Items = new List<NativeMenuItem>();
    }
  }

  public class NativeMenuItem
  {
    public string Name { get; set; }
    public string NameRight { get; set; }
    public string ExtraLeft { get; set; }
    public string ExtraRight { get; set; }

    public int[] ColorOverride { get; set; }

    public string Icon { get; set; }

    public bool IsBack { get; set; }
    public bool IsNavBack { get; set; }
    public bool IsUnselectable { get; set; }
    public bool IsHide { get; set; }
    public bool IsActive { get; set; }
    public bool IsColorAsUnavailable { get; set; }

    public bool IsTextInput { get; set; }
    public string TextInputRequest { get; set; } = "Enter text";
    public string TextInputPrefill { get; set; } = String.Empty;
    public int TextInputMaxLength { get; set; } = 60;

    public Action OnHover { get; set; }
    public Action<NativeMenuItem> OnSelected { get; set; }
    public Action<string> OnTextInput { get; set; }
    public Action<NativeMenuItem> OnRefresh { get; set; }
    public Func<NativeMenu> GetSubMenu { get; set; }

    public NativeMenu SubMenu { get; set; }
  }

  public class NativeMenuSprite
  {
    public float X { get; set; }
    public float Y { get; set; }
    public string TextureDict { get; set; }
    public string TextureName { get; set; }
  }
}
