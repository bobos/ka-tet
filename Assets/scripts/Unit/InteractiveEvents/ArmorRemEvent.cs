namespace UnitNS
{
  public class ArmorRemEvent
  {
    Unit unit;
    public ArmorRemEvent(Unit unit) {
      this.unit = unit;
      this.unit.ListenOnHeat(OnHeat);
    }

    public void OnHeat() {
      // TODO: interactive event
      unit.RemoveOnHeatListener(OnHeat);
    }

    public void Destroy() {
      unit.RemoveOnHeatListener(OnHeat);
    }

  }
}