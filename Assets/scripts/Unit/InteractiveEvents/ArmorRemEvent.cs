using MonoNS;

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
      if (!unit.tile.waterBound && unit.IsOnField() && Cons.SlimChance()) {
        // TODO: only affect player
        unit.hexMap.eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.Disarmor, unit,
        null, DefReduce(), MoraleReduce()));
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
        unit.rf.morale -= MoraleReduce();
      }
    }

    int DefReduce() {
      return (int)(unit.rf.def * 0.25f);
    } 

    int MoraleReduce() {
      return 10;
    } 

    public void Destroy() {
      unit.RemoveOnHeatListener(OnHeat);
    }

  }
}