using TextNS;

namespace UnitNS
{
  public class Riot
  {
    const int Max = 10;
    int discontent = 0;
    Unit unit;
    public Riot(Unit unit) {
      this.unit = unit;
    }

    public int Discontent(int point) {
      discontent += point;
      if (discontent >= Max) {
        discontent = 0;
        int moraleReduce = -35;
        unit.rf.morale += moraleReduce;
        return moraleReduce;
      }
      if (discontent < 0) {
        discontent = 0;
      }
      return 0;
    }

    public string GetDescription() {
      TextLib txtLib = Cons.GetTextLib();
      string str = "";
      if (discontent < 4) {
        str = txtLib.get("u_satisfied");
      } else if (discontent < 8) {
        str = txtLib.get("u_unsatisfied");
      } else {
        str = txtLib.get("u_riot");
      }
      return System.String.Format(str, discontent);
    }

  }
}