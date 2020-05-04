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
      if (!unit.ApplyDiscipline(Cons.FiftyFifty())) {
        return -3;
      }
      return 0;
    }

  }
}