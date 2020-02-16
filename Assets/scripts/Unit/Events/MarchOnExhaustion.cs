namespace UnitNS
{
  public class MarchOnExhaustion
  {
    Unit unit;
    public MarchOnExhaustion(Unit unit) {
      this.unit = unit;
    }

    public int Occur() {
      if (unit.GetStaminaLevel() == StaminaLvl.Exhausted && unit.type != Type.Scout
        && Cons.SlimChance()) {
        return 3;
      }
      return 0;
    }

  }
}