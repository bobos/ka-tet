using System.Collections.Generic;
using PathFind;
using MapTileNS;
using MonoNS;
using UnityEngine;
using CourtNS;
using FieldNS;
using System.Linq;

namespace UnitNS
{
  public abstract class Unit : PFUnit, DataModel
  {
    public abstract bool IsCavalry();
    protected abstract Unit Clone();

    public const int MovementcostOnHill = 25;
    public const int MovementcostOnPlain = 25;
    public const int MovementCostOnUnaccesible = -1;
    public const int DisbandUnitUnder = 200;
    public const int Sides2HaveVantage = 4;

    public const int L0Visibility = 1;
    public const int L1Visibility = 2;
    public const int L2Visibility = 3;
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
    public RetreatStress retreatStress;
    WeatherGenerator weatherGenerator;
    TurnController turnController;
    public int defeatStreak = 0;
    public Mental mentality = Mental.Normal;
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
      inCampComplain = new InCampComplain(this);
      onFieldComplain = new OnFieldComplain(this);
      retreatStress = new RetreatStress(this);
      InitAllowedAtmpt();
      InitForecast();
      InitFalseOrder();
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

    public string GetUnitName() {
      return rf.rank.Name(rf.province.region, IsCavalry());
    }

    public string GeneralName()
    {
      return rf.general != null ? rf.general.Name() : " ";
    }

    public bool IsCommander() {
      return Util.eq<General>(hexMap.GetWarParty(this).commanderGeneral, rf.general);
    }

    int _chargeChance = 100;
    public int chargeChance {
      get {
        return _chargeChance;
      }
      set {
        _chargeChance = value < 0 ? 0 : value;
      }
    }

    public int CanBeShaked(Unit charger) {
      if (!IsOnField() || tile.vantagePoint || IsVulnerable() || chargeChance == 0) {
        return 0;
      }
      int chance = MentallyWeak() || (IsCavalry() && !IsHeavyCavalry()) ? 90 : 5;
      if (Cons.IsGale(hexMap.windGenerator.current)) {
        WindAdvantage advantage = charger.tile.GetGaleAdvantage(tile);
        if (advantage == WindAdvantage.Advantage) {
          chance += 80;
        }
        if (advantage == WindAdvantage.Disadvantage) {
          chance -= 80;
        }
      }

      if (hasNoOpenning) {
        chance = 5;
      }

      return chance < 0 ? 0 : (chance > 100 ? 100 : chance);
    }

    public List<Unit> GetHarrasTargets() {
      List<Unit> units = new List<Unit>();
      foreach(Tile t in tile.neighbours) {
        Unit u = t.GetUnit();
        if (u != null && u.IsAI() != IsAI() && u.CanBeWaved() && !u.IsCamping()) {
          units.Add(u);
        }
      }
      return units;
    }

    public int Skirmished(Unit skirmisher) {
      int point = 0;
      if (IsCavalry()) {
        point = 10;
      } else {
        if (Util.eq<Rank>(rf.rank, Cons.rookie)) {
          point = 30;
        } else {
          point = 20;
        }
      }
      if (Cons.IsGale(hexMap.windGenerator.current)) {
        WindAdvantage advantage = skirmisher.tile.GetGaleAdvantage(tile);
        if (advantage == WindAdvantage.Advantage) {
          point += 30;
        }
        if (advantage == WindAdvantage.Disadvantage) {
          point -= 30;
        }
      }
      if (skirmisher.rf.general.Has(Cons.vanguard)) {
        point += 30;
      }
      if (rf.general.Has(Cons.holdTheGround)) {
        point -= 20;
      }
      if (tile.terrian == TerrianType.Hill) {
        point = (int)(point * 0.8f);
      }
      if (skirmisher.rf.soldiers < 1500 && skirmisher.rf.soldiers >= 800) {
        point = (int)(point * 0.8f);
      }
      if (skirmisher.rf.soldiers < 800) {
        point = (int)(point * 0.5f);
      }
      point = point < 0 ? 0 : point;
      wavingPoint += point;
      return wavingPoint;
    }

    public int wavingPoint {
      get {
        return _wavingPoint;
      }
      set {
        _wavingPoint = value > 100 ? 100 : (value < 0 ? 0 : value);
        if (_wavingPoint == 100 && (mentality != Mental.Defeating || mentality != Mental.Chaotic)) {
          mentality = Mental.Waving;
          _wavingPoint = 0;
        }
      }
    }

    int _wavingPoint = 0;
    public bool CanBeWaved() {
      if (!IsOnField() || tile.vantagePoint || IsVulnerable() || MentallyWeak()) {
        return false;
      }
      return true;
    }

    public int CanBeSurprised() {
      if (!IsOnField() || tile.vantagePoint || tile.field == FieldType.Forest || rf.general.Has(Cons.ambusher)) {
        return 0;
      }
      return (!MentallyWeak() && (rf.general.Is(Cons.conservative) || rf.general.Is(Cons.cunning))) ? 20 : 95;
    }

    public bool CanBeCrashed() {
      return !crashed;
    }

    public bool IsHeavyCavalry() {
      return IsCavalry() && rf.rank == Cons.veteran;
    }

    public bool IsVulnerable() {
      return mentality == Mental.Defeating || mentality == Mental.Chaotic|| rf.morale == 0;
    }

    public int allowedAtmpt = 1;
    void InitAllowedAtmpt() {
      allowedAtmpt = 1;
      if (rf.general.Has(Cons.staminaManager)) {
        allowedAtmpt++;
      }
    }

    public void UseAtmpt() {
      allowedAtmpt--;
      if (IsCavalry() && !IsHeavyCavalry()) {
        movementRemaining = GetFullMovement();
      } else {
        int p = (int)(GetFullMovement() / 2);
        movementRemaining = movementRemaining > p ? movementRemaining : p;
      }
    }

    public bool CanCharge() {
      return IsHeavyCavalry() && CanAttack() && !IsSurrounded() && !MentallyWeak();
    }

    public bool CanHarras() {
      return IsCavalry() && !IsHeavyCavalry() && CanAttack() && !IsSurrounded() && !MentallyWeak() && !IsCamping();
    }

    public bool CanBreakThrough() {
      return IsSurrounded() && CanAttack();
    }

    public bool CanFire() {
      return !fireDone;
    }

    public bool CanPoision() {
      return !poisionDone;
    }

    public bool canForecast = false;
    public bool CanForecast() {
      return canForecast;
    }

    public bool fooled = false;
    public bool fooledOnce = false;
    public bool canFalseOrder = false;
    public bool canAlienate = false;
    public bool CanFalseOrder() {
      return canFalseOrder;
    }

    public bool CanAlienate() {
      return canAlienate;
    }

    void InitFalseOrder() {
      canFalseOrder = canAlienate = rf.general.Has(Cons.conspirator);
    }

    public bool FalseOrder(Unit target) {
      canFalseOrder = false;
      bool work = false;
      if (target.rf.general.Is(Cons.cunning)) {
        return work;
      }
      if (target.inCommanderRange) {
        work = target.rf.general.Is(Cons.calm) ? Cons.FairChance(): (target.rf.general.Is(Cons.conservative) ? Cons.SlimChance() : false);
      } else {
        work = target.rf.general.Is(Cons.calm) ? Cons.HighlyLikely(): (target.rf.general.Is(Cons.conservative) ? Cons.FairChance() : Cons.SlimChance());
      }
      if (work && !fooledOnce) {
        target.fooled = target.fooledOnce = true;
      }
      return target.fooled;
    }

    public ConflictResult Alienate(Unit target) {
      canAlienate = false;
      return target.unitConflict.Occur();
    }

    public bool Forecast() {
      canForecast = false;
      return Cons.MostLikely();
    }

    void InitForecast() {
      canForecast = rf.general.Has(Cons.forecaster);
    }

    public bool retreated = false;
    public bool crashed = false;
    public bool poisionDone = false;
    public bool fireDone = false;
    public bool CanAttack() {
      return allowedAtmpt > 0;
    }

    public bool CanSurpriseAttack(HashSet<Tile> tiles = null) {
      bool ret = CanAttack() && tile.field == FieldType.Forest && rf.general.Has(Cons.ambusher);
      if (ret) {
        tiles = tiles == null ? hexMap.GetWarParty(this, true).GetVisibleArea() : tiles;
        ret = !tiles.Contains(tile);
      }
      return ret;
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

    public bool HasNoOpenning() {
      if (IsVulnerable() || IsCamping()) {
        return false;
      }

      int cnt = 0;
      foreach(Tile t in tile.neighbours) {
        if (!IsControlledTile(t)) {
          cnt++;
        }
      }
      return cnt <= (6 - Sides2HaveVantage);
    }

    bool IsTileControlled(Tile t) {
      Unit u = t.GetUnit();
      if (t.Accessible()) {
        // empty field
        return u != null && u.IsAI() == IsAI();
      } else {
        // mountain, water, burning/flooding tile, or settlement
        return t.settlement == null || t.settlement.owner.isAI == IsAI();
      }
    }

    bool IsControlledTile(Tile t) {
      if (IsTileControlled(t)) {
        return true;
      }
      
      // empty field or enemy controlled settlement
      bool controlled = false;
      if (t.GetUnit() == null && t.settlement == null) {
        // just empty field
        controlled = true;
        foreach(Tile t1 in t.neighbours) {
          if (!IsTileControlled(t1)) {
            controlled = false;
            break;
          }
        }
      }

      return controlled;
    }

    public List<Unit> GetFalseOrderTargets() {
      List<Unit> units = new List<Unit>();
      foreach(Tile t in tile.GetNeighboursWithinRange<Tile>(GetVisibleRange(), (Tile _tile) => true)) {
        Unit unit = t.GetUnit();
        if (unit != null && unit.IsAI() != IsAI() && !unit.fooledOnce && !unit.IsCommander()) {
          units.Add(unit);
        }
      }
      return units;
    }

    public List<Unit> GetAlienateTargets() {
      List<Unit> units = new List<Unit>();
      foreach(Tile t in tile.GetNeighboursWithinRange<Tile>(GetVisibleRange(), (Tile _tile) => true)) {
        Unit unit = t.GetUnit();
        if (unit != null && unit.IsAI() != IsAI() && !unit.unitConflict.conflicted && !unit.IsCommander()) {
          units.Add(unit);
        }
      }
      return units;
    }

    public Unit[] GetSurpriseTargets() {
      Tile[] tiles = GetSurpriseAttackTiles();
      if (tiles.Length == 0) { return new Unit[]{}; }

      List<Unit> targets = new List<Unit>();
      foreach(Tile t in tiles) {
        Unit unit = t.GetUnit();
        if (unit != null && unit.IsAI() != IsAI() && unit.CanBeSurprised() > 0) {
          targets.Add(unit);
        }
      }
      return targets.ToArray();
    }

    public Tile[] GetSurpriseAttackTiles() {
      return tile.GetNeighboursWithinRange(GetVisibleRange(), (Tile t) => FindAttackPath(t).Length > 0);
    }

    public int GetVisibleRange() {
      int v;
      if (Cons.IsMist(weatherGenerator.currentWeather) && !rf.general.Has(Cons.outlooker)) {
        v = L0Visibility;
      } else {
        v = vantage.IsAtVantagePoint() ?
          VantageVisibility : (rf.general.Has(Cons.ambusher) ? L2Visibility : L1Visibility);
        v = (IsCommander() && rf.general.commandSkill.GetCommandRange() > v) ?
          rf.general.commandSkill.GetCommandRange() : v;
      }
      return v;
    }

    public Tile[] GetVisibleArea() {
      Tile[] detectRange =
        !rf.general.Has(Cons.outlooker) ?
          tile.GetNeighboursWithinRange<Tile>(L0Visibility, (Tile _t) => true) : new Tile[]{}; 

      return tile.GetNeighboursWithinRange<Tile>(GetVisibleRange(), (Tile tile) => {
        if (tile.field == FieldType.Forest && !rf.general.Has(Cons.outlooker) && !detectRange.Contains(tile)) {
          return false;
        }
        return true;
      });
    }

    public Tile[] GetCommandArea() {
      int v = GetVisibleRange();
      int cv = rf.general.commandSkill.GetCommandRange();
      v = cv < v ? cv : v;
      if (v == 0) {
        return new Tile[]{};
      }

      return tile.GetNeighboursWithinRange<Tile>(v, (Tile _tile) => {
          return true;
      });
    }

    public bool inCommanderRange = false;
    bool UnderCommand() {
      bool inRange = false;
      if (IsCommander()) {
        return true;
      }

      foreach(Tile t in MyCommander().commandUnit.onFieldUnit.GetCommandArea()) {
        if (Util.eq<Tile>(tile, t)) {
          inRange = true;
          break;
        }
      }
      return inRange;
    }

    public bool FollowOrder() {
      return inCommanderRange && MyCommander().commandSkill.ObeyMyOrder();
    }

    public bool ImproviseOnSupply() {
      if(Util.eq<Region>(rf.province.region, Cons.tubo)) {
        return true;
      }

      return rf.general.Has(Cons.improvisor);
    }

    public bool MentallyWeak() {
      return mentality == Mental.Waving || mentality == Mental.Defeating
       || mentality == Mental.Chaotic || warWeary.IsWarWeary();
    }

    public bool StickAsNailWhenDefeat() {
      if (MentallyWeak()) {
        return Cons.EvenChance();
      }
      return (inCommanderRange &&
              MyCommander().commandSkill.TurningTide()) ||
              (rf.general.Has(Cons.holdTheGround) && Cons.MostLikely()) ||
              (rf.IsSpecial() && Cons.MostLikely()) || Cons.FiftyFifty();
    }

    public bool RetreatOnDefeat() {
      if (IsCamping()) {
        return false;
      }
      if (rf.general.Is(Cons.conservative) || MentallyWeak()) {
        return Cons.EvenChance();
      } else {
        return Cons.SlimChance();
      }
    }

    public bool ApplyDiscipline(bool applied) {
      return rf.IsSpecial() || (rf.general.Has(Cons.discipline) && Cons.HighlyLikely()) || applied;
    }

    public General MyCommander() {
      return hexMap.GetWarParty(this).commanderGeneral;
    }

    public int Defeat(int moraleDrop) {
      if (mentality == Mental.Supercharged) {
        mentality = Mental.Normal;
      }
      wavingPoint += 20;
      int drop = moraleDrop + (defeatStreak++ * -2);
      rf.morale += drop;
      return drop;
    }

    public int Victory(int moraleIncr) {
      mentality = Mental.Supercharged;
      wavingPoint = defeatStreak = 0;
      rf.morale += moraleIncr;
      return moraleIncr;
    }

    public void UpdateInCommanderRange() {
      inCommanderRange = UnderCommand();
    }

    public bool hasNoOpenning = false;
    public void UpdateOpenningStatus() {
      hasNoOpenning = HasNoOpenning(); 
    }

    // ==============================================================
    // ================= Unit Settled&Refresh =======================
    // ==============================================================

    // Before new turn starts
    public int[] RefreshUnit()
    {
      if ((mentality == Mental.Chaotic && Cons.MostLikely()) || (
           mentality == Mental.Defeating && Cons.EvenChance())) {
        mentality = Mental.Waving;
        wavingPoint = 0;
      } else {
        mentality = Mental.Normal;
      }

      if (rf.general.Has(Cons.discipline)) {
        mentality = Mental.Normal;
      }
      crashed = fooled = retreated = poisionDone = fireDone = false;
      defeatStreak = 0;
      InitForecast();
      InitFalseOrder();
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

    public int Killed(int killed, bool all = false) {
      int num = killed;
      if (!all && rf.general.Has(Cons.doctor)) {
        num = (int)(killed * 0.6f);
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
      rf.morale -= reduceMorale;
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
      int full = (int)(
        // ghost unit doesnt have vantage
        rf.mov *
        (IsSick() ? 0.4f : 1));
      if (Cons.IsHeavyRain(hexMap.weatherGenerator.currentWeather) && IsCavalry()) {
        full = (int)(full / 2);
      } else if (Cons.IsSnow(hexMap.weatherGenerator.currentWeather)) {
        full = (int)(full / 2);
      } else if (Cons.IsBlizard(hexMap.weatherGenerator.currentWeather)) {
        full = (int)(full / 4);
      }
      return full;
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

    public int unitPureCombatPoint {
      get {
        int total = vantage.TotalPoints(cp);
        total = (int)((total + total * rf.lvlBuf) * 0.001f * rf.morale);
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
      return GetGeneralBuf() + GetMentalBuf() - plainSickness.debuf + rf.lvlBuf - disarmorDefDebuf;
    }

    public float GetBuff()
    {
      return GetCampingAttackBuff() + vantage.Buf() + HeavyCavalryBuf();
    }

    public float HeavyCavalryBuf() {
      return IsHeavyCavalry() ? 0.2f : 0f;
    }

    public float GetGeneralBuf() {
      return rf.general.Has(Cons.formidable) ? 0.25f : 0f;
    }

    public float GetMentalBuf() {
      float ret = defeatStreak * -0.1f;
      if (mentality == Mental.Supercharged) {
        ret += 0.1f;
      } else if (mentality == Mental.Defeating) {
        ret += -0.4f;
      } else if (mentality == Mental.Chaotic) {
        ret = -0.99f;
      }
      ret += warWeary.GetBuf();
      return ret < -0.99f ? -0.99f : ret;
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
      hexMap.UpdateUnitStatus(this);
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

    public Tile[] FindAttackPath(Tile target)
    {
      List<Tile> tiles = new List<Tile>();
      target.ignoreUnit = true;
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