using NatureNS;

namespace MapTileNS
{
  public class Epidemic
  {
    Tile tile;
    public Epidemic(Tile tile) {
      this.tile = tile;
    }

    public bool OnWeatherChange(Weather weather)
    {
      if (tile.field != FieldType.Forest || !Cons.IsHeavyRain(weather)) { return false; }
      if (Cons.IsSummer(tile.weatherGenerator.season) && Cons.SlimChance() && tile.GetUnit() != null)
      {
        return true;
      }
      return false;
    }

  }

}