namespace UnitNS
{
  public class Vantage
  {
    Unit unit;
    public Vantage(Unit unit) {
      this.unit = unit;
    }

    public float AtkBuf() {
      if (unit.IsCamping()) {
        return 0.4f;
      }
      if (unit.tile.terrian == MapTileNS.TerrianType.Hill) {
        return 0.25f;
      }
      return 0f;
    }

    public float DefBuf() {
      if (unit.IsCamping()) {
        return 1f;
      }
      if (unit.tile.terrian == MapTileNS.TerrianType.Hill) {
        return 0.6f;
      }
      return 0f;
    }

  }
}