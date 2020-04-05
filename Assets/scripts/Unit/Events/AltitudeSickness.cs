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
      // TODO: for DLC, check remaining of movement point
      return 0;
    }

    public int Apply() {
      if (lastTurns == 0) {
        return 0;
      }
      lastTurns--;
      int killed = Util.Rand(1, 3);
      unit.kia += killed;
      unit.rf.soldiers -= killed;
      return killed;
    }

  }
}