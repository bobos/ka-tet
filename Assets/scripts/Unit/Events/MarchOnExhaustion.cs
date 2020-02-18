﻿namespace UnitNS
{
  public class MarchOnExhaustion
  {
    Unit unit;
    public MarchOnExhaustion(Unit unit) {
      this.unit = unit;
    }

    public int Occur() {
      if (unit.GetStaminaLevel() == StaminaLvl.Exhausted && Cons.SlimChance()) {
        return 3;
      }
      return 0;
    }

  }
}