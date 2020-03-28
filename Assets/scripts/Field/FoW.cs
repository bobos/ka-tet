using MonoNS;
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
      // init fow for player at game start
      Fog();
    }

    public HashSet<Tile> GetVisibleArea(bool enemy = false) {
      HashSet<Tile> tiles = new HashSet<Tile>();
      // TODO: AI test
      WarParty party;
      if (enemy) {
        if (!hexMap.deployDone) {
          return tiles;
        }
        party = hexMap.turnController.playerTurn ? hexMap.GetAIParty() : hexMap.GetPlayerParty();
      } else {
        if (!hexMap.deployDone) {
          return new HashSet<Tile>(hexMap.InitPlayerDeploymentZone());
        }
        party = hexMap.turnController.playerTurn ? hexMap.GetPlayerParty() : hexMap.GetAIParty();
      }
      party.GetVisibleArea(tiles);
      return tiles;
    }

    public void Fog() {
      HashSet<Tile> tiles = GetVisibleArea();
      foreach (Tile tile in hexMap.tiles)
      {
        if (tiles.Contains(tile)) {
          hexMap.OverlayDisable(tile, GetVisibleArea(true));
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