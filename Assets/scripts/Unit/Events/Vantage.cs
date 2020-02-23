namespace UnitNS
{
  public class Vantage
  {
    Unit unit;
    const int MaxEffectiveNum = 10000;
    const int LeastEffectiveNum = 2000;
    const int MaxEffectiveNum4Move = 12000;
    public Vantage(Unit unit) {
      this.unit = unit;
    }

    public float Buf() {
      if (unit.IsCamping()) {
        return unit.tile.settlement.wall.defensePoint * 0.015f;
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
      if (unit.rf.soldiers > effective) {
        return effective * pointPerSoldier;
      } else {
        return unit.rf.soldiers * pointPerSoldier;
      }
      //int exceeded = unit.rf.soldiers - effective;
      //if (exceeded <= 0) {
      //  return unit.rf.soldiers * pointPerSoldier; 
      //}
      //int point = effective * pointPerSoldier;
      //int step = 100;
      //while(step > 0) {
      //  step -= 2;
      //  int remaining = exceeded - 100;
      //  if (remaining > 0) {
      //    int addedPoint = (int)(100 * step * pointPerSoldier * 0.01f);
      //    point += addedPoint < 0 ? 0 : addedPoint;
      //    exceeded = remaining;
      //  } else {
      //    int addedPoint = (int)(exceeded * step * pointPerSoldier * 0.01f);
      //    point += addedPoint < 0 ? 0 : addedPoint;
      //    break;
      //  }
      //}
      //return point;
    }

    public int MovementPoint(int point) {
      return unit.GetTotalNum() > MaxEffectiveNum4Move ? (int)(point * 0.8f) : point;
    }

    int GetEffectiveNum() {
      return MaxEffectiveNum - (int)((MaxEffectiveNum - LeastEffectiveNum) * Rank.GetMoralePunish(unit.rf.morale));
    }

    public int GetEffective() {
      int eff = GetEffectiveNum();
      return unit.rf.soldiers > eff ?  eff : unit.rf.soldiers;
    }

  }
}