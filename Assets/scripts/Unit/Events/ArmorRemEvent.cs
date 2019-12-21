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
      if (unit.type != Type.Scout && !unit.IsAI() && !fired
        && !unit.tile.waterBound && unit.IsOnField() && Cons.TinyChance()) {
        // TODO apply general trait
        fired = true;
        return Cons.FiftyFifty() ? Discontent() : DefReduce();
      }
      return 0;
    }

    int DefReduce() {
      return (int)(unit.rf.def * 0.25f);
    } 

    int Discontent() {
      return -4;
    } 

    public void Destroy() {}

  }
}