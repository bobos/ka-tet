namespace UnitNS
{
  public enum Type
  {
    LightCavalry,
    HeavyCavalry,
    Infantry
  }

  public enum State
  {
    Stand,
    Routed,
    Camping,
    Disbanded,
    Retreated
  }

  public enum ActionType {
    UnitVisible,
    UnitLeft,
    UnitHidden,
    UnitDestroyed
  }

  public enum DestroyType {
    ByWildFire,
    ByFlood,
    ByDisband
  }

}