using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MonoNS;

public class InputField : BaseController
{

  public override void PreGameInit(HexMap hexMap, BaseController me)
  {
    base.PreGameInit(hexMap, me);
    input = this.GetComponentInChildren<Text>();
    self = this.transform.gameObject;
    self.SetActive(false);
  }

  GameObject self;
  Text input;

  public int GetInput()
  {
    int ret;
    if (!System.Int32.TryParse(input.text, out ret)) {
      throw new System.Exception("invalid input, must be number");
    }
    return ret; 
  }

  public void ActivateInput() {
    self.SetActive(true);
  }

  public void DeactivateInput() {
    self.SetActive(false);
  }

  public override void UpdateChild() {}
}