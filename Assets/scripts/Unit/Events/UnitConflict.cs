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
      if (conflicted || unit.IsCommander() || unit.ApplyDiscipline(false) || unit.rf.general.Is(Cons.cunning)) {
        return result;
      }

      conflicted = true;
      List<Region> conflictRegions = unit.rf.province.region.GetConflictRegions();
      Unit target = null;
      foreach(Tile tile in unit.tile.neighbours) {
        Unit u = tile.GetUnit();
        if (u != null &&
        !u.IsCommander() &&
        u.IsAI() == unit.IsAI()) {
          int chance = unit.inCommanderRange ? 0 : 40;
          if(conflictRegions.Contains(u.rf.province.region)) {
            chance += 50;
          } else if (!Util.eq<Region>(u.rf.province.region, unit.rf.province.region)) {
            chance += 30;
          } else if (!Util.eq<Province>(u.rf.province, unit.rf.province)) {
            chance += 20;
          } else {
            chance = 0;
          }

          if (Util.Rand(1, 100) < chance) {
            target = u;
            break;
          }
        }
      }

      if (target == null) {
        return result;
      }

      result.moralDrop = -Util.Rand(3, 5);
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