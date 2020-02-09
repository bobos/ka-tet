using NatureNS;

namespace MonoNS
{
  public class WindGenerator : BaseController
  {

    // TODO: instance per province
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      turnController = hexMap.turnController;
      turnController.onNewTurn += OnTurnEnd;
      current = GenerateWind();
      direction = DecideDirection();
      // initialize the wind
      OnTurnEnd();
    }

    public override void UpdateChild() {}

    public Current current;
    public Cons.Direction direction;
    TurnController turnController;
    Current nextCurrent = null;
    Cons.Direction nextDirection;

    public void OnTurnEnd()
    {
      if (nextCurrent == null)
      {
        // first turn
        nextCurrent = GenerateWind();
        nextDirection = DecideDirection();
      }
      else
      {
        direction = nextDirection;
        current = nextCurrent;
        nextCurrent = GenerateWind();
        nextDirection = DecideDirection();
      }
    }

    public Current ForecastWind()
    {
      return nextCurrent;
    }

    public Cons.Direction ForecastDirection()
    {
      return nextDirection;
    }

    Current GenerateWind()
    {
      int luckNum = Util.Rand(1, 10);
      if (luckNum < 6)
      {
        return Cons.nowind;
      }
      else if (luckNum < 9)
      {
        return Cons.wind;
      }
      else
      {
        return Cons.gale;
      }
    }

    Cons.Direction DecideDirection()
    {
      int luckNum = Util.Rand(1, 6);
      if (luckNum == 1) return Cons.Direction.dueSouth;
      if (luckNum == 2) return Cons.Direction.dueNorth;
      if (luckNum == 3) return Cons.Direction.northEast;
      if (luckNum == 4) return Cons.Direction.northWest;
      if (luckNum == 5) return Cons.Direction.southEast;
      return Cons.Direction.southWest;
    }
  }

}