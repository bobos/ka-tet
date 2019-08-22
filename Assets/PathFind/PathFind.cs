using System.Collections;
using System.Collections.Generic;

namespace PathFind
{

  public static class PathFinder
  {
    public static PFTile[] FindPath(PFTile start,
                                    PFTile target,
                                    PFUnit unit,
                                    //CostEstimateDelegate estimator,
                                    bool unaccessibleHill = false)
    {
      if (start == null || target == null || unit == null)
      {
        return null;
      }
      AStar<PFTile> resolver = new AStar<PFTile>(start, target, unit, false, 0, unaccessibleHill);
      return resolver.Find();
    }

    public static PFTile[] FindAccessibleTiles(PFTile start,
    PFUnit unit,
    int remainingPoint,
    bool unaccessibleHill = false)
    {
      if (start == null || unit == null)
      {
        return null;
      }
      AStar<PFTile> resolver = new AStar<PFTile>(start, start, unit,
                                       // anonymous function
                                       //(IQPathTile a, IQPathTile b) => 0,
                                       true, remainingPoint, unaccessibleHill);
      return resolver.Find();
    }
  }

  public delegate int CostEstimateDelegate(PFTile a, PFTile b);

}