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
  private int _civillian;
  private int _labor;
  public int civillian {
    get {
      return _civillian;
    }

    set {
      _civillian = value < 0 ? 0 : value;
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

  public void TurnEnd()
  {
    if (state == State.constructing) {
      buildWork -= HowMuchBuildWorkToFinish();
    }

    if (buildWork < 1)
    {
      buildWork = 0;
      state = State.normal;
      onSettlementReady(this);
      return;
    }
  }

  // Call Replenish before reduce
  public void ReplenishSupply(int supply)
  {
    supplyDeposit += supply;
  }

  public bool InRangeUnitConsumeSupply(Unit unit)
  {
    int neededPerTurn = unit.SupplyNeededPerTurn();
    if (neededPerTurn <= supplyDeposit)
    {
      supplyDeposit -= neededPerTurn;
      return true;
    }

    if (unit.MinSupplyNeeded() <= supplyDeposit)
    {
      supplyDeposit = 0;
      return true;
    }

    return false;
  }

  public bool IsNormal() {
    return state == State.normal;
  }

  public int MinSupplyNeeded(int labor = 0) {
    return (int)((civillian + (labor == 0 ? this.labor : labor)) / 10) * hexMap.FoodPerTenMenPerTurn(owner.isAI);
  }

  public int SupplyLastingTurns() {
    int remaining = supplyDeposit % MinSupplyNeeded();
    return (supplyDeposit - remaining) / MinSupplyNeeded();
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

  public void ReduceSupply()
  {
    if (state == State.constructing) {
      return;
    }

    int residents = civillian + labor; 
    if (supplyDeposit == 0)
    {
      if (residents == 0) return;
      if (residents > 500) defensePrep -= DefensePrepDeductRate;
      int death = (int)(civillian * Unit.Starving2DeathRate);
      deathToll += death;
      civillian -= death;
      death = (int)(labor * Unit.Starving2DeathRate);
      deathToll += death;
      labor -= death;
      return;
    }
    int sum = MinSupplyNeeded();
    if (supplyDeposit < sum)
    {
      if (residents > 500) defensePrep -= DefensePrepDeductRate / 2;
      supplyDeposit = 0;
      return;
    }

    supplyDeposit -= sum;
    // recovering defense preparation
    defensePrep += DefensePrepDeductRate / 2;

    if (supplyDeposit == 0)
    {
      return;
    }

    foreach (Unit unit in garrison)
    {
      if (supplyDeposit == 0) break;
      supplyDeposit = unit.ReplenishSupply(supplyDeposit);
    }

    Unit[] nearbyUnits = GetReachableUnits();
    foreach (Unit unit in nearbyUnits)
    {
      if (supplyDeposit == 0 || labor == 0) {
        break;
      }

      int neededPerTurn = unit.SupplyNeededPerTurn();
      int neededLabor = CalcNeededLabor(neededPerTurn);
      int supplyCanProvide = CalcSupplyCanProvide(labor);
      supplyCanProvide = supplyCanProvide > supplyDeposit ? supplyDeposit : supplyCanProvide;

      if (neededPerTurn > supplyDeposit || neededLabor > labor) {
        MsgBox.ShowMsg(unit.GeneralName() + " failed to retrieve supply from camp due to insufficient labor or supply");
        continue;
      }

      Unit ambusher = settlementMgr.IsSupplyRouteAmbushed(settlementMgr.GetRoute(this, unit.tile), unit.IsAI());
      if (ambusher != null) {
        // supply caravans ambushed
        SupplyIntercepted(ambusher, null, neededPerTurn, neededLabor, unit);
        continue;
      }

      unit.consumed = true;
      supplyDeposit -= neededPerTurn;
    }
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
    int maxLabor = (int)(supplyDeposit * 10 / hexMap.FoodPerTenMenPerTurn(owner.isAI));
    int canTake = maxLabor - labor;
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
      int remaining = supplyDeposit - MinSupplyNeeded(labor + laborNeeded);

      delta = remaining - supplyPerTurn;
      int supplyNeeded = delta > 0 ? 0 : -delta;  
      if (supplyNeeded != 0 || laborNeeded != 0) {
        suggestion.Add(new SupplySuggestion(scale, laborNeeded, supplyNeeded));
      }
    }

    return suggestion.ToArray();
  } 

  public Settlement[] GetReachableSettlements(bool link = false)
  {
    if (!IsFunctional()) return new Settlement[0];
    HashSet<Settlement> settlements = new HashSet<Settlement>();
    foreach(Tile tile in baseTile.linkedTilesForCamp) {
      if (tile.settlement != null && tile.settlement.owner.isAI == owner.isAI && tile.settlement.IsFunctional()) {
          Tile[] path = settlementMgr.GetRoute(this, tile);
          if (path.Length > 0) {
            settlements.Add(tile.settlement);
            if (link) {
              supply.RenderSupplyLine(path);
            }
          }
        }
      }
      Settlement[] ret = new Settlement[settlements.Count];
      settlements.CopyTo(ret);
      return ret;
  }

  public Unit[] GetReachableUnits(bool link = false) {
    if (!IsFunctional()) return new Unit[0];
    HashSet<Unit> all = owner.GetUnits();
    List<Unit> units = new List<Unit>();
    HashSet<Tile> tiles = settlementMgr.GetSupplyRangeTiles(this);
    foreach(Unit unit in all) {
      if (unit.IsCamping()) continue;
      if (tiles.Contains(unit.tile)) {
        units.Add(unit);
        if (link) {
          supply.RenderSupplyLine(settlementMgr.GetRoute(this, unit.tile));
        }
      }
    }

    // sort units from near to far
    units.Sort(delegate (Unit a, Unit b)
    {
      return (int)(Tile.Distance(baseTile, a.tile) - Tile.Distance(baseTile, b.tile));
    });

    return units.ToArray();
  }

  public virtual int SupplyCanTakeIn() {
    return int.MaxValue;
  }

  public virtual int MaxSupplyDeposit() {
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

  public City(string name, Tile tile, WarParty warParty, int supply, int civillian, int labor, Scale scale = Scale.Large) :
    base(name, tile, warParty, supply, GetSlots(scale))
  {
    this.labor = labor;
    this.civillian = civillian;
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
    this.civillian = 0;
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
    this.civillian = 0;
    this.labor = labor;
    type = Settlement.Type.camp;
    state = State.constructing;
  }

  public override int MaxSupplyDeposit() {
    int supplyTroop = MaxTroopSupply();
    int extraForLabor = (int)(CalcNeededLabor(supplyTroop) / 10) * SupportTurns * hexMap.FoodPerTenMenPerTurn(owner.isAI);
    return supplyTroop + extraForLabor;
  }

  public override int SupplyCanTakeIn() {
    return MaxSupplyDeposit() - supplyDeposit;
  }

  private int MaxTroopSupply() {
    return (int)((Infantry.MaxTroopNum * 5 * SupportTurns) / 10 ) * hexMap.FoodPerTenMenPerTurn(owner.isAI);
  }
}
