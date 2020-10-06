using MapTileNS;
using UnityEngine;
using MonoNS;
using UnitNS;
using FieldNS;
using CourtNS;

public class SiegeWall: Building
{

  // Use this for initialization
  public SiegeWall(Tile tile, WarParty warParty)
  {
    isSiegeWall = true;
    this.baseTile = tile;
    this.owner = warParty;
    hexMap = GameObject.FindObjectOfType<HexMap>();
    buildWork = tile.Work2BuildSettlement();
    settlementMgr = hexMap.settlementMgr;
  }

  protected override int HowMuchBuildWorkToFinish(bool actualBuild = false) {
    Unit unit = baseTile.GetUnit();
    int labor = 0;
    if (unit != null && unit.IsAI() == owner.isAI) {
      labor = unit.rf.soldiers;
    }
    if (Builder.Aval(unit)) {
      labor = (int)(labor * (1f + Builder.BuildEfficiencyIncr));
      if (actualBuild) {
        Builder.Get(unit.rf.general).Consume();
      }
    }
    return (int)(labor / 500);
  }

  public void Destroy()
  {
    settlementMgr.DestroySiegeWall(this);
    baseTile.siegeWall = null;
  }

  public bool IsFunctional() {
    Unit unit = baseTile.GetUnit();
    return unit != null && unit.IsAI() == owner.isAI && state == State.normal;
  }

}