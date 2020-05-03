using CourtNS;

namespace UnitNS
{
  abstract public class Rank {
    abstract public string Name(Region region, bool IsCavalry);
    abstract public string Description();
    abstract public float Buf(Troop troop);
    abstract public int RecoverPerTurn();
    abstract public int Level();
  }

  public class Rookie: Rank {

    public override int Level() {
      return 1;
    }

    public override string Name(Region region, bool IsCavalry) {
      if (Util.eq<Region>(region, Cons.han)) {
        return IsCavalry ? Cons.GetTextLib().get("rank_hanCavalryRookie")
                         : Cons.GetTextLib().get("rank_hanInfantryRookie");
      } else if (Util.eq<Region>(region, Cons.qidan)) {
        return IsCavalry ? Cons.GetTextLib().get("rank_qidanCavalryRookie")
                         : Cons.GetTextLib().get("rank_qidanInfantryRookie");
      } else if (Util.eq<Region>(region, Cons.dangxiang)) {
        return IsCavalry ? Cons.GetTextLib().get("rank_dangxiangCavalryRookie")
                         : Cons.GetTextLib().get("rank_dangxiangInfantryRookie");
      } else if (Util.eq<Region>(region, Cons.nvzhen)) {
        return IsCavalry ? Cons.GetTextLib().get("rank_nvzhenCavalryRookie")
                         : Cons.GetTextLib().get("rank_nvzhenInfantryRookie");
      } else {
        return IsCavalry ? Cons.GetTextLib().get("rank_tuboCavalryRookie")
                         : Cons.GetTextLib().get("rank_tuboInfantryRookie");
      }
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
    public override int Level() {
      return 2;
    }

    public override string Name(Region region, bool IsCavalry) {
      if (Util.eq<Region>(region, Cons.han)) {
        return IsCavalry ? Cons.GetTextLib().get("rank_hanCavalryVeteran")
                         : Cons.GetTextLib().get("rank_hanInfantryVeteran");
      } else if (Util.eq<Region>(region, Cons.qidan)) {
        return IsCavalry ? Cons.GetTextLib().get("rank_qidanCavalryVeteran")
                         : Cons.GetTextLib().get("rank_qidanInfantryVeteran");
      } else if (Util.eq<Region>(region, Cons.dangxiang)) {
        return IsCavalry ? Cons.GetTextLib().get("rank_dangxiangCavalryVeteran")
                         : Cons.GetTextLib().get("rank_dangxiangInfantryVeteran");
      } else if (Util.eq<Region>(region, Cons.nvzhen)) {
        return IsCavalry ? Cons.GetTextLib().get("rank_nvzhenCavalryVeteran")
                         : Cons.GetTextLib().get("rank_nvzhenInfantryVeteran");
      } else {
        return IsCavalry ? Cons.GetTextLib().get("rank_tuboCavalryVeteran")
                         : Cons.GetTextLib().get("rank_tuboInfantryVeteran");
      }
    }

    public override string Description() {
      return Cons.GetTextLib().get("rank_veteran_description");
    }

    public override float Buf(Troop troop) {
      return troop.province.region.LevelBuf(troop.type) *
        (1 - troop.morale < troop.province.region.MoralePunishLine() ? 1f : 0f);
    }

    public override int RecoverPerTurn() {
      return 200;
    }
  }

}