using System.Collections.Generic;
using UnityEngine;
using UnitNS;
using MapTileNS;
using MonoNS;
using FieldNS;
using BuildingNS;

public abstract class Settlement
{
  public const int MaxGarrisonPerCamp = 4;
  public const int MaxGarrisonPerBase = 20;
  public const int DefensePrepMax = 100;
  public const int DefensePrepDeductRate = 10;
  public const int Visibility = 3;

  public int supplyDeposit = 0;
  public int parkSlots { get; private set; }
  public List<Unit> garrison = new List<Unit>();
  public Tile baseTile;
  public WarParty owner;
  public int deathToll = 0;
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
  public int wall = 0; // TODO: 50, 100, 150
  private int _defensePrep = DefensePrepMax;
  public int defensePrep {
    get {
      return _defensePrep;
    }

    set {
      _defensePrep = value < 0 ? 0 : (value > DefensePrepMax ? DefensePrepMax : value);
    }
  }

  public State state;
  public Type type;
  public string name = "default";

  protected int buildWork = 0;
  protected HexMap hexMap;
  SettlementMgr settlementMgr;
  Supply supply;

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

  public Settlement(string name, Tile location, WarParty warParty, int supply, int room)
  {
    hexMap = GameObject.FindObjectOfType<HexMap>();
    settlementMgr = hexMap.settlementMgr;
    buildWork = location.Work2BuildSettlement();
    baseTile = location;
    location.settlement = this;
    owner = warParty;
    supplyDeposit = supply > SupplyCanTakeIn() ? SupplyCanTakeIn() : supply;
    parkSlots = room;
    this.name = name;
    this.supply = new Supply(hexMap);
  }

  public void Destroy(BuildingNS.DestroyType type)
  {
    UnitNS.DestroyType desType = UnitNS.DestroyType.ByBurningCamp;
    if (type == BuildingNS.DestroyType.ByFire) {
      hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.WildFireDestroyCamp, null, this));
    }
    if (type == BuildingNS.DestroyType.ByFlood) {
      desType = UnitNS.DestroyType.ByFlood;
      hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.FloodDestroyCamp, null, this));
    }
    foreach (Unit unit in garrison)
    {
      unit.Destroy(desType);
    }
  }

  public void Capture() {
    if (type == Settlement.Type.camp) {
      hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.EnemyCaptureCamp, null, this));
    }
    if (type == Settlement.Type.city) {
      hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.EnemyCaptureCity, null, this));
    }
  }

  public void RemoveUnit(Unit unit) {
    if (garrison != null && garrison.Contains(unit))
    {
      garrison.Remove(unit);
    }
  }

  public bool Encamp(Unit unit)
  {
    if (unit != null && owner.GetUnits().Contains(unit) && parkSlots > 0
        && (state == State.normal || state == State.constructing))
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

  public int MinSupplyNeeded() {
    int supplyNeeded = 0;
    foreach (Unit unit in garrison) {
      supplyNeeded += unit.MinSupplyNeeded();
    }
    return supplyNeeded;
  }

  public bool IsFunctional() {
    return state == State.normal;
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

  public void SupplyIntercepted(Unit enemy, Settlement to, int amount, int labors = 0, Unit unit = null) {
    int adjustedAmount = amount;
    int adjustedLabor = labors;
    if (adjustedLabor == 0) {
      adjustedAmount = supplyDeposit > amount ? amount : supplyDeposit;
      int neededLabor = CalcNeededLabor(adjustedAmount);
      adjustedLabor = availableLabor > neededLabor ? neededLabor : availableLabor; 
      int supplyCanSend = CalcSupplyCanProvide(adjustedLabor);

      adjustedAmount = adjustedAmount > supplyCanSend ? supplyCanSend : adjustedAmount; 
      adjustedLabor = CalcNeededLabor(adjustedAmount);
      availableLabor -= adjustedLabor;
    }

    int killedLaborEscort = (int)(Util.Rand(0.008f, 0.04f) * adjustedLabor);
    labor -= killedLaborEscort;
    supplyDeposit -= adjustedAmount;
    int needed = enemy.GetNeededSupply();
    int supplyTaken = needed > adjustedAmount ? adjustedAmount : needed;
    enemy.TakeInSupply(supplyTaken);
    if (to != null) {
      hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.SupplyIntercepted, null, to, adjustedAmount, killedLaborEscort));
    }
    if (unit != null) {
      hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.unitSupplyIntercepted, unit, null, adjustedAmount, killedLaborEscort));
    }
  }

  public void LaborIntercepted(Unit enemy, Settlement to, int amount) {
    int adjustedLabor = availableLabor > amount ? amount : availableLabor;
    availableLabor -= adjustedLabor;
    int killedLabor = (int)(Util.Rand(0.008f, 0.04f) * adjustedLabor);
    labor -= killedLabor;
    hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.LaborIntercepted, null, to, killedLabor));
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
      defensePrep -= DefensePrepDeductRate;
      units[0] = garrison;
      foreach(Unit u in nearbyUnits) {
        units[1].Add(u);
      }
      return units;
    }

    // recovering defense preparation
    defensePrep += DefensePrepDeductRate / 2;

    foreach (Unit unit in garrison)
    {
      if (supplyDeposit == 0) {
        units[0].Add(unit);
        continue;
      };
      supplyDeposit = unit.ReplenishSupply(supplyDeposit);
      if (!unit.consumed) {
        units[0].Add(unit);
      }
    }

    foreach (Unit unit in nearbyUnits)
    {
      if (supplyDeposit == 0) {
        units[1].Add(unit);
        continue;
      }

      int neededPerTurn = unit.SupplyNeededPerTurn();
      int neededLabor = CalcNeededLabor(neededPerTurn);
      if (!unit.IsCavalry() && unit.labor < neededLabor) {
        units[1].Add(unit);
        continue;
      }

      if (neededPerTurn > supplyDeposit) {
        units[1].Add(unit);
        continue;
      }

      /*
      disable nearby supply route interception

      Unit ambusher = settlementMgr.IsSupplyRouteAmbushed(settlementMgr.GetRoute(this, unit.tile), unit.IsAI());
      if (ambusher != null) {
        // supply caravans ambushed
        SupplyIntercepted(ambusher, null, neededPerTurn, neededLabor, unit);
        continue;
      }
      */

      unit.consumed = true;
      supplyDeposit -= neededPerTurn;
    }
    return units;
  }

  public int CalcNeededLabor(int supply) {
    return supply;
  }

  public int CalcSupplyCanProvide(int labor) {
    return labor;
  }

  public int MaxDistSupply() {
    int canProvide = CalcSupplyCanProvide(labor);
    return supplyDeposit > canProvide ? canProvide : supplyDeposit;
  }

  public int LaborCanTakeInForOneTurn() {
    int canTake = MaxPopulation() - labor;
    return canTake > 0 ? canTake : 0;
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

  public virtual int MaxSupplyDeposit() {
    return int.MaxValue;
  }

  // TODO: set population for different level of settlement
  public virtual int MaxPopulation() {
    return int.MaxValue;
  }
}

public class City : Settlement
{

  public enum Scale {
    Small,
    Large,
    Huge
  }

  // TODO: pass male, female, child
  public City(string name, Tile tile, WarParty warParty, int supply, int civillian, int labor, Scale scale = Scale.Large) :
    base(name, tile, warParty, supply, GetSlots(scale))
  {
    this.labor = labor;
    this.civillian_male = civillian;
    this.civillian_female = civillian;
    this.civillian_child = civillian;
    type = Settlement.Type.city;
    state = State.normal;
    buildWork = 0;
  }

  static int GetSlots(Scale scale) {
    return MaxGarrisonPerBase;
  }

}

public class StrategyBase : Settlement
{

  public StrategyBase(string name, Tile tile, WarParty warParty, int supply, int labor) :
  base(name, tile, warParty, supply, MaxGarrisonPerBase)
  {
    this.civillian_male = civillian_female = civillian_child = 0;
    this.labor = labor;
    type = Settlement.Type.strategyBase;
    state = State.normal;
    buildWork = 0;
  }
}

public class Camp : Settlement
{

  const int SupportTurns = 3;
  public Camp(string name, Tile tile, WarParty warParty, int supply, int labor) :
  base(name, tile, warParty, supply, MaxGarrisonPerCamp)
  {
    this.civillian_male = civillian_female = civillian_child = 0;
    this.labor = labor;
    type = Settlement.Type.camp;
    state = State.constructing;
  }

  public override int MaxSupplyDeposit() {
    return (int)((Infantry.MaxTroopNum * 5 * SupportTurns) / 10 ) * hexMap.FoodPerTenMenPerTurn(owner.isAI);
  }

}
