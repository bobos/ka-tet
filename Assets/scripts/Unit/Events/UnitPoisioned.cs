namespace UnitNS
{
  public class UnitPoisioned
  {
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
    bool poisioned = false;
    public UnitPoisioned(Unit unit) {
      this.unit = unit;
    }

    public bool Poision() {
      if (poisioned || unit.rf.general.Has(Cons.doctor)) {
        return false;
      }
      poisioned = true;
      lastTurns = GetLastTurns();
      return true;
    }

    public void Destroy() {}

    public bool IsValid() {
      return lastTurns > 0f;;
    }

    public int[] Apply() {
      int[] effects = new int[5]{0,0,0,0,0};
      if (IsValid())
      {
        lastTurns--;
        int morale = -20;
        unit.morale += morale;
        effects[0] = morale;
        effects[2] = unit.Killed(Util.Rand(3, 20));
      }
      return effects;
    }

    public int GetIllTurns()
    {
      return lastTurns;
    }

    int GetLastTurns() {
      return Util.Rand(1, 3);
    }

  }
}