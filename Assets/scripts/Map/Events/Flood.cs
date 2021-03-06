﻿using System.Collections.Generic;
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

    public HashSet<Tile> OnWeatherChange(Weather weather)
    {
      HashSet<Tile> tiles = new HashSet<Tile>();
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

    public HashSet<Tile> Start() {
      HashSet<Tile> affectedTiles = new HashSet<Tile>();
      if (!flooded) 
      {
        tile.isDam = false;
        GetTile2Flood(affectedTiles);
      }
      return affectedTiles;
    }

    public void GetTile2Flood(HashSet<Tile> tiles)
    {
      foreach (Tile tile in this.tile.DownstreamTiles<Tile>())
      {
        if ((tile.terrian == TerrianType.Water || tile.field == FieldType.Flooding) && Cons.FairChance())
        {
          tiles.Add(tile);
          tile.flood.GetTile2Flood(tiles);
        }
        else if (tile.flood != null && tile.flood.CanBeFloodedByNearByTile() &&
         (Cons.IsHeavyRain(tile.weatherGenerator.currentWeather) ? Cons.MostLikely() : Cons.FiftyFifty()))
        {
          tiles.Add(tile);
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
      return tile.field != FieldType.Settlement &&
        (tile.terrian == TerrianType.Plain || (tile.terrian == TerrianType.Hill && !tile.vantagePoint));
    }

    void FloodRecede()
    {
      tile.SetFieldType(FieldType.Flooded);
      tile.RemoveTurnEndListener(OnTurnEnd);
    }

  }

}