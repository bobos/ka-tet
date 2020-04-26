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
      if (!fired && Cons.IsQidan(unit.rf.province.region)
        && unit.IsCavalry() && unit.IsCamping() && !unit.ApplyDiscipline()) {
        fired = true;
        return Cons.MostLikely() ? MoraleDrop() : 0;
      }
      return 0;
    }

    int MoraleDrop() {
      return -8;
    } 

    public void Destroy() {}

  }
}