using System.Collections.Generic;
using UnitNS;

namespace MapTileNS
{
  public class Zone {
    int type;
    Tile tile;
    public Zone(int type, Tile tile) {
      this.type = type;
      this.tile = tile;
    }

    public int Cost(Unit unit) {
      if (!tile.Accessible()) {
        return Unit.MovementCostOnUnaccesible;
      }

      if (tile.field == FieldType.Flooded) {
        return 100;
      }

      if (tile.vantagePoint) {
        if (Red()) {
          return 100;
        }
        if (Yellow()) {
          return 80;
        }
        return 50;
      }

      if (Red()) {
        int cost = 60;
        foreach(Tile t in tile.neighbours) {
          Unit u = t.GetUnit();
          if (u != null && u.IsAI() != unit.IsAI() && u.IsOnField() && u.IsCavalry() && !u.NoRedZone()) {
            cost = 90;
            break;
          }
        }
        return cost;
      }

      if (unit.IsCavalry()) {
        if (Green()) {
          return 20;
        }
        return 30;
      } else {
        if (Green()) {
          return 25;
        }
        return 40;
      }
    }

    public bool Red() { return type == 3; } 
    public bool Green() { return type == 1; } 
    public bool Yellow() { return type == 2; } 

    public static Zone Get(Tile tile, HashSet<Tile>[] colorMap = null) {
      if (colorMap == null) {
        return new Zone(1, tile);
      }
      if (colorMap[1].Contains(tile)) {
        return new Zone(3, tile);
      }
      if (colorMap[0].Contains(tile)) {
        return new Zone(1, tile);
      }
      return new Zone(2, tile); 
    }
  }
}