namespace UnitNS
{
  public class ArmorRemEvent
  {
    Unit unit;
    bool fired = false;
    public ArmorRemEvent(Unit unit) {
      this.unit = unit;
    }

    public bool Occur() {
      if (!unit.IsAI() && !fired && !unit.tile.waterBound && unit.IsOnField() && Cons.TinyChance()) {
        fired = true;
        // TODO: only affect player
      }
      return false;
    }

    public int DefReduce() {
      return (int)(unit.rf.def * 0.25f);
    } 

    public int Discontent() {
      return 4;
    } 

    public void Destroy() {}

  }
}