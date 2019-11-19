using UnityEngine;
using UnityEngine.UI;

namespace MonoNS
{
  public class MsgBox : BaseController
  {

    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
    }
    public Text msg;
    public void Show(string msg)
    {
      this.msg.text = msg;
    }

    public override void UpdateChild() {}

  }
}
