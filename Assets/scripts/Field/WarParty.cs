using System.Collections.Generic;
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
    public WarParty(bool attackside, Faction faction, General commander, int supply)
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

    HashSet<Tile> discoveredTiles = new HashSet<Tile>();
    public HashSet<Tile> GetVisibleArea() {
      HashSet<Tile> tiles = new HashSet<Tile>();
      foreach (Unit u in GetUnits())
      {
        foreach(Tile tile in u.GetVisibleArea()) {
          tiles.Add(tile);
        }
      }

      foreach (Tile tile in discoveredTiles) {
        tiles.Add(tile);
      }
      return tiles;
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

    public void DiscoverTile(Tile tile) {
      // TODO: update the tile color map
      discoveredTiles.Add(tile);
    }

    public void ResetDiscoveredTiles() {
      discoveredTiles = new HashSet<Tile>();
    }

    public List<Tile> MyRedZone(HashSet<Tile> enemyVisibleArea) {
      List<Tile> tiles = new List<Tile>();
      foreach(Unit u in GetUnits()) {
        if (u.IsHidden(enemyVisibleArea)) {
          // we dont want to show hidden unit's red zone
          continue;
        }
        Tile tile = u.tile;
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

    public Dictionary<Tile, int> discoveredTileColorMap; // 1: green, 2: yellow 3: red
    public void UpdateTileColorMap(List<Tile> redzones, List<Tile> controlledTiles) {
      discoveredTileColorMap = new Dictionary<Tile, int>();
      foreach(Tile tile in discoveredTiles) {
        if (redzones.Contains(tile)) {
          // red tiles
          discoveredTileColorMap[tile] = 3;
          continue;
        }

        if (controlledTiles.Contains(tile)) {
          // green tiles
          discoveredTileColorMap[tile] = 1;
          continue;
        }

        // yellow tiles
        discoveredTileColorMap[tile] = 2;
      }
    }

    public int GetTotalPoint() {
      int point = 0;
      foreach(Unit u in GetUnits()) {
        point += u.unitCombatPoint;
      }
      return point;
    }

    public Unit GetAmbusher(Unit target) {
      HashSet<Tile> area = target.hexMap.GetWarParty(target).GetVisibleArea();
      foreach(Unit unit in GetUnits()) {
        if (unit.CanSurpriseAttack(area) && unit.GetSurpriseTargets().Contains(target)) {
          return unit;
        }
      }
      return null;
    }

  }

}