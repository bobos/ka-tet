using System.Collections.Generic;
using NatureNS;

namespace MapTileNS
{
  public class Flood
  {
    public const int FloodingLasts = 15;
    public int floodingCntDown = 0;

    Tile tile;
    bool flooded = false;

    public Flood(Tile tile) {
      this.tile = tile;
    }

    public List<Tile> OnWeatherChange(Weather weather)
    {
      List<Tile> tiles = new List<Tile>();
      if (!tile.isDam || !Cons.IsHeavyRain(weather)) { return tiles; }
      if (Cons.FairChance())
      {
        return Start();
      }
      return tiles;
    }

    public void OnTurnEnd() {
      floodingCntDown--;
      if (floodingCntDown < 1)
      {
        FloodRecede();
      }
    }

    public bool Floodable() {
      return !flooded && (Cons.IsSpring(tile.weatherGenerator.season) || Cons.IsSummer(tile.weatherGenerator.season))
          && (Cons.IsRain(tile.weatherGenerator.currentWeather) || Cons.IsHeavyRain(tile.weatherGenerator.currentWeather));
    }

    public List<Tile> Start() {
      List<Tile> affectedTiles = new List<Tile>();
      if (Floodable()) 
      {
        tile.isDam = false;
        GetTile2Flood(affectedTiles);
      }
      return affectedTiles;
    }

    public void GetTile2Flood(List<Tile> tiles)
    {
      foreach (Tile tile in this.tile.DownstreamTiles<Tile>())
      {
        if (tile.terrian == TerrianType.Water || tile.field == FieldType.Flooding)
        {
          if(!tiles.Contains(tile)) tiles.Add(tile);
          tile.flood.GetTile2Flood(tiles);
        }
        else if (tile.flood != null && tile.flood.CanBeFloodedByNearByTile() &&
         (Cons.IsHeavyRain(tile.weatherGenerator.currentWeather) ? Cons.HighlyLikely() : Cons.MostLikely()))
        {
          if(!tiles.Contains(tile)) tiles.Add(tile);
          tile.flood.GetTile2Flood(tiles);
        }
      }
    }

    public void FloodTile() {
      if (tile.terrian == TerrianType.Water || tile.field == FieldType.Flooding) { return; }
      tile.wildFire.PutOutFire();
      tile.flood.floodingCntDown = Util.Rand(2, FloodingLasts);
      tile.SetFieldType(FieldType.Flooding);
      tile.ListenOnTurnEnd(OnTurnEnd);
    }

    public bool CanBeFloodedByNearByTile()
    {
      if (tile.terrian == TerrianType.Plain)
      {
        if (tile.field == FieldType.Settlement)
        {
          // camp can be flooded
          if (tile.settlement.type == Settlement.Type.camp)
          {
            return true;
          }
          return false;
        }
        else
        {
          return true;
        }
      }
      return false;
    }

    void FloodRecede()
    {
      tile.SetFieldType(FieldType.Flooded);
      tile.RemoveTurnEndListener(OnTurnEnd);
    }

  }

}