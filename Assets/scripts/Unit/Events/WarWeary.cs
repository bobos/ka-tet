﻿namespace UnitNS
{
  public class WarWeary
  {
    public const float DesertRate = 0.01f;
    Unit unit;
    public WarWeary(Unit unit) {
      this.unit = unit;
    }

    public float GetBuf() {
      return IsWarWeary() ? -0.8f : 0;
    }

    public bool IsWarWeary() {
      int total = unit.kia + unit.rf.soldiers;
      return total * unit.rf.org < unit.kia; 
    }

    public int GetWarWearyDissertNum()
    {
      return (int)(unit.rf.soldiers * DesertRate);
    }

    public int[] Apply() {
      int[] effects = new int[5]{0,0,0,0,0};
      effects[2] = unit.Killed(GetWarWearyDissertNum());
      return effects;
    }

  }
}