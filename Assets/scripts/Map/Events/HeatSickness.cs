using System.Collections.Generic;
using System.Linq;
using PathFind;
using UnitNS;
using MonoNS;
using NatureNS;
using UnityEngine;

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
      if (tile.type != FieldType.Wild) {
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