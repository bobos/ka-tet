using MapTileNS;

namespace UnitNS
{
  public static class DisasterEffect
  {
    public static void Apply(DisasterType type, Unit unit) {
      if (type == DisasterType.WildFire) {
        unit.TakeEffect(type, 8, 1f, 0.025f, 0.0125f);
      }

      if (type == DisasterType.Flood) {
        unit.TakeEffect(type, 8, 1f, 0.05f, 0.025f);
      }

    }
  }
}