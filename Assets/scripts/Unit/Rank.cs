using System.Collections.Generic;

namespace UnitNS
{
  public interface Rank {
    string Name();
    string Description();
    float AtkBuf();
    float DefBuf();
    int RecoverPerTurn();
    int Level();
  }

  public class Rookie: Rank {
    public int Level() {
      return 1;
    }

    public string Name() {
      return Cons.GetTextLib().get("rank_rookie");
    }

    public string Description() {
      return Cons.GetTextLib().get("rank_rookie_description");
    }

    public float AtkBuf() {
      return 0;
    }

    public float DefBuf() {
      return 0;
    }

    public int RecoverPerTurn() {
      return 500;
    }
  }

  public class Veteran: Rank {
    public int Level() {
      return 2;
    }

    public string Name() {
      return Cons.GetTextLib().get("rank_veteran");
    }

    public string Description() {
      return Cons.GetTextLib().get("rank_veteran_description");
    }

    public float AtkBuf() {
      return 0.5f;
    }

    public float DefBuf() {
      return 0.5f;
    }

    public int RecoverPerTurn() {
      return 200;
    }
  }

  public class Elite: Rank {
    public int Level() {
      return 3;
    }

    public string Name() {
      return Cons.GetTextLib().get("rank_elite");
    }

    public string Description() {
      return Cons.GetTextLib().get("rank_elite_description");
    }

    public float AtkBuf() {
      return 1f;
    }

    public float DefBuf() {
      return 1f;
    }

    public int RecoverPerTurn() {
      return 100;
    }
  }
}