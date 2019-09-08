using MonoNS;
using MapTileNS;

namespace BuildingNS
{
  public class Supply
  {
    HexMap hexMap;
    public Supply(HexMap hexMap) {
      this.hexMap = hexMap;
    }

    public void RenderSupplyLine(Tile[] path) {
      foreach (Tile tile in path)
      {
        if(!hexMap.IsOverlayFoW(tile)) {
          hexMap.OverlaySupplyLine(tile);
        }
      }
    }
  }

}