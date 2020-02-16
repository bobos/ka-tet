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
      if (!Util.eq<Region>(unit.hexMap.warProvince.region, Cons.upLand) ||
       Util.eq<Region>(unit.rf.province.region, Cons.upLand) ||
       unit.type == Type.Scout ||
       triggered ||
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
      int wound = (int)(unit.rf.soldiers * Util.Rand(1, 5) * 0.001f);
      unit.rf.soldiers -= wound;
      unit.rf.wounded += wound;
      unit.hexMap.UpdateWound(unit, wound);
      return wound;
    }

  }
}