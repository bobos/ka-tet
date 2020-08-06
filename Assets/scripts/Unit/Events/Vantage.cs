namespace UnitNS
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

      if (unit.tile.terrian == MapTileNS.TerrianType.Hill) {
        if (unit.type != Type.Infantry) {
          // cavary debuf on hill
          return -0.15f;
        }
        if (IsAtVantagePoint()) return 0.4f;
        return 0f;
      }

      return 0f;
    }

    public int TotalPoints(int pointPerSoldier) {
      return unit.rf.soldiers * pointPerSoldier;
    }

  }
}