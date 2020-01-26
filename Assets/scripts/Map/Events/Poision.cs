using System.Collections.Generic;
using UnitNS;

namespace MapTileNS
{
  public class Poision
  {
    Tile tile;
    public HashSet<Tile> downStreams = new HashSet<Tile>();
    public Poision(Tile tile) {
      this.tile = tile;
      HashSet<Tile> tiles = AddDownstream(tile);
      for (int i = 0; i < 8; i++)
      {
        HashSet<Tile> tiles1 = new HashSet<Tile>();
        foreach(Tile t in tiles) {
          foreach(Tile t1 in AddDownstream(t)) {
            tiles1.Add(t1);
          }
        }
        tiles = tiles1;
      }
    }

    HashSet<Tile> AddDownstream(Tile tile) {
      HashSet<Tile> tiles = new HashSet<Tile>();
      foreach (Tile t in tile.DownstreamTiles<Tile>()) {
        if (t.terrian == TerrianType.Water) {
          downStreams.Add(t);
          tiles.Add(t);
        }
      }
      return tiles;
    }

    public Unit[] SetPoision(Unit initiator)
    {
      List<Unit> units = new List<Unit>();
      foreach(Tile tile in downStreams) {
        foreach (Tile t in tile.neighbours)
        {
          Unit unit = t.GetUnit();
          if (unit != null && !Util.eq<Unit>(unit, initiator)) {
            if (unit.Poisioned()) {
              units.Add(unit);
            }
          }
        }
      }
      return units.ToArray();
    }

  }

}