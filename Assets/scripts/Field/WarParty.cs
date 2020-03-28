﻿using System.Collections.Generic;
using UnitNS;
using CourtNS;
using MapTileNS;

namespace FieldNS
{
  public class WarPartyStat
  {
    public int numOfInfantryUnit;
    public int numOfCavalryUnit;
    public int numOfInfantry;
    public int numOfCavalry;
    public int numOfInfantryDead;
    public int numOfCavalryDead;
  }

  public class WarParty
  {
    public WarParty(bool attackside, Faction faction, int supply)
    {
      this.isAI = faction.IsAI();
      this.attackside = attackside;
      this.faction = faction;
      foreach (Party party in faction.GetParties())
      {
        fieldParties.Add(new FieldParty(party));   
      }
      this.supply = supply;
    }

    public bool ConsumeSupply(int amount) {
      if (supply < amount) {
        return false;
      }
      supply -= amount;
      return true;
    }

    public List<FieldParty> fieldParties = new List<FieldParty>();
    public Faction faction;
    public General commanderGeneral;
    public Unit firstRemoveArmor;
    public int supply;

    public bool isAI { get; private set; }
    public bool attackside { get; private set; }
    HashSet<Unit> units = new HashSet<Unit>();
    public int capturedHorse = 0;

    // TODO: phase ends, add horses to faction and reset the horse to 0
    public void CaptureHorse(int num) {
      capturedHorse += num;
    }

    public void JoinCampaign(General general) {
      Unit unit = general.commandUnit.onFieldUnit;
      units.Add(unit);
      unit.SpawnOnMap();
      Join(general);
    }

    public void Join(General general) {
      foreach (FieldParty fieldParty in fieldParties)
      {
        if (Util.eq<Party>(general.party, fieldParty.party)) {
          FieldParty counterParty = fieldParty.counterFieldParty;
          if (counterParty != null) counterParty.TheirGeneralEnterCampaign(general);
        }
      }
    }

    public HashSet<Unit> GetUnits()
    {
      HashSet<Unit> _units = new HashSet<Unit>();
      foreach (Unit unit in units)
      {
        if (!unit.IsGone()) _units.Add(unit);
      }
      return _units;
    }

    public WarPartyStat GetStat()
    {
      // TODO: for AI, consider fog of war
      WarPartyStat stat = new WarPartyStat();

      foreach (Unit unit in units)
      {
        int totalDead = unit.kia;
        if (unit.type == Type.Infantry) {
          stat.numOfInfantryDead += totalDead;
          if (!unit.IsGone()) {
            stat.numOfInfantry += unit.rf.soldiers;
            stat.numOfInfantryUnit++;
          }
        }

        if (unit.type == Type.Cavalry) {
          stat.numOfCavalryDead += totalDead;
          if (!unit.IsGone()) {
            stat.numOfCavalry += unit.rf.soldiers;
            stat.numOfCavalryUnit++;
          }
        }
      }

      return stat;
    }

    public void GetVisibleArea(HashSet<Tile> tiles) {
      foreach (Unit u in GetUnits())
      {
        foreach(Tile tile in u.GetVisibleArea()) {
          tiles.Add(tile);
        }
      }
    }

  }

}