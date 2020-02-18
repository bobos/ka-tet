namespace UnitNS
{
  public class UnitPoisioned
  {
    public const float DisableRate = 0.0025f;
    public const float KillRate = 0.001f;
    int __lastTurns = 0;
    public int lastTurns
    {
      get
      {
        return __lastTurns;
      }
      set
      {
        __lastTurns = value < 0 ? 0 : value;
      }
    }
    Unit unit;
    bool poisioned = false;
    public UnitPoisioned(Unit unit) {
      this.unit = unit;
    }

    public bool Poision() {
      if (poisioned) {
        return false;
      }
      poisioned = true;
      lastTurns = GetLastTurns();
      return true;
    }

    public void Destroy() {}

    public bool IsValid() {
      return lastTurns > 0f;;
    }

    public int[] Apply() {
      int[] effects = new int[9]{0,0,0,0,0,0,0,0,0};
      if (IsValid())
      {
        lastTurns--;
        int morale = -8;
        unit.rf.morale += morale;
        effects[0] = morale;

        int woundedNum = GetEffectNum();
        unit.hexMap.UpdateWound(unit, woundedNum);
        unit.rf.wounded += woundedNum;
        unit.rf.soldiers -= woundedNum;
        effects[2] = woundedNum;

        if (Cons.EvenChance()) {
          int kiaNum = (int)(unit.rf.soldiers * KillRate);
          unit.kia += kiaNum;
          unit.rf.soldiers -= kiaNum;
          effects[3] = kiaNum;
          int laborKilled = (int)(kiaNum / 5);

          laborKilled = laborKilled > unit.labor ? unit.labor : laborKilled;
          if(unit.hexMap.IsAttackSide(unit.IsAI())) {
            unit.hexMap.settlementMgr.attackerLaborDead += laborKilled;
          } else {
            unit.hexMap.settlementMgr.defenderLaborDead += laborKilled;
          }

          unit.labor -= laborKilled;
          effects[4] = laborKilled;
        }
      }

      return effects;
    }

    public int GetIllTurns()
    {
      return lastTurns;
    }

    public int GetEffectNum()
    {
      return (int)(unit.rf.soldiers * DisableRate);
    }

    int GetLastTurns() {
      return Util.Rand(1, 2);
    }

  }
}