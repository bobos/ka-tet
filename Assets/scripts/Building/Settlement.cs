using System.Collections.Generic;
using UnityEngine;
using UnitNS;
using MapTileNS;
using MonoNS;
using FieldNS;

public abstract class Settlement: Building
{
  public const int Visibility = 3;
  public WallDefense wall;
  public StorageLevel storageLvl;
  public int parkSlots { get; private set; }
  public List<Unit> garrison = new List<Unit>();
  public int lastingTurns;
  private int _civillian_male;
  private int _civillian_female;
  private int _civillian_child;
  public int civillian_male {
    get {
      return _civillian_male;
    }

    set {
      _civillian_male = value < 0 ? 0 : value;
    }
  }
  public int civillian_female {
    get {
      return _civillian_female;
    }

    set {
      _civillian_female = value < 0 ? 0 : value;
    }
  }
  public int civillian_child {
    get {
      return _civillian_child;
    }

    set {
      _civillian_child = value < 0 ? 0 : value;
    }
  }

  public Type type;
  public string name = "default";
  public List<Tile> myTiles = new List<Tile>();
  BuildingNS.Supply supply;

  public Tile[] GetVisibleArea() {
    return baseTile.GetNeighboursWithinRange<Tile>(Visibility, (Tile _tile) => true);
  }

  public enum Type
  {
    camp,
    city,
    strategyBase
  }

  public int room = 0;

  public Settlement(string name, Tile location, WarParty warParty, int room,
    StorageLevel storage, WallDefense wall)
  {
    hexMap = GameObject.FindObjectOfType<HexMap>();
    settlementMgr = hexMap.settlementMgr;
    buildWork = location.Work2BuildSettlement();
    baseTile = location;
    location.settlement = this;
    owner = warParty;
    storageLvl = storage;
    this.wall = wall;
    parkSlots = this.room = room;
    this.name = name;
    this.supply = new BuildingNS.Supply(hexMap);
    lastingTurns = storageLvl.LastingTurnsUnderSiege();
  }

  public virtual bool CanBeAbandoned() {
    return false;
  }

  public bool CanProvideSupply() {
    return (!IsUnderSiege() && IsLinkedToRoot()) || lastingTurns > 0;
  }

  protected override void TurnEndCB() {
    if (IsUnderSiege() || !IsLinkedToRoot()) {
      lastingTurns--;
    } else {
      lastingTurns += 2;
    }
    lastingTurns = lastingTurns < 0 ? 0 :
      (lastingTurns > storageLvl.LastingTurnsUnderSiege() ? storageLvl.LastingTurnsUnderSiege() : lastingTurns);

    if (IsUnderSiege()) {
      bool found = false;
      foreach(Tile tile in baseTile.neighbours) {
        Unit u = tile.GetUnit();
        if (u != null && u.IsAI() != owner.isAI && !u.IsCavalry() && u.rf.general.Has(Cons.diminisher)) {
          found = true;
          break;
        }
      }
      wall.DepleteDefense(found ? 2 : 1);
    } else {
      wall.RepairDefense();
    }
  }

   public void RemoveUnit(Unit unit) {
    if (garrison != null && garrison.Contains(unit))
    {
      garrison.Remove(unit);
    }
  }

  public bool HasRoom() {
    return parkSlots > 0;
  }

  public bool IsEmpty() {
    return garrison.Count == 0;
  }

  public bool IsUnderSiege() {
    bool underSiege = true;
    foreach(Tile tile in baseTile.neighbours) {
      if (!tile.Accessible()) {
        continue;
      }

      if (tile.siegeWall == null || !tile.siegeWall.IsFunctional() || tile.siegeWall.owner.isAI == owner.isAI) {
        underSiege = false;
        break;
      }
    }

    return underSiege;
  }

  public bool Encamp(Unit unit)
  {
    if (IsUnderSiege()) {
      return false;
    }

    if (unit != null && owner.GetUnits().Contains(unit) && parkSlots > 0)
    {
      garrison.Add(unit);
      parkSlots--;
      unit.Encamp(baseTile);
      return true;
    }
    return false;
  }
  
  protected override int HowMuchBuildWorkToFinish() {
    int laborForce = 0;
    foreach(Unit unit in garrison) {
      laborForce += unit.rf.soldiers;
    }
    return (int)(laborForce / 500);
  }

  public bool Decamp(Unit unit, Tile targetTile)
  {
    if (garrison.Contains(unit))
    {
      garrison.Remove(unit);
      parkSlots++;
      unit.Decamp(targetTile);
      return true;
    }
    else
    {
      return false;
    }
  }

  public int GetDefendForce() {
    int force = 0;
    if (IsEmpty()) {
      return force;
    }

    foreach(Unit unit in garrison) {
      int point = (int)(unit.unitCombatPoint * (
        unit.rf.general.Has(Cons.mechanician) ? 1.5f : 1f
      ));
      force += point;
    }

    return force;
  }

  public void GetLinked(List<Settlement> visited) {
    visited.Add(this);
    foreach(Tile tile in baseTile.linkedTilesForCamp) {
      if (visited.Contains(tile.settlement)) {
        continue;
      }

      if (tile.settlement.owner.isAI != owner.isAI
       || tile.settlement.IsUnderSiege()) {
        if (owner.attackside) {
          visited.Add(tile.settlement);
        }
        continue;
      }

      tile.settlement.GetLinked(visited);
    }
  }

  public bool IsLinkedToRoot() {
    List<Settlement> linked = new List<Settlement>();
    settlementMgr.GetRoot(owner.isAI).GetLinked(linked);
    return linked.Contains(this);
  }
}
