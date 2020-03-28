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
      // TODO: for DLC
      return 0;
      if (!Util.eq<Province>(unit.hexMap.warProvince, Cons.heHuang) || triggered ||
       (unit.GetStaminaLevel() != StaminaLvl.Exhausted &&
        unit.GetStaminaLevel() != StaminaLvl.Tired)) {
        return 0;
      }

      if ((unit.GetStaminaLevel() == StaminaLvl.Tired && Cons.EvenChance()) ||
       (unit.GetStaminaLevel() == StaminaLvl.Exhausted && Cons.MostLikely())) {
        triggered = true;
        lastTurns = Util.Rand(3, 6);
        unit.movementRemaining = 0;
        return 2;
      }

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