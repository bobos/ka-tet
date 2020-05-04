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
      Unit first = unit.hexMap.GetWarParty(unit).firstRemoveArmor;
      if (first != null && first.IsGone()) {
        return 0;
      }

      if ((unit.rf.rank == Cons.rookie || first != null)
          && !unit.ApplyDiscipline(Cons.FiftyFifty())
          && !unit.IsAI() && !fired && unit.IsOnField()) {
        fired = true;
        if (unit.rf.general.Has(Cons.discipline)) {
          return MoraleDrop();
        }
        return Cons.FiftyFifty() ? MoraleDrop() : DefReduce();
      }
      return 0;
    }

    int DefReduce() {
      return 20;
    } 

    int MoraleDrop() {
      return -8;
    } 

    public void Destroy() {}

  }
}