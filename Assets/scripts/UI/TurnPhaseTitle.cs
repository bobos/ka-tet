using UnityEngine;
using UnityEngine.UI;

namespace MonoNS
{

  public class TurnPhaseTitle : BaseController
  {

    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
    }

    public Text title;
    public Text subTitle;
    public void Set(string title, Color color, string subTitle, Color color1)
    {
      if (this != null && this.title != null)
      {
        this.title.text = title;
        this.title.color = color;
        this.subTitle.text = subTitle;
        this.subTitle.color = color1;
      }
    }

    public void Clear()
    {
      this.title.text = "";
      this.subTitle.text = "";
    }

    public override void UpdateChild() {}
  }

}