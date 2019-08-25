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
      indicator.text = "当前回合: " + turnNum + " " + party + "回合";
    }

    public override void UpdateChild() {}
  }

}