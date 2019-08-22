using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MonoNS
{
  public class HoverInfo : BaseController
  {

    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      hover = this.GetComponent<Text>();
    }

    Text hover;

    // Update is called once per frame
    public void Show(string msg)
    {
      hover.text = msg;
    }

    public override void UpdateChild() {}
  }

}