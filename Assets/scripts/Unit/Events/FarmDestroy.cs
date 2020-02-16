using MapTileNS;

namespace UnitNS
{
  public class FarmDestryResult {
    public bool destroyed = false;
    public int discontent = 0;
    public int influence = 0;
  }

  public class FarmDestroy
  {
    Unit unit;
    public FarmDestroy(Unit unit) {
      this.unit = unit;
    }

    public FarmDestryResult Occur() {
      FarmDestryResult result = new FarmDestryResult();
      if (unit.type != Type.Scout && unit.tile.field == FieldType.Village
        && !Cons.IsWinter(unit.hexMap.weatherGenerator.season)) {
        if (unit.hexMap.IsAttackSide(unit.IsAI())) {
          result.destroyed = true;
        } else if (!unit.IsAI() && Cons.FiftyFifty()) {
          result.destroyed = true;
          if(!Util.eq<CourtNS.Party>(unit.rf.general.party, unit.hexMap.warProvince.ownerParty)) {
            // only affect players
            CourtNS.Party.Relation relation = unit.rf.general.party.GetRelation();
            if ((relation == CourtNS.Party.Relation.normal && Cons.FairChance()) ||
              (relation == CourtNS.Party.Relation.tense && Cons.FiftyFifty()) ||
              relation == CourtNS.Party.Relation.xTense && Cons.HighlyLikely()) {
              result.discontent = Util.Rand(2,4);
              result.influence = 50;
            }
          }
        }

        if (result.destroyed) {
          unit.hexMap.warProvince.DeductOneAgriculturePoint();
        }
      }

      return result;
    }
  }
}