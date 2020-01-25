using MapTileNS;
using FieldNS;

public class Camp : Settlement
{
  public Camp(string name, Tile tile, WarParty warParty, int storageLevel) :
  base(name, tile, warParty, 0, 3,
    new StorageLevel(storageLevel > 2 ? 2 : storageLevel),
    new WallDefense(1))
  {
    civillian_male = civillian_female = civillian_child = 0;
    this.labor = 0;
    type = Settlement.Type.camp;
    state = State.constructing;
  }
}


