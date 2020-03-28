using System.Collections;
using UnitNS;
using System.Collections.Generic;

namespace MonoNS
{
  public class SettlementAnimationController : BaseController
  {
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      settlementMgr = hexMap.settlementMgr;
      eventDialog = hexMap.eventDialog;
    }

    public bool Animating = false;
    SettlementMgr settlementMgr;
    EventDialog eventDialog;

    public override void UpdateChild() {}

    public void DestroySiegeWall(SiegeWall siegeWall)
    {
      Animating = true;
      siegeWall.Destroy();
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoDestroySiegeWall(siegeWall));
    }

    IEnumerator CoDestroySiegeWall(SiegeWall siegeWall)
    {
      SiegeWallView view = (SiegeWallView)settlementMgr.GetView(siegeWall);
      view.DestroyAnimation();
      while (view.Animating) { yield return null; }
      view.Destroy();
      settlementMgr.settlement2GO.Remove(siegeWall);
      hexMap.cameraKeyboardController.EnableCamera();
      Animating = false;
    }
  }

}