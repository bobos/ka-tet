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
      if (CanSetFire()) {
        tile.ListenOnDry(OnDry);
      }
    }

    public void OnRain()
    {
      PutOutFire();
    }

    public void OnSeasonChange(Season season)
    {
      if (Cons.IsWinter(season))
      {
        PutOutFire();
      }
    }

    public void OnTurnEnd() {
      burningCntDown--;
      if (burningCntDown < 1)
      {
        PutOutFire();
      }
    }

    public void OnDry()
    {
      if (Cons.SlimChance()) {
        SpreadFire();
        tile.RemoveOnDryListener(OnDry);
      }
    }

    public bool Start()
    {
      if (Cons.IsSpring(tile.weatherGenerator.season) || Cons.IsWinter(tile.weatherGenerator.season))
      {
        return false;
      }

      if (Cons.IsRain(tile.weatherGenerator.currentWeather) || Cons.IsHeavyRain(tile.weatherGenerator.currentWeather))
      {
        return false;
      }

      if (!burnable)
      {
        return false;
      }
      
      SpreadFire();
      return true;
    }

    public void SpreadFire()
    {
      burnable = false;
      tile.DisasterAffectUnit(DisasterType.WildFire);
      tile.SetFieldType(FieldType.Burning);
      burningCntDown = Util.Rand(2, BurningLasts);
      tile.ListenOnTurnEnd(OnTurnEnd);
      tile.ListenOnSeason(OnSeasonChange);
      tile.ListenOnRain(OnRain);
      tile.ListenOnHeavyRain(OnRain);
      // spread to neighbours depends on season and wind and directions
      Cons.Direction windDirection = tile.windGenerator.direction;
      List<Tile> affectedTiles = new List<Tile>();
      bool chance1 = Cons.FairChance();
      bool chance2 = Cons.SlimChance();
      bool chance3 = Cons.SlimChance();
      if (Cons.IsGale(tile.windGenerator.current))
      {
        chance1 = Cons.MostLikely();
        chance2 = Cons.EvenChance();
        chance3 = Cons.EvenChance();
      }
      else if (Cons.IsWind(tile.windGenerator.current))
      {
        chance1 = Cons.EvenChance();
        chance2 = Cons.FairChance();
        chance3 = Cons.FairChance();
      }

      if (windDirection == Cons.Direction.dueNorth)
      {
        PickTile2Burn(affectedTiles, tile.NorthTile<Tile>(), tile.NorthEastTile<Tile>(),
                      tile.NorthWestTile<Tile>(), chance1, chance2, chance3);
      }

      if (windDirection == Cons.Direction.dueSouth)
      {
        PickTile2Burn(affectedTiles, tile.SouthTile<Tile>(), tile.SouthEastTile<Tile>(),
                      tile.SouthWestTile<Tile>(), chance1, chance2, chance3);
      }

      if (windDirection == Cons.Direction.northEast)
      {
        PickTile2Burn(affectedTiles, tile.NorthEastTile<Tile>(), tile.NorthTile<Tile>(),
                      tile.SouthEastTile<Tile>(), chance1, chance2, chance3);
      }

      if (windDirection == Cons.Direction.northWest)
      {
        PickTile2Burn(affectedTiles, tile.NorthWestTile<Tile>(), tile.NorthTile<Tile>(),
                      tile.SouthWestTile<Tile>(), chance1, chance2, chance3);
      }

      if (windDirection == Cons.Direction.southWest)
      {
        PickTile2Burn(affectedTiles, tile.SouthWestTile<Tile>(), tile.SouthTile<Tile>(),
                      tile.NorthWestTile<Tile>(), chance1, chance2, chance3);
      }

      if (windDirection == Cons.Direction.southEast)
      {
        PickTile2Burn(affectedTiles, tile.SouthEastTile<Tile>(), tile.SouthTile<Tile>(),
                      tile.NorthEastTile<Tile>(), chance1, chance2, chance3);
      }

      foreach (Tile tile in affectedTiles)
      {
        if ((tile.wildFire != null && tile.wildFire.CanSetFire())
          || (tile.terrian == TerrianType.Plain && tile.wildFire.CanPlainCatchFire()))
        {
          tile.wildFire.SpreadFire();
        }
      }
    }
    
    public void PutOutFire()
    {
      tile.SetFieldType(FieldType.Schorched);
      tile.RemoveTurnEndListener(OnTurnEnd);
      tile.RemoveOnHeavyRainListener(OnRain);
      tile.RemoveOnRainListener(OnRain);
      tile.RemoveOnSeasonListener(OnSeasonChange);
    }

    public bool CanSetFire()
    {
      if (tile.terrian == TerrianType.Hill || tile.terrian == TerrianType.Mountain)
      {
        return true;
      }
      return false;
    }

    // Can the plain tile caught fire by nearby burning tiles
    public bool CanPlainCatchFire()
    {
      if (tile.field == FieldType.Wild && Cons.IsAutumn(tile.weatherGenerator.season))
      {
        return true;
      }
      return false;
    }

    void PickTile2Burn(List<Tile> tiles, Tile tile1, Tile tile2, Tile tile3, bool chance1, bool chance2, bool chance3)
    {
      if (tile1 != null && chance1) tiles.Add(tile1);
      if (tile2 != null && chance2) tiles.Add(tile2);
      if (tile3 != null && chance3) tiles.Add(tile3);
    }
  }

}