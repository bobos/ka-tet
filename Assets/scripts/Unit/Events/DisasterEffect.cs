using MapTileNS;

namespace UnitNS
{
  public static class DisasterEffect
  {
    public static int[] Apply(DisasterType type, Unit unit) {
      if (type == DisasterType.WildFire) {
        return unit.TakeEffect(2, 0.1f, 0.005f);
      }

      if (type == DisasterType.Flood) {
        return unit.TakeEffect(2, 0.1f, 0.01f);
      }

      return new int[]{0,0,0,0,0};
    }
  }
}