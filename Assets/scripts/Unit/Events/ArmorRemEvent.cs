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

      if ((!unit.IsVeteran() || first != null)
          && !unit.ApplyDiscipline()
          && !unit.IsAI() && !fired && unit.IsOnField()) {
        fired = true;
        return Cons.FiftyFifty() ? MoraleDrop() : DefReduce();
      }
      return 0;
    }

    int DefReduce() {
      return 20; // cp reduced 20%
    }

    int MoraleDrop() {
      return -40;
    } 

    public void Destroy() {}

  }
}