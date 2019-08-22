namespace UnitNS
{
  public class SkillTree {
    int skillPoint = 0;
    LvlFire offense = LvlFire.Locked;
    LvlForest defense = LvlForest.Locked;

    public SkillTree(LvlFire fire = LvlFire.Locked, LvlForest forest = LvlForest.Locked) {
      offense = fire;
      defense = forest;
    }

    public float GetAttackBuff(Type type) {
      if (offense == LvlFire.Locked) return 0f;
      if (offense == LvlFire.Lvl1) return type == Type.Cavalry ? Cavalry.L1FireBuff : Infantry.L1FireBuff;
      if (offense == LvlFire.Lvl2) return type == Type.Cavalry ? Cavalry.L2FireBuff : Infantry.L2FireBuff;
      if (offense == LvlFire.Lvl3) return type == Type.Cavalry ? Cavalry.L3FireBuff : Infantry.L3FireBuff;
      return type == Type.Cavalry ? Cavalry.L4FireBuff : Infantry.L4FireBuff;
    }

    public float GetDefenseBuff(Type type) {
      if (defense == LvlForest.Locked) return 0f;
      if (defense == LvlForest.Lvl1) return type == Type.Cavalry ? Cavalry.L1ForestBuff : Infantry.L1ForestBuff;
      if (defense == LvlForest.Lvl2) return type == Type.Cavalry ? Cavalry.L2ForestBuff : Infantry.L2ForestBuff;
      if (defense == LvlForest.Lvl3) return type == Type.Cavalry ? Cavalry.L3ForestBuff : Infantry.L3ForestBuff;
      return type == Type.Cavalry ? Cavalry.L4ForestBuff : Infantry.L4ForestBuff;
    }

    public bool IsEliteUnit() {
      return defense == LvlForest.TopLvl && offense == LvlFire.TopLvl;
    }

  }
}