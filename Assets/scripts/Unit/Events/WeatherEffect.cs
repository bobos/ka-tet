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
          int kiaNum = Util.Rand(0, 10);
          unit.kia += kiaNum;
          unit.rf.soldiers -= kiaNum;
          effects[0] = morale;
          effects[2] = kiaNum;
        }
      } else if (Cons.IsBlizard(unit.hexMap.weatherGenerator.currentWeather)) {
        int movement = (int)(unit.movementRemaining / (-4)) * 3;
        unit.movementRemaining += movement;
        if (unit.IsCamping()) return effects;

        if (Cons.SlimChance()) {
          int morale = -4;
          unit.rf.morale += morale;
          int kiaNum = Util.Rand(4, 20);
          unit.kia += kiaNum;
          unit.rf.soldiers -= kiaNum;
          effects[0] = morale;
          effects[2] = kiaNum;
        }
      }
      return effects;
    }
  }
}