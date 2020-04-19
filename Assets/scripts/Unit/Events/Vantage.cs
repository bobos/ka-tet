namespace UnitNS
{
  public class Vantage
  {
    Unit unit;
    const int MaxEffectiveNum = 10000;
    const int LeastEffectiveNum = 2000;
    public Vantage(Unit unit) {
      this.unit = unit;
    }

    public bool IsAtVantagePoint() {
      return unit.tile.vantagePoint;
    }

    public float Buf() {
      if (unit.IsCamping() && unit.tile.settlement != null) {
        return unit.tile.settlement.wall.defensePoint * 0.03f;
      }

      if (unit.tile.terrian == MapTileNS.TerrianType.Hill) {
        if (IsAtVantagePoint()) {
          return 0.2f;
        }

        if (unit.IsCavalry()) {
          // cavary debuf on hill
          return -0.2f;
        }
      }

      return 0f;
    }

    public int TotalPoints(int pointPerSoldier) {
      int effective = GetEffectiveNum();
      if (unit.rf.soldiers > effective) {
        return effective * pointPerSoldier;
      } else {
        return unit.rf.soldiers * pointPerSoldier;
      }
    }

    int GetEffectiveNum() {
      return MaxEffectiveNum - (int)((MaxEffectiveNum - LeastEffectiveNum) * Rank.GetMoralePunish(unit.rf.morale, unit.rf.IsSpecial()));
    }

    public int GetEffective() {
      int eff = GetEffectiveNum();
      return unit.rf.soldiers > eff ?  eff : unit.rf.soldiers;
    }

  }
}