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

    public List<Unit> DestroySettlement(Settlement settlement, BuildingNS.DestroyType type)
    {
      Animating = true;
      List<Unit> garrison = settlement.Destroy();
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoDestroy(settlement, type));
      return garrison;
    }

    IEnumerator CoDestroy(Settlement settlement, BuildingNS.DestroyType type)
    {
      SettlementView view = (SettlementView)settlementMgr.GetView(settlement);
      view.DestroyAnimation(type);
      while (view.Animating) { yield return null; }
      view.Destroy();
      settlementMgr.settlement2GO.Remove(settlement);
      eventDialog.Show(new Event(
          type == BuildingNS.DestroyType.ByFlood ?
          EventDialog.EventName.FloodDestroyCamp :
          EventDialog.EventName.WildFireDestroyCamp, null, settlement));
      while (eventDialog.Animating) { yield return null; }
      hexMap.cameraKeyboardController.EnableCamera();
      Animating = false;
    }

    public void DestroySiegeWall(SiegeWall siegeWall, BuildingNS.DestroyType type)
    {
      Animating = true;
      siegeWall.Destroy();
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoDestroySiegeWall(siegeWall, type));
    }

    IEnumerator CoDestroySiegeWall(SiegeWall siegeWall, BuildingNS.DestroyType type)
    {
      SiegeWallView view = (SiegeWallView)settlementMgr.GetView(siegeWall);
      view.DestroyAnimation(type);
      while (view.Animating) { yield return null; }
      view.Destroy();
      settlementMgr.settlement2GO.Remove(siegeWall);
      hexMap.cameraKeyboardController.EnableCamera();
      Animating = false;
    }
  }

}