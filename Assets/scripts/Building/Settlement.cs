using System.Collections.Generic;
using UnityEngine;
using UnitNS;
using MapTileNS;
using MonoNS;
using FieldNS;

public abstract class Settlement: DataModel
{
  public const int Visibility = 3;

  public int supplyDeposit = 0;
  public WallDefense wall;
  public StorageLevel storageLvl;
  public int parkSlots { get; private set; }
  public List<Unit> garrison = new List<Unit>();
  public Tile baseTile;
  public WarParty owner;
  private int _civillian_male;
  private int _civillian_female;
  private int _civillian_child;
  private int _labor;
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
  public int labor {
    get {
      return _labor;
    }

    set {
      _labor = value < 0 ? 0 : value;
    }
  }
  public int availableLabor;

  public delegate void OnSettlementReady(Settlement settlement);
  public event OnSettlementReady onSettlementReady;

  public State state;
  public Type type;
  public string name = "default";

  protected int buildWork = 0;
  protected HexMap hexMap;
  SettlementMgr settlementMgr;
  BuildingNS.Supply supply;

  public int buildTurns {
    get {
      if (buildWork <= 0) return 0;
      int canBeDone = HowMuchBuildWorkToFinish();
      if (canBeDone == 0) return -1;
      if (canBeDone >= buildWork) return 1;
      int remaining  = buildWork % HowMuchBuildWorkToFinish();
      int turns  = (buildWork - remaining) / canBeDone;
      return turns + (remaining > 0 ? 1 : 0);
    }
  }

  public Tile[] GetVisibleArea() {
    return baseTile.GetNeighboursWithinRange<Tile>(Visibility, (Tile _tile) => true);
  }

  public class SupplySuggestion {
    public int supportTroop;
    public int laborNeeded;
    public int supplyNeeded;
    public SupplySuggestion(int supportTroop, int laborNeeded, int supplyNeeded) {
      this.supportTroop = supportTroop;
      this.laborNeeded = laborNeeded;
      this.supplyNeeded = supplyNeeded;
    }
  }

  public enum State
  {
    constructing,
    normal
  }

  public enum Type
  {
    camp,
    city,
    strategyBase
  }

  public int room = 0;

  public Settlement(string name, Tile location, WarParty warParty, int supply, int room,
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
    supplyDeposit = supply > SupplyCanTakeIn() ? SupplyCanTakeIn() : supply;
    parkSlots = this.room = room;
    this.name = name;
    this.supply = new BuildingNS.Supply(hexMap);
  }

  public List<Unit> Destroy()
  {
    settlementMgr.DestroySettlement(this);
    baseTile.settlement = null;
    // return the garrsion to be destroyed
    return garrison;
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

      if (!tile.sieged) {
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
  
  private int HowMuchBuildWorkToFinish() {
    int laborForce = labor;
    foreach(Unit unit in garrison) {
      laborForce += unit.labor;
    }
    return (int)(laborForce / 500);
  }

  public bool Decamp(Unit unit)
  {
    if (garrison.Contains(unit))
    {
      Tile targetTile = null;
      foreach (Tile tile in baseTile.neighbours)
      {
        if (tile.Deployable(unit))
        {
          targetTile = tile;
          break;
        } 
      }

      if (targetTile == null)
      {
        return false;
      }

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

  public bool TurnEnd()
  {
    if (state == State.constructing) {
      buildWork -= HowMuchBuildWorkToFinish();
    }

    if (buildWork < 1)
    {
      buildWork = 0;
      state = State.normal;
      onSettlementReady(this);
      return true;
    }
    return false;
  }

  public bool IsNormal() {
    return state == State.normal;
  }

  public int SupplyNeeded() {
    int supplyNeeded = 0;
    foreach (Unit unit in garrison) {
      supplyNeeded += unit.supply.SupplyNeededPerTurn();
    }
    return supplyNeeded;
  }

  public bool IsFunctional() {
    return state == State.normal && !IsUnderSiege();
  }

  public void DistSupply(int amount, Settlement to) {
    if (amount > supplyDeposit) {
      hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.InsufficientSupply, null, this, amount, supplyDeposit));
      return;
    }

    int neededLabor = CalcNeededLabor(amount);
    if (neededLabor > availableLabor) {
      hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.InsufficientSupplyLabor, null, this, amount,
        neededLabor, availableLabor));
      return;
    }

    availableLabor -= neededLabor;
    supplyDeposit -= amount;
    to.TakeInSupply(amount);
    hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.SupplyReached, null, to, amount));
  }

  public void DistLabor(int amount, Settlement to) {
    if (amount > availableLabor) {
      hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.InsufficientLabor, null, this, amount, availableLabor));
      return;
    }

    availableLabor -= amount;
    labor -= amount;
    to.TakeInLabor(amount);
    hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.LaborReached, null, to, amount));
  }

  // TODO: rework the supply system then remove this logic
  int[] InterceptResult(Unit enemy) {
    int[] result = new int[]{0, 0, 0};
    // TODO: general trait gaining
    if (enemy.rf.soldiers >= Util.Rand(Unit.DisbandUnitUnder, 100) &&
        enemy.rf.morale >= Util.Rand(45, 65)) {
      result[0] = 1;
      if (Cons.FairChance()) {
        result[1] = Util.Rand(1, 7);
        if (Cons.SlimChance()) {
          // killed
          result[2] = Util.Rand(1, 5);
        }
      }
      enemy.rf.morale += Cons.InterceptMoraleImpact;
    } else {
      if (Cons.HighlyLikely()) {
        result[1] = Util.Rand(5, 15);
        if (Cons.MostLikely()) {
          result[2] = Util.Rand(3, 10);
        }
      }
      enemy.rf.morale -= Cons.InterceptMoraleImpact * 2;
      enemy.DiscoveredByEnemy();
    }

    enemy.rf.soldiers -= (result[1] + result[2]);
    enemy.rf.wounded += result[1];
    hexMap.UpdateWound(enemy, result[1]);
    enemy.kia += result[2];
    return result;
  }

  public void SupplyIntercepted(Unit enemy, Settlement to, int amount) {
    int[] result = InterceptResult(enemy);
    if (result[0] == 0) {
      // Failed
      hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.InterceptFailed,
        enemy, null, Cons.InterceptMoraleImpact*2, result[1], result[2]));
      return;
    }
    int adjustedAmount = supplyDeposit > amount ? amount : supplyDeposit;
    int neededLabor = CalcNeededLabor(adjustedAmount);
    int adjustedLabor = availableLabor > neededLabor ? neededLabor : availableLabor; 
    int supplyCanSend = CalcSupplyCanProvide(adjustedLabor);

    adjustedAmount = adjustedAmount > supplyCanSend ? supplyCanSend : adjustedAmount; 
    adjustedLabor = CalcNeededLabor(adjustedAmount);
    availableLabor -= adjustedLabor;

    int killedLaborEscort = (int)(Util.Rand(0.008f, 0.01f) * adjustedLabor);
    labor -= killedLaborEscort;
    supplyDeposit -= adjustedAmount;
    int needed = enemy.supply.GetNeededSupply();
    int supplyTaken = needed > adjustedAmount ? adjustedAmount : needed;
    enemy.supply.TakeTransferSupply(supplyTaken);
    if (to.owner.attackside) {
      settlementMgr.attackerLaborDead += killedLaborEscort;
    } else {
      settlementMgr.defenderLaborDead += killedLaborEscort;
    }
    hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.SupplyIntercepted, enemy, to,
      adjustedAmount, killedLaborEscort, Cons.InterceptMoraleImpact, result[1], result[2]));
  }

  public void LaborIntercepted(Unit enemy, Settlement to, int amount) {
    int[] result = InterceptResult(enemy);
    if (result[0] == 0) {
      // Failed
      hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.InterceptFailed,
        enemy, null, Cons.InterceptMoraleImpact*2, result[1], result[2]));
      return;
    }
    int adjustedLabor = availableLabor > amount ? amount : availableLabor;
    availableLabor -= adjustedLabor;
    int killedLabor = (int)(Util.Rand(0.008f, 0.01f) * adjustedLabor);
    labor -= killedLabor;
    if (to.owner.attackside) {
      settlementMgr.attackerLaborDead += killedLabor;
    } else {
      settlementMgr.defenderLaborDead += killedLabor;
    }
    hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.LaborIntercepted, enemy, to,
      killedLabor, Cons.InterceptMoraleImpact, result[1], result[2]));
  }

  public void TakeInSupply(int supply) {
    supplyDeposit += supply;
    supplyDeposit = supplyDeposit > MaxSupplyDeposit() ? MaxSupplyDeposit() : supplyDeposit;
  }

  public void TakeInLabor(int labor) {
    this.labor += labor;
  }

  public List<List<Unit>> ReduceSupply()
  {
    List<List<Unit>> units = new List<List<Unit>>();

    units.Add(new List<Unit>());
    units.Add(new List<Unit>());
    Unit[] nearbyUnits = GetReachableUnits();

    if (supplyDeposit == 0)
    {
      units[0] = garrison;
      foreach(Unit u in nearbyUnits) {
        units[1].Add(u);
      }
      return units;
    }

    foreach (Unit unit in garrison)
    {
      if (supplyDeposit == 0) {
        units[0].Add(unit);
        continue;
      };
      supplyDeposit = unit.EndedInSettlement(supplyDeposit);
      if (!unit.supply.consumed) {
        units[0].Add(unit);
      }
    }

    if (IsUnderSiege()) {
      wall.DepleteDefense();
      foreach (Unit unit in nearbyUnits) {
        units[1].Add(unit);
      }
    } else {
      wall.RepairDefense();
      foreach (Unit unit in nearbyUnits)
      {
        if (supplyDeposit == 0) {
          units[1].Add(unit);
          continue;
        }

        int neededPerTurn = unit.supply.SupplyNeededPerTurn();
        int neededLabor = CalcNeededLabor(neededPerTurn);
        if (!unit.IsCavalry() && unit.labor < neededLabor) {
          units[1].Add(unit);
          continue;
        }

        if (neededPerTurn > supplyDeposit) {
          units[1].Add(unit);
          continue;
        }

        unit.supply.Consume(true);
        supplyDeposit -= neededPerTurn;
      }
    }
    return units;
  }

  public int CalcNeededLabor(int supply) {
    return (int)(supply / 10);
  }

  public int CalcSupplyCanProvide(int labor) {
    return labor * 10;
  }

  public int MaxDistSupply() {
    int canProvide = CalcSupplyCanProvide(labor);
    return supplyDeposit > canProvide ? canProvide : supplyDeposit;
  }

  public SupplySuggestion[] GetSuggestion() {
    int[] troopScale = new int[3];
    troopScale[0] = 12000;
    troopScale[1] = 24000;
    troopScale[2] = 48000;
    List<SupplySuggestion> suggestion = new List<SupplySuggestion>();

    foreach (int scale in troopScale) {
      int supplyPerTurn = (scale / 10) * hexMap.FoodPerTenMenPerTurn(owner.isAI);  
      int laborPerTurn = CalcNeededLabor(supplyPerTurn);
      int delta = labor - laborPerTurn;
      int laborNeeded = delta > 0 ? 0 : -delta;

      delta = supplyDeposit - supplyPerTurn;
      int supplyNeeded = delta > 0 ? 0 : -delta;  
      if (supplyNeeded != 0 || laborNeeded != 0) {
        suggestion.Add(new SupplySuggestion(scale, laborNeeded, supplyNeeded));
      }
    }

    return suggestion.ToArray();
  } 

  public Settlement[] GetReachableSettlements()
  {
    if (!IsFunctional()) return new Settlement[0];
    HashSet<Settlement> settlements = new HashSet<Settlement>();
    bool isAI = owner.isAI;
    foreach(Tile tile in baseTile.linkedTilesForCamp) {
      if (tile.settlement != null && tile.settlement.owner.isAI == owner.isAI
        && tile.settlement.IsFunctional() && baseTile.roads.ContainsKey(tile)) {
        Tile[] path = baseTile.roads[tile];
        bool blocked = false;
        foreach(Tile t in path) {
          if (!t.Passable(isAI)) {
            blocked = true;
            break;
           }
        }
        if (!blocked) {
          settlements.Add(tile.settlement);
        }
      }
    }
    Settlement[] ret = new Settlement[settlements.Count];
    settlements.CopyTo(ret);
    return ret;
  }

  public Unit[] GetReachableUnits() {
    if (!IsFunctional()) return new Unit[0];
    HashSet<Unit> all = owner.GetUnits();
    List<Unit> units = new List<Unit>();
    HashSet<Tile> tiles = settlementMgr.GetSupplyRangeTiles(this);
    foreach(Unit unit in all) {
      if (unit.IsCamping()) continue;
      if (tiles.Contains(unit.tile)) {
        units.Add(unit);
      }
    }

    // sort units from near to far
    units.Sort(delegate (Unit a, Unit b)
    {
      return (int)(Tile.Distance(baseTile, a.tile) - Tile.Distance(baseTile, b.tile));
    });

    return units.ToArray();
  }

  public int SupplyCanTakeIn() {
    return MaxSupplyDeposit() - supplyDeposit;
  }

  public int MaxSupplyDeposit() {
    return storageLvl.MaxStorage();
  }

}
