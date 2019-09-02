using System.Collections.Generic;
using System.Linq;
using PathFind;
using UnitNS;
using MonoNS;
using NatureNS;
using UnityEngine;

namespace MapTileNS
{
  public class Drowning
  {
    Tile tile;
    public Drowning(Tile tile) {
      this.tile = tile;
      tile.ListenOnRain(OnRain);
    }

    public void OnRain()
    {
      if (tile.type != FieldType.Settlement && Cons.SlimChance()
      && !Cons.IsAutumn(tile.weatherGenerator.season) && !Cons.IsWinter(tile.weatherGenerator.season)
      && tile.GetUnit() != null)
      {
        tile.GetUnit().SoldiersDrown();
      }
    }

  }

}