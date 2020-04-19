using CourtNS;

namespace UnitNS
{
  abstract public class Rank {
    public static int MoralePunishLine(bool isSpecial) {
      return isSpecial ? 20 : 60;
    }

    public static float GetMoralePunish(int morale, bool isSpecial) {
      int dropStarts = isSpecial ? 30 : 70;
      if (morale >= dropStarts) {
        return 0f;
      }
      if (morale < MoralePunishLine(isSpecial)) {
        return 1f;
      }

      return (dropStarts - morale) * 0.1f;
    }

    abstract public string Name(Region region, bool IsCavalry);
    abstract public string Description();
    abstract public float Buf(float buf, int morale, bool isSpecial);
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

    public override float Buf(float _buf, int _morale, bool _isSpecial) {
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

    public override float Buf(float buf, int morale, bool isSpecial) {
      return buf * (1 - Rank.GetMoralePunish(morale, isSpecial));
    }

    public override int RecoverPerTurn() {
      return 200;
    }
  }

}