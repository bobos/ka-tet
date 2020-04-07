namespace UnitNS
{
  public class MarchOnHeat
  {
    Unit unit;
    public MarchOnHeat(Unit unit) {
      this.unit = unit;
    }

    public int Occur() {
      if (Cons.IsWind(unit.hexMap.windGenerator.current)
      || Cons.IsGale(unit.hexMap.windGenerator.current)
      ||!Cons.IsHeat(unit.hexMap.weatherGenerator.currentWeather)
      || unit.IsCamping()) {
        return 0;
      }
      if (Cons.FairChance() && !unit.ApplyDiscipline()) {
        return -3;
      }
      return 0;
    }

  }
}