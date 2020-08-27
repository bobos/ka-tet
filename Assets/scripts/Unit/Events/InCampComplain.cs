namespace UnitNS
{
  public class InCampComplain
  {
    Unit unit;
    bool fired = false;
    public InCampComplain(Unit unit) {
      this.unit = unit;
    }

    public int Occur() {
      if (!fired && Cons.IsQidan(unit.rf.province.region) && !unit.hexMap.IsAttackSide(unit.IsAI())
        && unit.IsCavalry() && unit.IsCamping() && !unit.ApplyDiscipline()) {
        fired = true;
        return MoraleDrop();
      }
      fired = true;
      return 0;
    }

    int MoraleDrop() {
      return -25;
    } 

    public void Destroy() {}

  }
}