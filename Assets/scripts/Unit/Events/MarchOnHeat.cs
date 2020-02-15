namespace UnitNS
{
  public class MarchOnHeat
  {
    Unit unit;
    public MarchOnHeat(Unit unit) {
      this.unit = unit;
    }

    public int Occur() {
      if (unit.IsCavalry() || unit.IsHillLander()
      || Cons.IsWind(unit.hexMap.windGenerator.current) || Cons.IsGale(unit.hexMap.windGenerator.current)
      ||!Cons.IsHeat(unit.hexMap.weatherGenerator.currentWeather)
      || unit.IsCamping()) {
        return 0;
      }
      if (Cons.FairChance()) {
        return 1;
      }
      return 0;
    }

  }
}