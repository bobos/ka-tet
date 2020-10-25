using System.Collections.Generic;
using MapTileNS;
using UnityEngine;
using MonoNS;
using System.Collections;
using BuildingNS;

public class SettlementView : View
{

  // Use this for initialization
  MouseController mouseController;
  SettlementMgr settlementMgr;
  HexMap hexMap;
  GameObject nameGO;
  public Settlement settlement = null;
  public override void OnCreate(DataModel settlement)
  {
    this.settlement = (Settlement)settlement;
    hexMap = GameObject.FindObjectOfType<HexMap>();
    mouseController = hexMap.mouseController;
    mouseController.onSettlementSelect += OnSettlementSelect;
    mouseController.onSettlementDeselect += OnSettlementDeselect;
    settlementMgr = hexMap.settlementMgr;
  }

  public static Vector3 NamePosition(Vector3 p) {
    return new Vector3(p.x - 0.5f, p.y + 0.6f, p.z);
  }

  public void SetNameGO(GameObject nameGO) {
    this.nameGO = nameGO;
    nameGO.GetComponent<UnitNS.UnitNameView>().settlement = settlement;
  }

  public void DestroyAnimation()
  {
    Animating = true;
    StartCoroutine(CoDestroyAnimation());
  }

  IEnumerator CoDestroyAnimation() {
    yield return new WaitForSeconds(1);
    Animating = false;
  }

  public void Destroy()
  {
    OnSettlementDeselect(mouseController.selectedSettlement);
    mouseController.onSettlementSelect -= OnSettlementSelect;
    mouseController.onSettlementDeselect -= OnSettlementDeselect;
    GameObject.Destroy(gameObject);
    GameObject.Destroy(nameGO);
  }

  public void OnSettlementSelect(Settlement settlement)
  {
    if (settlement != null
      && Util.eq<Settlement>(settlement, this.settlement)
      && !settlement.IsUnderSiege())
    {
      hexMap.HighlightArea(settlement.myTiles, HexMap.RangeType.supplyRange);
    }
  }

  public void OnSettlementDeselect(Settlement settlement)
  {
    if (settlement != null && Util.eq<Settlement>(settlement, this.settlement))
    {
      hexMap.DehighlightArea();
    }
  }

}