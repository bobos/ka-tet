using System.Collections.Generic;
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
      conflicted = true;
      Unit target = null;
      List<Province> conflictProvinces = unit.rf.province.GetConflictProvinces();
      foreach(Tile tile in unit.tile.neighbours) {
        Unit u = tile.GetUnit();
        if (u != null && u.IsAI() == unit.IsAI()) {
          if(conflictProvinces.Contains(u.rf.province) || !Util.eq<Region>(u.rf.province.region, unit.rf.province.region)) {
            target = u;
            break;
          }
        }
      }

      if (target == null) {
        return result;
      }

      result.moralDrop = -Util.Rand(25, 40);
      result.unit1 = unit;
      result.unit2 = target;

      result.unit1Dead = unit.Killed(Util.Rand(10, 40));
      result.unit2Dead = target.Killed(Util.Rand(10, 40));
      unit.morale += result.moralDrop;
      target.morale += result.moralDrop;

      return result;
    }
  }
}