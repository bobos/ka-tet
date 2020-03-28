using System.Collections.Generic;
using NatureNS;

namespace MapTileNS
{
  public class WildFire
  {
    public const int BurningLasts = 6; 
    public int burningCntDown = 0;
    public bool burnable = false;

    Tile tile;

    public WildFire(Tile tile, bool burnable) {
      this.tile = tile;
      this.burnable = burnable;
    }

    public HashSet<Tile> OnWeatherChange(Weather weather)
    {
      HashSet<Tile> tiles = new HashSet<Tile>();
      if (burningCntDown > 0 && (Cons.IsRain(weather) || Cons.IsHeavyRain(weather))) {
        PutOutFire();
        return tiles;
      }
      if (tile.IsBurnable() && Cons.IsDry(weather) && Cons.SlimChance()) { return Start(); }
      return tiles;
    }

    public void OnTurnEnd() {
      burningCntDown--;
      if (burningCntDown < 1)
      {
        PutOutFire();
      }
    }

    public HashSet<Tile> Start() {
      HashSet<Tile> affectedTiles = new HashSet<Tile>();
      if (Burnable()) 
      {
        tile.burnable = false;
        affectedTiles.Add(tile);
        GetTile2Burn(affectedTiles);
      }
      return affectedTiles;
    }

    public bool Burnable() {
      if (Cons.IsSpring(tile.weatherGenerator.season) || Cons.IsWinter(tile.weatherGenerator.season))
      {
        return false;
      }

      if (Cons.IsRain(tile.weatherGenerator.currentWeather) || Cons.IsHeavyRain(tile.weatherGenerator.currentWeather))
      {
        return false;
      }

      return burnable;
    }

    public void GetTile2Burn(HashSet<Tile> affectedTiles)
    {
      tile.ListenOnTurnEnd(OnTurnEnd);
      HashSet<Tile> affectedTiles1 = new HashSet<Tile>();
      // spread to neighbours depends on season and wind and directions
      Cons.Direction windDirection = tile.windGenerator.direction;
      bool chance1 = Cons.FairChance();
      bool chance2 = Cons.SlimChance();
      bool chance3 = Cons.SlimChance();
      if (Cons.IsGale(tile.windGenerator.current))
      {
        chance1 = Cons.HighlyLikely();
        chance2 = Cons.HighlyLikely();
        chance3 = Cons.MostLikely();
      }
      else if (Cons.IsWind(tile.windGenerator.current))
      {
        chance1 = Cons.MostLikely();
        chance2 = Cons.EvenChance();
        chance3 = Cons.EvenChance();
      }

      if (windDirection == Cons.Direction.dueNorth)
      {
        PickTile2Burn(affectedTiles1, tile.NorthTile<Tile>(), tile.NorthEastTile<Tile>(),
                      tile.NorthWestTile<Tile>(), chance1, chance2, chance3);
      }

      if (windDirection == Cons.Direction.dueSouth)
      {
        PickTile2Burn(affectedTiles1, tile.SouthTile<Tile>(), tile.SouthEastTile<Tile>(),
                      tile.SouthWestTile<Tile>(), chance1, chance2, chance3);
      }

      if (windDirection == Cons.Direction.northEast)
      {
        PickTile2Burn(affectedTiles1, tile.NorthEastTile<Tile>(), tile.NorthTile<Tile>(),
                      tile.SouthEastTile<Tile>(), chance1, chance2, chance3);
      }

      if (windDirection == Cons.Direction.northWest)
      {
        PickTile2Burn(affectedTiles1, tile.NorthWestTile<Tile>(), tile.NorthTile<Tile>(),
                      tile.SouthWestTile<Tile>(), chance1, chance2, chance3);
      }

      if (windDirection == Cons.Direction.southWest)
      {
        PickTile2Burn(affectedTiles1, tile.SouthWestTile<Tile>(), tile.SouthTile<Tile>(),
                      tile.NorthWestTile<Tile>(), chance1, chance2, chance3);
      }

      if (windDirection == Cons.Direction.southEast)
      {
        PickTile2Burn(affectedTiles1, tile.SouthEastTile<Tile>(), tile.SouthTile<Tile>(),
                      tile.NorthEastTile<Tile>(), chance1, chance2, chance3);
      }

      foreach (Tile tile in affectedTiles1)
      {
        affectedTiles.Add(tile);
        tile.wildFire.GetTile2Burn(affectedTiles);
      }
    }

    public void BurnTile() {
      burnable = false;
      burningCntDown = Util.Rand(2, BurningLasts);
      tile.SetFieldType(FieldType.Burning);
      tile.ListenOnTurnEnd(OnTurnEnd);
    }
    
    public void PutOutFire()
    {
      burningCntDown = 0;
      tile.SetFieldType(FieldType.Schorched);
      tile.RemoveTurnEndListener(OnTurnEnd);
    }

    public bool CanCatchFire()
    {
      return tile.field != FieldType.Settlement
        && (tile.terrian == TerrianType.Hill || tile.terrian == TerrianType.Mountain);
    }

    // Can the plain tile caught fire by nearby burning tiles
    public bool CanPlainCatchFire()
    {
      return tile.field == FieldType.Wild && Cons.IsAutumn(tile.weatherGenerator.season);
    }

    void PickTile2Burn(HashSet<Tile> tiles, Tile tile1, Tile tile2, Tile tile3, bool chance1, bool chance2, bool chance3)
    {
      if (tile1 != null && chance1 &&
          ((tile1.wildFire != null && tile1.wildFire.CanCatchFire())
           || (tile1.terrian == TerrianType.Plain && tile1.wildFire.CanPlainCatchFire()))
          ) tiles.Add(tile1);
      if (tile2 != null && chance2 &&
          ((tile2.wildFire != null && tile2.wildFire.CanCatchFire())
           || (tile2.terrian == TerrianType.Plain && tile2.wildFire.CanPlainCatchFire()))
      ) tiles.Add(tile2);
      if (tile3 != null && chance3 &&
          ((tile3.wildFire != null && tile3.wildFire.CanCatchFire())
           || (tile3.terrian == TerrianType.Plain && tile3.wildFire.CanPlainCatchFire()))
      ) tiles.Add(tile3);
    }
  }

}