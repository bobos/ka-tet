namespace UnitNS
{
  public class Vantage
  {
    Unit unit;
    const int MaxEffectiveNumOnPlain = 10000;
    const int MaxEffectiveNumOnHill = 5000;
    const int LeastEffectiveNum = 2000;
    const int MaxEffectiveNum4Move = 11000;
    public Vantage(Unit unit) {
      this.unit = unit;
    }

    public float Buf() {
      if (unit.IsCamping()) {
        if (unit.IsCavalry()) {
          return -0.3f;
        }
        return unit.tile.settlement.wall.defensePoint * 0.005f;
      }

      if (unit.tile.terrian == MapTileNS.TerrianType.Hill) {
        if (unit.tile.vantagePoint) {
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
      int exceeded = unit.rf.soldiers - effective;
      if (exceeded <= 0) {
        return unit.rf.soldiers * pointPerSoldier; 
      }
      int point = effective * pointPerSoldier;
      int step = 100;
      while(step > 0) {
        step -= 2;
        int remaining = exceeded - 100;
        if (remaining > 0) {
          int addedPoint = (int)(100 * step * pointPerSoldier * 0.01f);
          point += addedPoint < 0 ? 0 : addedPoint;
          exceeded = remaining;
        } else {
          int addedPoint = (int)(exceeded * step * pointPerSoldier * 0.01f);
          point += addedPoint < 0 ? 0 : addedPoint;
          break;
        }
      }
      return point;
    }

    public int MovementPoint(int point) {
      int exceeded = unit.GetTotalNum() - MaxEffectiveNum4Move;
      if (exceeded <= 0) {
        return point;
      }
      int remaining = exceeded % 100;
      float reduce = ((exceeded - remaining) / 100) * 0.01f + (remaining > 0 ? 0.01f : 0f);
      reduce = reduce > 0.5f ? 0.5f : reduce;
      return point - (int)(point * reduce);
    }

    int GetEffectiveNum() {
      int normalEffectiveNum = 0;
      if (unit.IsCamping() || unit.tile.terrian == MapTileNS.TerrianType.Hill) {
        normalEffectiveNum = MaxEffectiveNumOnHill;
      } else {
        normalEffectiveNum = MaxEffectiveNumOnPlain;
      }
      return normalEffectiveNum - (int)((normalEffectiveNum - LeastEffectiveNum) * Rank.GetMoralePunish(unit.rf.morale));
    }

    public int GetEffective() {
      int eff = GetEffectiveNum();
      return unit.rf.soldiers > eff ?  eff : unit.rf.soldiers;
    }

  }
}