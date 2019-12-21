﻿namespace UnitNS
{
  public class Supply
  {
    public const float Starving2DeathRate = 0.01f;
    public const float Starving2EscapeRate = 0.02f;
    public bool consumed = false;
    public bool isStarving = false;
    Unit unit;
    public int supply = 0;
    public Supply(Unit unit, int supply) {
      this.unit = unit;
      this.supply = supply;
    }

    public int GetStarvingDessertNum()
    {
      return (int)(unit.rf.soldiers * Starving2EscapeRate);
    }

    public int GetStarvingKillNum()
    {
      return (int)(unit.rf.soldiers * Starving2DeathRate);
    }

    public int GetLastingTurns() {
      int needed = SupplyNeededPerTurn();
      int neededHalf = MinSupplyNeeded();

      int remaining = supply % needed;
      int turns = supply / needed;
      if (supply < neededHalf) {
        return 0;
      }
      if (supply < needed) {
        return 1;
      }
      return turns + (remaining < neededHalf ? 0 : 1);
    }

    public int TakeTransferSupply(int inSupply) {
      int ret = TakeInSupply(inSupply);
      if (ret != inSupply && isStarving) {
        Consume();
        isStarving = consumed = false;
      }
      return ret;
    }

    public int TakeInSupply(int inSupply) {
      if (inSupply == 0) return 0;
      int needed = GetNeededSupply();
      if (needed <= 0)
      {
        return inSupply;
      }

      int minNeeded = MinSupplyNeeded();
      if (inSupply >= minNeeded) {
        minNeeded = inSupply >= needed ? needed : minNeeded;
        inSupply -= minNeeded;
        supply += minNeeded;
        return inSupply;
      }
      return inSupply;
    }

    public int GetNeededSupply()
    {
      int needed = unit.GetMaxSupplySlots() * SupplyNeededPerTurn() - supply;
      return needed < 0 ? 0 : needed;
    }

    public int[] Consume(bool alreadyDone = false)
    {
      int[] effects = new int[8]{0,0,0,0,0,0,0,0};
      if (alreadyDone) {
        consumed = true;
        return effects;
      }

      int needed = SupplyNeededPerTurn();
      int neededHalf = MinSupplyNeeded();
      if (supply < neededHalf) {
        supply = 0;
        int moraleReduce = -8;
        unit.rf.morale += moraleReduce;
        int miaNum = GetStarvingDessertNum();
        unit.mia += miaNum;
        unit.rf.soldiers -= miaNum;
        int deathNum = GetStarvingKillNum();
        unit.kia += deathNum;
        unit.rf.soldiers -= deathNum;
        int laborDead = (int)(deathNum / 5);
        unit.labor -= laborDead;
        effects[0] = moraleReduce;
        effects[3] = deathNum;
        effects[4] = laborDead;
        effects[5] = miaNum;
        return effects;
      } else {
        consumed = true;
      }

      if (supply < needed) {
        supply = 0;
      } else {
        supply -= needed;
      }
      return effects;
    }

    public int SupplyNeededPerTurn()
    {
      return (int)(((unit.rf.soldiers + unit.rf.wounded) / 10 ) * unit.hexMap.FoodPerTenMenPerTurn(unit.IsAI()));
    }

    public int MinSupplyNeeded() {
      return (int)(SupplyNeededPerTurn() / 2);
    }

    public int ReplenishSupply(int supply)
    {
      int remaining = TakeInSupply(supply);
      int needed = SupplyNeededPerTurn();
      int neededHalf = MinSupplyNeeded();
      int consumed = remaining >= needed ? needed : (remaining >= neededHalf ? neededHalf : remaining);
      if (consumed >= neededHalf) {
        remaining -= consumed;
        this.consumed = true;
      }
      return remaining < 0 ? 0 : remaining;
    }

    public void RefreshSupply() {
      isStarving = !consumed;
      consumed = false;
      // recalculate supply based on labor
      int canCarry = unit.GetMaxSupplySlots() * SupplyNeededPerTurn();
      supply = supply > canCarry ? canCarry : supply;
    }

  }
}