using MapTileNS;

namespace UnitNS
{
  public class FarmDestroy
  {
    Unit unit;
    public FarmDestroy(Unit unit) {
      this.unit = unit;
    }

    public int Occur() {
      if (unit.type != Type.Scout && !unit.IsAI() && unit.tile.field == FieldType.Village
        && !unit.hexMap.IsAttackSide(unit.IsAI())
        && ((Cons.IsSpring(unit.hexMap.weatherGenerator.season) && Cons.EvenChance())
           || (Cons.IsSummer(unit.hexMap.weatherGenerator.season) && Cons.FairChance()))) {
        unit.tile.SetFieldType(FieldType.Wild);
        // TODO: general trait applies here
        return Cons.FiftyFifty() ? 0 : 2;
      }
      return -1;
    }
  }
}