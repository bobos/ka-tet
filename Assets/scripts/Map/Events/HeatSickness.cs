namespace MapTileNS
{
  public class HeatSickness
  {
    Tile tile;
    public HeatSickness(Tile tile) {
      this.tile = tile;
      tile.ListenOnHeat(OnHeat);
    }

    public void OnHeat()
    {
      if (tile.field != FieldType.Wild) {
        tile.RemoveOnHeatListener(OnHeat);
        return;
      }
      if (Cons.FairChance() && tile.GetUnit() != null)
      {
        tile.GetUnit().CaughtHeatSickness();
      }
    }

  }

}