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
        // TODO: apply general trait
        int moraleReduce = -5;
        unit.rf.morale += moraleReduce;
        int deathNum = Util.Rand(0, 30);
        unit.kia += deathNum;
        unit.rf.soldiers -= deathNum;
        effects[0] = moraleReduce;
        effects[2] = deathNum;
      }
    }

    public int SupplyNeededPerTurn()
    {
      return (int)(unit.rf.soldiers * unit.hexMap.FoodPerManPerTurn(unit.IsAI()));
    }

  }
}