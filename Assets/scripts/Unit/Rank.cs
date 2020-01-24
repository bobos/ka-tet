namespace UnitNS
{
  abstract public class Rank {
    public const int MoralePunishLine = 50;
    public static float GetMoralePunish(int morale) {
      const int dropStarts = 70;
      if (morale >= dropStarts || morale < MoralePunishLine) {
        return 0f;
      }

      return (dropStarts - morale) * 0.05f;
    }

    abstract public string Name();
    abstract public string Description();
    abstract public float AtkBuf(int morale);
    abstract public float DefBuf(int morale);
    abstract public int RecoverPerTurn();
    abstract public int Level();
  }

  public class Rookie: Rank {

    public override int Level() {
      return 1;
    }

    public override string Name() {
      return Cons.GetTextLib().get("rank_rookie");
    }

    public override string Description() {
      return Cons.GetTextLib().get("rank_rookie_description");
    }

    public override float AtkBuf(int _morale) {
      return 0;
    }

    public override float DefBuf(int _morale) {
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

    public override string Name() {
      return Cons.GetTextLib().get("rank_veteran");
    }

    public override string Description() {
      return Cons.GetTextLib().get("rank_veteran_description");
    }

    public override float AtkBuf(int morale) {
      return 0.5f * (1 - Rank.GetMoralePunish(morale));
    }

    public override float DefBuf(int morale) {
      return 0.5f * (1 - Rank.GetMoralePunish(morale));
    }

    public override int RecoverPerTurn() {
      return 200;
    }
  }

  public class Elite: Rank {
    public override int Level() {
      return 3;
    }

    public override string Name() {
      return Cons.GetTextLib().get("rank_elite");
    }

    public override string Description() {
      return Cons.GetTextLib().get("rank_elite_description");
    }

    public override float AtkBuf(int morale) {
      return 1f * (1 - Rank.GetMoralePunish(morale));
    }

    public override float DefBuf(int morale) {
      return 1f * (1 - Rank.GetMoralePunish(morale));
    }

    public override int RecoverPerTurn() {
      return 100;
    }
  }
}