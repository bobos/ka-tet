namespace UnitNS
{
  public class WeatherEffect
  {
    public const float SnowKillRate = 0.0025f;
    public const float SnowDisableRate = 0.005f;
    public const float BlizardKillRate = 0.0125f;
    public const float BlizardDisableRate = 0.025f;
    Unit unit;
    public WeatherEffect(Unit unit) {
      this.unit = unit;
    }

    public int[] Apply() {
      int[] effects = new int[8]{0,0,0,0,0,0,0,0};
      if (Cons.IsHeavyRain(unit.hexMap.weatherGenerator.currentWeather)) {
        if (unit.IsCamping()) return effects;
        int movement = (int)(unit.movementRemaining / (-2));
        unit.movementRemaining += movement;
        effects[1] = movement;
      } else if (Cons.IsSnow(unit.hexMap.weatherGenerator.currentWeather)) {
        if (unit.IsCamping()) return effects;
        int morale = -1;
        int movement = (int)(unit.movementRemaining / (-2));
        unit.rf.morale += morale;
        unit.movementRemaining += movement;
        int woundedNum = (int)(unit.rf.soldiers * SnowDisableRate);
        unit.rf.wounded += woundedNum;
        unit.rf.soldiers -= woundedNum;
        int kiaNum = (int)(unit.rf.soldiers * SnowKillRate);
        unit.kia += kiaNum;
        unit.rf.soldiers -= kiaNum;
        int laborKilled = (int)(kiaNum / 5);
        unit.labor -= laborKilled;
        effects[0] = morale;
        effects[1] = movement;
        effects[2] = woundedNum;
        effects[3] = kiaNum;
        effects[4] = laborKilled;
      } else if (Cons.IsBlizard(unit.hexMap.weatherGenerator.currentWeather)) {
        if (unit.IsCamping()) return effects;
        int morale = -5;
        unit.rf.morale += morale;
        int movement = (int)(unit.movementRemaining / (-4)) * 3;
        unit.movementRemaining += movement;
        int woundedNum = (int)(unit.rf.soldiers * BlizardDisableRate);
        unit.rf.wounded += woundedNum;
        unit.rf.soldiers -= woundedNum;
        int kiaNum = (int)(unit.rf.soldiers * BlizardKillRate);
        unit.kia += kiaNum;
        unit.rf.soldiers -= kiaNum;
        int laborKilled = (int)(kiaNum / 5);
        unit.labor -= laborKilled;
        effects[0] = morale;
        effects[1] = movement;
        effects[2] = woundedNum;
        effects[3] = kiaNum;
        effects[4] = laborKilled;
      }
      return effects;
    }

    public float AtkBuf() {
      if (Cons.IsHeavyRain(unit.hexMap.weatherGenerator.currentWeather)) {
        return -0.5f;
      }
      if (Cons.IsRain(unit.hexMap.weatherGenerator.currentWeather)) {
        return -0.2f;
      }
      return 0f;

    }

  }
}