﻿using CourtNS;

namespace UnitNS
{
  public class AltitudeSickness
  {
    Unit unit;
    public int lastTurns = 0;
    bool triggered = false;
    public AltitudeSickness(Unit unit) {
      this.unit = unit;
    }

    public int Occur() {
      if (triggered ||
        Util.eq<Region>(unit.rf.province.region, Cons.tubo) || 
        Util.eq<Region>(unit.rf.province.region, Cons.dangxiang) || 
        (!Util.eq<Province>(unit.hexMap.warProvince, Cons.heHuang)) ||
        unit.movementRemaining >= 50 ||
        Cons.SlimChance()) {
        return 0;
      }

      triggered = true;
      lastTurns = Util.Rand(1, 3);
      return -40;
    }

    public int Apply() {
      if (lastTurns == 0) {
        return 0;
      }
      lastTurns--;
      return unit.Killed(Util.Rand(2, 15));
    }

  }
}