using MapTileNS;
using FieldNS;

public class City : Settlement
{
  public City(string name, Tile tile, WarParty warParty,
    int supply, int male, int female, int child, int labor,
    int storageLevel,
    int wallLevel) :
    base(name, tile, warParty, supply, 8,
    new StorageLevel(storageLevel < 2 ? 2 : storageLevel),
    new WallDefense(wallLevel < 2 ? 2 : wallLevel)
    )
  {
    this.labor = labor;
    this.civillian_male = male;
    this.civillian_female = female;
    this.civillian_child = child;
    type = Settlement.Type.city;
    state = State.normal;
    buildWork = 0;
  }
}
