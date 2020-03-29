﻿namespace UnitNS
{
  public class WarWeary
  {
    public const float DesertRate = 0.005f;
    Unit unit;
    public WarWeary(Unit unit) {
      this.unit = unit;
    }

    public float GetBuf() {
      return IsWarWeary() ? -1f : 0;
    }

    public bool IsWarWeary() {
      return unit.rf.morale < unit.GetRetreatThreshold();
    }

    public int GetWarWearyDissertNum()
    {
      return (int)(unit.rf.soldiers * DesertRate);
    }

    public int[] Apply() {
      int[] effects = new int[9]{0,0,0,0,0,0,0,0,0};
      int miaNum = GetWarWearyDissertNum();
      effects[5] = miaNum;
      unit.kia += miaNum;
      unit.rf.soldiers -= miaNum;

      return effects;
    }

  }
}