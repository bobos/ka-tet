namespace UnitNS
{
  public class MarchOnHeat
  {
    Unit unit;
    public MarchOnHeat(Unit unit) {
      this.unit = unit;
    }

    public int Occur() {
      if (!Cons.IsHeat(unit.hexMap.weatherGenerator.currentWeather) || unit.IsCamping()) {
        return 0;
      }
      return unit.ApplyDiscipline() ? 0 : -10;
    }

  }
}