namespace MapTileNS
{
  public class HeatSickness
  {
    Tile tile;
    public HeatSickness(Tile tile) {
      this.tile = tile;
      tile.ListenOnHeavyRain(onHeavyRain);
    }

    public void onHeavyRain()
    {
      if (tile.field != FieldType.Wild) {
        tile.RemoveOnHeatListener(onHeavyRain);
        return;
      }
      if (((Cons.IsSpring(tile.weatherGenerator.season) && Cons.SlimChance())
          || (Cons.IsSummer(tile.weatherGenerator.season) && Cons.FairChance()))
          && tile.GetUnit() != null)
      {
        tile.GetUnit().CaughtHeatSickness();
      }
    }

  }

}