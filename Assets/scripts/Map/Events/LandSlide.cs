﻿using System.Collections.Generic;
using System.Linq;
using PathFind;
using UnitNS;
using MonoNS;
using NatureNS;
using UnityEngine;

namespace MapTileNS
{
  public class LandSlide
  {
    Tile tile;

    public LandSlide(Tile tile) {
      this.tile = tile;
      tile.ListenOnHeavyRain(OnHeavyRain);
    }

    public void OnHeavyRain()
    {
      if (tile.settlement != null) {
        tile.RemoveOnHeavyRainListener(OnHeavyRain);
        return;
      }
      if (Cons.FairChance() && tile.GetUnit() != null)
      {
        tile.DisasterAffectUnit(DisasterType.LandSlide);
        tile.RemoveOnHeavyRainListener(OnHeavyRain);
      }
    }

  }

}