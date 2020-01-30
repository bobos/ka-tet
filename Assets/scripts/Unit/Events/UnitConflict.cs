using MapTileNS;

namespace UnitNS
{
  public class ConflictResult {
    public Unit unit1;
    public Unit unit2;
    public int discontent = 0;
    public int unit1Wound;
    public int unit2Wound;
    public int unit1Dead;
    public int unit2Dead;
  }

  public class UnitConflict
  {
    Unit unit;
    public bool happened = false;
    public UnitConflict(Unit unit) {
      this.unit = unit;
    }

    public ConflictResult Occur() {
      ConflictResult result = new ConflictResult();
      if (unit.IsAI() || happened) {
        return result;
      }

      CourtNS.Party.Relation relation = unit.rf.general.party.GetRelation();
      if (relation == CourtNS.Party.Relation.normal) {
        return result;
      }

      Unit target = null;
      foreach(Tile tile in unit.tile.neighbours) {
        Unit u = tile.GetUnit();
        if (u != null &&
        u.IsAI() == unit.IsAI() &&
        !Util.eq<CourtNS.Party>(u.rf.general.party, unit.rf.general.party) &&
        !u.unitConflict.happened) {
          target = u;
          break;
        }
      }

      if (target == null) {
        return result;
      }

      if ((relation == CourtNS.Party.Relation.tense && Cons.FairChance()) ||
      relation == CourtNS.Party.Relation.xTense && Cons.EvenChance()) {
        happened = true;
        target.unitConflict.happened = true;
        result.discontent = Util.Rand(2,4);
        result.unit1 = unit;
        result.unit2 = target;

        result.unit1Wound = Util.Rand(10, Unit.DisbandUnitUnder - 10);
        unit.rf.wounded += result.unit1Wound;
        unit.rf.soldiers -= result.unit1Wound;
        unit.hexMap.UpdateWound(unit, result.unit1Wound);
        result.unit1Dead = Util.Rand(0, 9);
        unit.kia += result.unit1Dead;
        unit.rf.soldiers -= result.unit1Dead;

        result.unit2Wound = Util.Rand(10, Unit.DisbandUnitUnder - 10);
        target.rf.wounded += result.unit2Wound;
        target.rf.soldiers -= result.unit2Wound;
        unit.hexMap.UpdateWound(target, result.unit2Wound);
        result.unit2Dead = Util.Rand(0, 9);
        target.kia += result.unit2Dead;
        target.rf.soldiers -= result.unit2Dead;
      }

      return result;

    }
  }
}