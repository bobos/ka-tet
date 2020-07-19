namespace UnitNS
{
  public enum Type
  {
    Cavalry,
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