﻿using UnityEngine.UI;

namespace MonoNS
{

  public class TurnPhaseTitle : BaseController
  {

    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      title = this.GetComponent<Text>();
    }

    Text title;
    public void Set(string faction)
    {
      if (this != null && this.title != null)
      {
        this.title.text = faction + "回合";
      }
    }

    public void Clear()
    {
      this.title.text = "";
    }

    public override void UpdateChild() {}
  }

}