using System.Collections;
using System.Collections.Generic;

namespace PathFind
{
  public interface PFUnit
  {
    int GetFullMovement();
  }

  public interface PFTile
  {
		PFTile[] GetNeighbourTiles();
		int AggregateCostToEnter(int costSoFar, PFTile sourceTile, PFUnit unit);
    //float GetCost(PFUnit unit, bool unaccessibleHill=false);
  }

}