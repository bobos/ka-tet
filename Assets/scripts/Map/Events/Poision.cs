using System.Collections.Generic;
using System.Linq;
using PathFind;
using UnitNS;
using MonoNS;
using NatureNS;
using UnityEngine;

namespace MapTileNS
{
  public class Poision
  {
    Tile tile;
    public HashSet<Tile> downStreams = new HashSet<Tile>();
    public Poision(Tile tile) {
      this.tile = tile;
      List<Tile> tiles = AddDownstream(tile);
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

    List<Tile> AddDownstream(Tile tile) {
      List<Tile> tiles = new List<Tile>();
      foreach (Tile t in tile.DownstreamTiles()) {
        if (t.terrian == TerrianType.Water) {
          downStreams.Add(t);
          tiles.Add(t);
        }
      }
      return tiles;
    }

    public void Poision()
    {
      foreach(Tile tile in downStreams) {
        foreach (Unit unit in tile.GetUnitsNearBy())
        {
          unit.Poisioned();   
        }
      }
    }

  }

}