namespace UnitNS
{
  public class MarchOnHeat
  {
    Unit unit;
    public MarchOnHeat(Unit unit) {
      this.unit = unit;
    }

    public int Occur() {
      if (!Cons.IsHeat(unit.hexMap.weatherGenerator.currentWeather)
      || unit.tile.waterBound || unit.IsCamping()) {
        return 0;
      }
      return 1;
    }

  }
}