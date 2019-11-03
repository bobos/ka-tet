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
      if (tile.field != FieldType.Wild || !Cons.IsHeavyRain(weather)) { return false; }
      if (((Cons.IsSpring(tile.weatherGenerator.season) && Cons.SlimChance())
          || (Cons.IsSummer(tile.weatherGenerator.season) && Cons.FairChance()))
          && tile.GetUnit() != null)
      {
        return true;
      }
      return false;
    }

  }

}