namespace UnitNS
{
  public class RetreatStress
  {
    Unit unit;
    bool fired = false;
    public RetreatStress(Unit unit) {
      this.unit = unit;
    }

    public int Occur() {
      if (!fired && unit.kia >= unit.rf.soldiers && !unit.IsCamping() && !unit.ApplyDiscipline()) {
        fired = true;
        return Cons.MostLikely() ? MoraleDrop() : 0;
      }
      return 0;
    }

    int MoraleDrop() {
      return -10;
    } 

    public void Destroy() {}

  }
}