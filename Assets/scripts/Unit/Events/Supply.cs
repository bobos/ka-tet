using System.Collections.Generic;
using MapTileNS;

namespace UnitNS
{
  public class Supply
  {
    public bool consumed = true;
    Unit unit;
    public Supply(Unit unit) {
      this.unit = unit;
    }

    public void Consume(int[] effects, List<Tile> controlledTiles)
    {
      consumed = false;
      if (unit.IsCamping()) {
        if (unit.tile.settlement.CanProvideSupply()) {
          consumed = true;
        }
      } else if (controlledTiles.Contains(unit.tile)) {
        consumed = true;
      }
      consumed = consumed ? 
        unit.hexMap.GetWarParty(unit).ConsumeSupply(SupplyNeededPerTurn()) :
        false;

      if (!consumed) {
        bool improvise = unit.ImproviseOnSupply();
        int moraleReduce = improvise ? -1 : -5;
        unit.rf.morale += moraleReduce;
        effects[0] = moraleReduce;
        effects[2] = unit.Killed(Util.Rand(0, improvise ? 10 : 30));
      }
    }

    public int SupplyNeededPerTurn()
    {
      return (int)(unit.rf.soldiers * unit.hexMap.FoodPerManPerTurn(unit.IsAI()));
    }

  }
}