using MonoNS;
using UnitNS;
using MapTileNS;
using System.Collections.Generic;

namespace FieldNS
{
  public class FoW
  {
    HexMap hexMap;
    public static FoW instance;
    FoW(HexMap hexMap) {
      this.hexMap = hexMap;
      hexMap.actionBroker.onUnitAction += OnUnitAction;
      // init fow for player at game start
      Fog(false);
    }

    public void OnUnitAction(Unit unit, ActionType action, Tile _tile) {
      if (action == ActionType.UnitMove) {
        Fog(unit.IsAI());
      }
    }

    public HashSet<Tile> GetEnemyVisibleArea(bool isAI) {
      HashSet<Tile> tiles = new HashSet<Tile>();
      WarParty party = isAI ? hexMap.GetAIParty() : hexMap.GetPlayerParty();
      party.GetVisibleArea(tiles);
      hexMap.settlementMgr.GetVisibleArea(party.attackside, tiles);
      return tiles;
    }

    public void Fog(bool isAI) {
      HashSet<Tile> tiles = GetEnemyVisibleArea(isAI);
      foreach (Tile tile in hexMap.tiles)
      {
        if (tiles.Contains(tile)) {
          hexMap.OverlayDisable(tile);
        } else {
          hexMap.OverlayFoW(tile);
        }
      }
    }

    public static void Init(HexMap hexMap) {
      if (instance == null) {
        instance = new FoW(hexMap);
      }
    }

    public static FoW Get() {
      return instance;
    }
  }

}