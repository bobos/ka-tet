using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonoNS
{
  public class CameraKeyboardController : BaseController
  {

    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      turnController = hexMap.turnController;
    }

    float moveSpeed = 20;
    TurnController turnController;

    public override void UpdateChild()
    {
      if (turnController.showingTitle) return;
      Vector3 translate = new Vector3(
        Input.GetAxis("Horizontal"),
        0,
        Input.GetAxis("Vertical")
      );
      this.transform.Translate(translate * moveSpeed * Time.deltaTime, Space.World);
    }
  }

}