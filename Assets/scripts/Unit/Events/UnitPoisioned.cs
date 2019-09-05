namespace UnitNS
{
  public class UnitPoisioned
  {
    float __illDisableRate = 0f;
    float disableRatio
    {
      get
      {
        return __illDisableRate;
      }
      set
      {
        __illDisableRate = value < 0 ? 0 : value;
      }
    }
    float __illDeathRate = 0f;
    float killRatio
    {
      get
      {
        return __illDeathRate;
      }
      set
      {
        __illDeathRate = value < 0 ? 0 : value;
      }
    }
    Unit unit;
    bool poisioned = false;
    public UnitPoisioned(Unit unit) {
      this.unit = unit;
    }

    public void Poision() {
      if (poisioned) {
        return;
      }
      // TODO: emit event
      poisioned = true;
      disableRatio = GetDisableRatio();
      killRatio = GetKillRatio();
    }

    public void Destroy() {}

    public bool IsValid() {
      return disableRatio > 0f;;
    }

    public void Apply() {
       if (disableRatio > 0)
      {
        int woundedNum = GetIllDisableNum();
        unit.rf.wounded += woundedNum;
        unit.rf.soldiers -= woundedNum;
        unit.labor -= (int)(woundedNum / 4);
        disableRatio -= 0.005f;
      }

      if (killRatio  > 0)
      {
        unit.rf.morale -= 2;
        int kiaNum = GetIllKillNum();
        unit.kia += kiaNum;
        unit.rf.soldiers -= kiaNum;
        unit.labor -= kiaNum;
        killRatio -= 0.005f;
      }
    }

    public int GetIllTurns()
    {
      return (int)(disableRatio * 100);
    }

    public int GetIllDisableNum()
    {
      return (int)(unit.rf.soldiers * disableRatio);
    }

    public int GetIllKillNum()
    {
      return (int)(unit.rf.soldiers * killRatio);
    }

    float GetDisableRatio() {
      return Util.Rand(2, 4) * 0.01f;
    }

    float GetKillRatio() {
      return Util.Rand(1, 2) * 0.01f;
    }

  }
}