﻿using System.Collections.Generic;
using UnitNS;
using CourtNS;
using MapTileNS;
using System.Linq;

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
    public WarParty(bool attackside, Faction faction, General commander,
      int supply, MonoNS.HexMap hexmap)
    {
      this.isAI = faction.IsAI();
      this.attackside = attackside;
      this.faction = faction;
      foreach (Party party in faction.GetParties())
      {
        fieldParties.Add(new FieldParty(party));   
      }
      this.supply = supply;
      commanderGeneral = commander;
      this.hexmap = hexmap;
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
    public WarParty counterParty = null;
    MonoNS.HexMap hexmap = null;

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

    public Unit AssignNewCommander() {
      int lvl = 0;
      General general = null;
      foreach(Unit unit in GetUnits()) {
        General gen = unit.rf.general;
        if (gen.commandSkill.commandSkill > lvl) {
          lvl = gen.commandSkill.commandSkill;
          general = gen;
        }
      }
      AssignCommander(general);
      return general.commandUnit.onFieldUnit;
    }

    public void AssignCommander(General general) {
      commanderGeneral = general;
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
        } else { 
          stat.numOfCavalryDead += totalDead;
          if (!unit.IsGone()) {
            stat.numOfCavalry += unit.rf.soldiers;
            stat.numOfCavalryUnit++;
          }
        }
      }

      return stat;
    }

    void UpdateUnitFreeSpace(HashSet<Unit> units, HashSet<Tile> tiles, Unit unit) {
      if (units.Contains(unit)) {
        return;
      }
      units.Add(unit);

      foreach(Tile tile in unit.tile.neighbours) {
        Unit u = tile.GetUnit();
        //if (tile.Deployable(unit) || tile.settlement != null && tile.settlement.owner.isAI == unit.IsAI()) {
        if (tile.Deployable(unit)) {
          tiles.Add(tile);
        } else if (u != null && u.IsAI() == unit.IsAI()) {
          UpdateUnitFreeSpace(units, tiles, u);
        }
      }
    }

    public Dictionary<HashSet<Unit>, HashSet<Tile>> GetFreeSpaces() {
      Dictionary<HashSet<Unit>, HashSet<Tile>> info = new Dictionary<HashSet<Unit>, HashSet<Tile>>();
      foreach(Unit unit in GetUnits()) {
        if (unit.IsCamping()) {
          continue;
        }

        bool found = false;
        foreach(var key in info.Keys) {
          if (key.Contains(unit)) {
            found = true;
            break;
          }
        }
        if (found) {
          continue;
        }

        HashSet<Unit> units = new HashSet<Unit>();
        HashSet<Tile> tiles = new HashSet<Tile>(); 
        UpdateUnitFreeSpace(units, tiles, unit);

        info[units] = tiles;
      }

      return info;
    }

    public HashSet<Tile> GetAiIndicationArea(HashSet<Tile> playerVision) {
      HashSet<Tile> tiles = new HashSet<Tile>();
      foreach (Unit u in GetUnits())
      {
        if (u.tile.field == FieldType.Forest && u.IsOnField() && !playerVision.Contains(u.tile)) {
          continue;
        }
        foreach(Tile tile in u.GetVisibleArea()) {
          tiles.Add(tile);
        }
      }

      return tiles;
    }

    public HashSet<Tile> discoveredTiles = new HashSet<Tile>();
    public void DiscoverTile(Tile tile) {
      discoveredTiles.Add(tile);
    }

    public void ResetDiscoveredTiles() {
      discoveredTiles = new HashSet<Tile>();
      foreach (Unit u in GetUnits())
      {
        foreach(Tile tile in u.GetVisibleArea()) {
          discoveredTiles.Add(tile);
        }
      }
    }

    public HashSet<Tile> MyRedZone() {
      HashSet<Tile> tiles = new HashSet<Tile>();
      foreach(Unit u in GetUnits()) {
        if (u.NoRedZone()) {
          continue;
        }
        Tile tile = u.tile;
        tiles.Add(tile);
        foreach (Tile t in
         (Sentinel.Aval(u) && Sentinel.Get(u.rf.general).Consume()
          ? tile.GetNeighboursWithinRange<Tile>(Sentinel.RedzoneRange, (Tile t) => true) :
          tile.neighbours)
         ) {
          if (t.Deployable(u) && !tiles.Contains(t)) {
            tiles.Add(t);
          }
        }
      }
      return tiles;
    }

    public List<Settlement> MySettlements() {
      List<Settlement> settlements = new List<Settlement>();
      foreach(Settlement s in hexmap.settlementMgr.allNodes) {
        if (Util.eq<WarParty>(s.owner, this)) {
          settlements.Add(s);
        }
      }
      return settlements;
    }

    public HashSet<Tile>[] cachedColorMap;
    public HashSet<Tile>[] GetTileColorMap() {
      HashSet<Tile>[] ret = new HashSet<Tile>[2]; 
      HashSet<Tile> greenTiles = new HashSet<Tile>();
      foreach(Settlement s in MySettlements()) {
        greenTiles.UnionWith(s.myTiles);
      }
      ret[0] = greenTiles;
      ret[1] = counterParty.MyRedZone(); // red
      return ret;
    }

    public int GetTotalPoint() {
      int point = 0;
      foreach(Unit u in GetUnits()) {
        point += u.unitCombatPoint;
      }
      return point;
    }

    public Unit GetAmbusher(Unit target) {
      foreach(Unit unit in GetUnits()) {
        List<Unit> us = unit.CanSurpriseAttack();
        if (us.Count > 0 && us.Contains(target)) {
          return unit;
        }
      }
      return null;
    }

  }

}