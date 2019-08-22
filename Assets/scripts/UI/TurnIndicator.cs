using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MonoNS
{
  public class TurnIndicator : BaseController
  {

    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
    }

    public Text indicator;
    public void Set(int turnNum, string party)
    {
      indicator.text = "Turn: " + turnNum + " " + party + "'s turn";
    }

    public override void UpdateChild() {}
  }

}