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
      Fog();
    }

    public void OnUnitAction(Unit _unit, ActionType action, Tile _tile) {
      if (action == ActionType.UnitMove) {
        Fog();
      }
    }

    public HashSet<Tile> GetEnemyVisibleArea() {
      HashSet<Tile> tiles = new HashSet<Tile>();
      // TODO: AI test
      WarParty party = hexMap.turnController.playerTurn ? hexMap.GetPlayerParty() : hexMap.GetAIParty();
      party.GetVisibleArea(tiles);
      hexMap.settlementMgr.GetVisibleArea(party.attackside, tiles);
      return tiles;
    }

    public void Fog() {
      HashSet<Tile> tiles = GetEnemyVisibleArea();
      foreach (Tile tile in hexMap.tiles)
      {
        if (tiles.Contains(tile)) {
          hexMap.OverlayDisable(tile);
        } else {
          hexMap.OverlayFoW(tile);
        }
      }
    }

    public bool IsFogged(Tile tile) {
      return hexMap.IsOverlayFoW(tile);
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