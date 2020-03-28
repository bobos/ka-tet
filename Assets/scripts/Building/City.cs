using MapTileNS;
using FieldNS;

public class City : Settlement
{
  public City(string name, Tile tile, WarParty warParty,
    int male, int female, int child,
    int storageLevel,
    int wallLevel) :
    base(name, tile, warParty, 3,
    new StorageLevel(storageLevel),
    new WallDefense(wallLevel < 2 ? 2 : wallLevel)
    )
  {
    this.civillian_male = male;
    this.civillian_female = female;
    this.civillian_child = child;
    type = Settlement.Type.city;
    state = State.normal;
    buildWork = 0;
  }
}
