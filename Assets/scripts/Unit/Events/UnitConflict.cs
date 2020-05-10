﻿using System.Collections.Generic;
using CourtNS;
using MapTileNS;

namespace UnitNS
{
  public class ConflictResult {
    public Unit unit1;
    public Unit unit2;
    public int moralDrop = 0;
    public int unit1Dead;
    public int unit2Dead;
  }

  public class UnitConflict
  {
    public bool conflicted = false;
    Unit unit;
    public UnitConflict(Unit unit) {
      this.unit = unit;
    }

    public ConflictResult Occur() {
      ConflictResult result = new ConflictResult();
      if (conflicted || unit.ApplyDiscipline(Cons.HighlyLikely())) {
        return result;
      }

      List<Region> conflictRegions = unit.rf.province.region.GetConflictRegions();
      Unit target = null;
      foreach(Tile tile in unit.tile.neighbours) {
        Unit u = tile.GetUnit();
        if (u != null &&
        u.IsAI() == unit.IsAI() &&
        !u.unitConflict.conflicted &&
        conflictRegions.Contains(u.rf.province.region)) {
          target = u;
          break;
        }
      }

      if (target == null) {
        return result;
      }

      conflicted = target.unitConflict.conflicted = true;
      result.moralDrop = -Util.Rand(3, 8);
      result.unit1 = unit;
      result.unit2 = target;

      result.unit1Dead = unit.Killed(Util.Rand(3, 30));
      result.unit2Dead = target.Killed(Util.Rand(3, 30));
      unit.rf.morale += result.moralDrop;
      target.rf.morale += result.moralDrop;

      return result;
    }
  }
}