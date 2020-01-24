namespace UnitNS
{
  public class ArmyEpidemic
  {
    public const float DisableRate = 0.01f;
    public const float KillRate = 0.005f;
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
    public ArmyEpidemic(Unit unit) {
      this.unit = unit;
    }

    public int Occur() {
      lastTurns += GetLastTurns();
      return 1;
    }

    public void Destroy() {}

    public bool IsValid() {
      return lastTurns > 0;
    }

    public int[] Apply() {
      int[] effects = new int[8]{0,0,0,0,0,0,0,0};
      if (IsValid())
      {
        lastTurns--;
        int morale = -1;
        effects[0] = morale;
        unit.rf.morale += morale;
        int woundedNum = GetEffectNum();
        unit.rf.wounded += woundedNum;
        unit.rf.soldiers -= woundedNum;
        effects[2] = woundedNum;
        if (Cons.EvenChance()) {
          int kiaNum = (int)(unit.rf.soldiers * KillRate);
          unit.kia += kiaNum;
          unit.rf.soldiers -= kiaNum;
          effects[3] = kiaNum;
          int killedLabor = (int)(kiaNum / 5);
          unit.labor -= killedLabor; 
          effects[4] = killedLabor;
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
      return Util.Rand(3, 10);
    }

  }
}