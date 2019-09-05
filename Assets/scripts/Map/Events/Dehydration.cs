namespace MapTileNS
{
  public class Dehydration
  {
    Tile tile;
    public Dehydration(Tile tile) {
      this.tile = tile;
      tile.ListenOnHeat(OnHeat);
    }

    public void OnHeat()
    {
      if (tile.field == FieldType.Settlement || tile.field == FieldType.Burning || tile.field == FieldType.Flooding) {
        return;
      }
      if (Cons.FairChance() && tile.GetUnit() != null)
      {
        tile.GetUnit().Dehydrate();
      }
    }

  }

}