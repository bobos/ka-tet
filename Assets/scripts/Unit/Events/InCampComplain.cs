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
        && unit.type != Type.Infantry && unit.IsCamping() && !unit.ApplyDiscipline(Cons.MostLikely())) {
        fired = true;
        return MoraleDrop();
      }
      fired = true;
      return 0;
    }

    int MoraleDrop() {
      return -5;
    } 

    public void Destroy() {}

  }
}