namespace UnitNS
{
  public class WeatherEffect
  {
    Unit unit;
    public WeatherEffect(Unit unit) {
      this.unit = unit;
    }

    public int[] Apply() {
      int[] effects = new int[5]{0,0,0,0,0};
      if (Cons.IsHeavyRain(unit.hexMap.weatherGenerator.currentWeather)) {
        int movement = (int)(unit.movementRemaining / (-2));
        unit.movementRemaining += movement;
      } else if (Cons.IsSnow(unit.hexMap.weatherGenerator.currentWeather)) {
        int movement = (int)(unit.movementRemaining / (-2));
        unit.movementRemaining += movement;
        if (unit.IsCamping()) return effects;

        if (Cons.TinyChance()) {
          int morale = -1;
          unit.rf.morale += morale;
          effects[0] = morale;
          effects[2] = unit.Killed(Util.Rand(0, 10));
        }
      } else if (Cons.IsBlizard(unit.hexMap.weatherGenerator.currentWeather)) {
        int movement = (int)(unit.movementRemaining / (-4)) * 3;
        unit.movementRemaining += movement;
        if (unit.IsCamping()) return effects;

        if (Cons.SlimChance()) {
          int morale = -4;
          unit.rf.morale += morale;
          effects[0] = morale;
          effects[2] = unit.Killed(Util.Rand(4, 20));
        }
      }
      return effects;
    }
  }
}