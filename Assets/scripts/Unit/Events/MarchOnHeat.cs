namespace UnitNS
{
  public class MarchOnHeat
  {
    Unit unit;
    public MarchOnHeat(Unit unit) {
      this.unit = unit;
    }

    public bool Occur() {
      if (unit.tile.waterBound || unit.tile.settlement != null) {
        return false;
      }
      return unit.Discontent(1);
    }

  }
}