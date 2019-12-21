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
    protected abstract float GetMovementModifier();
    protected abstract int GetBaseSupplySlots();
    protected abstract Unit Clone();

    public const int BasicMovementCost = 30; // For actions like: attack
    public const int MovementcostOnHill = 50;
    public const int MovementcostOnHillRoad = 30;
    public const int MovementcostOnPlain = 30;
    public const int MovementcostOnPlainRoad = 25;
    public const int MovementCostOnUnaccesible = -1;
    public const int DisbandUnitUnder = 20;

    public const int L1Visibility = 5; // under 4000 
    public const int L2Visibility = 8; // > 4000
    public const int L1DiscoverRange = 1; // under 2000
    public const int L2DiscoverRange = 2; // > 4000
    public const int ConcealCoolDownIn = 3;
    public Type type;

    private TextNS.TextLib txtLib = Cons.GetTextLib();

    public HexMap hexMap;
    public bool clone = false;
    public ArmorRemEvent armorRemEvent;
    public ArmyEpidemic epidemic;
    public UnitPoisioned unitPoisioned;
    public Riot riot;
    public MarchOnHeat marchOnHeat;
    public FarmDestroy farmDestroy;
    public Supply supply;
    public WeatherEffect weatherEffect;
    public WarWeary warWeary;
    public Vantage vantage;
    WeatherGenerator weatherGenerator;
    TurnController turnController;
    int initSupply = 0;
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
      movementRemaining = movement < 0 ? GetFullMovement() : movement;
    }

    public void SpawnOnMap() {
      if (clone) {
        return;
      }
      epidemic = new ArmyEpidemic(this);
      armorRemEvent = new ArmorRemEvent(this);
      unitPoisioned = new UnitPoisioned(this);
      riot = new Riot(this);
      marchOnHeat = new MarchOnHeat(this);
      supply = new Supply(this, initSupply);
      weatherEffect = new WeatherEffect(this);
      warWeary = new WarWeary(this);
      vantage = new Vantage(this);
      farmDestroy = new FarmDestroy(this);
      hexMap.SpawnUnit(this);
      if (tile != null && tile.settlement != null && tile.settlement.owner.isAI == IsAI()) {
        // spawn unit in settlement
        // ghost unit will have null tile
        tile.settlement.Encamp(this);
      }
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

    public int honorMeter = 0; // -2, -1, 0, 1, 2
    public int concealCoolDownTurn = 0;
    public int movementRemaining {
      get {
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
    public int kills;
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

    int __labor;
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
      if (lvl == StaminaLvl.Fresh)
      {
        return txtLib.get("u_fresh");
      }
      return txtLib.get("u_vigorous");
    }

    public int GetHeatSickTurns()
    {
      return epidemic.GetIllTurns();
    }

    public int GetHeatSickDisableNum()
    {
      return epidemic.GetIllDisableNum();
    }

    public int GetHeatSickKillNum()
    {
      return epidemic.GetIllKillNum();
    }

    public int GetPoisionTurns()
    {
      return unitPoisioned.GetIllTurns();
    }

    public int GetPoisionDisableNum()
    {
      return unitPoisioned.GetIllDisableNum();
    }

    public int GetPoisionKillNum()
    {
      return unitPoisioned.GetIllKillNum();
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

    public bool IsAI()
    {
      return rf.faction.IsAI();
    }

    public bool IsDefeated() {
      return honorMeter < 0;
    }

    public bool IsVictory() {
      return honorMeter > 0;
    }

    public bool IsSicknessAffected() {
      return epidemic.IsValid();
    }

    public bool IsPoisioned() {
      return unitPoisioned.IsValid();
    }

    public bool IsStarving() {
      return (supply != null && supply.isStarving) || false;
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

    public bool IsAmbushing() {
      return tile.Ambushable() && IsConcealed();
    }

    public bool IsVisible() {
      return _IsVisible(this.state);
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

    public WarPoint GetWarPoint()
    {
      // TODO: add more
      return new WarPoint();
    }

    public bool IsShowingAnimation() {
      View view = hexMap.GetUnitView(this);
      // TODO: uncomment me
      // return !IsAI() || (IsAI() && view != null && view.viewActivated);
      return true;
    }

    public Tile[] GetScoutArea() {
      return tile.GetNeighboursWithinRange<Tile>(rf.soldiers > 2000 ? L2DiscoverRange : L1DiscoverRange,
                                                 (Tile _tile) => true);
    }

    public Tile[] GetVisibleArea() {
      return tile.GetNeighboursWithinRange<Tile>(rf.soldiers > 4000 ? L2Visibility : L1Visibility,
                                                 (Tile _tile) => true);
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
      rf.morale = IsWarWeary() ? (GetRetreatThreshold() + 1) : rf.morale;
      if (state == State.Routing) {
        SetState(State.Stand);
      }
      return this.supply.ReplenishSupply(supply);
    }

    // Before new turn starts
    public int[] RefreshUnit()
    {
      if (concealCoolDownTurn > 0) {
        concealCoolDownTurn--;
      } else {
        if (Concealable() && state == State.Stand) {
          if (!hexMap.IsInEnemyScoutRange(this.IsAI(), tile)) SetState(State.Conceal);
        }
      }

      movementRemaining = GetFullMovement();
      supply.RefreshSupply();
      turnDone = false;
      return weatherEffect.Apply();
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

    public void DestroyEvents() {
      epidemic.Destroy();
      armorRemEvent.Destroy();
      unitPoisioned.Destroy();
    }

    public void Retreat() {
      if (rf.general != null) {
        rf.general.TroopRetreat();
      }
      SetState(State.Retreated);
      tile.RemoveUnit(this);
      if (tile.settlement != null && tile.settlement.owner.isAI == this.IsAI()) {
        tile.settlement.RemoveUnit(this);
      }
      DestroyEvents();
      hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.Retreat, this, null));
    }

    public int Destroy()
    {
      int killed = rf.soldiers + rf.wounded;
      kia += killed;
      rf.soldiers = rf.wounded = labor = 0;
      SetState(State.Disbanded);
      tile.RemoveUnit(this);
      if (tile.settlement != null && tile.settlement.owner.isAI == this.IsAI()) {
        tile.settlement.RemoveUnit(this);
      }
      DestroyEvents();
      return killed;
    }

    public bool waitingForOrders()
    {
      if ((path != null && path.Count != 0) && GetAccessibleTiles().Length > 1)
        // maybe we can set skip flags to skip the turn, e.g. defend/fortify etc.
        return false;
      return true;
    }

    public int[] TakeEffect(int reduceMorale, float movementDropRatio = 0f,
      float disableRatio = 0f, float killRatio = 0f) {
      int[] reduced = new int[8]; // morale, movement, wounded, killed, laborKilled, disserter, attack, def
      rf.morale -= reduceMorale;
      reduced[0] = -reduceMorale;
      int moveReduce = (int)(movementRemaining * movementDropRatio);
      movementRemaining = movementRemaining - moveReduce; 
      reduced[1] = -moveReduce;
      int woundedNum = (int)(rf.soldiers * disableRatio);
      rf.wounded += woundedNum;
      rf.soldiers -= woundedNum;
      reduced[2] = woundedNum;
      int kiaNum = (int)(rf.soldiers * killRatio);
      kia += kiaNum;
      rf.soldiers -= kiaNum;
      reduced[3] = kiaNum;
      int killLabor = (int)(kiaNum * 0.8f);
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
          rf.mov * GetMovementModifier() *
          (rf.morale >= Troop.MaxMorale ? 1.2f : 1f) *
          (1f + (IsStarving() ? -0.45f : 0f)) * 
          (epidemic != null && epidemic.IsValid() ? 0.7f : 1f));
    }

    public StaminaLvl GetStaminaLevel()
    {
      if (IsStarving())
      {
        return StaminaLvl.Exhausted;
      }
      if (movementRemaining >= GetFullMovement())
      {
        return StaminaLvl.Vigorous;
      }
      if (movementRemaining > 30)
      {
        return StaminaLvl.Fresh;
      }
      if (movementRemaining > ((this.type == Type.Infantry) ? Infantry.ExhaustLine : Cavalry.ExhaustLine))
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
      return rf.province.region.RetreatThreshold();
    }

    // ==============================================================
    // ================= buff & debuff ==============================
    // ==============================================================
    public int disarmorDefDebuf = 0;
    public int def
    {
      get
      {
        int defence = (int)(rf.def + (rf.def *
        (GetBuff() + vantage.DefBuf())
        ) - disarmorDefDebuf);
        return defence <= 0 ? 1 : defence;
      }
    }
    public int atk
    {
      get
      {
        int attack = (int)(rf.atk + (rf.atk * 
        (GetBuff() + vantage.AtkBuf() + weatherEffect.AtkBuf())
        ));
        return attack <= 0 ? 1 : attack;
      }
    }

    public void Poisioned() {
      unitPoisioned.Poision();
    }

    float GetBuff()
    {
      return GetStarvingBuf() + GetStaminaBuf() + GetMoraleBuf() + GetStateBuf() + GetNewGeneralBuf();
    }

    float newGeneralDebuf = 0f;
    public void SetNewGeneralBuf() {
      newGeneralDebuf = -0.3f;
      rf.morale -= 5;
    }

    public float GetNewGeneralBuf() {
      return newGeneralDebuf;
    }

    float GetStarvingBuf()
    {
      return IsStarving() ? -0.3f : 0f;
    }

    float GetStateBuf()
    {
      if (state == State.Conceal)
      {
        return 0.2f;
      }
      if (state == State.Camping)
      {
        return 0.1f;
      }
      return 0f;
    }

    float GetMoraleBuf()
    {
      float buff = (rf.morale - rf.province.region.RetreatThreshold() * 2) * 0.008f;
      return buff > 0 ? buff : (IsWarWeary() ? -0.5f : 0f);
    }

    float GetSicknessBuf()
    {
      return IsSicknessAffected() ? -0.2f : 0f; 
    }

    float GetStaminaBuf()
    {
      if (GetStaminaLevel() == StaminaLvl.Vigorous)
      {
        return 0.05f;
      }

      if (GetStaminaLevel() == StaminaLvl.Fresh)
      {
        return 0f;
      }

      if (GetStaminaLevel() == StaminaLvl.Tired)
      {
        return -0.05f;
      }

      return -0.2f;
    }

    // ==============================================================
    // ================= preflight ==================================
    // ==============================================================
    public Unit Preflight(Tile target, Tile[] path = null)
    {
      // No preflight for camping unit
      if (state == State.Camping) return null;
      // TODO: target tile should be within unit's accessible range
      if (!target.DeployableForPathFind(this)) return null;
      Unit clone = Clone();
      int totalCost = 0;
      foreach(Tile tile in path) {
        if (!Util.eq<Tile>(this.tile, tile)) {
          // get rid of the first tile
          totalCost = AggregateCostToEnterTile(tile, totalCost, PathFind.Mode.Normal);
        }
      }
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

    // ==============================================================
    // ================= unit actions ===============================
    // ==============================================================
    public void UpdateGeneralName() {
      UnitView view = hexMap.GetUnitView(this);
      if (view != null) {
        view.UpdateGeneralName();
      }
    }

    public void UpdateUnitInfo() {
      UnitView view = hexMap.GetUnitView(this);
      if (view != null) {
        view.UpdateUnitInfo();
      }
    }

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

    bool AfterMoveUpdate(List<Unit> ambusher) {
      bool continueMoving = true;
      if (this.IsAI()) {
        // TODO: use an efficient way
        FoW.Get().Fog();
      }

      if (IsConcealed() && hexMap.IsInEnemyScoutRange(this.IsAI(), tile)) {
        DiscoveredByEnemy();
        continueMoving = false;
      }

      List<Unit> candidates = new List<Unit>();
      bool ambushed = false;
      foreach(Tile t in tile.neighbours) {
        Unit candidate = t.GetUnit();
        if (candidate != null && candidate.IsAI() != IsAI()) {
          candidates.Add(candidate);
          if (candidate.IsAmbushing()) {
            ambushed = true;
          }
        }
      }
      if (ambushed) {
        ambusher = candidates;
        continueMoving = false;
      }

      // discover nearby enemies
      foreach(Tile t in GetScoutArea()) {
        Unit candidate = t.GetUnit();
        if (candidate != null && candidate.IsAI() != IsAI() && candidate.IsConcealed()) {
          candidate.DiscoveredByEnemy();
        }
      }

      return continueMoving;
    }

    public bool DoMove(List<Unit> ambusher, Tile toTile = null)
    {
      if (movementRemaining <= 0) {
        return false;
      }

      Tile next = null;
      if (toTile != null) {
        path = null;
        next = toTile;
      } else if (path != null && path.Count > 0) {
        next = path.Peek();
      } else {
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

      movementRemaining -= takenMovement;
      SetTile(next);
      return AfterMoveUpdate(ambusher);
    }

    public void SetTile(Tile h, bool dontAddToNewTile = false)
    {
      if (tile != null)
      {
        tile.RemoveUnit(this);
      }
      if (!dontAddToNewTile && h != null)
      {
        // not encamp, do move
        h.AddUnit(this);
        UnitActionBroker broker = UnitActionBroker.GetBroker();
        broker.BrokeChange(this, ActionType.UnitMove, tile);
      }
      tile = h;
    }

    // ==============================================================
    // ================= path finding ===============================
    // ==============================================================

    // TODO: FIX THIS for AI
    public Tile[] GetAttackRange()
    {
      List<Tile> range = new List<Tile>();
      foreach (Tile h in tile.neighbours)
      {
        Unit u = h.GetUnit();
        if (u == null || u.IsAI())
        {
          range.Add(h);
        }
      }
      return range.ToArray();
    }

    public Tile[] GetAccessibleTiles()
    {
      return PFTile2Tile(PathFinder.FindAccessibleTiles(tile, this, movementRemaining));
    }

    public Tile[] GetAccessibleTilesForSupply(Tile start, int remaining)
    {
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
    public Tile[] FindPath(Tile source, Tile target, bool targetAlwaysReachable)
    {
      return PFTile2Tile(PathFinder.FindPath(source, target, this, PathFind.Mode.Supply));
    }

    // movement pathfind
    public Tile[] FindPath(Tile target)
    {
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