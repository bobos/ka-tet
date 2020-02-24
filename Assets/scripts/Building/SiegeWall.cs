using System.Collections.Generic;
using MapTileNS;
using UnityEngine;
using MonoNS;
using System.Collections;
using BuildingNS;
using UnitNS;
using FieldNS;

public class SiegeWall: Building
{

  // Use this for initialization
  public SiegeWall(Tile tile, WarParty warParty)
  {
    this.baseTile = tile;
    this.owner = warParty;
    hexMap = GameObject.FindObjectOfType<HexMap>();
    buildWork = tile.Work2BuildSettlement();
  }

  protected override int HowMuchBuildWorkToFinish() {
    Unit unit = baseTile.GetUnit();
    int labor = 0;
    if (unit != null && unit.IsAI() == owner.isAI) {
      labor = unit.labor;
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