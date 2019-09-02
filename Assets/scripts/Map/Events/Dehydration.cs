using System.Collections.Generic;
using System.Linq;
using PathFind;
using UnitNS;
using MonoNS;
using NatureNS;
using UnityEngine;

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
      if (tile.type == FieldType.Settlement || tile.type == FieldType.Burning || tile.type == FieldType.Flooding) {
        return;
      }
      if (Cons.FairChance() && tile.GetUnit() != null)
      {
        if (!tile.IsWaterBound()) {
          tile.GetUnit().Dehydrate();
        }
      }
    }

  }

}