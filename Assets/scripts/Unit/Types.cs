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
    Camping,
    Disbanded,
    Retreated
  }

  public enum Mental {
    Supercharged,
    Normal,
    Waving,
    Defeating,
    Chaotic
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