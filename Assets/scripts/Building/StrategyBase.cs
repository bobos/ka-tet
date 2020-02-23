﻿using MapTileNS;
using FieldNS;

public class StrategyBase : Settlement
{
  public StrategyBase(string name, Tile tile, WarParty warParty, int supply, int labor) :
  base(name, tile, warParty, supply, 2,
    new StorageLevel(3),
    new WallDefense(2))
  {
    this.civillian_male = civillian_female = civillian_child = 0;
    this.labor = labor;
    type = Settlement.Type.strategyBase;
    state = State.normal;
    buildWork = 0;
  }
}


