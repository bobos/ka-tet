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
    protected abstract Unit Clone();

    public const int ActionCost = 15; // For actions like: attack, bury
    public const int DefenceCost = 10;
    public const int MovementcostOnHill = 25;
    public const int MovementcostOnHillRoad = 15;
    public const int MovementcostOnPlain = 15;
    public const int MovementCostOnUnaccesible = -1;
    public const int DisbandUnitUnder = 200;

    public const int L0Visibility = 1;
    public const int L1Visibility = 2;
    public const int L2Visibility = 3;
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
    public MarchOnHeat marchOnHeat;
    public Supply supply;
    public WeatherEffect weatherEffect;
    public UnitConflict unitConflict;
    public PlainSickness plainSickness;
    public WarWeary warWeary;
    public Vantage vantage;
    WeatherGenerator weatherGenerator;
    TurnController turnController;
    public bool chaos = false;
    public int surroundCnt = 0;
    public Unit(bool clone, Troop troop, Tile tile, State state,
                int kia, int movement = -1)
    {
      rf = troop;
      this.clone = clone;
      this.kia = kia;
      hexMap = GameObject.FindObjectOfType<HexMap>();
      if (clone) {
        this.tile = tile;
        this.state = state;
      } else {
        weatherGenerator = hexMap.weatherGenerator;
        turnController = hexMap.turnController;

        SetTile(tile);
        SetState(state);
      }
      movementRemaining = movement;
    }

    public void Init() {
      epidemic = new ArmyEpidemic(this);
      altitudeSickness = new AltitudeSickness(this);
      armorRemEvent = new ArmorRemEvent(this);
      unitPoisioned = new UnitPoisioned(this);
      marchOnHeat = new MarchOnHeat(this);
      supply = new Supply(this);
      weatherEffect = new WeatherEffect(this);
      unitConflict = new UnitConflict(this);
      plainSickness = new PlainSickness(this);
      warWeary = new WarWeary(this);
      vantage = new Vantage(this);
      InitAllowedAtmpt();
    }

    public void CloneInit(float disarmorDefDebuf, Supply supply, PlainSickness plainSickness) {
      this.disarmorDefDebuf = disarmorDefDebuf;
      this.supply = supply;
      this.plainSickness = plainSickness;
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

    State[] visibleStates = {State.Stand};
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
        return __movementRemaining;
      }
      set {
        __movementRemaining = value < 0 ? 0 : (value > 100 ? 100 : value);
      }
    }
    public Tile tile;
    public int kia;
    public Troop rf;
    public State state = State.Stand;
    public bool defeating = false;

    public int __movementRemaining;
    Queue<Tile> path;
    // ==============================================================
    // ================= Unit Stat ==================================
    // ==============================================================
    public string Name()
    {
      return rf.name;
    }

    public string GetUnitName() {
      if (IsHeavyCavalry()) {
        return Cons.textLib.get("rank_heavyCav");
      }

      if (IsCavalry()) {
        return Cons.textLib.get("rank_lightCav");
      }

      return rf.rank.Name();
    }

    public string GeneralName()
    {
      return rf.general != null ? rf.general.Name() : " ";
    }

    public bool IsCommander() {
      return Util.eq<General>(hexMap.GetWarParty(this).commanderGeneral, rf.general);
    }

    public int CanBeShaked(Unit charger) {
      if (IsVulnerable()) {
        return 0;
      }
      int chance = 0;
      if (Cons.IsGale(hexMap.windGenerator.current)) {
        UnitPredict up = new UnitPredict();
        hexMap.combatController.SetGaleVantage(charger, this, up);
        if (up.windAdvantage) {
          chance = 30;
        }
        if (up.windDisadvantage) {
          chance = -20;
        }
      }
      if (tile.terrian == TerrianType.Hill) {
        chance += -30;
      }
      if(charger.IsHeavyCavalry() && !IsCavalry() && !IsCommander() && IsOnField() && !tile.vantagePoint) {
        if (Util.eq<Rank>(rf.rank, Cons.rookie)) {
          chance += 70;
        } else {
          chance += 40;
        }
      } else {
        chance = 0;
      }

      return chance < 0 ? 0 : (chance > 100 ? 100 : chance);
    }

    public bool IsHeavyCavalry() {
      return IsCavalry() && rf.rank == Cons.veteran;
    }

    public bool IsVulnerable() {
      return chaos || defeating;
    }

    public int allowedAtmpt = 1;
    void InitAllowedAtmpt() {
      allowedAtmpt = 1;
      if (IsCavalry() && rf.general.Has(Cons.staminaManager)) {
        allowedAtmpt = 2;
      }
    }
    public void UseAtmpt() {
      allowedAtmpt--;
    }

    public bool CanCharge() {
      return IsHeavyCavalry() &&
        CanAttack() && allowedAtmpt > 0 && rf.soldiers >= 800 && movementRemaining >= ActionCost; 
    }

    public bool retreated = false;
    public bool CanAttack() {
      return allowedAtmpt > 0;
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

    public int GetHeatSickTurns()
    {
      return epidemic.GetIllTurns();
    }

    public int GetAltitudeSickTurns()
    {
      return altitudeSickness.lastTurns;
    }

    public int GetPoisionTurns()
    {
      return unitPoisioned.GetIllTurns();
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

    public bool IsStarving() {
      return (supply != null && !supply.consumed) || false;
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
      int v = L1Visibility;
      if (Cons.IsMist(weatherGenerator.currentWeather)) {
        v = L0Visibility;
      } else if (IsCommander()) {
        v = vantage.IsAtVantagePoint() ? VantageVisibility : rf.general.commandSkill.GetCommandRange();
      }
      return tile.GetNeighboursWithinRange<Tile>(v, (Tile _tile) => true);
    }

    public bool InCommanderRange() {
      bool inRange = false;
      if (IsCommander()) {
        return true;
      }

      foreach(Tile t in MyCommander().commandUnit.onFieldUnit.GetVisibleArea()) {
        if (Util.eq<Tile>(tile, t)) {
          inRange = true;
          break;
        }
      }
      return inRange;
    }

    public bool ImproviseOnSupply() {
      if (!MyCommander().Has(Cons.backStabber)) {
        return false;
      }
      if (IsCommander()) {
        return true;
      }

      bool nearbyCommander = false;
      foreach(Tile tile in MyCommander().commandUnit.onFieldUnit.tile.neighbours) {
        if (tile.GetUnit() != null && Util.eq<Unit>(tile.GetUnit(), this)) {
          nearbyCommander = true;
          break;
        }
      }
      return nearbyCommander;
    }

    public bool StickAsNailWhenDefeat() {
      return (InCommanderRange() &&
              MyCommander().Has(Cons.turningTide) &&
              Cons.FiftyFifty()) ||
              (rf.general.Has(Cons.unshaken) && Cons.FiftyFifty());
    }

    public bool RetreatOnDefeat() {
      return rf.general.Has(Cons.retreater) && Cons.EvenChance();
    }

    public bool ApplyDiscipline() {
      return rf.general.Has(Cons.discipline) && Cons.FiftyFifty();
    }

    public General MyCommander() {
      return hexMap.GetWarParty(this).commanderGeneral;
    }

    protected virtual bool Concealable() {
      return false;
    }

    // ==============================================================
    // ================= Unit Settled&Refresh =======================
    // ==============================================================

    // Before new turn starts
    public int[] RefreshUnit()
    {
      chaos = false;
      defeating = false;
      retreated = false;
      InitAllowedAtmpt();

      if (concealCoolDownTurn > 0) {
        concealCoolDownTurn--;
      } else {
        if (Concealable() && state == State.Stand) {
          if (!hexMap.GetRangeForDiscoveryWarning(this.IsAI()).Contains(tile)) SetState(State.Conceal);
        }
      }

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

    public bool CanRetreat() {
      return !retreated && (hexMap.IsAttackSide(IsAI()) ? hexMap.AttackerZone : hexMap.DefenderZone).Contains(tile);
    }

    public bool SetRetreatPath() {
      Tile[] path = new Tile[0];
      foreach (Tile t in hexMap.IsAttackSide(IsAI()) ? hexMap.AttackerZone : hexMap.DefenderZone) {
        if (t.Deployable(this)) {
          path = FindPath(this.tile, t);
          if (path.Length > 0) {
            break;
          }
        }
      }
      if (path.Length == 0) {
        return false;
      }
      SetPath(path);
      return true;
    }

    public void Retreat() {
      // TODO: move the queuing unit in
      if (rf.general != null) {
        rf.general.TroopRetreat();
      }
      SetState(State.Retreated);
      tile.RemoveUnit(this);
    }

    public int Destroy()
    {
      // TODO: move the queuing unit in
      int killed = rf.soldiers;
      kia += killed;
      rf.soldiers = 0;
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
      float killRatio = 0f) {
      // morale, movement, killed, attack, def
      int[] reduced = new int[]{0,0,0,0,0};
      rf.morale -= reduceMorale;
      reduced[0] = -reduceMorale;
      int moveReduce = (int)(movementRemaining * movementDropRatio);
      movementRemaining = movementRemaining - moveReduce; 
      reduced[1] = -moveReduce;
      int kiaNum = (int)(rf.soldiers * killRatio);
      kia += kiaNum;
      rf.soldiers -= kiaNum;
      reduced[2] = kiaNum;
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
        (IsStarving() ? 0.8f : 1) *
        (plainSickness != null && plainSickness.affected ? (1 - plainSickness.moveDebuf) : 1) *
        (IsSick() ? 0.4f : 1));
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

    public int GetUnitDefendCombatPoint() {
      return (int)((IsCavalry() ? unitCombatPoint : (int)(unitCombatPoint * CombatController.DefendModifier)));
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
        total = (int)((total + total * GetCampingAttackBuff()) * 0.1f);
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

    float GetCampingAttackBuff()
    {
      return GetGeneralBuf() + GetChaosBuf() + GetWarwearyBuf() - plainSickness.debuf + rf.lvlBuf - disarmorDefDebuf;
    }

    public float GetBuff()
    {
      return GetCampingAttackBuff() + vantage.Buf();
    }

    public float GetWarwearyBuf()
    {
      return warWeary.GetBuf();
    }

    public float GetGeneralBuf() {
      return rf.general.Has(Cons.formidable) ? 0.25f : 0f;
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
      SetState(State.Stand);
    }

    public void DiscoveredByEnemy() {
      concealCoolDownTurn = ConcealCoolDownIn;
      SetState(State.Stand);
    }

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
      movementRemaining -= CostToEnterTile(next, PathFind.Mode.Normal);
      SetTile(next);
      foreach(Tile t in GetVisibleArea()) {
        hexMap.GetWarParty(this).DiscoverTile(t);
      }
      return true;
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
    public Tile[] GetPureAccessibleTiles(bool fullMovement = false) {
      return PFTile2Tile(PathFinder.FindAccessibleTiles(tile, this,
        fullMovement ? GetFullMovement() : movementRemaining));
    }

    public Tile[] GetAccessibleTiles(bool fullMovement = false)
    {
      if (!hexMap.deployDone) {
        return hexMap.InitPlayerDeploymentZone();
      }

      List<Tile> area = new List<Tile>();
      HashSet<Tile> visible = FieldNS.FoW.Get().GetVisibleArea();
      if (hexMap.wargameController.start) {
        visible = hexMap.wargameController.visibleArea;
      }

      List<Tile> zone = hexMap.IsAttackSide(this.IsAI()) ? hexMap.DefenderZone : hexMap.AttackerZone;
      foreach (Tile tile in GetPureAccessibleTiles(fullMovement)) {
        if (visible.Contains(tile) && tile.GetUnit() == null && !zone.Contains(tile)) {
          area.Add(tile);
        }
      }
      return area.ToArray();
    }

    // only for Ghost unit to pathfind settlement path
    public Tile[] FindPath(Tile source, Tile target)
    {
      return PFTile2Tile(PathFinder.FindPath(source, target, this, PathFind.Mode.Supply));
    }

    // movement pathfind
    public Tile[] FindPath(Tile target)
    {
      if (target.GetUnit() != null) {
        return new Tile[]{};
      }
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