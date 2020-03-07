using System.Collections.Generic;
using PathFind;
using MapTileNS;
using MonoNS;
using UnityEngine;
using CourtNS;
using FieldNS;

namespace UnitNS
{
  public abstract class Unit : PFUnit, DataModel
  {
    public abstract bool IsCavalry();
    protected abstract int GetBaseSupplySlots();
    protected abstract Unit Clone();

    public const int ActionCost = 30; // For actions like: attack, bury
    public const int DefenceCost = 15;
    public const int MovementcostOnHill = 25;
    public const int MovementcostOnHillRoad = 20;
    public const int MovementcostOnPlain = 20;
    public const int MovementcostOnPlainRoad = 15;
    public const int MovementCostOnUnaccesible = -1;
    public const int DisbandUnitUnder = 50;

    public const int L1Visibility = 2;
    public const int L2Visibility = 4;
    public const int VantageVisibility = 8;
    public const int L1DiscoverRange = 1; // under 2000
    public const int L2DiscoverRange = 2; // > 4000
    public const int ConcealCoolDownIn = 3;
    public Type type {
      get {
        return rf.type;
      }
    }

    private TextNS.TextLib txtLib = Cons.GetTextLib();

    public HexMap hexMap;
    public bool clone = false;
    public ArmorRemEvent armorRemEvent;
    public ArmyEpidemic epidemic;
    public AltitudeSickness altitudeSickness;
    public UnitPoisioned unitPoisioned;
    public Riot riot;
    public MarchOnHeat marchOnHeat;
    public MarchOnExhaustion marchOnExhaustion;
    public FarmDestroy farmDestroy;
    public Supply supply;
    public WeatherEffect weatherEffect;
    public UnitConflict unitConflict;
    public PlainSickness plainSickness;
    public WarWeary warWeary;
    public Vantage vantage;
    WeatherGenerator weatherGenerator;
    TurnController turnController;
    int initSupply = 0;
    public bool chaos = false;
    public Unit(bool clone, Troop troop, Tile tile, State state,
                int supply, int labor, int kia, int mia, int movement = -1)
    {
      rf = troop;
      this.clone = clone;
      // must be after num is set
      TakeInLabor(labor);
      this.initSupply = supply;
      this.kia = kia;
      this.mia = mia;
      hexMap = GameObject.FindObjectOfType<HexMap>();
      if (clone) {
        this.tile = tile;
        this.state = state;
      } else {
        weatherGenerator = hexMap.weatherGenerator;
        turnController = hexMap.turnController;

        SetTile(tile);
        SetState(rf.morale == 0 ? State.Routing : state);
      }
      movementRemaining = movement;
    }

    public void Init() {
      epidemic = new ArmyEpidemic(this);
      altitudeSickness = new AltitudeSickness(this);
      armorRemEvent = new ArmorRemEvent(this);
      unitPoisioned = new UnitPoisioned(this);
      riot = new Riot(this);
      marchOnHeat = new MarchOnHeat(this);
      marchOnExhaustion = new MarchOnExhaustion(this);
      supply = new Supply(this, initSupply);
      weatherEffect = new WeatherEffect(this);
      unitConflict = new UnitConflict(this);
      plainSickness = new PlainSickness(this);
      warWeary = new WarWeary(this);
      vantage = new Vantage(this);
      farmDestroy = new FarmDestroy(this);
    }

    public void CloneInit(float disarmorDefDebuf, Supply supply, PlainSickness plainSickness, WarWeary warWeary) {
      this.disarmorDefDebuf = disarmorDefDebuf;
      this.supply = supply;
      this.plainSickness = plainSickness;
      this.warWeary = warWeary;
    }

    public void SpawnOnMap() {
      if (clone) {
        return;
      }
      hexMap.SpawnUnit(this);
      if (tile != null && tile.settlement != null && tile.settlement.owner.isAI == IsAI()) {
        // spawn unit in settlement
        // ghost unit will have null tile
        tile.settlement.Encamp(this);
      }
      movementRemaining = GetFullMovement();
    }

    State[] visibleStates = {State.Routing, State.Stand};
    public void SetState(State state) {
      UnitActionBroker broker = UnitActionBroker.GetBroker();
      if (state == State.Stand && Concealable() && concealCoolDownTurn == 0) {
        state = State.Conceal;
      }
      this.state = state;
      if (state == State.Retreated) {
        broker.BrokeChange(this, ActionType.UnitDestroyed, tile);
        return;
      }
      if (state == State.Camping) {
        broker.BrokeChange(this, ActionType.UnitLeft, tile);
        return;
      }
      if (state == State.Routing) {
        broker.BrokeChange(this, ActionType.UnitVisible, tile);
        return;
      }
      // TODO: AI test
      bool myTurn = !turnController.playerTurn == IsAI();
      FoW fow = FoW.Get();
      if (!myTurn && fow != null) {
        // is covered by fow
        if (fow.IsFogged(tile)) {
          broker.BrokeChange(this, ActionType.UnitHidden, tile);
          return;
        }
      }

      if (state == State.Conceal) {
        if (myTurn) {
          broker.BrokeChange(this, ActionType.UnitVisible, tile);
          return;
        }
        broker.BrokeChange(this, ActionType.UnitHidden, tile);
        return;
      }
      broker.BrokeChange(this, ActionType.UnitVisible, tile);
    }

    public int concealCoolDownTurn = 0;
    public int movementRemaining {
      get {
        if (__movementRemaining > GetFullMovement()) {
          __movementRemaining = GetFullMovement();
        }
        return __movementRemaining;
      }
      set {
        __movementRemaining = value < 0 ? 0 : (value > GetFullMovement() ? GetFullMovement() : value);
      }
    }
    public Reaction attackReaction = Reaction.Stand;
    public Tile tile;
    public int kia;
    public int mia;
    public int labor
    {
      get
      {
        return __labor;
      }
      set
      {
        __labor = value < 0 ? 0 : value;
      }
    }
    public Troop rf;
    public State state = State.Stand;
    public bool defeating = false;

    int __labor = 0;
    int __movementRemaining;
    Queue<Tile> path;
    // ==============================================================
    // ================= Unit Stat ==================================
    // ==============================================================
    public string Name()
    {
      return rf.name;
    }

    public string GeneralName()
    {
      return rf.general != null ? rf.general.Name() : " ";
    }

    public bool IsCommander() {
      return Util.eq<General>(hexMap.GetWarParty(this).commanderGeneral, rf.general);
    }

    public int CanBeShaked(Unit charger) {
      // TODO: apply general trait
      if(!IsCavalry()) {
        if (Util.eq<Rank>(rf.rank, Cons.rookie)) {
          return 100;
        } else {
          return 50;
        }
      } else {
        return 0;
      }
    }

    public bool CanCharge() {
      return rf.royalGuard && rf.soldiers >= 800 && movementRemaining >= ActionCost; 
    }

    public string GetStateName()
    {
      if (state == State.Conceal)
      {
        return txtLib.get("u_concealing");
      }
      if (state == State.Camping)
      {
        return txtLib.get("u_camping");
      }
      if (state == State.Routing)
      {
        return txtLib.get("u_routing");
      }
      if (state == State.Disbanded)
      {
        return txtLib.get("u_disbanded");
      }
      if (state == State.Retreated)
      {
        return txtLib.get("u_retreated");
      }
      return txtLib.get("u_standing");
    }

    public string GetStaminaLvlName()
    {
      StaminaLvl lvl = GetStaminaLevel();
      if (lvl == StaminaLvl.Exhausted)
      {
        return txtLib.get("u_exhausted");
      }
      if (lvl == StaminaLvl.Tired)
      {
        return txtLib.get("u_tired");
      }
      return txtLib.get("u_fresh");
    }

    public int GetHeatSickTurns()
    {
      return epidemic.GetIllTurns();
    }

    public int GetAltitudeSickTurns()
    {
      return altitudeSickness.lastTurns;
    }

    public int GetHeatSickEffectNum()
    {
      return epidemic.GetEffectNum();
    }

    public int GetPoisionTurns()
    {
      return unitPoisioned.GetIllTurns();
    }

    public int GetPoisionEffectNum()
    {
      return unitPoisioned.GetEffectNum();
    }

    public int GetStarvingDessertNum()
    {
      return supply.GetStarvingDessertNum();
    }

    public int GetStarvingKillNum()
    {
      return supply.GetStarvingKillNum();
    }

    public string GetDiscontent()
    {
      return riot.GetDescription();
    }

    public int GetTotalNum() {
      return rf.soldiers + rf.wounded + labor;
    }

    public bool IsAI()
    {
      return rf.faction.IsAI();
    }

    public bool IsHeatSicknessAffected() {
      return epidemic.IsValid();
    }

    public bool IsSick() {
      return ((epidemic != null && epidemic.IsValid())
        || (unitPoisioned != null && unitPoisioned.IsValid()));
    }

    public bool IsPoisioned() {
      return unitPoisioned.IsValid();
    }

    public bool IsHungry() {
      return IsStarving() || IsHalfStarving();
    }

    public bool IsStarving() {
      return (supply != null && supply.isStarving) || false;
    }

    public bool IsHalfStarving() {
      return supply != null && !supply.isStarving && supply.halfFeed;
    }

    public bool IsWarWeary() {
      return warWeary.IsWarWeary();
    }

    public bool IsConcealed() {
      return state == State.Conceal;
    }

    public bool IsOnField() {
      return state == State.Conceal || state == State.Stand;
    }

    public bool IsVisible() {
      return _IsVisible(this.state);
    }

    public bool IsHillLander() {
      return Util.eq<Region>(rf.province.region, Cons.hillLand);
    }

    bool _IsVisible(State state) {
      foreach (State s in visibleStates)
      {
        if (state == s) {
          return true;
        }
      }
      return false;
    }

    public bool IsCamping() {
      return state == State.Camping;
    }

    public bool IsGone() {
      return state == State.Disbanded || state == State.Retreated;
    }

    public bool IsShowingAnimation() {
      View view = hexMap.GetUnitView(this);
      // TODO: uncomment me
      // return !IsAI() || (IsAI() && view != null && view.viewActivated);
      return true;
    }

    public Tile[] GetScoutArea() {
      return tile.GetNeighboursWithinRange<Tile>(rf.soldiers > 5000 ? L2DiscoverRange : L1DiscoverRange,
                                                 (Tile _tile) => true);
    }

    public Tile[] GetVisibleArea() {
      // TODO: apply commander lvl and vantage point
      int v = L1Visibility;
      if (IsCommander()) {
        v = vantage.IsAtVantagePoint() ? VantageVisibility : L2Visibility;
      }
      return tile.GetNeighboursWithinRange<Tile>(v, (Tile _tile) => true);
    }

    protected virtual bool Concealable() {
      return false;
    }

    // ==============================================================
    // ================= Unit Settled&Refresh =======================
    // ==============================================================

    // When unit ends in settlement
    public int EndedInSettlement(int supply)
    {
      rf.morale = rf.morale > GetRetreatThreshold() ? rf.morale: GetRetreatThreshold();
      if (state == State.Routing) {
        SetState(State.Stand);
      }
      return this.supply.ReplenishSupply(supply);
    }

    // Before new turn starts
    public int[] RefreshUnit()
    {
      if (chaos) {
        chaos = false;
        //hexMap.GetUnitView(this).UpdateUnitInfo();
      }

      if (defeating) {
        defeating = false;
      }

      if (concealCoolDownTurn > 0) {
        concealCoolDownTurn--;
      } else {
        if (Concealable() && state == State.Stand) {
          if (!hexMap.GetRangeForDiscoveryWarning(this.IsAI()).Contains(tile)) SetState(State.Conceal);
        }
      }

      supply.RefreshSupply();
      turnDone = false;
      movementRemaining = GetFullMovement();
      int[] ret = weatherEffect.Apply();
      if (altitudeSickness.lastTurns > 0) {
        movementRemaining = 0;
      }
      return ret;
    }

    bool turnDone = false;
    public bool TurnDone()
    {
      return turnDone;
    }

    public void SetEndState(ActionController.actionName action)
    {
      turnDone = true;
    }

    public void Retreat() {
      // TODO: return labor
      labor = 0;
      if (rf.general != null) {
        rf.general.TroopRetreat();
      }
      SetState(State.Retreated);
      tile.RemoveUnit(this);
      if (tile.settlement != null && tile.settlement.owner.isAI == this.IsAI()) {
        tile.settlement.RemoveUnit(this);
      }
      hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.Retreat, this, null));
    }

    public int Destroy()
    {
      int killed = rf.soldiers + rf.wounded;
      hexMap.UpdateWound(this, -rf.wounded);
      kia += killed;
      if (hexMap.IsAttackSide(IsAI())) {
        hexMap.settlementMgr.attackerLaborDead += labor;
      } else {
        hexMap.settlementMgr.defenderLaborDead += labor;
      }

      rf.soldiers = rf.wounded = labor = 0;
      SetState(State.Disbanded);
      tile.RemoveUnit(this);
      if (tile.settlement != null && tile.settlement.owner.isAI == this.IsAI()) {
        tile.settlement.RemoveUnit(this);
      }
      return killed;
    }

    public bool waitingForOrders()
    {
      if ((path != null && path.Count != 0) && GetPureAccessibleTiles().Length > 1)
        // maybe we can set skip flags to skip the turn, e.g. defend/fortify etc.
        return false;
      return true;
    }

    public int[] TakeEffect(int reduceMorale, float movementDropRatio = 0f,
      float disableRatio = 0f, float killRatio = 0f) {
      // morale, movement, wounded, killed, laborKilled, disserter, attack, def, discontent
      int[] reduced = new int[]{0,0,0,0,0,0,0,0,0};
      rf.morale -= reduceMorale;
      reduced[0] = -reduceMorale;
      int moveReduce = (int)(movementRemaining * movementDropRatio);
      movementRemaining = movementRemaining - moveReduce; 
      reduced[1] = -moveReduce;
      int woundedNum = (int)(rf.soldiers * disableRatio);
      hexMap.UpdateWound(this, woundedNum);
      rf.wounded += woundedNum;
      rf.soldiers -= woundedNum;
      reduced[2] = woundedNum;
      int kiaNum = (int)(rf.soldiers * killRatio);
      kia += kiaNum;
      rf.soldiers -= kiaNum;
      reduced[3] = kiaNum;
      int killLabor = (int)(kiaNum * 0.8f);
      killLabor = killLabor > labor ? labor : killLabor;
      if(hexMap.IsAttackSide(IsAI())) {
        hexMap.settlementMgr.attackerLaborDead += killLabor;
      } else {
        hexMap.settlementMgr.defenderLaborDead += killLabor;
      }

      labor -= killLabor;
      reduced[4] = killLabor;
      reduced[5] = 0;
      reduced[6] = 0;
      reduced[7] = 0;
      return reduced;
    }

    // ==============================================================
    // ================= movement mangement =========================
    // ==============================================================

    public int GetFullMovement()
    {
      return (int)(
        // ghost unit doesnt have vantage
        (vantage != null ? vantage.MovementPoint(rf.mov) : rf.mov) *
        (IsHungry() ? 0.4f : 1) *
        (plainSickness != null && plainSickness.affected ? (1 - plainSickness.moveDebuf) : 1) *
        (IsSick() ? 0.5f : 1));
    }

    public StaminaLvl GetStaminaLevel()
    {
      if (movementRemaining >= 50)
      {
        return StaminaLvl.Fresh;
      }
      if (movementRemaining >= 30)
      {
        return StaminaLvl.Tired;
      }
      return StaminaLvl.Exhausted;
    }

    public virtual int LaborCanTakeIn() {
      return 0;
    }

    public int TakeInLabor(int labor) {
      int canTakeIn = LaborCanTakeIn(); 
      int gap = labor - canTakeIn;
      if (gap > 0) {
        this.labor += canTakeIn;
        return gap;
      }
      this.labor += labor;
      return 0;  
    }

    // ==============================================================
    // ================= slots mangement ===========================
    // ==============================================================

    public virtual Dictionary<int, int> GetLaborSuggestion() {
      return new Dictionary<int, int>();
    }

    public int slots
    {
      get
      {
        return supply.GetLastingTurns();
      }
    }

    public int GetMaxSupplySlots()
    {
      return rf.province.region.ExtraSupplySlot() + GetBaseSupplySlots() - (IsWarWeary() ? 1 : 0);
    }

    // ==============================================================
    // ================= morale mangement ===========================
    // ==============================================================
    public int GetRetreatThreshold()
    {
      return Rank.MoralePunishLine - 10;
    }

    // ==============================================================
    // ================= buff & debuff ==============================
    // ==============================================================
    public float disarmorDefDebuf = 0;
    public int cp
    {
      get
      {
        return rf.combatPoint;
      }
    }

    public int unitCombatPoint {
      get {
        int total = vantage.TotalPoints(cp);
        total = (int)((total + total * GetBuff()) * 0.1f);
        return total < 0 ? 0 : total;
      }
    }

    public int GetUnitAttackCombatPoint() {
      return (int)(unitCombatPoint * (1 + GetStaminaDebuf(false)));
    }

    public int GetUnitDefendCombatPoint(bool asMainDefender) {
      return (int)((IsCavalry() ? unitCombatPoint : (int)(unitCombatPoint * CombatController.DefendModifier))
                   * (1 + GetStaminaDebuf(asMainDefender)));
    }

    public int unitPureCombatPoint {
      get {
        int total = vantage.TotalPoints(cp);
        total = (int)((total + total * rf.lvlBuf) * 0.1f);
        return total < 0 ? 0 : total;
      }
    }

    public int unitCampingAttackCombatPoint {
      get {
        int total = vantage.TotalPoints(cp);
        total = (int)((total + total * GetCampingAttackBuff()) * 0.1f * (1 + GetStaminaDebuf(false)));
        return total < 0 ? 0 : total;
      }
    }

    public int unitPureDefendCombatPoint {
      get {
        return IsCavalry() ? unitPureCombatPoint : (int)(unitPureCombatPoint * CombatController.DefendModifier);
      }
    }

    public bool CanBePoisioned() {
      return unitPoisioned.Poision();
    }

    public float GetCampingAttackBuff()
    {
      return GetChaosBuf() + GetWarwearyBuf() - plainSickness.debuf + rf.lvlBuf - disarmorDefDebuf;
    }

    public float GetBuff()
    {
      return GetCampingAttackBuff() + vantage.Buf();
    }

    public float GetStaminaDebuf(bool asMainDefender) {
      if (GetStaminaLevel() == StaminaLvl.Tired) {
        return -0.3f;
      } else if (GetStaminaLevel() == StaminaLvl.Exhausted) {
        return asMainDefender ? -0.5f : -1f;
      } else {
         return 0f;
      }
    }

    public float GetWarwearyBuf()
    {
      return warWeary.GetBuf();
    }

    public float GetChaosBuf() {
      return chaos ? -0.99f : (defeating ? -0.3f : 0f);
    }

    // ==============================================================
    // ================= preflight ==================================
    // ==============================================================
    public Unit Preflight(Tile target, Tile[] path = null)
    {
      // NOTE: must NOT destroy clone unit, because there are objects borrowed from host
      // No preflight for camping unit
      if (state == State.Camping) return null;
      // TODO: target tile should be within unit's accessible range
      if (!target.DeployableForPathFind(this)) return null;
      Unit clone = Clone();
      int totalCost = GetPathCost(path);
      if (totalCost < 0 || totalCost > movementRemaining) {
        // out of reach, preflight fails
        return null;
      }
      clone.movementRemaining = movementRemaining - totalCost;
      clone.PreflightSetTile(target);
      return clone;
    }

    public void PreflightSetTile(Tile tile)
    {
      // TODO: add unit to virtual hex
      this.tile = tile;
    }

    public int GetPathCost(Tile[] path) {
      int totalCost = 0;
      foreach(Tile tile in path) {
        if (!Util.eq<Tile>(this.tile, tile)) {
          // get rid of the first tile
          totalCost = AggregateCostToEnterTile(tile, totalCost, PathFind.Mode.Normal);
        }
      }
      return totalCost;
    }

    // ==============================================================
    // ================= unit actions ===============================
    // ==============================================================
    //public void UpdateGeneralName() {
    //  UnitView view = hexMap.GetUnitView(this);
    //  if (view != null) {
    //    view.UpdateUnitInfo();
    //  }
    //}

    //public void UpdateUnitInfo() {
    //  UnitView view = hexMap.GetUnitView(this);
    //  if (view != null) {
    //    view.UpdateUnitInfo();
    //  }
    //}

    public void Encamp(Tile tile)
    {
      SetTile(tile, true);
      path = null;
      SetState(State.Camping);
    }

    public void Decamp(Tile tile)
    {
      SetTile(tile);
      path = null;
      if (rf.morale == 0) {
        SetState(State.Routing);
      } else {
        SetState(State.Stand);
      }
    }

    public void DiscoveredByEnemy() {
      concealCoolDownTurn = ConcealCoolDownIn;
      SetState(State.Stand);
    }

    bool AfterMoveUpdate(List<Unit> knownUnit, bool allyOnTile) {
      bool continueMoving = true;

      if (IsConcealed() && hexMap.GetRangeForDiscoveryCheck(this.IsAI()).Contains(tile)) {
        DiscoveredByEnemy();
        continueMoving = false;
      }

      // discover nearby enemies
      foreach(Tile t in GetScoutArea()) {
        Unit candidate = t.GetUnit();
        if (candidate != null && candidate.IsAI() != IsAI() && candidate.IsConcealed()) {
          candidate.DiscoveredByEnemy();
        }
      }

      if (stopAtNextMove) {
        stopAtNextMove = false;
        return false;
      }

      // Unit should stop once spots new enemy
      foreach(Tile t in GetVisibleArea()) {
        Unit u = t.GetUnit();
        if (u != null && u.IsAI() != IsAI() && u.IsVisible() && !knownUnit.Contains(u)) {
          continueMoving = false;
        }
      }

      if (!continueMoving && allyOnTile) {
        stopAtNextMove = true;
        continueMoving = true;
      }

      return continueMoving;
    }

    bool stopAtNextMove = false;
    public bool DoMove(Tile toTile = null)
    {
      Tile next = null;
      if (toTile != null) {
        path = null;
        next = toTile;
      } else if (path != null && path.Count > 0) {
        //next = path.Peek();
        next = path.Dequeue();
      } else {
        return false;
      }

/*
      if (movementRemaining <= 0) {
        return false;
      }

      int takenMovement = CostToEnterTile(next, PathFind.Mode.Normal);
      if (takenMovement < 0)
      {
        // path blocked, empty path and set idle
        path = null;
        return false;
      }
      if (movementRemaining < takenMovement)
      {
        return false;
      }
      if (toTile == null) {
        next = path.Dequeue();
      }
      if (!next.Deployable(this))
      {
        return false;
      }
*/
      int takenMovement = CostToEnterTile(next, PathFind.Mode.Normal);
      List<Unit> knownUnits = hexMap.combatController.GetKnownEnemies();
      movementRemaining -= takenMovement;
      bool allyOnTile = next.GetUnit() != null;
      SetTile(next);
      return AfterMoveUpdate(knownUnits, allyOnTile);
    }

    public void SetWargameTile(Tile h) {
      if (tile != null)
      {
        tile.RemoveUnit(this);
      }
      h.AddUnit(this);
      // TODO: remove for AI
      //if (!IsAI()) {
        hexMap.OnWargameMove(this, h);
      //}
      tile = h;
    }

    public void SetTile(Tile h, bool dontAddToNewTile = false)
    {
      if (tile != null)
      {
        tile.RemoveUnit(this);
      }
      if (!dontAddToNewTile && h != null)
      {
        // not encamp
        h.AddUnit(this);
      }
      tile = h;
    }

    // ==============================================================
    // ================= path finding ===============================
    // ==============================================================
    public HashSet<Tile> enemyControlledTiles;
    public Tile[] GetPureAccessibleTiles() {
      enemyControlledTiles = hexMap.settlementMgr.GetControlledTiles(!IsAI());
      return PFTile2Tile(PathFinder.FindAccessibleTiles(tile, this, movementRemaining));
    }

    public Tile[] GetAccessibleTiles()
    {
      if (!hexMap.deployDone) {
        return hexMap.InitPlayerDeploymentZone();
      }

      List<Tile> area = new List<Tile>();
      HashSet<Tile> visible = FieldNS.FoW.Get().GetVisibleArea();
      if (hexMap.wargameController.start) {
        visible = hexMap.wargameController.visibleArea;
      }

      foreach (Tile tile in GetPureAccessibleTiles()) {
        if (visible.Contains(tile) && tile.GetUnit() == null) {
          area.Add(tile);
        }
      }
      return area.ToArray();
    }

    public Tile[] GetAccessibleTilesForSupply(Tile start, int remaining)
    {
      enemyControlledTiles = null;
      return PFTile2Tile(PathFinder.FindAccessibleTiles(start, this, remaining, PathFind.Mode.Supply));
      /*
      HashSet<Tile> tiles = new HashSet<Tile>();
      foreach (Tile tile in PFTile2Tile(PathFinder.FindAccessibleTiles(start, this, remaining, true)))
      {
        foreach (Tile h in tile.neighbours)
        {
          if (h.terrian == TerrianType.Hill && tile.settlement == null)
          {
            tiles.Add(h);
          }
        }
        tiles.Add(tile);
      }
      return tiles.ToArray();
      */
    }

    // only for Ghost unit to pathfind settlement path
    public Tile[] FindPath(Tile source, Tile target)
    {
      enemyControlledTiles = null;
      return PFTile2Tile(PathFinder.FindPath(source, target, this, PathFind.Mode.Supply));
    }

    // movement pathfind
    public Tile[] FindPath(Tile target)
    {
      if (target.GetUnit() != null) {
        return new Tile[]{};
      }
      enemyControlledTiles = hexMap.settlementMgr.GetControlledTiles(!IsAI());
      return PFTile2Tile(PathFinder.FindPath(tile, target, this));
    }

    protected Tile[] PFTile2Tile(PFTile[] tiles)
    {
      List<Tile> ret = new List<Tile>();
      foreach (PFTile tile in tiles)
      {
        ret.Add((Tile)tile);
      }
      return ret.ToArray();
    }

    public void SetPath(Tile[] tiles)
    {
      path = new Queue<Tile>(tiles);
      path.Dequeue(); // get rid of the start tile
    }

    public Tile[] GetPath()
    {
      Tile[] p = new Tile[0];
      if (path != null) p = path.ToArray();
      return p;
    }

    public int MovementCostToEnterTile(Tile tile, PathFind.Mode mode)
    {
      return tile.GetCost(this, mode);
    }

    public int AggregateCostToEnterTile(Tile tile, int costToDate, PathFind.Mode mode)
    {
      int cost = CostToEnterTile(tile, mode);
      cost = cost < 0 ? cost : (cost + costToDate);
      return cost;
    }

    public int CostToEnterTile(Tile target, PathFind.Mode mode)
    {
      int cost = MovementCostToEnterTile(target, mode);
      // if tile takes 3 turns to enter, we can still enter it with 1 full turn
      cost = cost > GetFullMovement() ? GetFullMovement() : cost;
      return cost;
    }
  }
}