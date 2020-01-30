using MapTileNS;

namespace UnitNS
{
  public static class DisasterEffect
  {
    public static int[] Apply(DisasterType type, Unit unit) {
      if (type == DisasterType.WildFire) {
        return unit.TakeEffect(8, 0.1f, 0.025f, 0.0125f);
      }

      if (type == DisasterType.Flood) {
        return unit.TakeEffect(8, 0.1f, 0.05f, 0.025f);
      }

      return new int[]{0,0,0,0,0,0,0,0,0};
    }
  }
}