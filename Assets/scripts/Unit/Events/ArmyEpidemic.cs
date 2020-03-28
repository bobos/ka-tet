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

    public int Occur() {
      if (!unit.IsHeatSicknessAffected()) {
        lastTurns += GetLastTurns();
        return 3;
      }
      return 0;
    }

    public void Destroy() {}

    public bool IsValid() {
      return lastTurns > 0;
    }

    public int[] Apply() {
      int[] effects = new int[9]{0,0,0,0,0,0,0,0,0};
      if (IsValid())
      {
        lastTurns--;
        int morale = -10;
        effects[0] = morale;
        unit.rf.morale += morale;
        int kiaNum = (int)(unit.rf.soldiers * KillRate);
        unit.kia += kiaNum;
        unit.rf.soldiers -= kiaNum;
        effects[3] = kiaNum;
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