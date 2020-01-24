namespace UnitNS
{
  public class UnitPoisioned
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
    bool poisioned = false;
    public UnitPoisioned(Unit unit) {
      this.unit = unit;
    }

    public void Poision() {
      if (unit.type == Type.Scout || poisioned) {
        return;
      }
      unit.hexMap.eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.Poision, unit, null));
      poisioned = true;
      lastTurns = GetLastTurns();
    }

    public void Destroy() {}

    public bool IsValid() {
      return lastTurns > 0f;;
    }

    public int[] Apply() {
      int[] effects = new int[8]{0,0,0,0,0,0,0,0};
      if (IsValid())
      {
        lastTurns--;
        int morale = -1;
        unit.rf.morale += morale;
        effects[0] = morale;

        int woundedNum = GetEffectNum();
        unit.rf.wounded += woundedNum;
        unit.rf.soldiers -= woundedNum;
        effects[2] = woundedNum;

        if (Cons.EvenChance()) {
          int kiaNum = (int)(unit.rf.soldiers * KillRate);
          unit.kia += kiaNum;
          unit.rf.soldiers -= kiaNum;
          effects[3] = kiaNum;
          int laborKilled = (int)(kiaNum / 5);
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
      return Util.Rand(3, 9);
    }

  }
}