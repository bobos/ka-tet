namespace UnitNS
{
  public class ArmyEpidemic
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
    public ArmyEpidemic(Unit unit) {
      this.unit = unit;
    }

    public int Occur() {
      disableRatio += GetDisableRatio();
      killRatio += GetKillRatio();
      return 1;
    }

    public void Destroy() {}

    public bool IsValid() {
      return disableRatio > 0f;
    }

    public int[] Apply() {
      int[] effects = new int[8]{0,0,0,0,0,0,0,0};
      if (disableRatio > 0)
      {
        int woundedNum = GetIllDisableNum();
        unit.rf.wounded += woundedNum;
        unit.rf.soldiers -= woundedNum;
        disableRatio -= 0.005f;
        effects[2] = woundedNum;
      }

      if (killRatio > 0)
      {
        int morale = -2;
        effects[0] = morale;
        unit.rf.morale += morale;
        int kiaNum = GetIllKillNum();
        unit.kia += kiaNum;
        unit.rf.soldiers -= kiaNum;
        effects[3] = kiaNum;
        int killedLabor = (int)(kiaNum / 5);
        unit.labor -= killedLabor; 
        effects[4] = killedLabor;
        killRatio -= 0.005f;
      }

      return effects;
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
      return Util.Rand(2, 5) * 0.01f;
    }

    float GetKillRatio() {
      return Util.Rand(1, 2) * 0.01f;
    }

  }
}