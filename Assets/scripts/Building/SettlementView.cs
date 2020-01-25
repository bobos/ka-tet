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

  public void DestroyAnimation(DestroyType type)
  {
    Animating = true;
    StartCoroutine(CoDestroyAnimation(type));
  }

  IEnumerator CoDestroyAnimation(DestroyType type) {
    yield return new WaitForSeconds(1);
    Animating = false;
  }

  public void Destroy()
  {
    mouseController.onSettlementSelect -= OnSettlementSelect;
    mouseController.onSettlementDeselect -= OnSettlementDeselect;
    GameObject.Destroy(gameObject);
  }

  public void OnSettlementSelect(Settlement settlement)
  {
    if (settlement != null
      && Util.eq<Settlement>(settlement, this.settlement)
      && !settlement.IsUnderSiege())
    {
      HashSet<Tile> tiles = settlementMgr.GetSupplyRangeTiles(settlement);
      Tile[] tileArray = new Tile[tiles.Count];
      tiles.CopyTo(tileArray);
      hexMap.HighlightArea(tileArray, HexMap.RangeType.supplyRange);
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