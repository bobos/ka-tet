namespace UnitNS
{
  public class WeatherEffect
  {
    public const float SnowKillRate = 0.0025f;
    public const float SnowDisableRate = 0.005f;
    public const float BlizardKillRate = 0.005f;
    public const float BlizardDisableRate = 0.01f;
    Unit unit;
    public WeatherEffect(Unit unit) {
      this.unit = unit;
    }

    public int[] Apply() {
      int[] effects = new int[8]{0,0,0,0,0,0,0,0};
      if (unit.type == Type.Scout) {
        return effects;
      }

      if (Cons.IsHeavyRain(unit.hexMap.weatherGenerator.currentWeather)) {
        int movement = (int)(unit.movementRemaining / (-2));
        unit.movementRemaining += movement;
        effects[1] = movement;
      } else if (Cons.IsSnow(unit.hexMap.weatherGenerator.currentWeather)) {
        int movement = (int)(unit.movementRemaining / (-2));
        unit.movementRemaining += movement;
        effects[1] = movement;
        if (unit.IsCamping()) return effects;

        int morale = -1;
        unit.rf.morale += morale;
        int woundedNum = (int)(unit.rf.soldiers * SnowDisableRate);
        unit.hexMap.UpdateWound(unit, woundedNum);
        unit.rf.wounded += woundedNum;
        unit.rf.soldiers -= woundedNum;
        int kiaNum = (int)(unit.rf.soldiers * SnowKillRate);
        unit.kia += kiaNum;
        unit.rf.soldiers -= kiaNum;
        int laborKilled = (int)(kiaNum / 5);

        laborKilled = laborKilled > unit.labor ? unit.labor : laborKilled;
        if(unit.hexMap.IsAttackSide(unit.IsAI())) {
          unit.hexMap.settlementMgr.attackerLaborDead += laborKilled;
        } else {
          unit.hexMap.settlementMgr.defenderLaborDead += laborKilled;
        }

        unit.labor -= laborKilled;
        effects[0] = morale;
        effects[2] = woundedNum;
        effects[3] = kiaNum;
        effects[4] = laborKilled;
      } else if (Cons.IsBlizard(unit.hexMap.weatherGenerator.currentWeather)) {
        int movement = (int)(unit.movementRemaining / (-4)) * 3;
        unit.movementRemaining += movement;
        effects[1] = movement;
        if (unit.IsCamping()) return effects;

        int morale = -5;
        unit.rf.morale += morale;
        int woundedNum = (int)(unit.rf.soldiers * BlizardDisableRate);
        unit.hexMap.UpdateWound(unit, woundedNum);
        unit.rf.wounded += woundedNum;
        unit.rf.soldiers -= woundedNum;
        int kiaNum = (int)(unit.rf.soldiers * BlizardKillRate);
        unit.kia += kiaNum;
        unit.rf.soldiers -= kiaNum;
        int laborKilled = (int)(kiaNum / 5);

        laborKilled = laborKilled > unit.labor ? unit.labor : laborKilled;
        if(unit.hexMap.IsAttackSide(unit.IsAI())) {
          unit.hexMap.settlementMgr.attackerLaborDead += laborKilled;
        } else {
          unit.hexMap.settlementMgr.defenderLaborDead += laborKilled;
        }

        unit.labor -= laborKilled;
        effects[0] = morale;
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