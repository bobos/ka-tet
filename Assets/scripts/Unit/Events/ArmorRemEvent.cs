namespace UnitNS
{
  public class ArmorRemEvent
  {
    Unit unit;
    bool fired = false;
    public ArmorRemEvent(Unit unit) {
      this.unit = unit;
    }

    public int Occur() {
      if (unit.rf.rank == Cons.rookie && !unit.IsAI() && !unit.IsHillLander() && !fired && unit.IsOnField() && Cons.FiftyFifty()) {
        // TODO apply general trait
        fired = true;
        return Cons.FiftyFifty() ? Discontent() : DefReduce();
      }
      return 0;
    }

    int DefReduce() {
      return 30;
    } 

    int Discontent() {
      return -4;
    } 

    public void Destroy() {}

  }
}