﻿namespace UnitNS
{
  public class UnitPoisioned
  {
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

        int kiaNum = Util.Rand(1, 15);
        unit.kia += kiaNum;
        unit.rf.soldiers -= kiaNum;
        effects[3] = kiaNum;
      }
      return effects;
    }

    public int GetIllTurns()
    {
      return lastTurns;
    }

    int GetLastTurns() {
      return Util.Rand(1, 2);
    }

  }
}