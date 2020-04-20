using CourtNS;

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
      return -5;
    }

    public int Apply() {
      if (lastTurns == 0) {
        return 0;
      }
      lastTurns--;
      int killed = Util.Rand(1, 10);
      unit.kia += killed;
      unit.rf.soldiers -= killed;
      return killed;
    }

  }
}