﻿namespace UnitNS
{
  public class Vantage
  {
    Unit unit;
    public Vantage(Unit unit) {
      this.unit = unit;
    }

    public bool IsAtVantagePoint() {
      return unit.tile.vantagePoint;
    }

    public float Buf() {
      if (unit.IsCamping() && unit.tile.settlement != null) {
        return unit.tile.settlement.wall.defensePoint * 0.04f;
      }

      if (unit.tile.terrian == MapTileNS.TerrianType.Plain && unit.IsCavalry()) {
        return 0.1f;
      }

      if (unit.tile.terrian == MapTileNS.TerrianType.Hill) {
        if (IsAtVantagePoint()) return 0.5f;
      }

      return 0f;
    }

    public int TotalPoints(int pointPerSoldier) {
      return unit.rf.soldiers * pointPerSoldier;
    }

  }
}