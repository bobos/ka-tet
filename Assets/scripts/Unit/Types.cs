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
    Routing,
    Disbanded,
    Retreated
  }

  public enum StaminaLvl
  {
    Vigorous,
    Fresh,
    Tired,
    Exhausted
  }

  public enum Reaction
  {
    Disband,
    Stepback,
    Retreat,
    Rout,
    Stand
  }

  public enum ActionType {
    UnitVisible,
    UnitLeft,
    UnitHidden,
    UnitDestroyed,
    UnitMove
  }

  public enum LvlFire
  {
    Locked,
    Lvl1,
    Lvl2,
    Lvl3,
    TopLvl
  }

  public enum LvlForest
  {
    Locked,
    Lvl1,
    Lvl2,
    Lvl3,
    TopLvl
  }

}