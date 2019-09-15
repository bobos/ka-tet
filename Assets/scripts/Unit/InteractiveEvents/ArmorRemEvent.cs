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
      if (!unit.tile.waterBound && unit.IsOnField() && Cons.TinyChance()) {
        // TODO: only affect player
        unit.hexMap.eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.Disarmor, unit,
        null, DefReduce(), Discontent()));
        unit.RemoveOnHeatListener(OnHeat);
        unit.hexMap.eventDialog.onDisarmorDecisionClick += OnDisamorDecisionClick;
      }
    }

    public void OnDisamorDecisionClick(bool accepted, Unit unit) {
      if (!Util.eq<Unit>(this.unit, unit)) {
        return;
      }
      unit.hexMap.eventDialog.onDisarmorDecisionClick -= OnDisamorDecisionClick;
      if (accepted) {
        unit.disarmorDefDebuf = DefReduce(); 
      } else {
        unit.Discontent(Discontent());
      }
    }

    int DefReduce() {
      return (int)(unit.rf.def * 0.25f);
    } 

    int Discontent() {
      return 4;
    } 

    public void Destroy() {
      unit.RemoveOnHeatListener(OnHeat);
    }

  }
}