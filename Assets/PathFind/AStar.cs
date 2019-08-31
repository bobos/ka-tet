using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PathFind
{

  public class AStar<T> where T : PFTile
  {
    public AStar(T start,
    T target,
    PFUnit unit,
    //CostEstimateDelegate estimator,
    bool findRange,
    int remaining,
    bool targetAlwaysAccessible,
    bool ignoreUnit)
    {
      this.start = start;
      this.target = target;
      this.unit = unit;
      this.findRange = findRange;
      this.remaining = remaining;
      this.targetAlwaysAccessible = targetAlwaysAccessible;
      this.ignoreUnit = ignoreUnit;
      path = new Queue<T>();
    }

    T start;
    T target;
    PFUnit unit;
    bool findRange;
    int remaining;
    bool targetAlwaysAccessible;
    bool ignoreUnit;

    Queue<T> path;
    public T[] Find()
    {
      HashSet<T> closedSet = new HashSet<T>();
      PathfindingPriorityQueue<T> openSet = new PathfindingPriorityQueue<T>();

      openSet.Enqueue(start, 0);
      Dictionary<T, T> came_from = new Dictionary<T, T>();

      // total cost to get to a tile
      Dictionary<T, int> g_score = new Dictionary<T, int>();
      g_score[start] = 0;

      // estimated cost to get to a tile
      Dictionary<T, int> f_score = new Dictionary<T, int>();
      //f_score[start] = estimator(start, target);
      f_score[start] = 0;
      while (openSet.Count > 0)
      {
        T current = openSet.Dequeue();

        // Check to see if we are there.
        if (System.Object.ReferenceEquals(current, target) && !findRange)
        {
          Reconstruct_path(came_from, current);
          break;
        }

        closedSet.Add(current);

        foreach (T neighbour in current.GetNeighbourTiles())
        {
          if (closedSet.Contains(neighbour))
          {
            continue; // ignore this already completed neighbor
          }

          // If it's target tile, ignore the restrain
          int total_pathfinding_cost_to_neighbor = 
            (targetAlwaysAccessible && System.Object.ReferenceEquals(neighbour, target)) ? 0 :
            neighbour.AggregateCostToEnter(g_score[current], current, unit, ignoreUnit);

          if (total_pathfinding_cost_to_neighbor < 0)
          {
            // Values less than zero represent an invalid/impassable tile
            continue;
          }
          //Debug.Log(total_pathfinding_cost_to_neighbor);

          int tentative_g_score = total_pathfinding_cost_to_neighbor;

          // Is the neighbour already in the open set?
          //   If so, and if this new score is worse than the old score,
          //   discard this new result.
          if (openSet.Contains(neighbour) && tentative_g_score >= g_score[neighbour])
          {
            continue;
          }

          // This is either a new tile or we just found a cheaper route to it
          came_from[neighbour] = current;
          g_score[neighbour] = tentative_g_score;
          //f_score[neighbour] = g_score[neighbour] + estimator(neighbour, target);
          f_score[neighbour] = g_score[neighbour] + 0;

          openSet.EnqueueOrUpdate(neighbour, f_score[neighbour]);
        } // foreach neighbour
      } // while

      if (findRange) {
        List<T> ret = new List<T>();
        foreach(KeyValuePair<T, int> item in g_score) {
          if(remaining >= item.Value) ret.Add(item.Key);
        }
        return ret.ToArray();
      }

      return path.ToArray();
    }
    private void Reconstruct_path(
      Dictionary<T, T> came_From,
      T current)
    {
      // So at this point, current IS the goal.
      // So what we want to do is walk backwards through the Came_From
      // map, until we reach the "end" of that map...which will be
      // our starting node!
      Queue<T> total_path = new Queue<T>();
      total_path.Enqueue(current); // This "final" step is the path is the goal!

      while (came_From.ContainsKey(current))
      {
        /*    Came_From is a map, where the
      *    key => value relation is real saying
      *    some_node => we_got_there_from_this_node
      */

        current = came_From[current];
        total_path.Enqueue(current);
      }

      // At this point, total_path is a queue that is running
      // backwards from the END tile to the START tile, so let's reverse it.
      path = new Queue<T>(total_path.Reverse());
    }

  }

}
