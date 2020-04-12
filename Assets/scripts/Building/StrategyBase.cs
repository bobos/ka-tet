using MapTileNS;
using FieldNS;

public class StrategyBase : Settlement
{
  public StrategyBase(string name, Tile tile, WarParty warParty) :
  base(name, tile, warParty, 2,
    new StorageLevel(3),
    new WallDefense(1))
  {
    this.civillian_male = civillian_female = civillian_child = 0;
    type = Settlement.Type.strategyBase;
    state = State.normal;
    buildWork = 0;
  }
}


