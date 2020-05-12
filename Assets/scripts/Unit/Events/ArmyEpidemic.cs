using FieldNS;

namespace UnitNS
{
  public class ArmyEpidemic
  {
    public const float KillRate = 0.0025f;
    int __lastTurns = 0;
    public int lastTurns
    {
      get
      {
        return __lastTurns;
      }
      set
      {
        __lastTurns = value < 0 ? 0 : value;
      }
    }
    Unit unit;
    public ArmyEpidemic(Unit unit) {
      this.unit = unit;
    }

    public bool Occur() {
      if (!unit.IsHeatSicknessAffected() && !unit.rf.general.Has(Cons.doctor)) {
        lastTurns += GetLastTurns();
        return true;
      }
      return false;
    }

    public void Destroy() {}

    public bool IsValid() {
      return lastTurns > 0;
    }

    public int[] Apply() {
      int[] effects = new int[5]{0,0,0,0,0};
      if (IsValid())
      {
        lastTurns--;
        int morale = -3;
        effects[0] = morale;
        unit.rf.morale += morale;
        effects[2] = unit.Killed((int)(unit.rf.soldiers * KillRate));
      }

      return effects;
    }

    public int GetIllTurns()
    {
      return lastTurns;
    }

    int GetLastTurns() {
      return Util.Rand(2, 3);
    }

  }
}