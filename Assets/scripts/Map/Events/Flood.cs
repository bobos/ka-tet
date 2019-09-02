using System.Collections.Generic;
using System.Linq;
using PathFind;
using UnitNS;
using MonoNS;
using NatureNS;
using UnityEngine;

namespace MapTileNS
{
  public class Flood
  {
    public const int FloodingLasts = 15;
    public int floodingCntDown = 0;

    Tile tile;
    bool flooded = false;
    bool isDam = false;

    public Flood(Tile tile, bool isDam) {
      this.tile = tile;
      this.isDam = isDam;
      if (isDam) {
        tile.ListenOnHeavyRain(OnHeavyRain);
      }
    }

    public void OnHeavyRain()
    {
      if (Cons.FairChance())
      {
        Start();
        tile.RemoveOnHeavyRainListener(OnHeavyRain);
      }
    }

    public void OnTurnEnd() {
      floodingCntDown--;
      if (floodingCntDown < 1)
      {
        FloodRecede();
      }
    }

    public bool Start()
    {
      if (!flooded && (Cons.IsSpring(weatherGenerator.season) || Cons.IsSummer(weatherGenerator.season))
          && (Cons.IsRain(weatherGenerator.currentWeather) || Cons.IsHeavyRain(weatherGenerator.currentWeather))) 
      {
        SpreadFlood();
        return true;
      }
      return false;
    }

    public void SpreadFlood()
    {
      flooded = true;
      tile.DisasterAffectUnit(DisasterType.Flood);
      foreach (Tile tile in this.tile.DownstreamTiles<Tile>())
      {
        if (tile.terrian == TerrianType.Water || tile.field == FieldType.Flooding)
        {
          tile.flood.SpreadFlood();
        }
        else if (tile.flood.CanBeFloodedByNearByTile() &&
         (Cons.IsHeavyRain(weatherGenerator.currentWeather) ? Cons.HighlyLikely() : Cons.MostLikely()))
        {
          tile.wildFire.PutOutFire();
          tile.floodingCntDown = Util.Rand(2, FloodingLasts);
          tile.SetFieldType(FieldType.Flooding);
          tile.ListenOnTurnEnd(OnTurnEnd);
          tile.flood.SpreadFlood();
        }
      }
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