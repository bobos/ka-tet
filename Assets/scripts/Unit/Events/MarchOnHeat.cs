namespace UnitNS
{
  public class MarchOnHeat
  {
    Unit unit;
    public MarchOnHeat(Unit unit) {
      this.unit = unit;
    }

    public bool Occur() {
      if (unit.tile.waterBound || unit.tile.settlement != null) {
        return false;
      }
      MonoNS.MsgBox.ShowMsg("不满+1");
      return unit.Discontent(1);
    }

  }
}