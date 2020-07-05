using CourtNS;

namespace UnitNS
{
  abstract public class Rank {
    abstract public string Name(Region region, Type unitType);
    abstract public string Description();
    abstract public float Buf(Troop troop);
    abstract public int RecoverPerTurn();
  }

  public class Rookie: Rank {
    public override string Name(Region region, Type unitType) {
      if (unitType == Type.Infantry) {
        return Cons.GetTextLib().get("rank_hanInfantryRookie");
      }
      return Cons.GetTextLib().get("rank_hanLightCavRookie");
      //if (Util.eq<Region>(region, Cons.han)) {
      //  return IsCavalry ? Cons.GetTextLib().get("rank_hanCavalryRookie")
      //                   : Cons.GetTextLib().get("rank_hanInfantryRookie");
      //} else if (Util.eq<Region>(region, Cons.qidan)) {
      //  return IsCavalry ? Cons.GetTextLib().get("rank_qidanCavalryRookie")
      //                   : Cons.GetTextLib().get("rank_qidanInfantryRookie");
      //} else if (Util.eq<Region>(region, Cons.dangxiang)) {
      //  return IsCavalry ? Cons.GetTextLib().get("rank_dangxiangCavalryRookie")
      //                   : Cons.GetTextLib().get("rank_dangxiangInfantryRookie");
      //} else if (Util.eq<Region>(region, Cons.nvzhen)) {
      //  return IsCavalry ? Cons.GetTextLib().get("rank_nvzhenCavalryRookie")
      //                   : Cons.GetTextLib().get("rank_nvzhenInfantryRookie");
      //} else {
      //  return IsCavalry ? Cons.GetTextLib().get("rank_tuboCavalryRookie")
      //                   : Cons.GetTextLib().get("rank_tuboInfantryRookie");
      //}
    }

    public override string Description() {
      return Cons.GetTextLib().get("rank_rookie_description");
    }

    public override float Buf(Troop _troop) {
      return 0;
    }

    public override int RecoverPerTurn() {
      return 500;
    }
  }

  public class Veteran: Rank {
    public override string Name(Region region, Type unitType) {
      if (unitType == Type.Infantry) {
        return Cons.GetTextLib().get("rank_hanInfantryVeteran");
      }
      if (unitType == Type.LightCavalry) {
        return Cons.GetTextLib().get("rank_hanLightCavVeteran");
      }
      return Cons.GetTextLib().get("rank_hanHeavyCavVeteran");
    }

    public override string Description() {
      return Cons.GetTextLib().get("rank_veteran_description");
    }

    public override float Buf(Troop troop) {
      return troop.province.region.LevelBuf(troop.type) *
        (1 - (troop.morale < troop.province.region.MoralePunishLine() ? 1f : 0f));
    }

    public override int RecoverPerTurn() {
      return 200;
    }
  }

}