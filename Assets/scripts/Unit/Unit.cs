using System.Collections.Generic;
using PathFind;
using MapTileNS;
using MonoNS;
using UnityEngine;
using CourtNS;
using FieldNS;
using System.Linq;
using NatureNS;

namespace UnitNS
{
  public abstract class Unit : PFUnit, DataModel
  {
    protected abstract Unit Clone();
    public const int MovementCostOnUnaccesible = -1;
    public virtual bool IsCavalry() {
      return false;
    }

    public bool IsVeteran() {
      return rf.IsVeteran();
    }

    public const int DisbandUnitUnder = 200;

    public const int L0Visibility = 1;
    public const int L1Visibility = 2;
    public const int VantageVisibility = 5;
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
    public InCampComplain inCampComplain;
    public OnFieldComplain onFieldComplain;
    WeatherGenerator weatherGenerator;
    TurnController turnController;
    public int surroundCnt = 0;
    const int MaxMorale = 100;
    private int _morale = MaxMorale;
    public int morale {
      get {
        return _morale;
      }
      set {
        _morale = value < 0 ? 0 : (value > MaxMorale ? MaxMorale : value);
      }
    }
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
      inCampComplain = new InCampComplain(this);
      onFieldComplain = new OnFieldComplain(this);
      InitAllowedAtmpt();
      InitChargeAtmpt();
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

      broker.BrokeChange(this, ActionType.UnitVisible, tile);
    }

    public int movementRemaining {
      get {
        return __movementRemaining;
      }
      set {
        __movementRemaining = value < 0 ? 0 : value;
      }
    }
    public Tile tile;
    public int kia;
    public Troop rf;
    public State state = State.Stand;

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

    public bool CanBeShaked(Unit charger) {
      return IsOnField() && !IsVulnerable();
    }

    public int CanBeSurprised() {
      if (!IsOnField() || IsVulnerable() || tile.vantagePoint || Ambusher.Get(rf.general) != null) {
        return 0;
      }
      if (rf.general.Is(Cons.reckless)) {
        return 95;
      }
      return 60;
    }

    public bool CanBeCrashed() {
      return !crashed;
    }

    public bool IsVulnerable() {
      return morale == 0;
    }

    public bool NoRedZone() {
      return IsHidden() || IsVulnerable() || IsWarWeary();
    }

    public int allowedAtmpt = 1;
    void InitAllowedAtmpt() {
      allowedAtmpt = 1;
      if (StaminaManager.Aval(this) && StaminaManager.Get(rf.general).Consume()) {
        allowedAtmpt++;
      }
    }

    public void UseAtmpt() {
      allowedAtmpt--;
      int p = (int)(GetFullMovement() / 2);
      movementRemaining = movementRemaining > p ? movementRemaining : p;
    }

    int _chargeAtmpt = 0;
    public int chargeAtmpt {
      get {
        return _chargeAtmpt;
      }
      set {
        _chargeAtmpt = value < 0 ? 0 : value;
      }
    }

    void InitChargeAtmpt() {
      chargeAtmpt = IsCavalry() ? 10000 : 0;
    }

    bool charged = false;
    public List<Unit> CanCharge() {
      List<Unit> targets = new List<Unit>();
      if (charged || IsSurrounded() || IsVulnerable() || chargeAtmpt == 0) {
        return targets;
      }
      return GetChargeTargets();
    }

    public List<Tile> CanFire() {
      List<Tile> tiles = new List<Tile>();
      foreach (Tile t in tile.neighbours) {
        if (t.burnable || FireBug.Aval(this) && t.field == FieldType.Forest) {
          tiles.Add(t);
        }
      }
      return tiles;
    }

    public void Charge() {
      charged = true;
      chargeAtmpt--;
    }

    public List<Unit> CanBreakThrough() {
      if (!IsSurrounded() || CanAttack().Count == 0 && IsVulnerable()) {
        return new List<Unit>();
      }
      return OnFieldEnemy();
    }

    public List<Tile> GetBurnableTiles() {
      List<Tile> tiles = new List<Tile>();
      Weather weather = weatherGenerator.currentWeather;
      if(Cons.IsHeavyRain(weather) || Cons.IsRain(weather)
        || Cons.IsSnow(weather) || Cons.IsBlizard(weather)) {
        return tiles;
      }

      foreach(Tile t in tile.neighbours) {
        if (t.field != FieldType.Forest || tile.GetGaleAdvantage(t) == WindAdvantage.Disadvantage) {
          continue;
        }
        if (t.burnable ||
            FireBug.Aval(this) ||
            tile.GetGaleAdvantage(t) == WindAdvantage.Advantage) {
            tiles.Add(t);
          }
      }

      return tiles;
    }

    public List<Tile> CanAttack() {
      List<Tile> tiles = new List<Tile>();
      if (allowedAtmpt == 0) {
        return tiles;
      }

      foreach(Tile t in tile.neighbours) {
        if (t.settlement != null && t.settlement.owner.isAI != IsAI()) {
          tiles.Add(t);
          continue;
        }
        Unit u = t.GetUnit();
        if (u != null && u.IsAI() != IsAI()) {
          tiles.Add(t);
        }
      }
      return tiles;
    }

    public List<Tile> CanPoison() {
      List<Tile> tiles = new List<Tile>();
      foreach(Tile t in tile.neighbours) {
        if (t.poision != null) {
          tiles.Add(t);
        }
      }
      return tiles;
    }

    public bool fooled = false;
    bool freezed = false;
    public List<Unit> CanFreeze() {
      if (freezed || !Freezer.Aval(this)) {
        return new List<Unit>();
      }
      return GetFreezeTargets();
    }

    bool decieved = false;
    public List<Unit> CanDecieve() {
      if (decieved || !Deciever.Aval(this) || FindDeployableTile() == null) {
        return new List<Unit>();
      }
      return OnFieldEnemy();
    }

    bool ploted = false;
    public List<Unit> CanPlot() {
      if (ploted || !Agitator.Aval(this)) {
        return new List<Unit>();
      }
      return GetPlotTargets();
    }

    bool surprised = false;
    public List<Unit> CanSurpriseAttack() {
      if (!surprised && Ambusher.Aval(this) && IsHidden()) {
        return GetSurpriseTargets();
      }
      return new List<Unit>();
    }

    bool rallyed = false;
    public List<Unit> CanRally() {
      if (!rallyed && Rally.Aval(this) && IsOnField()) {
        return OnFieldAllies(true);
      }
      return new List<Unit>();
    }

    public List<Tile> CanDecamp(Settlement s) {
      List<Tile> deployableTiles = new List<Tile>();
      if (IsOnField()) {
        return deployableTiles;
      }

      foreach(Tile tile in s.baseTile.neighbours) {
        Unit u = tile.GetUnit();
        if (tile.Deployable(this)) {
          deployableTiles.Add(tile);
        }
      }
      return deployableTiles;
    }

    public bool CanTriggerRetreat() {
      return !retreated;
    }

    public bool CanBury() {
      return !IsCavalry() && tile.deadZone.DecompositionCntDown > 0;
    }

    public Settlement CanEncamp() {
      Settlement s = AllySettment();
      if (s == null || s.IsUnderSiege() || !s.HasRoom()) {
        return null;
      }
      return s;
    }

    public Tile CanSabotage() {
      Tile t = null;
      if (IsCavalry()) {
        return t;
      }

      if (tile.siegeWall != null && tile.siegeWall.owner.isAI != IsAI()) {
        return tile;
      }

      foreach(Tile t1 in tile.neighbours) {
        if (t1.isDam) {
          t = t1;
          break;
        }
      }
      return t;
    }

    public bool CanSiege() {
      Settlement s = EnemySettment(); 
      return !IsCavalry() && s != null && !s.IsEmpty();
    }

    public Settlement AllySettment() {
      Settlement s = null;
      if (IsCamping()) {
        return s;
      }

      foreach (Tile t in tile.neighbours) {
        Settlement s1 = t.settlement;
        if (s1 != null && s1.owner.isAI == IsAI()) {
          s = s1;
        }
      }
      return s;
    }

    public Settlement EnemySettment() {
      Settlement s = null;
      if (IsCamping()) {
        return s;
      }

      foreach (Tile t in tile.neighbours) {
        Settlement s1 = t.settlement;
        if (s1 != null && s1.owner.isAI != IsAI()) {
          s = s1;
        }
      }
      return s;
    }

    public void Surprise() {
      surprised = true;
    }

    public bool Freeze(Unit target) {
      Freezer.Get(rf.general).Consume();
      freezed = true;
      if (target.rf.general.Is(Cons.conservative) || Cons.FiftyFifty()) {
        target.fooled = true;
        return true;
      }
      return false;
    }

    public bool Decieve(Unit target) {
      decieved = true;
      Deciever.Get(rf.general).Consume();
      if (target.rf.general.Is(Cons.reckless) || Cons.FiftyFifty()) {
        return true;
      }
      return false;
    }

    public ConflictResult Plot(Unit target) {
      ploted = true;
      Agitator.Get(rf.general).Consume();
      return target.unitConflict.Occur();
    }

    public int RallyAlly() {
      rallyed = true;
      Rally.Get(rf.general).Consume();
      return Rally.MoraleBuf;
    }

    public bool retreated = false;
    public bool crashed = false;
    public bool IsHidden() {
      return tile.field == FieldType.Forest && !hexMap.GetWarParty(this).counterParty.discoveredTiles.Contains(tile);
    }

    public List<Unit> OnFieldAllies(bool selfIncluded = false) {
      List<Unit> allies = new List<Unit>();
      if (selfIncluded) {
        allies.Add(this);
      }
      foreach(Tile t in tile.neighbours) {
        Unit u = t.GetUnit();
        if (u != null && u.IsOnField() && u.IsAI() == IsAI()) {
          allies.Add(u);
        }
      }
      return allies;
    }

    public List<Unit> OnFieldEnemy() {
      List<Unit> enemy = new List<Unit>();
      foreach(Tile t in tile.neighbours) {
        Unit u = t.GetUnit();
        if (u != null && u.IsOnField() && u.IsAI() != IsAI()) {
          enemy.Add(u);
        }
      }
      return enemy;
    }

    public string GetStateName()
    {
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

    public bool IsOnField() {
      return state == State.Stand;
    }

    public bool IsSurrounded() {
      bool isSurrounded = false;
      if (IsCamping()) {
        isSurrounded = true;
        foreach(Tile tile in tile.neighbours) {
          if(tile.Passable(IsAI()) ||
            (tile.settlement != null && tile.settlement.owner.isAI == IsAI())) {
            isSurrounded = false;
            break;
          }
        }
      } else {
        foreach(KeyValuePair<HashSet<Unit>, HashSet<Tile>> kvp in 
          hexMap.GetWarParty(this).GetFreeSpaces()) {
          if (kvp.Value.Count == 0 && kvp.Key.Contains(this)) {
            isSurrounded = true;
            break;
          }
        }
      }
      return isSurrounded;
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

    public List<Unit> GetFreezeTargets() {
      List<Unit> units = new List<Unit>();
      foreach(Tile t in tile.GetNeighboursWithinRange<Tile>(GetVisibleRange(), (Tile _tile) => true)) {
        Unit unit = t.GetUnit();
        if (unit != null && unit.IsAI() != IsAI() && !unit.fooled && !unit.IsCommander()) {
          units.Add(unit);
        }
      }
      return units;
    }

    public List<Unit> GetPlotTargets() {
      List<Unit> units = new List<Unit>();
      foreach(Tile t in tile.GetNeighboursWithinRange<Tile>(GetVisibleRange(), (Tile _tile) => true)) {
        Unit unit = t.GetUnit();
        if (unit != null && unit.IsAI() != IsAI() && !unit.unitConflict.conflicted && !unit.ApplyDiscipline()) {
          List<Province> conflictProvinces = unit.rf.province.GetConflictProvinces();
          foreach(Unit u in unit.OnFieldAllies()) {
            if(conflictProvinces.Contains(u.rf.province) ||
              !Util.eq<Region>(u.rf.province.region, unit.rf.province.region)) {
              units.Add(unit);
            }
          }
        }
      }
      return units;
    }

    public List<Unit> GetSurpriseTargets() {
      Tile[] tiles = GetSurpriseAttackTiles();
      if (tiles.Length == 0) { return new List<Unit>(); }

      List<Unit> targets = new List<Unit>();
      foreach(Tile t in tiles) {
        Unit unit = t.GetUnit();
        if (unit != null && unit.IsAI() != IsAI() && unit.CanBeSurprised() > 0) {
          targets.Add(unit);
        }
      }
      return targets;
    }

    public List<Unit> GetChargeTargets() {
      List<Unit> targets = new List<Unit>();
      foreach (Unit u in OnFieldEnemy()) {
        if (u.CanBeShaked(this)) {
          targets.Add(u);
        }
      }
      return targets;
    }

    public Tile[] GetSurpriseAttackTiles() {
      int range = GetVisibleRange();
      return tile.GetNeighboursWithinRange(range > L1Visibility ? L1Visibility : range,
        (Tile t) => FindAttackPath(t).Length > 0);
    }

    public int GetVisibleRange() {
      int v;
      if (Cons.IsMist(weatherGenerator.currentWeather) && !Outlooker.Aval(this)) {
        v = L0Visibility;
      } else {
        v = vantage.IsAtVantagePoint() ? VantageVisibility : L1Visibility;
      }
      return v;
    }

    public Tile[] GetVisibleArea() {
      int range = GetVisibleRange();
      Tile[] detectRange =
        !Outlooker.Aval(this) ?
          tile.GetNeighboursWithinRange<Tile>(vantage.IsAtVantagePoint() ? range : L0Visibility,
            (Tile _t) => true) : new Tile[]{}; 

      return tile.GetNeighboursWithinRange<Tile>(range, (Tile tile) => {
        if (tile.field == FieldType.Forest && !Outlooker.Aval(this) && !detectRange.Contains(tile)) {
          return false;
        }
        return true;
      });
    }

    public bool Obedient() {
      return MyCommander().commandSkill.Obey();
    }

    public int ImproviseOnSupply() {
      if (Ambusher.Aval(this) && Ambusher.Get(rf.general).Consume()) {
        return Ambusher.SupplyPunishment; 
      }
      return -25;
    }

    public bool RetreatOnDefeat() {
      if (IsVulnerable()) {
        return true;
      }
      return false;
    }

    public bool ApplyDiscipline() {
      return rf.IsSpecial() || Cons.FiftyFifty();
    }

    public General MyCommander() {
      return hexMap.GetWarParty(this).commanderGeneral;
    }

    public int Defeat(int moraleDrop) {
      morale += moraleDrop;
      return moraleDrop;
    }

    public int Victory(int moraleIncr) {
      morale += moraleIncr;
      return moraleIncr;
    }

    // ==============================================================
    // ================= Unit Settled&Refresh =======================
    // ==============================================================

    // Before new turn starts
    public int[] RefreshUnit()
    {
      rf.general.commandSkill.Reset();
      morale += IsCamping() ? 20 : 5;
      crashed = retreated = ploted = decieved = freezed = rallyed = charged = surprised = false;
      InitAllowedAtmpt();
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

    public Tile FindDeployableTile() {
      List<Tile> tiles = new List<Tile>();
      foreach(Tile t in tile.neighbours) {
        if (t.Deployable(this)) {
          tiles.Add(t);
        }
      }
      int len = tiles.Count;
      if (len == 0) {
        return null;
      }
      Tile[] array = tiles.ToArray();
      return array[Util.Rand(0, len - 1)];
    }

    public bool CanRetreat() {
      return CanTriggerRetreat() && (hexMap.IsAttackSide(IsAI()) ? hexMap.AttackerZone : hexMap.DefenderZone).Contains(tile);
    }

    public bool SetRetreatPath() {
      Tile[] path = new Tile[0];
      foreach (Tile t in hexMap.IsAttackSide(IsAI()) ? hexMap.AttackerZone : hexMap.DefenderZone) {
        path = FindPath(this.tile, t);
        if (path.Length > 0) {
          break;
        }
      }
      if (path.Length == 0) {
        return false;
      }
      SetPath(path);
      return true;
    }

    public int Killed(int killed, bool all = false) {
      int num = killed;
      if (!all && Evader.Aval(this) && Evader.Get(rf.general).Consume()) {
        num = (int)(killed * (1 - Evader.DeathDropBy));
      }
      rf.soldiers -= num;
      kia += num;
      return num;
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
      Killed(killed, true);
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
      morale -= reduceMorale;
      reduced[0] = -reduceMorale;
      int moveReduce = (int)(movementRemaining * movementDropRatio);
      movementRemaining = movementRemaining - moveReduce; 
      reduced[1] = -moveReduce;
      reduced[2] = Killed((int)(rf.soldiers * killRatio));
      return reduced;
    }

    // ==============================================================
    // ================= movement mangement =========================
    // ==============================================================

    public int GetFullMovement()
    {
      int full = (int)( rf.mov * (IsSick() ? 0.4f : 1));
      if ( Cons.IsSnow(hexMap.weatherGenerator.currentWeather)) {
        full = (int)(full / 2);
      } else if (Cons.IsHeavyRain(hexMap.weatherGenerator.currentWeather) && !Ambusher.Aval(this) || Cons.IsBlizard(hexMap.weatherGenerator.currentWeather)) {
        full = (int)(full / 4);
      }
      return full;
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

    public int unitCampingAttackCombatPoint {
      get {
        int total = vantage.TotalPoints(cp);
        total = (int)((total + total * GetCampingAttackBuff()) * 0.1f);
        return total < 0 ? 0 : total;
      }
    }

    public bool CanBePoisioned() {
      return unitPoisioned.Poision();
    }

    float GetCampingAttackBuff()
    {
      return GetMentalBuf() - plainSickness.debuf - disarmorDefDebuf;
    }

    public float GetBuff()
    {
      return GetCampingAttackBuff() + vantage.Buf();
    }

    public float GetMentalBuf() {
      return warWeary.GetBuf();
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
      UpdateVisibleTiles();
      path = null;
      SetState(State.Stand);
    }

    public bool DoMove(Tile toTile = null)
    {
      Tile next = null;
      int takenMovement;
      if (toTile != null) {
        path = null;
        next = toTile;
        takenMovement = CostToEnterTile(next, PathFind.Mode.Normal);
      } else {
        if (path != null && path.Count > 0) {
          next = path.Peek();
        } else {
          return false;
        }

        if (movementRemaining <= 0) {
          return false;
        }

        takenMovement = CostToEnterTile(next, PathFind.Mode.Normal);
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
        next = path.Dequeue();
      }

      Unit u = next.GetUnit();
      if (u != null && u.IsAI() != IsAI())
      {
        // stumble upon a hidden enemy
        path = null;
        return false;
      }
      movementRemaining -= takenMovement;
      SetTile(next);
      UpdateVisibleTiles();
      return true;
    }

    void UpdateVisibleTiles() {
      foreach(Tile t in GetVisibleArea()) {
        hexMap.GetWarParty(this).DiscoverTile(t);
      }
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

    public bool IsAtRetreatZone() {
      foreach (Tile t in hexMap.IsAttackSide(IsAI()) ? hexMap.AttackerZone : hexMap.DefenderZone) {
        if (Util.eq<Tile>(t, tile)) {
          return true;
        }
      }
      return false;
    }

    public Tile FindBreakThroughPoint() {
    Tile[] tiles = this.tile.GetNeighboursWithinRange<Tile>(5, (Tile t) => true);
    List<Tile> deployables = new List<Tile>(tiles){};
    // sort tiles from near to far
    deployables.Sort(delegate (Tile a, Tile b)
    {
      return (int)(Tile.Distance(this.tile, a) - Tile.Distance(this.tile, b));
    });

    Tile target = null;
    foreach (Tile t in hexMap.IsAttackSide(IsAI()) ? hexMap.AttackerZone : hexMap.DefenderZone) {
      target = t;
      break;
    }

    Tile tile = null;
    float score = 0f;
    List<Tile> first8 = new List<Tile>();
    int cnt = 0;
    foreach(Tile t in deployables) {
      if (cnt > 8) {
        break;
      }
      if (t.Deployable(this)) {
        first8.Add(t);
        cnt++;
      }
    }

    foreach(Tile t in first8) {
      float dist = Tile.Distance(t, target);
      if (tile == null || dist < score) {
        tile = t;
        score = dist;
      }
    }

    return tile;
  }

    // ==============================================================
    // ================= path finding ===============================
    // ==============================================================
    void InitCache() {
      WarParty wp = hexMap.GetWarParty(this);
      wp.cachedColorMap = wp.GetTileColorMap();
    }

    Tile[] GetPureAccessibleTiles(bool fullMovement = false) {
      InitCache();
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

    public Tile[] FindAttackPath(Tile target)
    {
      List<Tile> tiles = new List<Tile>();
      target.ignoreUnit = true;
      InitCache();
      foreach(Tile t in PFTile2Tile(PathFinder.FindPath(tile, target, this))) {
        if (!Util.eq<Tile>(t, target)) {
          tiles.Add(t);
        }
      }
      target.ignoreUnit = false;
      return tiles.ToArray();
    }

    // only for Ghost unit to pathfind settlement path
    public Tile[] FindPath(Tile source, Tile target)
    {
      hexMap.GetWarParty(this).cachedColorMap = null;
      return PFTile2Tile(PathFinder.FindPath(source, target, this, PathFind.Mode.Supply));
    }

    // movement pathfind
    public Tile[] FindPath(Tile target)
    {
      if (target.GetUnit() != null) {
        return new Tile[]{};
      }
      InitCache();
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
      return path != null ? path.ToArray() : new Tile[]{};
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