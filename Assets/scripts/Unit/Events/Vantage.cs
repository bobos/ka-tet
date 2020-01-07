namespace UnitNS
{
  public class Vantage
  {
    Unit unit;
    const int MaxEffectiveNumOnPlain = 10000;
    const int MaxEffectiveNumOnHill = 5000;
    const int MaxEffectiveNum4Move = 11000;
    public Vantage(Unit unit) {
      this.unit = unit;
    }

    public float AtkBuf() {
      if (unit.IsCamping()) {
        if (unit.IsCavalry()) {
          return -0.25f;
        }
        return 0.4f;
      }

      if (unit.tile.terrian == MapTileNS.TerrianType.Hill) {
        if (unit.IsCavalry()) {
          // cavary debuf on hill
          return -0.2f;
        }
        return 0.2f;
      }

      if (unit.tile.terrian == MapTileNS.TerrianType.Plain) {
        if (unit.IsCavalry()) {
          return 0.2f;
        }
        return 0f;
      }

      return 0f;
    }

    public float DefBuf() {
      if (unit.IsCamping()) {
        return 1f;
      }
      if (unit.tile.terrian == MapTileNS.TerrianType.Hill && !unit.IsCavalry()) {
        return 0.6f;
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
      float step = 1f;
      while(step > 0 || exceeded > 0) {
        step -= 0.02f;
        int remaining = exceeded - 100;
        if (remaining > 0) {
          int addedPoint = (int)(100 * step * pointPerSoldier);
          point += addedPoint < 0 ? 0 : addedPoint;
          exceeded = remaining;
        } else {
          int addedPoint = (int)(exceeded * step * pointPerSoldier);
          point += addedPoint < 0 ? 0 : addedPoint;
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
      if (unit.IsCamping() || unit.tile.terrian == MapTileNS.TerrianType.Hill) {
        return MaxEffectiveNumOnHill;
      }
      return MaxEffectiveNumOnPlain;
    }

  }
}