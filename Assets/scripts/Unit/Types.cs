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
    Conceal,
    Camping,
    Disbanded,
    Retreated
  }

  public enum StaminaLvl
  {
    Fresh,
    Tired,
    Exhausted
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