using System.Collections.Generic;
using MapTileNS;
using UnityEngine;

namespace MonoNS
{
  public class SettlementView : MonoBehaviour
  {

    // Use this for initialization
    MouseController mouseController;
    SettlementMgr settlementMgr;
    HexMap hexMap;
    public Settlement settlement = null;
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
      hexMap = GameObject.FindObjectOfType<HexMap>();
      mouseController = hexMap.mouseController;
      mouseController.onSettlementSelect += OnSettlementSelect;
      mouseController.onSettlementDeselect += OnSettlementDeselect;
      settlementMgr = hexMap.settlementMgr;
    }

    public void Destroy()
    {
      mouseController.onSettlementSelect -= OnSettlementSelect;
      mouseController.onSettlementDeselect -= OnSettlementDeselect;
    }

    public void OnSettlementSelect(Settlement settlement)
    {
      if (settlement != null && Util.eq<Settlement>(settlement, this.settlement))
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

}