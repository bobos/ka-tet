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
      StartCoroutine(CoDestroy(settlement, type));
      return garrison;
    }

    IEnumerator CoDestroy(Settlement settlement, BuildingNS.DestroyType type)
    {
      SettlementView view = settlementMgr.GetView(settlement);
      view.DestroyAnimation(type);
      while (view.Animating) { yield return null; }
      view.Destroy();
      eventDialog.Show(new Event(
          type == BuildingNS.DestroyType.ByFlood ?
          EventDialog.EventName.FloodDestroyCamp :
          EventDialog.EventName.WildFireDestroyCamp, null, settlement));
      while (eventDialog.Animating) { yield return null; }
      Animating = false;
    }
  }

}