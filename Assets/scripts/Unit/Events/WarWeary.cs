namespace UnitNS
{
  public class WarWeary
  {
    public const float DesertRate = 0.01f;
    Unit unit;
    public WarWeary(Unit unit) {
      this.unit = unit;
    }

    public bool IsWarWeary() {
      return unit.rf.morale <= unit.GetRetreatThreshold();
    }

    public int GetWarWearyDissertNum()
    {
      return (int)(unit.rf.soldiers * DesertRate);
    }

    public int[] Apply() {
      int[] effects = new int[8]{0,0,0,0,0,0,0,0};
      int morale = -1;
      effects[0] = morale;
      unit.rf.morale += morale;

      int miaNum = GetWarWearyDissertNum();
      effects[5] = miaNum;
      unit.mia += miaNum;
      unit.rf.soldiers -= miaNum;
      int laborDisserter = (int) (miaNum / 4);
      unit.labor -= laborDisserter;
      effects[4] = laborDisserter;

      return effects;
    }

  }
}