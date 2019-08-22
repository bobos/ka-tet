using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using PathFind;
using UnitNS;
using MonoNS;

namespace MapTileNS
{
  public abstract class Hex
  {
    public abstract int GetCost(Unit unit);
    // Q + R + S = 0
    // S = -(Q + R)
    public readonly int Q; // Column
    public readonly int R; // Row
    public readonly int S;

    // ==============================================================
    // ================= APIs hex must implement ====================
    // ==============================================================
    // TODO: need to update according to actual map's orientation
    public T NorthTile<T>() where T: Hex
    {
      if (_northHex == null)
      {
        _northHex = hexMap.GetHex(Q + 1, R);
      }
      return (T)_northHex;
    }

    public T SouthTile<T>() where T: Hex 
    {
      if (_southHex == null)
      {
        _southHex = hexMap.GetHex(Q - 1, R);
      }
      return (T)_southHex;
    }

    public T NorthWestTile<T> () where T: Hex
    {
      if (_northWestHex == null)
      {
        _northWestHex = hexMap.GetHex(Q, R + 1);
      }
      return (T)_northWestHex;
    }

    public T NorthEastTile<T> () where T: Hex
    {
      if (_northEastHex == null)
      {
        _northEastHex = hexMap.GetHex(Q + 1, R - 1);
      }
      return (T)_northEastHex;
    }

    public T SouthEastTile<T> () where T: Hex
    {
      if (_southEastHex == null)
      {
        _southEastHex = hexMap.GetHex(Q, R - 1);
      }
      return (T)_southEastHex;
    }

    public T SouthWestTile<T> () where T: Hex
    {
      if (_southWestHex == null)
      {
        _southWestHex = hexMap.GetHex(Q - 1, R + 1);
      }
      return (T)_southWestHex;
    }

    public T[] DownstreamTiles<T> () where T: Hex
    {
      if (_downstreamHexes == null)
      {
        // TODO: need to update according to actual map's orientation
        List<Hex> downstream = new List<Hex>();
        Hex downstreamHex = hexMap.GetHex(Q + 1, R);
        if (downstreamHex != null) downstream.Add(downstreamHex);
        downstreamHex = hexMap.GetHex(Q + 1, R - 1);
        if (downstreamHex != null) downstream.Add(downstreamHex);
        _downstreamHexes = downstream.ToArray();
      }
      return ToDescendentType<T>(_downstreamHexes);
    }

    public T[] Neighbours<T>() where T: Hex
    {
      if (_neighbours == null)
      {
        List<Hex> ns = new List<Hex>();
        Hex hex = hexMap.GetHex(Q + 1, R);
        if (hex != null) ns.Add(hex);
        hex = hexMap.GetHex(Q - 1, R);
        if (hex != null) ns.Add(hex);
        hex = hexMap.GetHex(Q, R + 1);
        if (hex != null) ns.Add(hex);
        hex = hexMap.GetHex(Q, R - 1);
        if (hex != null) ns.Add(hex);
        hex = hexMap.GetHex(Q + 1, R - 1);
        if (hex != null) ns.Add(hex);
        hex = hexMap.GetHex(Q - 1, R + 1);
        if (hex != null) ns.Add(hex);
        _neighbours = ns.ToArray();
      }
      return ToDescendentType<T>(_neighbours);
    }

    public T[] GetTilesWithinRangeOf<T>(int radius) where T: Hex
    {
      List<Hex> hexes = new List<Hex>();
      for (int dx = -radius; dx < radius - 1; dx++)
      {
        for (int dy = Mathf.Max(-radius + 1, -dx - radius); dy < Mathf.Min(radius, -dx + radius - 1); dy++)
        {
          Vector2 coord = this.GetCoord();
          Hex tile = hexMap.GetHex((int)coord.x + dx, (int)coord.y + dy);
          if (tile != null) hexes.Add(tile);
        }
      }
      return ToDescendentType<T>(hexes.ToArray());
    }

    T[] ToDescendentType<T> (Hex[] hexes) where T: Hex {
      List<T> ret = new List<T>();
      foreach(Hex h in hexes) {
        ret.Add((T)h);
      }
      return ret.ToArray();
    }

    Hex _northHex;
    Hex _southHex;
    Hex _northWestHex;
    Hex _northEastHex;
    Hex _southEastHex;
    Hex _southWestHex;
    Hex[] _downstreamHexes;
    Hex[] _neighbours;

    // ==============================================================
    // ==============================================================

    float Elevation;
    protected readonly HexMap hexMap;
    static readonly float WIDTH_MULTIPLIER = Mathf.Sqrt(3) / 2;
    float radius = 1.05f; // model height is 1 unity unit

    public Hex(int q, int r, HexMap hexMap)
    {
      this.Q = q;
      this.R = r;
      this.S = -(q + r);
      this.hexMap = hexMap;
    }

    public Vector2 GetCoord()
    {
      return new Vector2(Q, R);
    }

    public void setElevation(float e)
    {
      Elevation = e;
    }

    public float getElevation()
    {
      return Elevation;
    }

    public Vector3 Position()
    {
      return new Vector3(
        HorizSpacing() * (this.Q + this.R / 2f),
        0,
        VertSpacing() * this.R
      );
    }

    public static float Distance(Hex from, Hex to) {
      return Mathf.Max(
        Mathf.Abs(from.Q - to.Q),
        Mathf.Abs(from.R - to.R),
        Mathf.Abs(from.S - to.S)
      );
    }

    public delegate bool NeighboursFilter<T>(T hex) where T: Hex;

    public T[] GetNeighboursWithinRange<T>(int range, NeighboursFilter<T> filter) where T: Hex
    {
      //if (range < 1) return new Hex[0];
      HashSet<Hex> hexes = new HashSet<Hex>();
      HashSet<Hex> core = new HashSet<Hex>();
      hexes.Add(this);
      core.Add(this);
      FindNeighbours(hexes, core, range, filter);
      hexes.Remove(this);
      return ToDescendentType<T>(hexes.ToArray());
    }
   
    float Height()
    {
      return radius * 2;
    }

    float Width()
    {
      return WIDTH_MULTIPLIER * Height();
    }

    float VertSpacing()
    {
      return Height() * 0.75f;
    }

    float HorizSpacing()
    {
      return Width();
    }

    void FindNeighbours<T>(HashSet<Hex> hexes, HashSet<Hex> innerRing, int range, NeighboursFilter<T> filter) where T: Hex
    {
      if (range-- == 0) return;
      HashSet<Hex> outerRing = new HashSet<Hex>();
      foreach (Hex h in innerRing)
      {
        foreach (Hex h1 in h.Neighbours<Hex>())
        {
          if (!hexes.Contains(h1))
          {
            // outer ring
            outerRing.Add(h1);
            hexes.Add(h1);
          }
        }
      }
      FindNeighbours<T>(hexes, outerRing, range, filter);
    }
  }

}