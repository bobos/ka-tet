using TextNS;
using UnityEngine;

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

    public bool Discontent(int point) {
      unit.hexMap.AddOnDonePop(System.String.Format(Cons.GetTextLib().get("pop_discontent"), point), Color.red, unit);
      discontent += point;
      if (discontent == Max) {
        discontent = 0;
        int moraleReduce = 20;
        unit.rf.morale -= 20;
        unit.hexMap.eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.Riot, unit, null, moraleReduce));
        unit.Riot();
        return true;
      }
      return false;
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