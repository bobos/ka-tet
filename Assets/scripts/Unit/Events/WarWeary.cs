namespace UnitNS
{
  public class WarWeary
  {
    public const float DesertRate = 0.005f;
    Unit unit;
    public WarWeary(Unit unit) {
      this.unit = unit;
    }

    public float GetBuf() {
      return IsWarWeary() ? -0.8f : 0;
    }

    public bool IsWarWeary() {
      return unit.type != Type.Scout && unit.rf.morale < unit.GetRetreatThreshold();
    }

    public int GetWarWearyDissertNum()
    {
      return (int)(unit.rf.soldiers * DesertRate);
    }

    public int[] Apply() {
      int[] effects = new int[9]{0,0,0,0,0,0,0,0,0};
      int miaNum = GetWarWearyDissertNum();
      effects[5] = miaNum;
      unit.mia += miaNum;
      unit.rf.soldiers -= miaNum;
      int laborDisserter = (int) (miaNum / 4);
      laborDisserter = laborDisserter > unit.labor ? unit.labor : laborDisserter;
      unit.labor -= laborDisserter;
      if(unit.hexMap.IsAttackSide(unit.IsAI())) {
        unit.hexMap.settlementMgr.attackerLaborDead += laborDisserter;
      } else {
        unit.hexMap.settlementMgr.defenderLaborDead += laborDisserter;
      }
      effects[4] = laborDisserter;

      return effects;
    }

  }
}