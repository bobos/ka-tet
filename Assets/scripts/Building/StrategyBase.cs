using MapTileNS;
using FieldNS;

public class StrategyBase : Settlement
{
  public StrategyBase(string name, Tile tile, WarParty warParty, int supply, int labor, int storageLevel) :
  base(name, tile, warParty, supply, 6,
    new StorageLevel(storageLevel < 2 ? 2 : storageLevel),
    new WallDefense(1))
  {
    this.civillian_male = civillian_female = civillian_child = 0;
    this.labor = labor;
    type = Settlement.Type.strategyBase;
    state = State.normal;
    buildWork = 0;
  }
}


