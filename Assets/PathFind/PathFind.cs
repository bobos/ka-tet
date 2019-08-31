namespace PathFind
{

  public enum Mode {
    Normal,
    Supply
  }

  public static class PathFinder
  {
    public static PFTile[] FindPath(PFTile start,
                                    PFTile target,
                                    PFUnit unit,
                                    //CostEstimateDelegate estimator,
                                    Mode mode = Mode.Normal)
    {
      if (start == null || target == null || unit == null)
      {
        return null;
      }
      AStar<PFTile> resolver = new AStar<PFTile>(start, target, unit, false, 0, mode);
      return resolver.Find();
    }

    public static PFTile[] FindAccessibleTiles(PFTile start,
    PFUnit unit,
    int remainingPoint,
    Mode mode = Mode.Normal)
    {
      if (start == null || unit == null)
      {
        return null;
      }
      AStar<PFTile> resolver = new AStar<PFTile>(start, start, unit,
                                       // anonymous function
                                       //(IQPathTile a, IQPathTile b) => 0,
                                       true, remainingPoint, mode);
      return resolver.Find();
    }
  }

  public delegate int CostEstimateDelegate(PFTile a, PFTile b);

}